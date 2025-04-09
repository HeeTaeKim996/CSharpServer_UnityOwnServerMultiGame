using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public abstract class NetObject : MonoBehaviour
{
    public byte owner { get; protected set; }
    public byte pool_code { get; protected set; }
    public byte id { get; protected set; }


    public bool isMasterClients { get; protected set; }
    public bool isMine { get; protected set; }


    public void Set_netObject_info(byte owner, byte pool_code, byte id)
    {
        this.owner = owner;
        this.pool_code = pool_code;
        this.id = id;

        isMine = CNetworkManager.instance.room_id == owner ? true : false; 
    }

    public abstract void After_Set_start();

    public abstract void NetMethod(CPacket msg);
}
