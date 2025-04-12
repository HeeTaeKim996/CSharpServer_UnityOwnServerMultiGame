using System;
using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class Item : NetObject
{
    public enum NetEnum__31_60
    {

    }
    public override void NetMethod(CPacket msg)
    {
        base.NetMethod(msg);

    }

    public event Action Event_on_destroy;

    public virtual void Use_masterClient(PlayerHealth playerHealth)
    {

    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Event_on_destroy?.Invoke();
    }
}
