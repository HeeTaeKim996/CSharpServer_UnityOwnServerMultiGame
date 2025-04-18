﻿using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using CGameServer;
using FreeNet;

namespace GameServer
{
    public class CGameUser : IPeer
    {
        private CUserToken token;

        public CGameRoom game_room { get; private set; }

        public bool isMasterClient { get; private set; }

        private bool isOnGame = false;

        public CGameUser(CUserToken token)
        {
            this.token = token;
            this.token.Set_peer(this);
        }

        public void on_enter_room(CGameRoom room, bool isMasterClient)
        {
            game_room = room;
            this.isMasterClient = isMasterClient;
        }
        public void on_exit_room(CGameRoom room)
        {
            game_room = null;
            isOnGame = false;
        }

        void IPeer.On_message(Const_buffer buffer)
        {
            CPacket msg = CPacket.Pop_forCopy_for_serverQueue(buffer, this);
            Program.cGameServer.Enqueue_packet(msg);
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
            if(game_room != null)
            {
                if (isMasterClient)
                {
                    game_room.QuitThePlayingGame_on_masterClient_quit(this);
                }
                else
                {
                    game_room.Remove_user(this);
                }
            }
            else
            {
                Program.cGameServer.Exit_lobby(this);
            }
        }

        void IPeer.Disconnect()
        {
            token.socket.Disconnect(false);
        }

        void IPeer.Process_user_operation(CPacket msg)
        {
            if (!isOnGame)
            {
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
                            if (game_room != null)
                            {
                                game_room.Room_task(msg);
                            }
                            else
                            {
                                Console.WriteLine("CGameUser : Game Room 이 존재하지 않습니다");
                            }
                        }
                        break;
                }
            }
            else
            {
                game_room.OnGame_task(msg);
                
            }

            CPacket.Push_back(msg);
        }
        public void Set_isOnGame(bool isOnGame)
        {
            this.isOnGame = isOnGame;
        }
    }
}
