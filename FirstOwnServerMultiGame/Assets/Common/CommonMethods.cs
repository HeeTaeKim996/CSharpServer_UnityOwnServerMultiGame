using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeNet;

public static class CommonMethods
{
    public static void Instantiate_netObject(byte ownerCode, NetObjectCode objectCode ,Vector3 position, Vector3 rotation)
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Instantaite);
        send_msg.Push((byte)ownerCode);
        send_msg.Push((byte)objectCode);
        send_msg.Push(position.x);
        send_msg.Push(position.y);
        send_msg.Push(position.z);
        send_msg.Push(rotation.x);
        send_msg.Push(rotation.y);
        send_msg.Push(rotation.z);

        CNetworkManager.instance.Send(send_msg);
    }
}
