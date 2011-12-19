using System.Collections.Generic;
using System.Linq;
using IHI.Server.Libraries.Cecer1.Messenger;

namespace IHI.Server.Networking.Messages
{
    public class MMessengerInit : OutgoingMessage
    {
        public ICollection<Category> Categories
        {
            get;
            set;
        }
        public ICollection<Friend> Friends
        {
            get;
            set;
        }
        public int UnknownA
        {
            get;
            set;
        }
        public int UnknownB
        {
            get;
            set;
        }
        public int UnknownC
        {
            get;
            set;
        }
        public int MaximumFriends
        {
            get;
            set;
        }
        
        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(12)
                    .AppendInt32(UnknownA)
                    .AppendInt32(UnknownB)
                    .AppendInt32(UnknownC)
                    .AppendInt32(Categories.Count - 1); // -1 because the default category doesn't count

                foreach (Category category in Categories.Where(category => category.GetID() != 0))
                {
                    InternalOutgoingMessage
                        .AppendInt32(category.GetID())
                        .AppendString(category.GetName());
                }

                InternalOutgoingMessage
                    .AppendInt32(Friends.Count());

                foreach (Friend friend in Friends)
                {
                    InternalOutgoingMessage
                        .AppendInt32(friend.GetID())
                        .AppendString(friend.GetDisplayName())
                        .AppendBoolean(true) // TODO: Find out what this does.
                        .AppendBoolean(friend.IsLoggedIn())
                        .AppendBoolean(friend.IsStalkable())
                        .AppendString(friend.GetFigure().ToString())
                        .AppendInt32(friend.Category)
                        .AppendString(friend.GetMotto())
                        .AppendString(friend.GetLastAccess().ToString());
                }

                InternalOutgoingMessage
                    .AppendInt32(MaximumFriends)
                    .AppendBoolean(false); // TODO: Find out what this does.
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }
    }
}