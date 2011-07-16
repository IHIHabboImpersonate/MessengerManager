using IHI.Server.Networking;

namespace IHI.Server.Plugins.Cecer1.Subscriptions
{
    public partial class SubscriptionCore : Plugin
    {
        public override void Start()
        {
            CoreManager.GetCore().GetConnectionManager().OnConnectionOpen += new ConnectionEventHandler(PacketHandlers.RegisterHandlers);
        }

        public override void Stop()
        {

        }
    }
}
