using IHI.Database;
using System.Linq;
using IHI.Server.Libraries.Cecer1.Messenger;
using IHI.Server.Networking.Messages;
using NHibernate;
using Friend = IHI.Server.Libraries.Cecer1.Messenger.Friend;
using Habbo = IHI.Server.Habbos.Habbo;

namespace IHI.Server.Plugins.Cecer1.MessengerManager
{
    public static class MessengerExtensionMethods
    {
        public static MMessengerUpdate GetWaitingUpdateMessage(this MessengerObject messenger)
        {
            return messenger.GetOwner().GetInstanceVariable("Messenger.WaitingUpdateMessage") as MMessengerUpdate;
        }

        public static MessengerObject SendWaitingUpdateMessage(this MessengerObject messenger)
        {
            Habbo owner = messenger.GetOwner();
            var message = owner.GetInstanceVariable("Messenger.WaitingUpdateMessage") as MMessengerUpdate;
            message.Friends.Union(messenger.GetAllFriends());
            message.Categories.Union(messenger.GetAllCategories());
            message.Send(owner);

            message.FriendUpdates.Clear();
            message.Friends.Clear();
            message.Categories.Clear();
            return messenger;
        }

        public static MessengerObject SendFriendRequest(this MessengerObject messenger, Habbo to)
        {
            if (messenger.IsRequestedBy(to))
            {
                messenger.RemoveFriendRequest(to);
                messenger.GetCategory(0).AddFriend(new Friend(to)
                                                       {
                                                           Category = 0
                                                       });

                if (to.IsLoggedIn())
                {
                    MessengerObject toMessenger = to.GetMessenger();
                    toMessenger.GetCategory(0).AddFriend(new Friend(messenger.GetOwner())
                                                             {
                                                                 Category = 0
                                                             });
                }

                MessengerFriendship friendship = new MessengerFriendship
                                                     {
                                                         category_a = null,
                                                         category_b = null,
                                                         habbo_a = new Database.Habbo
                                                                       {
                                                                           habbo_id = to.GetID()
                                                                       },
                                                         habbo_b = new Database.Habbo
                                                                       {
                                                                           habbo_id = messenger.GetOwner().GetID()
                                                                       }
                                                     };

                using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
                {
                    db.SaveOrUpdate(friendship);
                    db.CreateQuery(
                        "delete MessengerFriendship f where f.habbo_from_id = :fromID and f.habbo_to_id = :toID")
                        .SetInt32("fromID", to.GetID())
                        .SetInt32("toID", messenger.GetOwner().GetID())
                        .ExecuteUpdate();
                }
                return messenger;
            }

            if (to.IsLoggedIn())
                to.GetMessenger().NotifyFriendRequest(messenger.GetOwner());
            MessengerFriendRequest friendRequest = new MessengerFriendRequest
                                                       {
                                                           habbo_from_id = messenger.GetOwner().GetID(),
                                                           habbo_to_id = to.GetID()
                                                       };

            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                db.SaveOrUpdate(friendRequest);
            }
            return messenger;
        }
    }
}