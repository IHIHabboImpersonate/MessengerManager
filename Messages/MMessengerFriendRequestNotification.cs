namespace IHI.Server.Networking.Messages
{
    public class MMessengerFriendRequestNotification : OutgoingMessage
    {
        public int ID
        {
            get;
            set;
        }
        public string Username
        {
            get;
            set;
        }
        
        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(132)
                    .AppendInt32(ID)
                    .AppendString(Username)
                    .AppendString(ID.ToString());
            }
            target.SendMessage(InternalOutgoingMessage);
            return this;
        }
    }
}