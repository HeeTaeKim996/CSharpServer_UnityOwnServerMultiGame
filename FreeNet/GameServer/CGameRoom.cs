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
        private int load_completed_users;

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

            Inform_updated_room_info();
        }
        public void remove_user(CGameUser remove_user)
        {
            remove_user.on_exit_room(this);
            game_users.Remove(remove_user);

            Inform_updated_room_info();
        }
        private void Inform_updated_room_info()
        {
            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)Pr_client_action.room_action);
            msg.Push((byte)Pr_ca_room_action.room_info);
            msg.Push((byte)user_count);
            Cast_all(msg);
        }




        public void Room_task(CPacket msg)
        {
            CPacket send_packet = CPacket.Pop_forCreate();

            Pr_ta_room_target room_target = (Pr_ta_room_target)msg.Pop_byte();


            Pr_ta_room_action lobby_action = (Pr_ta_room_action)msg.Pop_byte();


            switch (lobby_action)
            {
                case Pr_ta_room_action.ts:
                    {
                        send_packet.Push((byte)Pr_client_action.ts);

                        string recv_string = msg.Pop_string();

                        send_packet.Push(recv_string);
                    }
                    break;
                case Pr_ta_room_action.game_start_masterClient:
                    {
                        if (((CGameUser)msg.owner).isMasterClient)
                        {
                            load_completed_users = 0;
                            send_packet.Push((byte)Pr_client_action.room_action);
                            send_packet.Push((byte)Pr_ca_room_action.game_start);
                        }
                        else
                        {
                            Console.WriteLine("CGameRoom : 마스터 클라이언트가 아닌 유저로부터, 게임시작 명령 오류 발생");
                        }
                    }
                    break;
                case Pr_ta_room_action.game_load_completed:
                    {
                        load_completed_users++;
                        Console.WriteLine($"CGameRoom 디버그 용도. load_completed_users : {load_completed_users}, room_user_count : {user_count}");
                        if (load_completed_users == user_count)
                        {
                            Console.WriteLine("CGameRoom 임시 용도. 모든 유저가 준비됨 확인");
                        }
                    }
                    break;
            }


            switch (room_target)
            {
                case Pr_ta_room_target.all:
                    {
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
            CPacket.Push_back(send_msg);
        }
        
       

    }
}
