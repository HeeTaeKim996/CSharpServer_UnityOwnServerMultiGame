using System.Numerics;
using CGameServer;
using FreeNet;

namespace GameServer
{
    public class CGameUser : IPeer
    {
        private CUserToken token;

        public CGameRoom game_room { get; private set; }

        public bool isMasterClient { get; private set; }

        public CGameUser(CUserToken token)
        {
            this.token = token;
            this.token.Set_peer(this);
        }

        public void on_enter_room(CGameRoom room, bool isMasterClient)
        {
            game_room = room;
            this.isMasterClient = this.isMasterClient;
        }
        public void on_exit_room(CGameRoom room)
        {
            game_room = null;
        }

        void IPeer.On_message(Const_buffer buffer)
        {
            CPacket msg = CPacket.Pop_forCopy_for_serverQueue(buffer, this);
            Program.cGameServer.Enqueue_packet(msg);
            Console.WriteLine("CGameUser : Message tcp received Check_(1)");
        }

        void IPeer.Send(CPacket msg)
        {
            token.Send(msg);
            // 브로드 캐스트일 수 있기 때문에, 여기에 CPcket.Push_back 처리하면 안됨. CGameRoon또는 CGameServer에서 처리 필요
        }

        void IPeer.On_removed()
        {
            Console.WriteLine("Player Disconnected");

            Program.Remove_user(this);
        }

        void IPeer.Disconnect()
        {
            token.socket.Disconnect(false);
        }

        void IPeer.Process_user_operation(CPacket msg)
        {
            Console.WriteLine("CGameUser : Message tcp received Check_(2)");
            Pr_target target = (Pr_target)msg.Pop_byte();

            switch (target)
            {
                case Pr_target.lobby:
                    {
                        Program.cGameServer.Lobby_task(msg);
                    }
                    break;

                case Pr_target.room:
                    {
                        if(game_room != null)
                        {
                            Console.WriteLine("CGameUser : Message tcp received Check_(3)");
                            game_room.Room_task(msg);
                        }
                        else
                        {
                            Console.WriteLine("CGameUser : Game Room 이 존재하지 않습니다");
                            CPacket.Push_back(msg);
                        }
                    }
                    break;
            }




            #region ○ For Echo_test
            {
                //Protocol protocol = (Protocol)msg.Pop_protocol_id();
                //string text = msg.Pop_string();

                //CPacket echo_packet = CPacket.Pop_forCreate((short)Protocol.server_to_client_string);
                //echo_packet.Push(text);

                //token.Send(echo_packet);
                //CPacket.Push_back(msg);
                //CPacket.Push_back(echo_packet);
            }
            #endregion
        }
    }
}
