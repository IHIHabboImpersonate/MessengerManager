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
using IHI.Server.Libraries.Cecer1.Messenger;
using IHI.Server.Networking.Messages;
using NHibernate;
using NHibernate.Criterion;

#endregion

namespace IHI.Server.Plugins.Cecer1.MessengerManager
{
    public partial class Manager
    {
        private static void RegisterHandlers(object source, HabboEventArgs args)
        {
            Habbo target = source as Habbo;
            if (target == null)
                return;
            target.
                GetConnection().
                AddHandler(12, PacketHandlerPriority.DefaultAction, ProcessMessengerInit).
                AddHandler(39, PacketHandlerPriority.DefaultAction, ProcessSendFriendRequest).
                AddHandler(40, PacketHandlerPriority.DefaultAction, ProcessRemoveFriends).
                AddHandler(41, PacketHandlerPriority.DefaultAction, ProcessMessengerSearch);
        }

        private static void RegisterEvents(object source, MessengerEventArgs e)
        {
            (source as MessengerObject).OnFriendRequestReceived += FriendRequestNotify;
        }

        private static void ProcessSendFriendRequest(Habbo sender, IncomingMessage message)
        {
            string username = message.PopPrefixedString();

            sender.GetMessenger()
                .SendFriendRequest(
                    CoreManager.
                        ServerCore.
                        GetHabboDistributor().
                        GetHabbo(username));
        }

        private static void ProcessMessengerInit(Habbo sender, IncomingMessage message)
        {
            MessengerObject messenger = CreateMessenger(
                sender);

            sender.SetInstanceVariable("Messenger.Instance", messenger);
            sender.SetInstanceVariable("Messenger.WaitingUpdateMessage", new MMessengerUpdate());

            new MMessengerInit
                {
                    Categories = messenger.GetAllCategories(),
                    Friends = messenger.GetAllFriends(),
                    UnknownA = 10,
                    UnknownB = 20,
                    UnknownC = 30,
                }.Send(sender);


            if (OnMessengerReady != null)
                OnMessengerReady.Invoke(messenger, new MessengerEventArgs(sender));
        }

        private static void ProcessRemoveFriends(Habbo sender, IncomingMessage message)
        {
            // How many friends have been deleted?
            int amount = message.PopWiredInt32();
            MessengerObject messenger = sender.GetMessenger();

            // Handle each one.
            for (int i = 0; i < amount; i++)
            {
                // Get the ID of the friend about to be removed.
                int friendID = message.PopWiredInt32();

                // Remove the friend from all categories.
                messenger.RemoveFriend(friendID);
            }
            messenger.SendWaitingUpdateMessage();
        }

        private static void ProcessMessengerSearch(Habbo sender, IncomingMessage message)
        {
            string searchString = message.PopPrefixedString();

            List<Database.Habbo> matching;
            // Using IHIDB.Habbo rather than IHIDB.Friend because this will be passed to the HabboDistributor
            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                matching = db.CreateCriteria<Database.Habbo>().
                               Add(new LikeExpression("username", searchString + "%")).
                               SetMaxResults(20). // TODO: External config
                               List<Database.Habbo>() as List<Database.Habbo>;
            }

            List<IBefriendable> friends = new List<IBefriendable>();
            List<IBefriendable> strangers = new List<IBefriendable>();

            MessengerObject messenger = sender.GetMessenger();
            HabboDistributor habboDistributor = CoreManager.ServerCore.GetHabboDistributor();

            foreach (Database.Habbo match in matching)
            {
                IBefriendable habbo = habboDistributor.GetHabbo(match);
                if (messenger.IsFriend(habbo))
                    friends.Add(habbo);
                else
                    strangers.Add(habbo);
            }

            new MMessengerSearchResults(friends, strangers).Send(sender);
        }

        private static void FriendRequestNotify(object source, MessengerFriendRequestEventArgs e)
        {
            new MMessengerFriendRequestNotification
                {
                    ID = e.GetFrom().GetID(),
                    Username = e.GetFrom().GetDisplayName()
                }.Send((source as MessengerObject).Owner);
        }
    }
}