﻿
using System.Net.Sockets;
using FreeNet;
using GameServer;

namespace CGameServer
{
    public class CGameRoomManager
    {
        private Dictionary<string, CGameRoom> game_rooms = new Dictionary<string, CGameRoom>();
        
        public bool Create_room(string room_name)
        {
            if (game_rooms.ContainsKey(room_name))
            {
                Console.WriteLine($"CGameRoomManager : {room_name}의 키를 갖고 있는 룸은 이미 존재합니다");
                return false;
            }
            else
            {
                game_rooms[room_name] = new CGameRoom(room_name);
                return true;
            }
        }

        public bool is_room_full(string room_name)
        {
            return game_rooms[room_name].user_count >= StaticValues.room_full_count;
        }

        public void Add_player_to_room(string room_name, CGameUser user)
        {
            game_rooms[room_name].Add_user(user);
        }
        
        public bool is_room_exists(string room_name)
        {
            return game_rooms.ContainsKey(room_name);
        }

        public void Remove_room(string room_name)
        {
            game_rooms[room_name].Remove_room();
            game_rooms.Remove(room_name);
            Program.cGameServer.Inform_rooms_info();
        }


        public CPacket Return_rooms_info_packet()
        {
            CPacket return_packet = CPacket.Pop_forCreate();
            return_packet.Push((byte)(Pr_client_action.lobby_actin));
            return_packet.Push((byte)Pr_ca_lobby__action.lobby_list_info);
            return_packet.Push((byte)game_rooms.Count);
            foreach(KeyValuePair<string, CGameRoom> kvp in game_rooms)
            {
                return_packet.Push((string)kvp.Key);
                return_packet.Push((byte)kvp.Value.user_count);
                return_packet.Push((byte)Convert.ToByte(kvp.Value.is_room_sealed));
            }

            return return_packet;
        }
    }
}
