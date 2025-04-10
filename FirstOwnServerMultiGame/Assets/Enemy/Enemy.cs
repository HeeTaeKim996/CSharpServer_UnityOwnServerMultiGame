using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public abstract class Enemy : LivingEntity
{
    public enum NetEnum__61_90 : byte
    {

    }
    public override void NetMethod(CPacket msg)
    {
        base.NetMethod(msg);

    }

    protected LayerMask playerLayer;
    protected PlayerHealth attackTarget;
    protected float distanceToTarget;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        dead = false;
        playerLayer = LayerMask.GetMask("Player");
    }


    protected virtual void Update()
    {
        // Calculate Calculations
        {
            if (attackTarget != null && !attackTarget.dead)
            {
                UpdateCalcutations();
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        attackTarget.onDead -= On_PlayerDead;
    }

    protected virtual void UpdateCalcutations()
    {
        distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);
    }

    protected virtual void On_discernPlayer()
    {
        attackTarget.onDead += On_PlayerDead;
    }

    protected virtual void On_PlayerDead()
    {

    }

    
}
