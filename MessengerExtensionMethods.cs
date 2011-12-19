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
            return messenger.Owner.GetInstanceVariable("Messenger.WaitingUpdateMessage") as MMessengerUpdate;
        }

        public static MessengerObject SendWaitingUpdateMessage(this MessengerObject messenger)
        {
            Habbo owner = messenger.Owner;
            var message = owner.GetInstanceVariable("Messenger.WaitingUpdateMessage") as MMessengerUpdate;
            message.Categories.Union(messenger.GetAllCategories());
            message.Send(owner);

            message.FriendUpdates.Clear();
            message.Categories.Clear();
            return messenger;
        }
    }
}