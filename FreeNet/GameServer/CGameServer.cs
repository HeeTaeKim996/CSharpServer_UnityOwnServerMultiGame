using CGameServer;
using FreeNet;
using Microsoft.VisualBasic;

namespace GameServer
{
    internal class CGameServer
    {
        private Queue<CPacket> user_operations = new Queue<CPacket>();
        private object cs_user_operations = new object();

        private Thread logic_thread;
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        public CGameRoomManager roomManager = new CGameRoomManager();


        private List<IPeer> lobby_users = new List<IPeer>();
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
            Pr_ta_lobby_action lobby_action = (Pr_ta_lobby_action)msg.Pop_byte();
            Console.WriteLine($"CGameServer : {lobby_action}");
            switch (lobby_action)
            {
                case Pr_ta_lobby_action.create_room:
                    {
                        Console.WriteLine($"CGameServer : Check");
                        Create_room_requested((CGameUser)msg.owner, msg.Pop_string());
                    }
                    break;
                case Pr_ta_lobby_action.enter_room:
                    {
                        Enter_room_requested((CGameUser)msg.owner, msg.Pop_string());
                    }
                    break;
                default:
                    {
                        Console.WriteLine();
                        throw new ArgumentNullException($"CGameServer : default 감지 {lobby_action.ToString()}");
                    }
            }
        }
        

        public void Enter_lobby(CGameUser game_user)
        {
            lock (cs_lobby_users)
            {
                lobby_users.Add(game_user);
            }
            Inform_rooms_info();
        }
        public void Exit_lobby(CGameUser game_user)
        {
            lock (cs_lobby_users)
            {
                lobby_users.Remove(game_user);
            }
        }

        public void Inform_rooms_info()
        {
            CPacket msg = roomManager.Return_rooms_info_packet();
            Cast_lobby_users(msg);
        }
        private void Cast_lobby_users(CPacket msg)
        {
            lock (cs_lobby_users)
            {
                foreach(IPeer lobby_user in lobby_users)
                {
                    lobby_user.Send(msg);
                }
            }
            CPacket.Push_back(msg);
        }


        private void Create_room_requested(CGameUser game_user, string room_name)
        {
            if (game_user.game_room != null) return;

            if (roomManager.Create_room(room_name))
            {
                roomManager.Add_player_to_room(room_name, game_user);
                Exit_lobby(game_user);

                Console.WriteLine("CGameServer : Check");
            }
            else
            {
                CPacket packet = CPacket.Pop_forCreate();

                packet.Push((byte)Pr_client_action.ts);

                packet.Push("You are alredy in room");
                ((IPeer)game_user).Send(packet);
                CPacket.Push_back(packet);
            }

            Inform_rooms_info();
        }
        public void Remove_room(string room_name)
        {
            roomManager.Remove_room(room_name);
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
                    Exit_lobby(game_user);

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

            Inform_rooms_info();
        }
    }
}
