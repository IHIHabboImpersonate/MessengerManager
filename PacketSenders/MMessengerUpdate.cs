using System.Collections.Generic;
using System.Linq;
using IHI.Server.Libraries.Cecer1.Messenger;

namespace IHI.Server.Networking.Messages
{
    public class MMessengerUpdate : OutgoingMessage
    {
        #region Fields

        private readonly ICollection<Category> _categories;
        private readonly ICollection<MessengerFriendEventArgs> _friendUpdates;
        private readonly ICollection<Friend> _friends;

        #endregion

        #region Constructors

        public MMessengerUpdate() : this(
            new List<Friend>(),
            new List<Category>(),
            new List<MessengerFriendEventArgs>())
        {
        }

        public MMessengerUpdate(ICollection<Friend> friends) : this(
            friends,
            new List<Category>(),
            new List<MessengerFriendEventArgs>())
        {
        }

        public MMessengerUpdate(ICollection<Category> categories) : this(
            new List<Friend>(),
            categories,
            new List<MessengerFriendEventArgs>())
        {
        }

        public MMessengerUpdate(ICollection<MessengerFriendEventArgs> friendUpdates) : this(
            new List<Friend>(),
            new List<Category>(),
            friendUpdates)
        {
        }

        public MMessengerUpdate(ICollection<Friend> friends, ICollection<Category> categories) : this(
            friends,
            categories,
            new List<MessengerFriendEventArgs>())
        {
        }

        public MMessengerUpdate(ICollection<Friend> friends, ICollection<MessengerFriendEventArgs> friendUpdates)
            : this(
                friends,
                new List<Category>(),
                friendUpdates)
        {
        }

        public MMessengerUpdate(ICollection<Category> categories, ICollection<MessengerFriendEventArgs> friendUpdates)
            : this(
                new List<Friend>(),
                categories,
                friendUpdates)
        {
        }

        public MMessengerUpdate(ICollection<Friend> friends, ICollection<Category> categories,
                                ICollection<MessengerFriendEventArgs> friendUpdates)
        {
            _friends = friends;
            _categories = categories;
            _friendUpdates = friendUpdates;
        }

        #endregion

        #region Methods

        public MMessengerUpdate Add(Friend friend)
        {
            _friends.Add(friend);
            return this;
        }

        public MMessengerUpdate Add(IEnumerable<Friend> friends)
        {
            foreach (var friend in friends)
                Add(friend);
            return this;
        }

        public MMessengerUpdate Add(Category category)
        {
            _categories.Add(category);
            return this;
        }

        public MMessengerUpdate Add(IEnumerable<Category> categories)
        {
            foreach (var category in categories)
                Add(category);
            return this;
        }

        public MMessengerUpdate Add(MessengerFriendEventArgs friendUpdate)
        {
            _friendUpdates.Add(friendUpdate);

            return this;
        }

        public MMessengerUpdate Add(IEnumerable<MessengerFriendEventArgs> friendUpdates)
        {
            foreach (var friendUpdate in friendUpdates)
                Add(friendUpdate);
            return this;
        }

        public MMessengerUpdate Reset()
        {
            _friends.Clear();
            _categories.Clear();
            _friendUpdates.Clear();
            return this;
        }

        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(13)
                    .AppendInt32(_categories.Count - 1); // -1 because of the default category
                foreach (var category in _categories.Where(category => category.GetID() != 0))
                {
                    InternalOutgoingMessage
                        .AppendInt32(category.GetID())
                        .AppendString(category.GetName());
                }

                InternalOutgoingMessage
                    .AppendInt32(_friends.Count);

                foreach (var friend in _friends)
                {
                    InternalOutgoingMessage
                        .AppendInt32(friend.GetID())
                        .AppendString(friend.GetDisplayName())
                        .AppendBoolean(false) // Find out what the f*** this does.
                        .AppendBoolean(friend.IsLoggedIn())
                        .AppendBoolean((friend.GetRoom() != null) &&
                                       friend.GetInstanceVariable("Messenger.StalkBlock") == null)
                        .AppendString(friend.GetFigure().ToString())
                        .AppendInt32(friend.GetCategory())
                        .AppendString(friend.GetMotto())
                        .AppendString(friend.GetLastAccess().ToString())
                        .AppendString("DevilName")
                        .AppendString("UNKNOWN");
                }

                InternalOutgoingMessage
                    .AppendInt32(_friendUpdates.Count);

                foreach (var friendUpdate in _friendUpdates)
                {
                    var friend = friendUpdate.GetFriend();

                    InternalOutgoingMessage
                        .AppendInt32((int) friendUpdate.GetUpdateType());

                    if (friendUpdate.GetUpdateType() != FriendUpdateType.Removed)
                    {
                        InternalOutgoingMessage
                            .AppendInt32(friend.GetID())
                            .AppendString(friend.GetDisplayName())
                            .AppendBoolean(false) // Find out what the f*** this does.
                            .AppendBoolean(friend.IsLoggedIn())
                            .AppendBoolean((friend.GetRoom() != null) &&
                                           friend.GetInstanceVariable("Messenger.StalkBlock") == null)
                            .AppendString(friend.GetFigure().ToString())
                            .AppendInt32(friendUpdate.GetCategory().GetID())
                            .AppendString(friend.GetMotto())
                            .AppendString(friend.GetLastAccess().ToString())
                            .AppendString("DevilName")
                            .AppendString("UNKNOWN");
                    }
                    else
                    {
                        InternalOutgoingMessage
                            .AppendInt32(friend.GetID());
                    }
                }
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }

        #endregion
    }
}