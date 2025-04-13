using System;
using System.Collections;
using System.Collections.Generic;
using FreeNet;
using JetBrains.Annotations;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerMovement : MonoBehaviour
{

    private PlayerHealth playerHealth;
    private PlyaerPointerAdmin playerPointerAdmin;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    private Coroutine currentAction;


    private float movementSpeed = 5f;

    private string currentAnimation;

    private LayerMask enemyLayer;
    private LayerMask itemLayer;
    private float damage = 20f;

    public enum PlayerActionState
    {
        Normal,
        Missle
    }
    private PlayerActionState playerActionState;
    private LineRenderer lineRenderer;

    public enum AnimationEnum : byte
    {
        Idle,
        Walk,
        Attack,
        DieMotion
    }

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        playerPointerAdmin = GetComponent<PlyaerPointerAdmin>();
        lineRenderer = GetComponent<LineRenderer>();
    }
    private void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
        itemLayer = LayerMask.GetMask("Item");

        playerActionState = PlayerActionState.Normal;

        lineRenderer.enabled = false;
    }
    private void Update()
    {
        if (playerHealth.isMine && currentAction == null)
        {
            BaseAnimationCrossFade_Mine(AnimationEnum.Idle, 0.05f);
        }
    }
    private void FixedUpdate()
    {
        if (playerHealth.isMine)
        {
            Invoke_Update_fixed_sync();
        }
    }

    private void Invoke_Update_fixed_sync()
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
        send_msg.Push((byte)playerHealth.pool_code);
        send_msg.Push((byte)playerHealth.id);
        send_msg.Push((byte)PlayerHealth.NetEnum__61_90.Update_fixed_sync);
        send_msg.Push((byte)RoomMember.Others);
        send_msg.Push((short)(6 * sizeof(float)));

        send_msg.Push((float)transform.position.x);
        send_msg.Push((float)transform.position.y);
        send_msg.Push((float)transform.position.z);

        Vector3 euler = transform.eulerAngles;
        send_msg.Push((float)euler.x);
        send_msg.Push((float)euler.y);
        send_msg.Push((float)euler.z);

        CNetworkManager.instance.Send(send_msg);
    }

    public void Update_fixed_sync(CPacket msg)
    {
        float pos_x = msg.Pop_float(); float pos_y = msg.Pop_float(); float pos_z = msg.Pop_float();
        float rot_x = msg.Pop_float(); float rot_y = msg.Pop_float(); float rot_z = msg.Pop_float();

        transform.position = new Vector3(pos_x, pos_y, pos_z);
        transform.rotation = Quaternion.Euler(new Vector3(rot_x, rot_y, rot_z));
    }

    public void On_touch_start(Vector2 touchPosition)
    {
        if (playerHealth.dead) return;

        if(playerActionState == PlayerActionState.Normal)
        {
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
                {
                    if (currentAction != null)
                    {
                        StopCoroutine(currentAction);
                    }
                    currentAction = StartCoroutine(AttackEnemy(hit.collider.gameObject.GetComponent<Enemy>()));
                }
                else if (((1 << hit.collider.gameObject.layer) & itemLayer) != 0)
                {
                    if (currentAction != null)
                    {
                        StopCoroutine(currentAction);
                    }
                    currentAction = StartCoroutine(GetItem(hit.collider.gameObject.GetComponent<Item>()));
                }
                else
                {
                    Vector3 hitPosition = hit.point;
                    if (currentAction != null)
                    {
                        StopCoroutine(currentAction);
                    }
                    currentAction = StartCoroutine(MoveToPoint(new Vector2(hitPosition.x, hitPosition.z)));
                }
            }
        }
        else if(playerActionState == PlayerActionState.Missle)
        {
            if (currentAction != null)
            {
                StopCoroutine(currentAction);
                currentAction = null;
            }
        }

    }
    public void On_touching(Vector2 touchPosition)
    {
        if (playerHealth.dead) return;

        if(playerActionState == PlayerActionState.Normal)
        {

        }
        else if(playerActionState == PlayerActionState.Missle)
        {
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                Vector3 lookingVector = hit.point - transform.position;
                lookingVector.y = 0;

                transform.rotation = Quaternion.LookRotation(lookingVector);
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, transform.position + lookingVector * 10f);
                lineRenderer.enabled = true;
            }
        }
    }
    public void On_touch_end(Vector2 touchPosition)
    {
        if (playerHealth.dead) return;

        if(playerActionState == PlayerActionState.Normal)
        {

        }
        else if(playerActionState == PlayerActionState.Missle)
        {
            CPacket send_msg = CommonMethods.Instan_fetcher_helper(playerHealth.owner, NetObjectCode.PlayersMissile, transform.position, transform.eulerAngles, 
                playerHealth.pool_code, playerHealth.id, (byte)PlayerHealth.NetEnum__61_90.Invoke_Set_missle_MasterClient, RoomMember.MasterClient);

            send_msg.Push((short)20);

            Vector3 forwardDirection = transform.forward;
            send_msg.Push((float)forwardDirection.x);
            send_msg.Push((float)forwardDirection.y);
            send_msg.Push((float)forwardDirection.z);
            send_msg.Push((float)30f);
            send_msg.Push((float)5f);

            CNetworkManager.instance.Send(send_msg);

            // if(CPAcketManaager.instance.IsMasterClient 이 true이면, COmmonMethods.Instaitate  -> 직접 매서드 처리 도 될까 생각해봤는데, 서버에 생성하라고 보내고, 생성이 된 후에, 매서드를 처리한다는 보장을 못하기 때문에, 동일한 방법으로 처리
            lineRenderer.enabled = false;
        }
    }


    public IEnumerator MoveToPoint(Vector2 goalPosition)
    {
        Vector3 lookingVector = (new Vector3(goalPosition.x, 0, goalPosition.y) - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookingVector);

        float powDistance;


        BaseAnimationCrossFade_Mine(AnimationEnum.Walk, 0.05f);

        playerPointerAdmin.Retrieve_pointer();

        do
        {
            playerRigidbody.MovePosition(playerRigidbody.position + lookingVector * movementSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();

            float xDiff = goalPosition.x - transform.position.x;
            float zDiff = goalPosition.y - transform.position.z;

            powDistance = xDiff * xDiff + zDiff * zDiff;
        } while (powDistance > 0.01f);

        currentAction = null;
    }

    public IEnumerator AttackEnemy(Enemy enemy)
    {
        float distanceToEnemy;

        playerPointerAdmin.Attach_healthSlider(enemy);

    Back:
        BaseAnimationCrossFade_Mine(AnimationEnum.Walk, 0.05f);
        do
        {
            Vector3 enemyPosition = enemy.transform.position;

            Vector3 lookingVector = (enemyPosition - transform.position).normalized;
            lookingVector.y = 0;
            transform.rotation = Quaternion.LookRotation(lookingVector);

            playerRigidbody.MovePosition(playerRigidbody.position + lookingVector * movementSpeed * Time.fixedDeltaTime);


            distanceToEnemy = Vector3.Distance(transform.position, enemyPosition);
            yield return new WaitForFixedUpdate();
        } while (distanceToEnemy > 1f);


        float elapsedTime = 0f;
        bool isAttacking = false; bool didAttackInvoked = false;

        while (true)
        {
            if (!isAttacking)
            {
                elapsedTime = 0f;
                didAttackInvoked = false;
                BaseAnimationCoroutine_Mine(AnimationEnum.Attack, 0.05f);
                isAttacking = true;

                if (enemy.dead)
                {
                    currentAction = null;
                    yield break;
                }
            }
            else
            {

                if(elapsedTime >= 0.383f && !didAttackInvoked)
                {
                    if (!enemy.dead)
                    {
                        if (CNetworkManager.instance.isMasterClient)
                        {
                            playerHealth.InvokeDamage_Master(enemy, damage);
                        }
                        else
                        {
                            playerHealth.InvokeDamageToMaster_Others(playerHealth, enemy, damage);
                        }
                    }

                    didAttackInvoked = true;
                }

                distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if(distanceToEnemy > 1.5f)
                {
                    goto Back;
                }

                if(elapsedTime >= 1.267f)
                {
                    isAttacking = false;
                }
            }


            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator GetItem(Item item)
    {
        float distanceToItem;

        playerPointerAdmin.Attach_itemPointer(item);

        BaseAnimationCrossFade_Mine(AnimationEnum.Walk, 0.05f);
        do
        {
            Vector3 itemPosition = item.transform.position;

            Vector3 lookingVector = (itemPosition - transform.position).normalized;
            lookingVector.y = 0;
            transform.rotation = Quaternion.LookRotation(lookingVector);

            playerRigidbody.MovePosition(playerRigidbody.position + lookingVector * movementSpeed * Time.fixedDeltaTime);


            distanceToItem = Vector3.Distance(transform.position, itemPosition);
            yield return new WaitForFixedUpdate();
        } while (distanceToItem > 1f);

        if (CNetworkManager.instance.isMasterClient)
        {
            item.Use_masterClient(playerHealth);
        }
        else
        {
            // Send To Master To Use Item
            { 
                CPacket send_msg = CPacket.Pop_forCreate();
                send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
                send_msg.Push((byte)playerHealth.pool_code);
                send_msg.Push((byte)playerHealth.id);
                send_msg.Push((byte)PlayerHealth.NetEnum__61_90.Use_item);
                send_msg.Push((byte)RoomMember.MasterClient);

                send_msg.Push((short)2);

                send_msg.Push((byte)item.pool_code);
                send_msg.Push((byte)item.id);
                CNetworkManager.instance.Send(send_msg);
            }
        }

        currentAction = null;
    }

    public void Invoke_use_item_masterClient(CPacket msg)
    {
        Item item = (Item)NetObjectManager.instance.Get_netObject(msg.Pop_byte(), msg.Pop_byte());
        item.Use_masterClient(playerHealth);
    }


    public void Invoke_DieAction()
    {
        if(currentAction != null)
        {
            StopCoroutine(currentAction);
        }
        currentAction = StartCoroutine(Die_action());
    }
    private IEnumerator Die_action()
    {
        BaseAnimationCoroutine_Mine(AnimationEnum.DieMotion, 0.2f);

        float duration = 1.883f;

        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(3f);
        playerHealth.On_die_action_finished();

        currentAction = null;
    }
    

    public void Invoke_Set_missle_MasterClient(CPacket msg)
    {
        byte pool_code = msg.Pop_byte();
        byte id = msg.Pop_byte();
        Vector3 direction = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
        float damage = msg.Pop_float();
        float destroyTime = msg.Pop_float();

        PlayerMissile playerMissile = (PlayerMissile)NetObjectManager.instance.Get_netObject(pool_code, id);
        playerMissile.GetFire_MasterClient(playerHealth, direction, damage, destroyTime);
    }



    public void BaseAnimationCrossFade_Mine(AnimationEnum animationEnum, float blendTime)
    {
        string animationName = animationEnum.ToString();

        if (currentAnimation == animationName) return;

        playerAnimator.CrossFade(animationName, blendTime, 0, 0);
        currentAnimation = animationName;

        CommonMethods.Sync_animation_Mine(playerHealth.pool_code, playerHealth.id, (byte)PlayerHealth.NetEnum__61_90.Animation_Sync, (byte)RoomMember.Others, (byte)animationEnum, blendTime, 0f);
    }
    public void BaseAnimationCoroutine_Mine(AnimationEnum animationEnum, float blendTime)
    {
        string animationName = animationEnum.ToString();
        playerAnimator.CrossFade(animationName, blendTime, 0, 0);
        currentAnimation = animationName;

        CommonMethods.Sync_animation_Mine(playerHealth.pool_code, playerHealth.id, (byte)PlayerHealth.NetEnum__61_90.Animation_Sync, (byte)RoomMember.Others, (byte)animationEnum, blendTime, 0f);
    }
    public void Sync_Animation_Others(CPacket msg)
    {
        AnimationEnum animationEnum = (AnimationEnum)msg.Pop_byte();
        string animationName = animationEnum.ToString();
        playerAnimator.CrossFade(animationName, msg.Pop_float(), 0, msg.Pop_float());
    }

    public void Switch_PlayerActionState(PlayerActionState playerActionState)
    {
        if(this.playerActionState == playerActionState)
        {
            this.playerActionState = PlayerActionState.Normal;
        }
        else
        {
            this.playerActionState = playerActionState;

        }
    }
}
