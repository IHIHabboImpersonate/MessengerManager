using IHI.Server.Habbos;
using IHI.Server.Libraries.Cecer1.Messenger;
using IHI.Server.Networking.Messages;

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
            var owner = messenger.GetOwner();
            var message = owner.GetInstanceVariable("Messenger.WaitingUpdateMessage") as MMessengerUpdate;
            message.
                Add(messenger.GetAllFriends()).
                Add(messenger.GetAllCategories()).
                Send(owner);

            message.Reset();
            return messenger;
        }

        public static MessengerObject GetMessenger(this Habbo habbo)
        {
            return habbo.GetInstanceVariable("Messenger.Instance") as MessengerObject;
        }
    }
}