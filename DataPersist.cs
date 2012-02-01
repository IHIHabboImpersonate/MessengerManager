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

using IHI.Server.Libraries.Cecer1.Messenger;

#endregion

namespace IHI.Server.Plugins.Cecer1.MessengerManager
{
    internal class DataPersist
    {
        internal static void RegisterEventHandlers(MessengerObject messenger)
        {
            messenger.OnFriendStateChanged += PersistFriendStateChanged;
            messenger.OnCategoryChanged += messenger_OnCategoryChanged;
        }

        private static void messenger_OnCategoryChanged(object source, MessengerCategoryEventArgs e)
        {
        }

        private static void PersistFriendStateChanged(object source, MessengerFriendEventArgs e)
        {
            switch (e.Type)
            {
                case FriendUpdateType.Added:
                    {
                        break;
                    }
                case FriendUpdateType.Removed:
                    {
                        break;
                    }
            }
        }
    }
}