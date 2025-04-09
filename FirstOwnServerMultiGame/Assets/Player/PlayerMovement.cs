using System;
using System.Collections;
using System.Collections.Generic;
using FreeNet;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private PlayerHealth playerHealth;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    private Coroutine movingCoroutine;


    private float movementSpeed = 5f;
    private bool isMoving = false;

    private string currentAnimation;
    public enum AnimationEnum : byte
    {
        Idle,
        Walk
    }

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }
    private void Start()
    {

    }
    private void Update()
    {
        if (playerHealth.isMine && !isMoving)
        {
            BaseAnimationCrossFade_Mine(AnimationEnum.Idle, 0.05f);
        }
    }
    private void FixedUpdate()
    {
        if (playerHealth.isMine)
        {
            Invoke_Update_position_rotation_Mine();
        }
    }

    private void Invoke_Update_position_rotation_Mine()
    {
        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
        send_msg.Push((byte)playerHealth.pool_code);
        send_msg.Push((byte)playerHealth.id);
        send_msg.Push((byte)PlayerHealth.NetEnum.Update_fixed_sync);
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

    public void Get_Touch_Position(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPosition = hit.point;

            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
            }
            movingCoroutine = StartCoroutine(MoveToPoint(new Vector2(hitPosition.x, hitPosition.z)));
        }
    }

    public IEnumerator MoveToPoint(Vector2 goalPosition)
    {
        Vector3 lookingVector = (new Vector3(goalPosition.x, 0, goalPosition.y) - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookingVector);

        float powDistance;

        isMoving = true;
        BaseAnimationCrossFade_Mine(AnimationEnum.Walk, 0.05f);

        do
        {
            playerRigidbody.MovePosition(playerRigidbody.position + lookingVector * movementSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();

            float xDiff = goalPosition.x - transform.position.x;
            float zDiff = goalPosition.y - transform.position.z;

            powDistance = xDiff * xDiff + zDiff * zDiff;
        } while (powDistance > 0.01f);

        isMoving = false;
        movingCoroutine = null;
    }


    public void BaseAnimationCrossFade_Mine(AnimationEnum animationEnum, float blendTime)
    {
        string animationName = animationEnum.ToString();

        if (currentAnimation == animationName) return;

        playerAnimator.CrossFade(animationName, blendTime, 0, 0);
        currentAnimation = animationName;

        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
        send_msg.Push((byte)playerHealth.pool_code);
        send_msg.Push((byte)playerHealth.id);
        send_msg.Push((byte)PlayerHealth.NetEnum.Animation_Sync);
        send_msg.Push((byte)RoomMember.Others);

        send_msg.Push((short)9);
        send_msg.Push((byte)animationEnum);
        send_msg.Push((float)blendTime);
        send_msg.Push((float)0);
        CNetworkManager.instance.Send(send_msg);
    }
    public void BaseAnimationCoroutine_Mine(AnimationEnum animationEnum, float blendTime)
    {
        string animationName = animationEnum.ToString();
        playerAnimator.CrossFade(animationName, blendTime, 0, 0);
        currentAnimation = animationName;


        CPacket send_msg = CPacket.Pop_forCreate();
        send_msg.Push((byte)InGameAction_server.Object_transfer_copy);
        send_msg.Push((byte)playerHealth.pool_code);
        send_msg.Push((byte)playerHealth.id);
        send_msg.Push((byte)PlayerHealth.NetEnum.Animation_Sync);
        send_msg.Push((byte)RoomMember.Others);

        send_msg.Push((short)9);
        send_msg.Push((byte)animationEnum);
        send_msg.Push((float)blendTime);
        send_msg.Push((float)0);
        CNetworkManager.instance.Send(send_msg);
    }
    public void Sync_Animation_Others(CPacket msg)
    {
        AnimationEnum animationEnum = (AnimationEnum)msg.Pop_byte();
        string animationName = animationEnum.ToString();
        Debug.Log(animationName);
        playerAnimator.CrossFade(animationName, msg.Pop_float(), 0, msg.Pop_float());
    }

    
}
