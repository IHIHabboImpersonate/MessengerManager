using System.Collections.Generic;
using IHI.Database;
using IHI.Server.Libraries.Cecer1.Messenger;
using NHibernate;
using NHibernate.Criterion;
using Friend = IHI.Server.Libraries.Cecer1.Messenger.Friend;
using Habbo = IHI.Server.Habbos.Habbo;

namespace IHI.Server.Plugins.Cecer1.MessengerManager
{
	[CompatibilityLock(36)]
    public partial class Manager : Plugin
    {
        public static event MessengerEventHandler OnMessengerReady;

        public override void Start()
        {
            CoreManager.ServerCore.GetHabboDistributor().OnHabboLogin += RegisterHandlers;
            OnMessengerReady += RegisterEvents;
        }

        private static MessengerObject CreateMessenger(Habbo habbo)
        {
            MessengerObject messenger = new MessengerObject(habbo);


            List<MessengerCategory> categoriesOutput;
            List<MessengerFriendship> friendsOutput;
            List<MessengerFriendRequest> friendsRequestOutput;
            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
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

            foreach (MessengerCategory category in categoriesOutput)
            {
                messenger.SetCategory(category.category_id,
                                      new Category(category.category_id, category.name, messenger));
            }

            foreach (MessengerFriendship friendship in friendsOutput)
            {
                Habbo friendHabbo;
                Category friendCategory;
                if (friendship.habbo_a.habbo_id == habbo.GetID())
                {
                    friendHabbo = CoreManager.ServerCore.GetHabboDistributor().GetHabbo(friendship.habbo_b.habbo_id);
                    friendCategory = messenger.GetCategory(friendship.category_b);
                }
                else
                {
                    friendHabbo = CoreManager.ServerCore.GetHabboDistributor().GetHabbo(friendship.habbo_a.habbo_id);
                    friendCategory = messenger.GetCategory(friendship.category_a);
                }
                friendCategory.AddFriend(new Friend(friendHabbo)
                                             {
                                                 Category = friendCategory.GetID()
                                             });
            }

            foreach (MessengerFriendRequest request in friendsRequestOutput)
            {
                messenger.NotifyFriendRequest(
                    CoreManager.ServerCore.GetHabboDistributor().GetHabbo(request.habbo_from_id));
            }

            messenger.OnFriendStateChanged += Messenger_OnMessengerFriendStateChanged;
            return messenger;
        }

        

        #region Messenger Updates

        private static void Messenger_OnMessengerFriendStateChanged(object source, MessengerFriendEventArgs e)

        {
            (source as MessengerObject).GetWaitingUpdateMessage().FriendUpdates.Add(e);
        }

        #endregion
    }
}