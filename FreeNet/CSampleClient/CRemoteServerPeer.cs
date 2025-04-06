using System;
using FreeNet;

namespace CSampleClient
{
    internal class CRemoteServerPeer : IPeer
    {
        public CUserToken token { get; private set; }
        public CRemoteServerPeer(CUserToken token)
        {
            this.token = token;
            this.token.Set_peer(this);
        }

        void IPeer.On_message(Const_buffer buffer)
        {
            CPacket msg = CPacket.Pop_forCopy_for_clientRead(buffer);

            Pr_client_action actionType = (Pr_client_action)msg.Pop_byte();

            switch (actionType)
            {
                case Pr_client_action.ts:
                    {
                        Console.WriteLine();
                        Console.WriteLine("---Message_received---");
                        Console.WriteLine(msg.Pop_string());
                        Console.WriteLine("-----------------------");
                        CPacket.Push_back(msg);
                    }
                    break;
            }



            #region For Echo Test
            {
                //Protocol protocol = (Protocol)msg.Pop_protocol_id();
                //Console.WriteLine(msg.Pop_string());

                //CPacket.Push_back(msg);
            }
            #endregion
        }

        public void Send(CPacket msg)
        {
            token.Send(msg);
            CPacket.Push_back(msg); // Server에서는 브로드캐스트 때문에 이 위치에 CPacket.Push_back 이 있지 않지만, 클라이언트의 경우 이 위치에 있어도 될듯
        }

        void IPeer.On_removed()
        {
            Console.WriteLine("서버와 연결이 해제됐습니다");
        }
        void IPeer.Disconnect()
        {
            token.socket.Disconnect(false);
        }

        void IPeer.Process_user_operation(CPacket msg) { }


    }
}
