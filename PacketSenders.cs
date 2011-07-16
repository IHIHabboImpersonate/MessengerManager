using IHI.Server.Messenger;
using IHI.Server.SubPackets;
using IHI.Server.Habbos;

using IHI.Server.Networking.Messages;

namespace IHI.Server.Plugins.PacketSenders
{
    public static class Subscription
    {
        /// <summary>
        /// Send the information for the given subscription.
        /// </summary>
        /// <param name="SubscriptionName">The type of subscription.</param>
        /// <param name="CurrentDay">The amount of days into the month.</param>
        /// <param name="ElapsedMonths">The amount of passed months.</param>
        /// <param name="PrepaidMonths">The amount of unused months.</param>
        /// <param name="IsActive">Is the subscription active?</param>
        public static void Send_SubscriptionInfo(this PacketSender PS, string SubscriptionName, byte CurrentDay, byte ElapsedMonths, byte PrepaidMonths, bool IsActive)
        {
            OutgoingMessage Message = new OutgoingMessage(7);	// "@G"
            Message.AppendString(SubscriptionName);
            Message.AppendInt32(CurrentDay);
            Message.AppendInt32(ElapsedMonths);
            Message.AppendInt32(PrepaidMonths);
            Message.AppendBoolean(IsActive);
        }
    }
}
