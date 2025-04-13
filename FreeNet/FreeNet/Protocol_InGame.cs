using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet
{
    public enum InGameAction_server : byte
    {
        game_start = 121,

        Instantaite = 122,

        Destroy = 123,

        Object_transfer_copy = 124,

        Instan_transfer_copy = 125,
    }

    public enum InGameAction_client : byte
    {
        Intantaite_object = 121,

        Delete_object = 122,

        Object_transfer = 123,

        ETC = 124,
    }

    public enum Ga_c_Etc_action : byte
    {
        BackToLobby = 121,

    }
    public enum NetObjectCode : byte 
    {
        // @@ NetObjectPoolManager에서 pool을 byte로만 관리한다면, 굳이 FreeNet에 배치하지 않고, 유니티의 클라이언트 내에서 처리해도 됨. 일단 만들어서 그대로 사용함
        None = 122,
        Player = 123,
        Enemy_skeleton = 124,
        Item_health = 125,
        PlayersMissile = 126
    }

    public struct NetVector3
    {
        public float x { get; private set; }
        public float y { get; private set; }
        public float z { get; private set; }

        public NetVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public enum RoomMember : byte
    {
        All = 121,
        Others = 122,
        MasterClient = 123,
        Own =124,
    }

}
