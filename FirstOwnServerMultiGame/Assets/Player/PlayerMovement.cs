using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private PlayerHealth playerHealth;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;

    private Coroutine movingCoroutine;


    private float movementSpeed = 5f;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
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
        send_msg.Push((byte)PlayerHealth.NetEnum.Update_position_rotation);
        send_msg.Push((byte)RoomMember.Others);
        send_msg.Push((short)(6 * sizeof(float)));

        send_msg.Push((float)transform.position.x);
        send_msg.Push((float)transform.position.y);
        send_msg.Push((float)transform.position.z);

        send_msg.Push((float)transform.rotation.x);
        send_msg.Push((float)transform.rotation.y);
        send_msg.Push((float)transform.rotation.z);

        CNetworkManager.instance.Send(send_msg);
    }

    public void Update_position_rotation_others(CPacket msg)
    {
        transform.position = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
        transform.rotation = Quaternion.Euler(new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float()));

        Debug.Log("Check");
    }

    public void Get_Touch_Position(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            Vector3 hitPosition = hit.point;
            Debug.Log(hitPosition);

            if(movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
            }
            movingCoroutine = StartCoroutine(MoveToPoint(new Vector2(hitPosition.x, hitPosition.z)));
        }
    }

    public IEnumerator MoveToPoint(Vector2 goalPosition)
    {
        Vector3 lookingVector = (new Vector3(goalPosition.x, 0, goalPosition.y) - transform.position).normalized;

        float powDistance;

        do
        {
            playerRigidbody.MovePosition(playerRigidbody.position + lookingVector * movementSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();

            float xDiff = goalPosition.x - transform.position.x;
            float zDiff = goalPosition.y - transform.position.z;

            powDistance = xDiff * xDiff + zDiff * zDiff;
        } while (powDistance > 0.05f);

        movingCoroutine = null;
    }
}
