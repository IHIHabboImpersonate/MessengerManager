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
using IHI.Database;
using IHI.Server.Libraries.Cecer1.Messenger;
using NHibernate;
using NHibernate.Criterion;
using Habbo = IHI.Server.Habbos.Habbo;
using IHIDB = IHI.Database;

#endregion

namespace IHI.Server.Plugins.Cecer1.MessengerManager
{
    [CompatibilityLock(36)]
    public partial class Manager : Plugin
    {
        public static event MessengerEventHandler OnMessengerReady;

        public override void Start()
        {
            CoreManager.ServerCore.GetHabboDistributor().OnHabboLogin += RegisterHandlers;
            OnMessengerReady += RegisterEvents;
        }

        private static MessengerObject CreateMessenger(Habbo habbo)
        {
            MessengerObject messenger = new MessengerObject(habbo);


            IList<MessengerCategory> categoriesOutput;
            IList<MessengerFriendship> friendsOutput;
            IList<MessengerFriendRequest> friendRequestsOutput;

            using (ISession db = CoreManager.ServerCore.GetDatabaseSession())
            {
                categoriesOutput = db.CreateCriteria<MessengerCategory>()
                    .Add(
                        Restrictions.Eq("habbo_id", habbo.GetID()))
                    .List<MessengerCategory>();

                friendsOutput = db.CreateCriteria<MessengerFriendship>()
                    .Add(
                        new OrExpression(
                            Restrictions.Eq("habbo_a.id", habbo.GetID()),
                            Restrictions.Eq("habbo_b.id", habbo.GetID())))
                    .List<MessengerFriendship>();

                friendRequestsOutput = db.CreateCriteria<MessengerFriendRequest>()
                    .Add(
                        Restrictions.Eq("habbo_to_id",
                                        habbo.GetID()))
                    .List<MessengerFriendRequest>();
            }

            messenger.AddCategory(new Category(messenger, 0)
                                      {
                                          Name = ""
                                      });

            foreach (MessengerCategory category in categoriesOutput)
            {
                messenger.AddCategory(new Category(messenger, category.category_id)
                                          {
                                              Name = category.name
                                          });
            }

            foreach (MessengerFriendship friendship in friendsOutput)
            {
                Habbo friendHabbo;
                int categoryID = 0;

                if (friendship.habbo_a.habbo_id == habbo.GetID())
                {
                    friendHabbo = CoreManager.ServerCore.GetHabboDistributor().GetHabbo(friendship.habbo_b.habbo_id);
                    if (friendship.category_b_id.HasValue)
                        categoryID = friendship.category_b_id.Value;
                }
                else
                {
                    friendHabbo = CoreManager.ServerCore.GetHabboDistributor().GetHabbo(friendship.habbo_a.habbo_id);
                    if (friendship.category_a_id.HasValue)
                        categoryID = friendship.category_a_id.Value;
                }

                Category category = messenger.GetCategory(categoryID);
                category.AddFriend(friendHabbo);
            }

            foreach (MessengerFriendRequest request in friendRequestsOutput)
            {
                messenger.ReceiveFriendRequest(
                    CoreManager.ServerCore.GetHabboDistributor().GetHabbo(request.habbo_from_id));
            }

            messenger.OnFriendStateChanged += Messenger_OnMessengerFriendStateChanged;
            return messenger;
        }

        #region Messenger Updates

        private static void Messenger_OnMessengerFriendStateChanged(object source, MessengerFriendEventArgs e)
        {
            (source as MessengerObject).GetWaitingUpdateMessage().FriendUpdates.Add(e);
        }

        #endregion
    }
}