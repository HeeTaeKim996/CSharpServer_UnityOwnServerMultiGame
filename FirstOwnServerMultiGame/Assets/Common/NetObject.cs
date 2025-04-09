using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class NetObject : MonoBehaviour
{
    public byte owner { get; protected set; }
    public byte pool_code { get; protected set; }
    public byte id { get; protected set; }


    public void Set_netObject_info(byte owner, byte pool_code, byte id)
    {
        this.owner = owner;
        this.pool_code = pool_code;
        this.id = id;
    }
}
