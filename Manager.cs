using System.Collections.Generic;
using IHI.Server.Habbos;
using IHIDB = IHI.Database;
using IHI.Server.Libraries.Cecer1.Messenger;
using NHibernate;
using NHibernate.Criterion;

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


            IList<IHIDB.MessengerCategory> categoriesOutput;
            IList<IHIDB.MessengerFriendship> friendsOutput;
            IList<IHIDB.MessengerFriendRequest> friendRequestsOutput;

            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                categoriesOutput = db.CreateCriteria<IHIDB.MessengerCategory>()
                                        .Add(
                                            Restrictions.Eq("habbo_id", habbo.GetID()))
                                        .List<IHIDB.MessengerCategory>();

                friendsOutput = db.CreateCriteria<IHIDB.MessengerFriendship>()
                                        .Add(
                                            new OrExpression(
                                                Restrictions.Eq("habbo_a.id", habbo.GetID()),
                                                Restrictions.Eq("habbo_b.id", habbo.GetID())))
                                        .List<IHIDB.MessengerFriendship>();

                friendRequestsOutput = db.CreateCriteria<IHIDB.MessengerFriendRequest>()
                                        .Add(
                                            Restrictions.Eq("habbo_to_id",
                                                            habbo.GetID()))
                                        .List<IHIDB.MessengerFriendRequest>();
            }

            messenger.AddCategory(new Category(messenger, 0)
                                      {
                                          Name = ""
                                      });

            foreach (IHIDB.MessengerCategory category in categoriesOutput)
            {
                messenger.AddCategory(new Category(messenger, category.category_id)
                                          {
                                              Name = category.name
                                          });
            }

            foreach (IHIDB.MessengerFriendship friendship in friendsOutput)
            {
                Habbo friendHabbo;
                int categoryID = 0;

                if (friendship.habbo_a.habbo_id == habbo.GetID())
                {
                    friendHabbo = CoreManager.ServerCore.GetHabboDistributor().GetHabbo(friendship.habbo_b.habbo_id);
                    if (friendship.category_b_id.HasValue)
                        categoryID = friendship.category_b_id.Value;
                }
                else
                {
                    friendHabbo = CoreManager.ServerCore.GetHabboDistributor().GetHabbo(friendship.habbo_a.habbo_id);
                    if (friendship.category_a_id.HasValue)
                        categoryID = friendship.category_a_id.Value;
                }

                Category category = messenger.GetCategory(categoryID);
                category.AddFriend(friendHabbo);
            }

            foreach (IHIDB.MessengerFriendRequest request in friendRequestsOutput)
            {
                messenger.ReceiveFriendRequest(
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