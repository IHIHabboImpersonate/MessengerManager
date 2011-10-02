using System.Collections.Generic;
using IHI.Server.Habbos;

namespace IHI.Server.Networking.Messages
{
    public class MMessengerSearchResults : OutgoingMessage
    {
        #region Fields

        private readonly ICollection<IBefriendable> _friends;
        private readonly ICollection<IBefriendable> _strangers;

        #endregion

        #region Constructors

        public MMessengerSearchResults() : this(
            new List<IBefriendable>(),
            new List<IBefriendable>())
        {
        }

        public MMessengerSearchResults(ICollection<IBefriendable> friends, ICollection<IBefriendable> strangers)
        {
            _friends = friends;
            _strangers = strangers;
        }

        #endregion

        #region Methods

        public MMessengerSearchResults AddFriend(IBefriendable friend)
        {
            _friends.Add(friend);
            return this;
        }

        public MMessengerSearchResults AddStranger(IBefriendable stranger)
        {
            _strangers.Add(stranger);
            return this;
        }

        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(435)
                    .AppendInt32(_friends.Count);

                foreach (var friend in _friends)
                {
                    InternalOutgoingMessage.
                        AppendInt32(friend.GetID()).
                        AppendString(friend.GetDisplayName()).
                        AppendString(friend.GetMotto()).
                        AppendBoolean(friend.IsLoggedIn()).
                        AppendBoolean((friend.GetRoom() != null) &&
                                      friend.GetInstanceVariable("Messenger.StalkBlock") == null).
                        AppendString("").
                        AppendBoolean(true).
                        AppendString(friend.GetFigure().ToString()).
                        AppendString(friend.GetLastAccess().ToString()).
                        AppendString("");
                }

                InternalOutgoingMessage
                    .AppendInt32(_strangers.Count);

                foreach (var stranger in _strangers)
                {
                    InternalOutgoingMessage.
                        AppendInt32(stranger.GetID()).
                        AppendString(stranger.GetDisplayName()).
                        AppendString(stranger.GetMotto()).
                        AppendBoolean(stranger.IsLoggedIn()).
                        AppendBoolean((stranger.GetRoom() != null) &&
                                      stranger.GetInstanceVariable("Messenger.StalkBlock") == null).
                        AppendString("").
                        AppendBoolean(false).
                        AppendString(stranger.GetFigure().ToString()).
                        AppendString(stranger.GetLastAccess().ToString()).
                        AppendString("");
                }
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }

        #endregion
    }
}