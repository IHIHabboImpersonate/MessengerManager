using System.Collections.Generic;
using IHI.Database;
using IHI.Server.Habbos;
using IHI.Server.Libraries.Cecer1.Messenger;
using IHI.Server.Networking.Messages;
using NHibernate.Criterion;
using Friend = IHI.Server.Libraries.Cecer1.Messenger.Friend;
using Habbo = IHI.Server.Habbos.Habbo;

namespace IHI.Server.Plugins.Cecer1.MessengerManager
{
    public class Manager : Plugin
    {
        public static event MessengerEventHandler OnMessengerReady;

        public override void Start()
        {
            Habbo.OnHabboLogin += RegisterHandlers;
        }


        private static void RegisterHandlers(object source, HabboEventArgs args)
        {
            var target = source as Habbo;
            if (target == null)
                return;
            target.
                GetConnection().
                AddHandler(12, PacketHandlerPriority.DefaultAction, ProcessMessengerInit).
                AddHandler(41, PacketHandlerPriority.DefaultAction, ProcessMessengerSearch);
        }

        private static MessengerObject CreateMessenger(Habbo habbo)
        {
            var messenger = new MessengerObject(
                habbo,
                habbo.GetPersistantVariable("Messenger.StalkBlock") != null,
                habbo.GetPersistantVariable("Messenger.RequestBlock") != null,
                habbo.GetPersistantVariable("Messenger.InviteBlock") != null);


            List<MessengerCategory> categoriesOutput;
            List<MessengerFriendship> friendsOutput;
            List<MessengerFriendRequest> friendsRequestOutput;
            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                categoriesOutput = (List<MessengerCategory>) (db.CreateCriteria<MessengerCategory>()
                                                                 .Add(
                                                                     Restrictions.Eq("habbo_id", habbo.GetID()))
                                                                 .List<MessengerCategory>());

                friendsOutput = (List<MessengerFriendship>) (db.CreateCriteria<MessengerFriendship>()
                                                                .Add(
                                                                    new OrExpression(
                                                                        Restrictions.Eq("habbo_a.id", habbo.GetID()),
                                                                        Restrictions.Eq("habbo_b.id", habbo.GetID())))
                                                                .List<MessengerFriendship>());

                friendsRequestOutput = (List<MessengerFriendRequest>) (db.CreateCriteria<MessengerFriendRequest>()
                                                                          .Add(
                                                                              Restrictions.Eq("habbo_to_id",
                                                                                              habbo.GetID()))
                                                                          .List<MessengerFriendRequest>());
            }

            messenger.SetCategory(0, new Category(0, "", messenger)); // Default category

            foreach (var category in categoriesOutput)
            {
// ReSharper disable PossibleInvalidOperationException
                messenger.SetCategory(category.category_id.Value,
                                      new Category((int)category.category_id, category.name, messenger));
// ReSharper restore PossibleInvalidOperationException
            }

            foreach (var friendship in friendsOutput)
            {
                Habbo friendHabbo;
                Category friendCategory;
                MessengerObject friendMessenger;
                if (friendship.habbo_a.habbo_id == habbo.GetID())
                {
                    friendHabbo = CoreManager.GetServerCore().GetHabboDistributor().GetHabbo(friendship.habbo_b.habbo_id);
                    friendMessenger = friendHabbo.GetInstanceVariable("Messenger.Instance") as MessengerObject;
                    friendCategory = messenger.GetCategory(friendship.category_a.category_id);
                }
                else
                {
                    friendHabbo = CoreManager.GetServerCore().GetHabboDistributor().GetHabbo(friendship.habbo_b.habbo_id);
                    friendMessenger = friendHabbo.GetInstanceVariable("Messenger.Instance") as MessengerObject;
                    friendCategory = messenger.GetCategory(friendship.category_a.category_id);
                }
                friendCategory.AddFriend(new Friend(friendHabbo, friendCategory.GetID(), friendMessenger.IsStalkable()));
            }

            foreach (var request in friendsRequestOutput)
            {
                messenger.AddFriendRequest(
                    CoreManager.GetServerCore().GetHabboDistributor().GetHabbo(request.habbo_from_id));
            }

            messenger.OnMessengerFriendStateChanged += Messenger_OnMessengerFriendStateChanged;
            return messenger;
        }

        private static void ProcessMessengerInit(Habbo sender, IncomingMessage message)
        {
            var messenger = CreateMessenger(
                sender);

            sender.SetInstanceVariable("Messenger.Instance", messenger);
            new MMessengerInit(10, 20, 30, 40, messenger.GetAllCategories(), messenger.GetAllFriends()).Send(sender);

            sender.SetInstanceVariable("Messenger.WaitingUpdateMessage", new MMessengerUpdate());

            if (OnMessengerReady != null)
                OnMessengerReady.Invoke(messenger, new MessengerEventArgs(sender));
        }

        private static void ProcessMessengerSearch(Habbo sender, IncomingMessage message)
        {
            var searchString = message.PopPrefixedString();

            List<Database.Habbo> matching;
            // Using IHIDB.Habbo rather than IHIDB.Friend because this will be passed to the HabboDistributor
            using (var db = CoreManager.GetServerCore().GetDatabaseSession())
            {
                matching = db.CreateCriteria<Database.Habbo>().
                               Add(new LikeExpression("username", searchString + "%")).
                               SetMaxResults(20). // TODO: External config
                               List<Database.Habbo>() as List<Database.Habbo>;
            }

            var friends = new List<IBefriendable>();
            var strangers = new List<IBefriendable>();

            var messenger = sender.GetMessenger();
            var habboDistributor = CoreManager.GetServerCore().GetHabboDistributor();

            foreach (var match in matching)
            {
                IBefriendable habbo = habboDistributor.GetHabbo(match);
                if (messenger.IsFriend(match.habbo_id))
                    friends.Add(habbo);
                else
                    strangers.Add(habbo);
            }

            new MMessengerSearchResults(friends, strangers).Send(sender);
        }

        #region Messenger Updates

        private static void Messenger_OnMessengerFriendStateChanged(object source, MessengerFriendEventArgs e)

        {
            (source as MessengerObject).
                GetWaitingUpdateMessage()
                .Add(e);
        }

        #endregion
    }
}