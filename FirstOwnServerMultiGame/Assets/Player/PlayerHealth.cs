using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FreeNet;
using UnityEngine;

public class PlayerHealth : LivingEntity
{
    public enum NetEnum__61_90 : byte 
    {
        Update_fixed_sync = 61,

        Animation_Sync = 62,
    }
    
    public override void NetMethod(CPacket msg)
    {
        base.NetMethod(msg);
        NetEnum__61_90 netEnum = (NetEnum__61_90)byteNetEnum;
        switch (netEnum)
        {
            case NetEnum__61_90.Update_fixed_sync:
                {
                    playerMovement.Update_fixed_sync(msg);
                }
                break;
            case NetEnum__61_90.Animation_Sync:
                {
                    playerMovement.Sync_Animation_Others(msg);
                }
                break;
        }
    }


    private PlayerMovement playerMovement;
    private PlayerController playerController;

    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<PlayerMovement>();
        maxHealth = 100f;
        health = maxHealth;
    }
    protected override void Start()
    {
        base.Start();
    }

    public override void After_Set_start()
    {
        if (CNetworkManager.instance.room_id == this.owner)
        {
            CinemachineVirtualCamera virtualCamera = FindObjectOfType<CameraController>().virtualCamera;
            virtualCamera.LookAt = transform;
            virtualCamera.Follow = transform;

            PlayerControllerManager playerControllerManager = FindObjectOfType<PlayerControllerManager>();

            playerController = playerControllerManager.Set_PlayerMovement(playerMovement);
            playerControllerManager.playerHealthSliderAdmin.Get_player(this);

        }
    }

    protected override void Die()
    {
        base.Die();

        playerMovement.Invoke_DieAction();
    }
    public void On_die_action_finished()
    {
        playerController.gameObject.SetActive(false);
        CommonMethods.Destroy_netObject_MasterClient(pool_code, id);
    }
}
