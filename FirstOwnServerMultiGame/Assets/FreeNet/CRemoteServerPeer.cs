using System;
using FreeNet;

namespace FreeNetUnity
{
    public class CRemoteServerPeer : IPeer
    {
        public CUserToken token { get; private set; }
        private WeakReference weakRef_netEventQueuer;

        public CRemoteServerPeer(CUserToken token)
        {
            this.token = token;
            token.Set_peer(this);
        }
        public void Set_netEventQueuer(NetEventQueuer netEventQueuer)
        {
            weakRef_netEventQueuer = new WeakReference(netEventQueuer);
        }

        void IPeer.On_message(Const_buffer buffer)
        {
            CPacket recv_msg = CPacket.Pop_forCopy_for_clientRead(buffer);
            (weakRef_netEventQueuer.Target as NetEventQueuer).Enqueue_network_message(recv_msg);
        }
        void IPeer.Send(CPacket msg)
        {
            token.Send(msg);
            CPacket.Push_back(msg);  // 받는 대상이 대응되는 서버 소켓 하나 뿐이니, 여기서 Push_back 처리해도 될듯
        }
        void IPeer.On_removed()
        {
            (this.weakRef_netEventQueuer.Target as NetEventQueuer).Enqueue_network_event(NetworkEvent.disconnected);
        }
        void IPeer.Disconnect()
        {

        }
        void IPeer.Process_user_operation(CPacket msg)
        {

        }
    }
}

