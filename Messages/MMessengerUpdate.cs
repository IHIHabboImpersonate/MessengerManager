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
        public ICollection<Friend> Friends
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
            Friends = new List<Friend>();
        }
        #endregion

        #region Methods
        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(13)
                    .AppendInt32(Categories.Count - 1); // -1 because the default category doesn't count.
                foreach (Category category in Categories.Where(category => category.GetID() != 0))
                {
                    InternalOutgoingMessage
                        .AppendInt32(category.GetID())
                        .AppendString(category.GetName());
                }

                InternalOutgoingMessage
                    .AppendInt32(FriendUpdates.Count + Friends.Count);

                foreach (MessengerFriendEventArgs friendUpdate in FriendUpdates)
                {
                    IBefriendable friend = friendUpdate.Friend;

                    InternalOutgoingMessage
                        .AppendInt32((int)friendUpdate.Type)
                        .AppendInt32(friend.GetID());

                    if (friendUpdate.Type != FriendUpdateType.Removed)
                    {
                        InternalOutgoingMessage
                            .AppendString(friend.GetDisplayName())
                            .AppendBoolean(false) // TODO: Find out what this does.
                            .AppendBoolean(friend.IsLoggedIn())
                            .AppendBoolean(friend.IsStalkable())
                            .AppendString(friend.GetFigure().ToString())
                            .AppendInt32(friendUpdate.Category.GetID())
                            .AppendString(friend.GetMotto())
                            .AppendString(friend.GetLastAccess().ToString());
                    }
                }

                foreach (Friend friend in Friends)
                {
                    InternalOutgoingMessage
                        .AppendBoolean(false) // TODO: Find out what this does.
                        .AppendInt32(friend.GetID())
                        .AppendString(friend.GetDisplayName())
                        .AppendBoolean(false) // TODO: Find out what this does.
                        .AppendBoolean(friend.IsLoggedIn())
                        .AppendBoolean(friend.IsStalkable())
                        .AppendString(friend.GetFigure().ToString())
                        .AppendInt32(friend.Category)
                        .AppendString(friend.GetMotto())
                        .AppendString(friend.GetLastAccess().ToString());
                }
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }
        #endregion
    }
}