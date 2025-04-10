using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class EnemySpawner : NetObject
{
    public enum NetEnum
    {

    }
    public override void NetMethod(CPacket msg)
    {
        
    }


    public Enemy_Skeleton enemy_skeleton;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();

        GameManager.instance.event_lateStart += Late_start;
    }

    private void Late_start()
    {
        if (CNetworkManager.instance.isMasterClient)
        {
            CommonMethods.Instantiate_netObject(CNetworkManager.instance.room_id, NetObjectCode.Enemy_skeleton, new Vector3(0, 0, 4f), Vector3.zero);
        }
    }
}
