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
using System.Linq;
using IHI.Server.Libraries.Cecer1.Messenger;

#endregion

namespace IHI.Server.Networking.Messages
{
    public class MMessengerInit : OutgoingMessage
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Friend> Friends { get; set; }
        public int UnknownA { get; set; }
        public int UnknownB { get; set; }
        public int UnknownC { get; set; }
        public int MaximumFriends { get; set; }

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