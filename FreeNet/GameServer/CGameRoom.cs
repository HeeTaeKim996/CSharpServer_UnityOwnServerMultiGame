﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CGameServer;
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
        private byte room_id_index = 0;
        private bool isOnGame = false;

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
            if (is_room_sealed) return;

            bool isMasterClient = user_count == 0 ? true : false;
            game_users.Add(added_user);
            added_user.on_enter_room(this, isMasterClient);

            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)Pr_client_action.room_action);
            msg.Push((byte)Pr_ca_room_action.room_start);
            room_id_index++; // roon_id_index == owner_code 의 0은 룸 오브젝트 로 처리하는 게 좋을듯
            msg.Push((byte)room_id_index);
            ((IPeer)added_user).Send(msg);
            CPacket.Push_back(msg);


            Inform_updated_room_info();
        }
        public void Remove_user(CGameUser remove_user)
        {
            remove_user.on_exit_room(this);
            game_users.Remove(remove_user);

            if (!isOnGame)
            {
                Inform_updated_room_info();
            }
        }
        private void Inform_updated_room_info()
        {
            Console.WriteLine("CGameRoom : Inform_update_room_info__Invoked Check");
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
                            Console.WriteLine("CGameRoom : 모든 유저가 준비됨 확인");
                            isOnGame = true;
                            objectPoolManager = new NetObjectPoolManager(this);

                            CPacket send_msg = CPacket.Pop_forCreate();
                            send_msg.Push((byte)Pr_client_action.game_late_start);
                            Cast_all(send_msg);

                            Seal_the_room();
                        }
                    }
                    break;
                case Pr_ta_room_action.exit_room:
                    {
                        CGameUser sender = (CGameUser)msg.owner;
                        if(!sender.isMasterClient)
                        {
                            Remove_user(sender);
                            Program.cGameServer.Enter_lobby(sender);
                        }
                        else
                        {
                            CPacket send_msg = CPacket.Pop_forCreate();
                            send_msg.Push((byte)Pr_client_action.room_action);
                            send_msg.Push((byte)Pr_ca_room_action.back_to_lobby);
                            Cast_others(send_msg, msg.owner);

                            Program.cGameServer.Remove_room(room_name);
                        }
                    }
                    break;
                    
                default:
                    {
                        throw new ArgumentNullException($"CGameRoom : lobby_action에서 디펄트 수신 {lobby_action.ToString()}");
                    }
            }


            switch (room_target)
            {
                case Pr_ta_room_target.all:
                    {
                        Cast_all(send_packet);
                    }
                    break;
                case Pr_ta_room_target.room:
                    {
                        CPacket.Push_back(send_packet);
                    }
                    break;
                default:
                    {
                        Console.WriteLine(Convert.ToInt16(room_target));
                        throw new ArgumentNullException($"CGameRoom : room_target에서 디펄트 수신 {room_target.ToString()}");
                    }
            }
        }
        
        public void OnGame_task(CPacket msg)
        {
            InGameAction_server inGameAction = (InGameAction_server)msg.Pop_byte();
            switch (inGameAction)
            {
                case InGameAction_server.Instantaite:
                    {
                        byte ownerCode = msg.Pop_byte();
                        byte objectCode = msg.Pop_byte();
                        NetVector3 position = new NetVector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
                        NetVector3 rotation = new NetVector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());

                        (byte pool_code, byte id) = objectPoolManager.Add_object();

                        On_object_instantiated(ownerCode, objectCode, pool_code, id, position, rotation);
                    }
                    break;
                case InGameAction_server.Destroy:
                    {
                        byte pool_code = msg.Pop_byte();
                        byte id = msg.Pop_byte();

                        if(objectPoolManager.Remove_object(pool_code, id))
                        {
                            CPacket send_msg = CPacket.Pop_forCreate();
                            send_msg.Push((byte)InGameAction_client.Delete_object);
                            send_msg.Push((byte)pool_code);
                            send_msg.Push((byte)id);
                            Cast_all(send_msg);
                        }
                        else
                        {
                            Console.WriteLine("@@@@@@ 주의!!!  CGameRoom : 클라이언트로부터 poolManager에 없는 오브젝트를 삭제하라는 요청을 받음. Scene에 이미 있던 오브젝트를 삭제요청한 것으로 예상됨");
                        }
                    }
                    break;
                case InGameAction_server.Object_transfer_copy:
                    {
                        byte pool_code = msg.Pop_byte();
                        byte id = msg.Pop_byte();
                        byte netEnum = msg.Pop_byte();
                        RoomMember roomMember = (RoomMember)msg.Pop_byte();
                        short coppyLength = msg.Pop_int16();

                        CPacket send_msg = CPacket.Pop_forCreate();
                        send_msg.Push((byte)InGameAction_client.Object_transfer);
                        send_msg.Push((byte)pool_code);
                        send_msg.Push((byte)id);
                        send_msg.Push((byte)netEnum);
                        send_msg.Copy_buffer_with_startPoint(msg.buffer, msg.position, coppyLength);

                        switch (roomMember) 
                        {
                            case RoomMember.All:
                                {
                                    Cast_all(send_msg);
                                }
                                break;
                            case RoomMember.Others:
                                {
                                    Cast_others(send_msg, msg.owner);
                                }
                                break;
                            case RoomMember.Own:
                                {
                                    Cast_Own(send_msg, msg.owner);
                                }
                                break;
                            case RoomMember.MasterClient:
                                {
                                    Cast_MasterClient(send_msg);
                                }
                                break;
                            default:
                                {
                                    throw new ArgumentNullException($"CGameRoom : roomMember에서 디펄트 수신 {roomMember.ToString()}");
                                }
                        }
                    }
                    break;

                case InGameAction_server.Instan_transfer_copy:
                    {
                        byte ownerCode = msg.Pop_byte();
                        byte netObjectCode = msg.Pop_byte();
                        NetVector3 position = new NetVector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
                        NetVector3 rotation = new NetVector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());

                        (byte instan_pool_code, byte instan_id) = objectPoolManager.Add_object();

                        On_object_instantiated(ownerCode, netObjectCode, instan_pool_code, instan_id, position, rotation);


                        byte fetchers_pool_code = msg.Pop_byte();
                        byte fetchers_id = msg.Pop_byte();
                        byte fetchers_byteNetEnum = msg.Pop_byte();
                        RoomMember roomMember = (RoomMember)msg.Pop_byte();

                        short copy_length = msg.Pop_int16();

                        // Transfer To Target about instantiated object info
                        {
                            CPacket send_msg2 = CPacket.Pop_forCreate();
                            send_msg2.Push((byte)InGameAction_client.Object_transfer);
                            send_msg2.Push((byte)fetchers_pool_code);
                            send_msg2.Push((byte)fetchers_id);
                            send_msg2.Push((byte)fetchers_byteNetEnum);
                            send_msg2.Push((byte)instan_pool_code);
                            send_msg2.Push((byte)instan_id);

                            send_msg2.Copy_buffer_with_startPoint(msg.buffer, msg.position, copy_length);

                            switch (roomMember)
                            {
                                case RoomMember.MasterClient:
                                    {
                                        Cast_MasterClient(send_msg2);
                                    }
                                    break;
                                case RoomMember.Own:
                                    {
                                        Cast_Own(send_msg2, msg.owner);
                                    }
                                    break;
                                default:
                                    {
                                        throw new ArgumentNullException($"CGameRoom : 디펄트 수신 감지. RoomMemeber : {roomMember}");
                                    }
                            }

                            Console.WriteLine("CGameRoom : @@@@@@@@@@@@@@@@@@@@@@IntantFetcher Check@@@@@@@@@@@@@@@@@@@@@@@@@");
                        }

                    }
                    break;
                default:
                    {
                        throw new ArgumentNullException($"CGameRoom : InGameAction에서 디펄트 수신 {inGameAction.ToString()}");
                    }
            }

        }

        public void On_object_instantiated(byte ownerCode, byte objectCode, byte pool_code, byte id, NetVector3 position, NetVector3 rotation)
        {
            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)InGameAction_client.Intantaite_object);
            msg.Push((byte)ownerCode);
            msg.Push((byte)objectCode);
            msg.Push((byte)pool_code);
            msg.Push((byte)id);
            msg.Push((float)position.x);
            msg.Push((float)position.y);
            msg.Push((float)position.z);
            msg.Push((float)rotation.x);
            msg.Push((float)rotation.y);
            msg.Push((float)rotation.z);
            Cast_all(msg);
        }
        private void On_delete_object(byte pool_code, byte id)
        {
            CPacket msg = CPacket.Pop_forCreate();
            msg.Push((byte)InGameAction_client.Delete_object);
            msg.Push((byte)pool_code);
            msg.Push((byte)id);
            Cast_all(msg);
        }

        public void Remove_room()
        {
            for(int i = game_users.Count - 1; i >= 0; i--)
            {
                CGameUser user = (CGameUser)game_users[i];
                Remove_user(user);
                Program.cGameServer.Enter_lobby(user);
            }
        }

        private void Seal_the_room()
        {
            is_room_sealed = true;
            Program.cGameServer.Inform_rooms_info();
        }

        public void QuitThePlayingGame_on_masterClient_quit(IPeer masterClientsPeer)
        {
            Remove_user((CGameUser)masterClientsPeer);

            CPacket send_msg = CPacket.Pop_forCreate();
            send_msg.Push((byte)InGameAction_client.ETC);
            send_msg.Push((byte)Ga_c_Etc_action.BackToLobby);
            Cast_others(send_msg, masterClientsPeer);

            Program.cGameServer.Remove_room(room_name);
        }

        private void Cast_all(CPacket send_msg)
        {
            foreach(IPeer game_user in game_users)
            {
                game_user.Send(send_msg);
            }
            CPacket.Push_back(send_msg);
        }

        private void Cast_others(CPacket send_msg, IPeer excepted)
        {
            foreach(IPeer game_user in game_users)
            {
                if (excepted == game_user) continue;

                game_user.Send(send_msg);
            }
            CPacket.Push_back(send_msg);
        }
        private void Cast_Own(CPacket send_msg, IPeer owner)
        {

            foreach (IPeer game_user in game_users)
            {
                if (owner == game_user)
                {
                    game_user.Send(send_msg);
                    break;
                }
            }
            CPacket.Push_back(send_msg);
        }
        private void Cast_MasterClient(CPacket send_msg)
        {
            foreach(IPeer game_user in game_users)
            {
                if (((CGameUser)game_user).isMasterClient)
                {
                    game_user.Send(send_msg);
                    break;
                }
            }
            CPacket.Push_back(send_msg);
        } 
    }
}
