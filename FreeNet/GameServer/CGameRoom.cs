﻿using System;
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
        private List<IPeer> game_users = new List<IPeer>();     // cGameServer에서 비동기로 온 패킷들을, 단일로직으로 정렬해서 처리하기 때문에, cs 관리 필요 없음
        public bool is_room_sealed { get; private set; } = false;
        private int load_completed_users;
        private string room_name;
        private NetObjectPoolManager objectPoolManager;

        public CGameRoom(string room_name)
        {
            this.room_name = room_name;
        }

        public int user_count
        {
            get
            {
                return game_users.Count;
            }
        }
        public void Add_user(CGameUser added_user)
        {
            Console.WriteLine("CGameRoom 디버그 체크_(1)");

            bool isMasterClient = user_count == 0 ? true : false;
            game_users.Add(added_user);
            added_user.on_enter_room(this, isMasterClient);

            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)Pr_client_action.room_action);
            msg.Push((byte)Pr_ca_room_action.room_start);
            ((IPeer)added_user).Send(msg);
            CPacket.Push_back(msg);

            Inform_updated_room_info();

            Console.WriteLine($"CGAmeRoom 디버그. {user_count}");
        }
        public void remove_user(CGameUser remove_user)
        {
            remove_user.on_exit_room(this);
            game_users.Remove(remove_user);

            Inform_updated_room_info();
        }
        private void Inform_updated_room_info()
        {
            foreach(CGameUser game_user in game_users)
            {
                CPacket msg = CPacket.Pop_forCreate();
                msg.Push((byte)Pr_client_action.room_action);
                msg.Push((byte)Pr_ca_room_action.room_info);
                msg.Push(room_name);
                msg.Push((byte)user_count);
                msg.Push((byte)(game_user.isMasterClient ? 0 : 1 ));
                ((IPeer)game_user).Send(msg);
                CPacket.Push_back(msg);
            }
        }




        public void Room_task(CPacket msg)
        {
            CPacket send_packet = CPacket.Pop_forCreate();

            Pr_ta_room_target room_target = (Pr_ta_room_target)msg.Pop_byte();


            Pr_ta_room_action lobby_action = (Pr_ta_room_action)msg.Pop_byte();

            Console.WriteLine("CGameRoom Debug CHeck _(1)");

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
                        Console.WriteLine("CGameRoom Debug CHeck _(2)");
                        if (((CGameUser)msg.owner).isMasterClient)
                        {
                            Console.WriteLine("CGameRoom Debug CHeck _(3)");
                            load_completed_users = 0;
                            send_packet.Push((byte)Pr_client_action.room_action);
                            send_packet.Push((byte)Pr_ca_room_action.game_start);
                            Console.WriteLine("CGameRoom Debug CHeck _(4)");
                        }
                        else
                        {
                            Console.WriteLine("CGameRoom : 마스터 클라이언트가 아닌 유저로부터, 게임시작 명령 오류 발생");
                        }
                    }
                    break;
                case Pr_ta_room_action.game_load_completed:
                    {
                        ((CGameUser)msg.owner).Set_isOnGame(true);

                        load_completed_users++;
                        Console.WriteLine($"CGameRoom 디버그 용도. load_completed_users : {load_completed_users}, room_user_count : {user_count}");
                        if (load_completed_users == user_count)
                        {
                            Console.WriteLine("CGameRoom 임시 용도. 모든 유저가 준비됨 확인");
                            objectPoolManager = new NetObjectPoolManager(this);

                            CPacket send_msg = CPacket.Pop_forCreate();
                            send_msg.Push((byte)Pr_client_action.game_late_start);
                            Cast_all(send_msg);
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
        }
        
        public void OnGame_task(CPacket msg)
        {
            InGameAction_server inGameAction = (InGameAction_server)msg.Pop_byte();
            switch (inGameAction)
            {
                case InGameAction_server.Instantaite:
                    {
                        NetObjectCode objectCode = (NetObjectCode)msg.Pop_byte();
                        NetVector3 position = new NetVector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
                        NetVector3 rotation = new NetVector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());

                        short id = objectPoolManager.Add_object(objectCode);


                        CPacket send_msg = CPacket.Pop_forCreate();
                        On_object_instantiated(objectCode, id, position, rotation);
                    }
                    break;
            }

        }

        public void On_new_objectPool_instantaited(NetObjectCode netObjectCode)
        {
            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)InGameAction_client.Instantiate_object_pool);
            msg.Push((byte)netObjectCode);
            Cast_all(msg);
        }
        public void On_object_instantiated(NetObjectCode objectCode, short id, NetVector3 position, NetVector3 rotation)
        {
            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)InGameAction_client.Intantaite_object);
            msg.Push((byte)objectCode);
            msg.Push((short)id);
            msg.Push((float)position.x);
            msg.Push((float)position.y);
            msg.Push((float)position.z);
            msg.Push((float)rotation.x);
            msg.Push((float)rotation.y);
            msg.Push((float)rotation.z);
            Cast_all(msg);
        }
        private void Delete_object(NetObjectCode objectCode, short id)
        {
            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)InGameAction_client.Delete_object);
            msg.Push((byte)objectCode);
            msg.Push((short)id);
            Cast_all(msg);
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
