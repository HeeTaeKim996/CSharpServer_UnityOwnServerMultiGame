using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Skeleton : FieldEnemy
{
    public enum NetEnum__121_150 : byte
    {
        UpdateFixedSync_Others = 121,
        Sync_Animation_Others = 122,
    }
    public override void NetMethod(CPacket msg)
    {
        base.NetMethod(msg);
        NetEnum__121_150 netEnum = (NetEnum__121_150)byteNetEnum;
        Debug.Log(netEnum.ToString());
        switch (netEnum)
        {
            case NetEnum__121_150.UpdateFixedSync_Others:
                {
                    UpdateFixedSync_Others(msg);
                }
                break;
            case NetEnum__121_150.Sync_Animation_Others:
                {
                    Debug.Log("Check");
                    Sync_Animation_Others(msg);
                }
                break;
        }
    }

    private Animator enemyAnimator;
    private NavMeshAgent navMeshAgent;

    private string currentAnimation;
    private Coroutine currentAction;
    private IEnumerator nextAction;
    private float movementSpeed = 4f;

    private float damage = 1f;

    public enum AnimationEnum : byte
    {
        Idle,
        Walk,
        Attack,
        DieMotion
    }



    protected override void Awake()
    {
        base.Awake();
        enemyAnimator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    protected override void Start()
    {
        base.Start();
        dead = false;
        maxHealth = 100f;
        health = maxHealth;

        if (CNetworkManager.instance.isMasterClient)
        {
            currentAction = StartCoroutine(On_idle_layder());
        }
    }
    private void FixedUpdate()
    {
        // UpdateFixedSync
        {
            if (CNetworkManager.instance.isMasterClient)
            {
                Vector3 position = transform.position;
                Vector3 eulerAngles = transform.eulerAngles;

                CommonMethods.Sync_FixedUpdateSyncs(pool_code, id, (byte)NetEnum__121_150.UpdateFixedSync_Others, position.x, position.y, position.z, eulerAngles.x, eulerAngles.y, eulerAngles.z);
            }
        }
    }

    private void UpdateFixedSync_Others(CPacket msg)
    {
        transform.position = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
        transform.rotation = Quaternion.Euler(new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float()));
    }

    protected override void Die()
    {
        base.Die();

        if (CNetworkManager.instance.isMasterClient)
        {
            StopCoroutineRoutine();
            currentAction = StartCoroutine(DieAction());
        }
    }

    private IEnumerator DieAction()
    {
        BaseAnimationCoroutine_Mine(AnimationEnum.DieMotion, 0.2f);

        float duration = 1.883f;

        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(0f);

        CommonMethods.Destroy_netObject_MasterClient(pool_code, id);
    }

    protected override void On_PlayerDead()
    {
        base.On_PlayerDead();
        nextAction = DecideAction();
        if (currentAction != null)
        {
            StopCoroutine(currentAction);
        }
        Debug.Log($"Enemy_Skeleton : {attackTarget} // {nextAction}");
        if(currentAction != null)
        {
            currentAction = StartCoroutine(nextAction);
        }
    }

    private IEnumerator On_idle_layder()
    {
        BaseAnimationCrossFade_Mine(AnimationEnum.Idle, 0.2f);

        while (true)
        {
            Collider[] collider = Physics.OverlapSphere(transform.position, 7f, playerLayer);
            if(collider.Length > 0)
            {
                for(int i = 0; i < collider.Length; i++)
                {
                    PlayerHealth playerHealth = collider[i].GetComponent<PlayerHealth>();
                    if (playerHealth.dead) continue;

                    attackTarget = playerHealth;
                    UpdateCalcutations();
                    nextAction = DecideAction();
                    currentAction = StartCoroutine(nextAction);
                    yield break;
                }

                break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }


    private IEnumerator DecideAction()
    {
        if (attackTarget != null)
        {
            if (distanceToTarget > 1f)
            {
                return MoveTowardPlayer();
            }
            else
            {
                return Attack();
            }
        }
        else
        {
            return On_idle_layder();
        }
    }

    private IEnumerator MoveTowardPlayer()
    {
        navMeshAgent.enabled = true;
        navMeshAgent.speed = movementSpeed;

        BaseAnimationCrossFade_Mine(AnimationEnum.Walk, 0.2f);

        while (true)
        {
            navMeshAgent.SetDestination(attackTarget.transform.position);


            if(distanceToTarget <= 1f)
            {
                navMeshAgent.enabled = false;
                nextAction = DecideAction();
                break;
            }

            yield return new WaitForSeconds(0.2f);
        }


        currentAction = StartCoroutine(nextAction);
        nextAction = null;
    }
    private IEnumerator Attack()
    {
        BaseAnimationCoroutine_Mine(AnimationEnum.Attack, 0.2f);

        float elapsedTime = 0f; float duration = 1.267f; float attackingTime = 0.383f;
        bool didAttacked = false;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            transform.rotation = Quaternion.LookRotation(attackTarget.transform.position - transform.position);
            
            if(elapsedTime >= attackingTime && !didAttacked)
            {
                if (!attackTarget.dead)
                {
                    attackTarget.OnDamage_MasterClient(damage, this);
                }
                didAttacked = true;
            }

            yield return null;
        }

        nextAction = DecideAction();
        currentAction = StartCoroutine(nextAction);
    }


    private void StopCoroutineRoutine()
    {
        if(currentAction != null)
        {
            StopCoroutine(currentAction);
            currentAction = null;
        }
        navMeshAgent.enabled = false;
    }

    


    public void BaseAnimationCrossFade_Mine(AnimationEnum animationEnum, float blendTime)
    {
        string animationName = animationEnum.ToString();

        if (currentAnimation == animationName) return;

        enemyAnimator.CrossFade(animationName, blendTime, 0, 0);
        currentAnimation = animationName;

        CommonMethods.Sync_animation_Mine(pool_code, id, (byte)NetEnum__121_150.Sync_Animation_Others, (byte)RoomMember.Others, (byte)animationEnum, blendTime, 0f);
    }
    public void BaseAnimationCoroutine_Mine(AnimationEnum animationEnum, float blendTime)
    {
        string animationName = animationEnum.ToString();
        enemyAnimator.CrossFade(animationName, blendTime, 0, 0);
        currentAnimation = animationName;

        CommonMethods.Sync_animation_Mine(pool_code, id, (byte)NetEnum__121_150.Sync_Animation_Others, (byte)RoomMember.Others, (byte)animationEnum, blendTime, 0f);
    }
    public void Sync_Animation_Others(CPacket msg)
    {
        AnimationEnum animationEnum = (AnimationEnum)msg.Pop_byte();
        string animationName = animationEnum.ToString();
        enemyAnimator.CrossFade(animationName, msg.Pop_float(), 0, msg.Pop_float());
    }
}
