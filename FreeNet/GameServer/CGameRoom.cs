using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeNet;
using GameServer;

namespace GameServer
{
    public class CGameRoom
    {
        private List<CGameUser> game_users = new List<CGameUser>();     // cGameServer에서 비동기로 온 패킷들을, 단일로직으로 정렬해서 처리하기 때문에, cs 관리 필요 없음
        public bool is_room_sealed { get; private set; } = false;

        public int user_count
        {
            get
            {
                return game_users.Count;
            }
        }
        public void Add_user(CGameUser added_user)
        {
            bool isMasterClient = user_count == 0 ? true : false;
            game_users.Add(added_user);
            added_user.on_enter_room(this, isMasterClient);

            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)Pr_client_action.ts);
            msg.Push($"Player Entered To Room. current players count : {user_count}");
            Cast_all(msg);

        }
        public void remove_user(CGameUser remove_user)
        {
            remove_user.on_exit_room(this);
            game_users.Remove(remove_user);
        }

        public void Room_task(CPacket msg)
        {
            CPacket send_packet = CPacket.Pop_forCreate();

            Pr_room_target room_target = (Pr_room_target)msg.Pop_byte();


            Pr_room_action lobby_action = (Pr_room_action)msg.Pop_byte();

            Console.WriteLine(lobby_action.ToString());

            switch (lobby_action)
            {
                case Pr_room_action.ts:
                    {
                        Console.WriteLine("CGameRoom Eror Check _(4)");
                        send_packet.Push((byte)Pr_client_action.ts);

                        string recv_string = msg.Pop_string();

                        send_packet.Push(recv_string);
                    }
                    break;
            }


            switch (room_target)
            {
                case Pr_room_target.all:
                    {
                        Console.WriteLine("CGameRoom Eror Check _(5)");
                        Cast_all(send_packet);
                    }
                    break;
            }

            CPacket.Push_back(msg);
        }


        private void Cast_all(CPacket send_msg)
        {
            foreach(CGameUser game_user in game_users)
            {

                ((IPeer)game_user).Send(send_msg);
            }
            Console.WriteLine("CGameRoom Eror Check _(7)");
            CPacket.Push_back(send_msg);
            Console.WriteLine("CGameRoom Eror Check _(8)");
        }
        
       

    }
}
