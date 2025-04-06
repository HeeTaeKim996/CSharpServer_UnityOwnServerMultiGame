using CGameServer;
using FreeNet;

namespace GameServer
{
    internal class CGameServer
    {
        private Queue<CPacket> user_operations = new Queue<CPacket>();
        private object cs_user_operations = new object();

        private Thread logic_thread;
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        public CGameRoomManager roomManager = new CGameRoomManager();


        private List<CGameUser> lobby_users = new List<CGameUser>();
        private object cs_lobby_users = new object();



        public CGameServer()
        {
            logic_thread = new Thread(Logic_loop);
            logic_thread.Start();
        }

        
        private void Logic_loop()
        {
            while (true)
            {
                CPacket packet = null;

                lock (cs_user_operations)
                {
                    if(user_operations.Count > 0)
                    {
                        packet = user_operations.Dequeue();
                    }
                }

                if(packet != null)
                {
                    packet.owner.Process_user_operation(packet);
                }
                if(user_operations.Count <= 0)
                {
                    autoResetEvent.WaitOne();
                }
            }
        }

        public void Enqueue_packet(CPacket packet)
        {
            lock (cs_user_operations)
            {
                user_operations.Enqueue(packet);
            }
            autoResetEvent.Set();
        }



        public void Lobby_task(CPacket msg)
        {
            Pr_lobbyAction lobby_action = (Pr_lobbyAction)msg.Pop_byte();
            switch (lobby_action)
            {
                case Pr_lobbyAction.create_room:
                    {
                        Create_room_requested((CGameUser)msg.owner, msg.Pop_string());
                    }
                    break;
                case Pr_lobbyAction.enter_room:
                    {
                        Enter_room_requested((CGameUser)msg.owner, msg.Pop_string());
                    }
                    break;
            }
            CPacket.Push_back(msg);
        }


        public void Enter_lobby(CGameUser game_user)
        {
            lock (cs_lobby_users)
            {
                lobby_users.Add(game_user);
            }

            
        }


        private void Create_room_requested(CGameUser game_user, string room_name)
        {
            if (game_user.game_room != null) return;

            if (roomManager.Create_room(room_name))
            {
                CPacket packet = CPacket.Pop_forCreate();

                packet.Push((byte)Pr_client_action.ts);

                packet.Push("Creating_Room_Succeeded");
                ((IPeer)game_user).Send(packet);
                CPacket.Push_back(packet);

                roomManager.Add_player_to_room(room_name, game_user);
            }
            else
            {
                CPacket packet = CPacket.Pop_forCreate();

                packet.Push((byte)Pr_client_action.ts);

                packet.Push("You are alredy in room");
                ((IPeer)game_user).Send(packet);
                CPacket.Push_back(packet);
            }

        }

        private void Enter_room_requested(CGameUser game_user, string room_name)
        {
            if (game_user.game_room != null)
            {
                CPacket packet = CPacket.Pop_forCreate();
                packet.Push((byte)Pr_client_action.ts);
                packet.Push("You are already in room");
                ((IPeer)game_user).Send(packet);
                CPacket.Push_back(packet);

                return;
            }

            if (roomManager.is_room_exists(room_name))
            {
                if (!roomManager.is_room_full(room_name))
                {
                    roomManager.Add_player_to_room(room_name, game_user);

                    CPacket packet = CPacket.Pop_forCreate();
                    packet.Push((byte)Pr_client_action.ts);
                    packet.Push("room in succeded");
                    ((IPeer)game_user).Send(packet);
                    CPacket.Push_back(packet);
                }
                else
                {
                    CPacket packet = CPacket.Pop_forCreate();

                    packet.Push((byte)Pr_client_action.ts);

                    packet.Push("Room is full");
                    ((IPeer)game_user).Send(packet);
                    CPacket.Push_back(packet);
                }
            }
            else
            {
                CPacket packet = CPacket.Pop_forCreate();

                packet.Push((byte)Pr_client_action.ts);

                packet.Push("Room is not exist");
                ((IPeer)game_user).Send(packet);
                CPacket.Push_back(packet);
            }
        }
    }
}
