#region GPLv3

// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#endregion

#region Usings

using System.Collections.Generic;
using IHI.Server.Habbos;

#endregion

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

                foreach (IBefriendable friend in _friends)
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
                        AppendString(friend.GetLastAccess().ToString());
                }

                InternalOutgoingMessage
                    .AppendInt32(_strangers.Count);

                foreach (IBefriendable stranger in _strangers)
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
                        AppendString(stranger.GetLastAccess().ToString());
                }
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }

        #endregion
    }
}