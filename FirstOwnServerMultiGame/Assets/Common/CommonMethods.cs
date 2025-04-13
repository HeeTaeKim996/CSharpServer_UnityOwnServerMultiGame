using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeNet;
using static PlayerMovement;
using System;

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
    public static void Destroy_netObject_MasterClient(byte pool_code, byte id)
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Destroy);
        send_msg.Push((byte)pool_code);
        send_msg.Push((byte)id);

        CNetworkManager.instance.Send(send_msg);
    }


    public static void Sync_animation_Mine(byte pool_code, byte id, byte byteNetEnum, byte roomMember, byte byte_animationEnum, float blendTime, float offset_normalizedTime)
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
        send_msg.Push((byte)pool_code);
        send_msg.Push((byte)id);
        send_msg.Push((byte)byteNetEnum);
        send_msg.Push((byte)roomMember);

        send_msg.Push((short)9);
        send_msg.Push((byte)byte_animationEnum);
        send_msg.Push((float)blendTime);
        send_msg.Push((float)offset_normalizedTime);
        CNetworkManager.instance.Send(send_msg);
    }

    public static void Sync_FixedUpdateSyncs(byte pool_code, byte id, byte byteNetEnum, float pos_x, float pos_y, float pos_z, float rot_x, float rot_y, float rot_z)
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
        send_msg.Push((byte)pool_code);
        send_msg.Push((byte)id);
        send_msg.Push((byte)byteNetEnum);
        send_msg.Push((byte)RoomMember.Others);

        send_msg.Push((short)24);

        send_msg.Push((float)pos_x);
        send_msg.Push((float)pos_y);
        send_msg.Push((float)pos_z);

        send_msg.Push((float)rot_x);
        send_msg.Push((float)rot_y);
        send_msg.Push((float)rot_z);
        CNetworkManager.instance.Send(send_msg);
    }

    public static CPacket Instan_fetcher_helper(byte ownerCode, NetObjectCode netObjectCode, Vector3 position, Vector3 eulerAngles, byte fetchers_pool_code, byte fetchers_id, byte fetchers_byteNetEnum, RoomMember roomMember)
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Instan_transfer_copy);
        send_msg.Push((byte)ownerCode);
        send_msg.Push((byte)netObjectCode);
        send_msg.Push((float)position.x);
        send_msg.Push((float)position.y);
        send_msg.Push((float)position.z);
        send_msg.Push((float)eulerAngles.x);
        send_msg.Push((float)eulerAngles.y);
        send_msg.Push((float)eulerAngles.z);
        send_msg.Push((byte)fetchers_pool_code);
        send_msg.Push((byte)fetchers_id);
        send_msg.Push((byte)fetchers_byteNetEnum);
        send_msg.Push((byte)roomMember);

        return send_msg;
    }
}
