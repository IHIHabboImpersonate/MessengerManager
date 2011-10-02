using System.Collections.Generic;
using System.Linq;
using IHI.Server.Libraries.Cecer1.Messenger;

namespace IHI.Server.Networking.Messages
{
    public class MMessengerInit : OutgoingMessage
    {
        private readonly ICollection<Category> _categories;
        private readonly ICollection<Friend> _friends;
        private readonly int _unknownA;
        private readonly int _unknownB;
        private readonly int _unknownC;
        private readonly int _unknownD;


        public MMessengerInit(int unknownA, int unknownB, int unknownC, int unknownD)
        {
            _unknownA = unknownA;
            _unknownB = unknownB;
            _unknownC = unknownC;
            _unknownD = unknownD;

            _categories = new List<Category>();
            _friends = new List<Friend>();
        }

        public MMessengerInit(int unknownA, int unknownB, int unknownC, int unknownD,
                              ICollection<Category> categories, ICollection<Friend> friends)
        {
            _unknownA = unknownA;
            _unknownB = unknownB;
            _unknownC = unknownC;
            _unknownD = unknownD;

            _categories = categories;
            _friends = friends;
        }


        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(12)
                    .AppendInt32(_unknownA)
                    .AppendInt32(_unknownB)
                    .AppendInt32(_unknownC)
                    .AppendInt32(_unknownD)
                    .AppendInt32(_categories.Count - 1); // -1 because of the default category
                foreach (var category in _categories.Where(category => category.GetID() != 0))
                {
                    InternalOutgoingMessage
                        .AppendInt32(category.GetID())
                        .AppendString(category.GetName());
                }

                InternalOutgoingMessage
                    .AppendInt32(_friends.Count());

                foreach (var friend in _friends)
                {
                    InternalOutgoingMessage
                        .AppendInt32(friend.GetID())
                        .AppendString(friend.GetDisplayName())
                        .AppendBoolean(true) // Find out what the f*** this does.
                        .AppendBoolean(friend.IsLoggedIn())
                        .AppendBoolean((friend.GetRoom() != null) && friend.IsStalkingAllowed())
                        .AppendString(friend.GetFigure().ToString())
                        .AppendInt32(friend.GetCategory())
                        .AppendString(friend.GetMotto())
                        .AppendString(friend.GetLastAccess().ToString())
                        .AppendString("DevilName")
                        .AppendString("UNKNOWN1");
                }
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }
    }
}