using System;
using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public abstract class LivingEntity : NetObject
{
    public enum NetEnum__31_60 : byte
    {
        OnDamage_others = 31,

        Sync_health_others = 32,

        InvokeDamage_Master_fromOthers = 33,

        RestoreHealth_Others = 34,
    }

    public override void NetMethod(CPacket msg)
    {
        base.NetMethod(msg);
        NetEnum__31_60 netEnum = (NetEnum__31_60)byteNetEnum;
        switch (netEnum) 
        {
            case NetEnum__31_60.Sync_health_others:
                {
                    Sync_health_others(msg);
                }
                break;
            case NetEnum__31_60.OnDamage_others:
                {
                    OnDamage_Others(msg);
                }
                break;
            case NetEnum__31_60.InvokeDamage_Master_fromOthers:
                {
                    InvokeDamage_Master_fromOthers(msg);
                }
                break;
            case NetEnum__31_60.RestoreHealth_Others:
                {
                    RestoreHealth_Others(msg);
                }
                break;
        }

    }

    public bool dead { get; protected set; }
    public float health { get; protected set; }
    public float maxHealth { get; protected set; }

    public event Action Event_invoke_detach_from_entity;
    public event Action Event_on_attached_entitys_GetDamaged;
    public event Action Event_on_RestoreHealth;

    public void OnDamage_MasterClient(float damage, LivingEntity fromEntity)
    {

        health -= damage;

        // Sync_health_others
        {
            CPacket send_msg = CPacket.Pop_forCreate();
            send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
            send_msg.Push((byte)pool_code);
            send_msg.Push((byte)id);
            send_msg.Push((byte)NetEnum__31_60.Sync_health_others);
            send_msg.Push((byte)RoomMember.Others);

            send_msg.Push((short)4);

            send_msg.Push((float)health);

            CNetworkManager.instance.Send(send_msg);
        }

        // OnDamage_Others
        {
            CPacket send_msg2 = CPacket.Pop_forCreate();
            send_msg2.Push((byte)InGameAction_server.Object_transfer_copy);
            send_msg2.Push((byte)pool_code);
            send_msg2.Push((byte)id);
            send_msg2.Push((byte)NetEnum__31_60.OnDamage_others);
            send_msg2.Push((byte)RoomMember.Others);

            send_msg2.Push((short)2);

            send_msg2.Push((byte)fromEntity.pool_code);
            send_msg2.Push((byte)fromEntity.id);

            CNetworkManager.instance.Send(send_msg2);
        }


        OnDamage(fromEntity);

    }
    public void Sync_health_others(CPacket msg)
    {
        this.health = msg.Pop_float();
        //Debug.Log($"LivingEntity : Sync_health_others : {health}");
    }
    public void OnDamage_Others(CPacket msg)
    {
        OnDamage((LivingEntity)NetObjectManager.instance.Get_netObject(msg.Pop_byte(), msg.Pop_byte()));
    }

    protected virtual void OnDamage(LivingEntity fromEntity)
    {
        //Debug.Log($"LivingEntity OnDamage Check: {health} // {fromEntity}");
        if (health <= 0)
        {
            Die();
        }
        Event_on_attached_entitys_GetDamaged?.Invoke();
    }
    protected virtual void Die()
    {
        dead = true;
        Event_invoke_detach_from_entity?.Invoke();
    }




    public void InvokeDamageToMaster_Others(LivingEntity fromEntity, LivingEntity attackedEntity, float damage)
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
        send_msg.Push((byte)fromEntity.pool_code);
        send_msg.Push((byte)fromEntity.id);
        send_msg.Push((byte)NetEnum__31_60.InvokeDamage_Master_fromOthers);
        send_msg.Push((byte)RoomMember.MasterClient);

        send_msg.Push((short)6);

        send_msg.Push((byte)attackedEntity.pool_code);
        send_msg.Push((byte)attackedEntity.id);
        send_msg.Push((float)damage);
        CNetworkManager.instance.Send(send_msg);
    }

    public void InvokeDamage_Master_fromOthers(CPacket msg)
    {
        Debug.Log("LivingEntity : InvokeDamage_Master_fromOthers Check");
        LivingEntity attackedEntity = (LivingEntity)NetObjectManager.instance.Get_netObject(msg.Pop_byte(), msg.Pop_byte());
        InvokeDamage_Master(attackedEntity, msg.Pop_float());
    }

    public void InvokeDamage_Master(LivingEntity attackedEntity, float damage)
    {
        //Debug.Log($"LivingEntity : InvokeDamage_Master Check");
        attackedEntity.OnDamage_MasterClient(damage, this);
    }

    public void RestoreHealth_MasterCient(float restoringHealth)
    {
        health = Mathf.Min(maxHealth, health += restoringHealth);
        // Sync_health_others
        {
            CPacket send_msg = CPacket.Pop_forCreate();
            send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
            send_msg.Push((byte)pool_code);
            send_msg.Push((byte)id);
            send_msg.Push((byte)NetEnum__31_60.Sync_health_others);
            send_msg.Push((byte)RoomMember.Others);

            send_msg.Push((short)4);

            send_msg.Push((float)health);

            CNetworkManager.instance.Send(send_msg);
        }
        // RestoreHEalth_Others
        {
            CPacket send_msg = CPacket.Pop_forCreate();
            send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
            send_msg.Push((byte)pool_code);
            send_msg.Push((byte)id);
            send_msg.Push((byte)NetEnum__31_60.RestoreHealth_Others);
            send_msg.Push((byte)RoomMember.Others);

            send_msg.Push((short)0);
        }

        RestoreHealth();
    }
    public void RestoreHealth_Others(CPacket msg)
    {
        RestoreHealth();
    }
    public virtual void RestoreHealth()
    {
        Event_on_RestoreHealth?.Invoke();
    }
}
