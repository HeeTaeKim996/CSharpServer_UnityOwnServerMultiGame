using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FreeNet;
using UnityEngine;

public class PlayerHealth : LivingEntity
{
    public enum NetEnum : byte 
    {
        Update_position_rotation
    }
    
    public override void NetMethod(CPacket msg)
    {
        NetEnum netEnum = (NetEnum)msg.Pop_byte();
        switch (netEnum)
        {
            case NetEnum.Update_position_rotation:
                {
                    playerMovement.Update_position_rotation_others(msg);
                }
                break;
        }
    }


    private PlayerMovement playerMovement;


    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public override void After_Set_start()
    {
        if (CNetworkManager.instance.room_id == this.owner)
        {
            CinemachineVirtualCamera virtualCamera = FindObjectOfType<CameraController>().virtualCamera;
            virtualCamera.LookAt = transform;
            virtualCamera.Follow = transform;

            FindObjectOfType<PlayerControllerManager>().Set_PlayerMovement(playerMovement);
        }
    }
}
