using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class Item_health : Item
{
    public enum NetEnum61_90
    {

    }
    public override void NetMethod(CPacket msg)
    {
        base.NetMethod(msg);
        NetEnum61_90 netEnum = (NetEnum61_90)byteNetEnum;
        switch (netEnum)
        {

        }
    }

    public override void Use_masterClient(PlayerHealth playerHealth)
    {
        base.Use_masterClient(playerHealth);
        playerHealth.RestoreHealth_MasterCient(30f);

        CommonMethods.Destroy_netObject_MasterClient(pool_code, id);
    }
}
