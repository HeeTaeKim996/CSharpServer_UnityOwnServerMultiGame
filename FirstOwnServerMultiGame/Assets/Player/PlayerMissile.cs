using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class PlayerMissile : NetObject
{
    public enum NetEnum__31_60
    {
        Sync_FixedUpdateSyncs = 31,
    }
    public override void NetMethod(CPacket msg)
    {
        base.NetMethod(msg);
        NetEnum__31_60 netEnum = (NetEnum__31_60)byteNetEnum;
        switch (netEnum)
        {
            case NetEnum__31_60.Sync_FixedUpdateSyncs:
                {
                    Sync_FixedUpdateSyncs_Others(msg);
                }
                break;
        }
    }

    private PlayerHealth playerHealth;
    private Rigidbody missileRigidbody;
    private LayerMask enemyLayer;
    private Vector3 direction;
    private float damage;
    private float elapsedTime = 0f;
    private float destroyTime;
    

    protected override void Awake()
    {
        base.Awake();
        missileRigidbody = GetComponent<Rigidbody>();
        enemyLayer = LayerMask.GetMask("Enemy");
    }

    public void GetFire_MasterClient(PlayerHealth playerHealth, Vector3 direction, float damage, float destroyTime)
    {
        this.playerHealth = playerHealth;
        this.direction = direction;
        this.damage = damage;
        this.destroyTime = destroyTime;
    }

    private void FixedUpdate()
    {
        missileRigidbody.MovePosition(missileRigidbody.position + direction * 5f * Time.fixedDeltaTime);

        if (CNetworkManager.instance.isMasterClient)
        {
            // Sync_fixedUpdateSyncs
            {
                Vector3 position = transform.position;
                Vector3 eulerAngles = transform.eulerAngles;
                CommonMethods.Sync_FixedUpdateSyncs(pool_code, id, (byte)NetEnum__31_60.Sync_FixedUpdateSyncs, position.x, position.y, position.z, eulerAngles.x, eulerAngles.y, eulerAngles.z);
            }
        }
    }
    private void Update()
    {
        if (CNetworkManager.instance.isMasterClient)
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime >= destroyTime)
            {
                CommonMethods.Destroy_netObject_MasterClient(pool_code, id);
                Debug.Log("Check2");
            }
        }
    }
    private void Sync_FixedUpdateSyncs_Others(CPacket msg)
    {
        transform.position = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
        transform.rotation = Quaternion.Euler(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CNetworkManager.instance.isMasterClient)
        {
            if( ( ( 1 << other.gameObject.layer) & enemyLayer) != 0)
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (!enemy.dead)
                {
                    enemy.OnDamage_MasterClient(damage, playerHealth);
                }
                CommonMethods.Destroy_netObject_MasterClient(pool_code, id);

                Debug.Log("Check");
            }
        }
    }
}
