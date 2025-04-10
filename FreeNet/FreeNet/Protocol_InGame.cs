using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet
{
    public enum InGameAction_server : byte
    {
        game_start,

        Instantaite,

        Destroy,

        Object_transfer_copy,
    }

    public enum InGameAction_client : byte
    {
        Intantaite_object = 0,

        Delete_object = 1,

        Object_transfer = 2
    }

    public enum NetObjectCode : byte 
    {
        None,
        Player,
        Enemy_skeleton,
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
        All,
        Others,
        MasterClient,
        Own
    }

}
