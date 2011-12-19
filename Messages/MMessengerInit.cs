using System.Collections.Generic;
using System.Linq;
using IHI.Server.Libraries.Cecer1.Messenger;
using IHI.Server.Habbos;

namespace IHI.Server.Networking.Messages
{
    public class MMessengerInit : OutgoingMessage
    {
        public IEnumerable<Category> Categories
        {
            get;
            set;
        }
        public IEnumerable<Friend> Friends
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
                    .AppendInt32(Categories.Count() - 1); // -1 because the default category doesn't count

                foreach (Category category in Categories.Where(category => category.ID != 0))
                {
                    InternalOutgoingMessage
                        .AppendInt32(category.ID)
                        .AppendString(category.Name);
                }

                InternalOutgoingMessage
                    .AppendInt32(Friends.Count());

                foreach (Friend friend in Friends)
                {
                    foreach (Category category in friend.GetCategories())
                    {
                        InternalOutgoingMessage
                            .AppendInt32(friend.Befriendable.GetID())
                            .AppendString(friend.Befriendable.GetDisplayName())
                            .AppendBoolean(true) // TODO: Find out what this does.
                            .AppendBoolean(friend.Befriendable.IsLoggedIn())
                            .AppendBoolean(friend.Befriendable.IsStalkable())
                            .AppendString(friend.Befriendable.GetFigure().ToString())
                            .AppendInt32(category.ID)
                            .AppendString(friend.Befriendable.GetMotto())
                            .AppendString(friend.Befriendable.GetLastAccess().ToString());
                    }
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