using System.Collections.Generic;
using System.Linq;
using IHI.Server.Habbos;
using IHI.Server.Libraries.Cecer1.Messenger;

namespace IHI.Server.Networking.Messages
{
    public class MMessengerUpdate : OutgoingMessage
    {
        #region Properties
        public ICollection<Category> Categories
        {
            get;
            private set;
        }
        public ICollection<MessengerFriendEventArgs> FriendUpdates
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public MMessengerUpdate()
        {
            Categories = new List<Category>();
            FriendUpdates = new List<MessengerFriendEventArgs>();
        }

        #endregion

        #region Methods
        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(13)
                    .AppendInt32(Categories.Count - 1); // -1 because the default category doesn't count.
                foreach (Category category in Categories.Where(category => category.ID != 0))
                {
                    InternalOutgoingMessage
                        .AppendInt32(category.ID)
                        .AppendString(category.Name);
                }

                InternalOutgoingMessage
                    .AppendInt32(FriendUpdates.Count);

                foreach (MessengerFriendEventArgs friendUpdate in FriendUpdates)
                {
                    Friend friend = friendUpdate.Friend;

                    InternalOutgoingMessage
                        .AppendInt32((int)friendUpdate.Type)
                        .AppendInt32(friend.Befriendable.GetID());

                    if (friendUpdate.Type != FriendUpdateType.Removed)
                    {
                        InternalOutgoingMessage
                            .AppendString(friend.Befriendable.GetDisplayName())
                            .AppendBoolean(false) // TODO: Find out what this does.
                            .AppendBoolean(friend.Befriendable.IsLoggedIn())
                            .AppendBoolean(friend.Befriendable.IsStalkable())
                            .AppendString(friend.Befriendable.GetFigure().ToString())
                            .AppendInt32(friendUpdate.Category.ID)
                            .AppendString(friend.Befriendable.GetMotto())
                            .AppendString(friend.Befriendable.GetLastAccess().ToString());
                    }
                }
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }
        #endregion
    }
}