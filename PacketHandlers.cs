using System;
using System.Linq;
using IHI.Server.Networking.Messages;
using IHI.Server.Plugins.PacketSenders;
using IHI.Server.Habbos;
using IHI.Server.Networking;

using IHI.Server.Plugin.Cecer1.Subscriptions;

namespace IHI.Server.Plugins.Cecer1.Subscriptions
{
    class PacketHandlers
    {
        internal static void RegisterHandlers(object source, ConnectionEventArgs args)
        {
            PacketHandler PRequestSubscriptionData = new PacketHandler(Process_RequestSubscriptionData);
            //PacketHandler P = new PacketHandler(Process_);
            //PacketHandler P = new PacketHandler(Process_);
            //PacketHandler P = new PacketHandler(Process_);
            //PacketHandler P = new PacketHandler(Process_);
            //PacketHandler P = new PacketHandler(Process_);
            //PacketHandler P = new PacketHandler(Process_);

            (source as IonTcpConnection).
                AddHandler(26, PacketHandlerPriority.DefaultAction, PRequestSubscriptionData);
                //AddHandler(, PacketHandlerPriority.DefaultAction, ).
                //AddHandler(, PacketHandlerPriority.DefaultAction, ).
                //AddHandler(, PacketHandlerPriority.DefaultAction, ).
                //AddHandler(, PacketHandlerPriority.DefaultAction, ).
                //AddHandler(, PacketHandlerPriority.DefaultAction, )
                
        }

        private static void Process_RequestSubscriptionData(Habbo Sender, IncomingMessage Message)
        {
            string SubscriptionName = Message.PopPrefixedString();

            SubscriptionData Data = new SubscriptionData(Sender, SubscriptionName);

            // 86400    = 24 hours in seconds.
            // 2678400  = 31 days in seconds.
            byte RemainingFullMonths = (byte)(Data.GetRemainingSeconds() / 2678400);
            byte ExpiredFullMonths = (byte)(Data.GetExpiredSeconds() / 2678400);
            byte ExpiredMonthDays = (byte)((Data.GetExpiredSeconds() % 2678400) / 86400);

            Sender.GetPacketSender().Send_SubscriptionInfo(SubscriptionName, ExpiredMonthDays, ExpiredFullMonths, RemainingFullMonths, Data.IsActive());
        }
    }
}
