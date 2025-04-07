using System;
using System.Collections.Generic;
using System.Net;
using FreeNet;

namespace CSampleClient
{
    class Program
    {
        private static List<CRemoteServerPeer> game_servers = new List<CRemoteServerPeer>();
        private static object cs_game_Servers = new object();

        static void Main(string[] args)
        {
            CPacketBufferManager.Initialize(20);

            CNetworkService cNetworkService = new CNetworkService();

            CConnector cConnector = new CConnector(cNetworkService);
            cConnector.callback_on_connected += On_connected_server;
            IPEndPoint remote_endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7979);
            cConnector.Connect(remote_endPoint);

            while (true)
            {
                CPacket msg = CPacket.Pop_forCreate();

                Console.Write("Target >");
                string target = Console.ReadLine();

                if(Enum.TryParse<Pr_target>(target, out var parsedTarget))
                {
                    msg.Push((byte)parsedTarget);

                    switch (parsedTarget)
                    {
                        case Pr_target.lobby:
                            {

                                Console.Write("LobbyAction > ");
                                string lobbyAction = Console.ReadLine();
                                if(Enum.TryParse<Pr_ta_lobby_action>(lobbyAction, out var parsed_lobbyAction))
                                {
                                    msg.Push((byte)parsed_lobbyAction);

                                    switch (parsed_lobbyAction)
                                    {
                                        case Pr_ta_lobby_action.create_room:
                                            {
                                                string room_name = Console.ReadLine();
                                                msg.Push(room_name);
                                                game_servers[0].Send(msg);
                                            }
                                            break;
                                        case Pr_ta_lobby_action.enter_room:
                                            {
                                                string room_name = Console.ReadLine();
                                                msg.Push(room_name);
                                                game_servers[0].Send(msg);
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("잘못된 입력 입니다. 다시 입력해주세요");
                                    CPacket.Push_back(msg);
                                    continue;
                                }
                            }
                            break;
                        case Pr_target.room:
                            {
                                Console.Write("Room Target > ");
                                string room_target = Console.ReadLine();
                                if (Enum.TryParse<Pr_ta_room_target>(room_target, out var parsed_room_target))
                                {
                                    msg.Push((byte)parsed_room_target);
                                }
                                else
                                {
                                    Console.WriteLine("잘못된 입력 입니다. 다시 입력해주세요");
                                    CPacket.Push_back(msg);
                                    continue;
                                }



                                Console.Write("Room Action > ");
                                string room_action = Console.ReadLine();
                                if(Enum.TryParse<Pr_ta_room_action>(room_action, out var parsed_room_action))
                                {
                                    msg.Push((byte)parsed_room_action);

                                    switch (parsed_room_action)
                                    {
                                        case Pr_ta_room_action.ts:
                                            {
                                                Console.WriteLine("Test string > ");
                                                string test_string = Console.ReadLine();
                                                msg.Push(test_string);
                                                game_servers[0].Send(msg);
                                            }
                                            break;
                                        case Pr_ta_room_action.game_start_masterClient:
                                            {
                                                game_servers[0].Send(msg);
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("잘못된 입력 입니다. 다시 입력해주세요");
                                    CPacket.Push_back(msg);
                                    continue;
                                }
                            }
                            break;

                    }
                }
                else
                {
                    Console.WriteLine("잘못된 입력 입니다. 다시 입력해주세요");
                    CPacket.Push_back(msg);
                    continue;
                }


                //switch (target)
                //{
                //    case :
                //        {
                //            Console.WriteLine();
                //            Console.Write("Room name : ");
                //            string room_name = Console.ReadLine();

                //            CPacket msg = CPacket.Pop_forCreate((short)Protocol.client_to_server_string);
                //            msg.Push("Cretae_room");
                //            msg.Push(room_name);
                //            game_servers[0].Send(msg); // 우선 Client에서는 cRemoteSErverPeer 에서 CPacket.Push_back 을 처리 시도함. 여기서 Push_back 처리할 필요 없음
                //        }
                //        break;
                //    case "Enter_room":
                //        {
                //            Console.WriteLine();
                //            Console.Write("Room name : ");
                //            string room_name = Console.ReadLine();

                //            CPacket msg = CPacket.Pop_forCreate((short)Protocol.client_to_server_string);
                //            msg.Push("Enter_room");
                //            msg.Push(room_name);
                //            game_servers[0].Send(msg);
                //        }
                //        break;

                //    default:
                //        {
                //            CPacket msg = CPacket.Pop_forCreate((short)Protocol.client_to_room_all);

                //            msg.Push(send_text);

                //            game_servers[0].Send(msg);
                //        }
                //        break;
                //}


                #region ○ For Echo_test
                {
                    //CPacket msg = CPacket.Pop_forCreate((short)Protocol.client_to_server_string);
                    //msg.Push(send_text);
                    //game_servers[0].Send(msg);
                }
                #endregion
            }
        }


        private static void On_connected_server(CUserToken token)
        {
            CRemoteServerPeer cRemoteServerPeer = new CRemoteServerPeer(token);
            lock (cs_game_Servers)
            {
                game_servers.Add(cRemoteServerPeer);
            }
            Console.WriteLine("서버와 연결을 성공했습니다");
        }
    }

}