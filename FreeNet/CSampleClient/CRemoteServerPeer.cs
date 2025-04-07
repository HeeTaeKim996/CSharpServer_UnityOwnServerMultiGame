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

            Pr_client_action client_action = (Pr_client_action)msg.Pop_byte();

            switch (client_action)
            {
                case Pr_client_action.ts:
                    {
                        Console.WriteLine();
                        Console.WriteLine("---ts_Message_received---");
                        Console.WriteLine(msg.Pop_string());
                        Console.WriteLine("-----------------------");
                        CPacket.Push_back(msg);
                    }
                    break;

                case Pr_client_action.lobby_actin:
                    {
                        Pr_ca_lobby__action lobby_action = (Pr_ca_lobby__action)msg.Pop_byte();
                        switch (lobby_action) 
                        {
                            case Pr_ca_lobby__action.lobby_list_info:
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("---cs_lobby_action__lobby_list_info---");

                                    int room_count = msg.Pop_byte();
                                    for(int i = 0; i < room_count; i++)
                                    {
                                        string room_name = msg.Pop_string();
                                        int room_filled_count = msg.Pop_byte();

                                        Console.WriteLine($"Room Name : {room_name}, Room filled count : {room_filled_count}");
                                    }
                                    Console.WriteLine("--------------------------------------");
                                    CPacket.Push_back(msg);
                                }
                                break;
                        }
                    }
                    break;

                case Pr_client_action.room_action:
                    {
                        Pr_ca_room_action room_action = (Pr_ca_room_action)msg.Pop_byte();
                        switch (room_action)
                        {
                            case Pr_ca_room_action.room_info:
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("---ca_room_action__room_info---");
                                    Console.WriteLine($"Current room's player count : {msg.Pop_byte()}");
                                    Console.WriteLine("-------------------------------");
                                    CPacket.Push_back(msg);
                                }
                                break;
                            case Pr_ca_room_action.game_start:
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("---ca_room_action__game_start---");
                                    Console.WriteLine("--------------------------------");
                                    CPacket.Push_back(msg);

                                    Thread.Sleep(1_000); // 실제 유니티에서는, Awkae-start 작업 종료(Net Object Pool이 Start()에서, 다시 서버로 게임 로드 완료 메세지 밠송. 모든 플레이어가 위 발송을 마치면, 서버에서 게임 오브젝트 생성 주문하면 될듯

                                    CPacket load_completed_message = CPacket.Pop_forCreate();
                                    load_completed_message.Push((byte)Pr_target.room);
                                    load_completed_message.Push((byte)Pr_ta_room_target.room);
                                    load_completed_message.Push((byte)Pr_ta_room_action.game_load_completed);
                                    Send(load_completed_message);
                                }
                                break;
                        }
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
