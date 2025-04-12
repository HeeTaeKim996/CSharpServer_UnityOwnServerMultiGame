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
        None = 122,
        Player = 123,
        Enemy_skeleton = 124,
        Item_health = 125,
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
