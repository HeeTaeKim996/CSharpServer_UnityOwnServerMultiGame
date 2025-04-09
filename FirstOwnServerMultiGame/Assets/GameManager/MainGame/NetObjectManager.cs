using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class NetObjectManager : MonoBehaviour
{
    public static NetObjectManager instance;
    private Dictionary<byte, Dictionary<byte, NetObject>> pools_pool = new Dictionary<byte, Dictionary<byte, NetObject>>();
    public PlayerHealth player_prefab;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        if (CNetworkManager.instance.isDevelopMode)
        {
            StartCoroutine(ForDevelop_lateReady());
        }
        else
        {
            Inform_client_is_ready(); // 넷 옵젝 매니저가 처리하는 게 좋을듯 해서 여기서, 서버로 보내는 코드 작성
        }
    }
    private IEnumerator ForDevelop_lateReady()
    {
        yield return new WaitForSeconds(0.5f);
        Inform_client_is_ready();
    }

    private void Inform_client_is_ready()
    {
        CPacket msg = CPacket.Pop_forCreate();
        msg.Push((byte)Pr_target.room);
        msg.Push((byte)Pr_ta_room_target.room);
        msg.Push((byte)Pr_ta_room_action.game_load_completed);

        CNetworkManager.instance.Send(msg);
    }

    public void Instantiate_object(byte ownerCode, NetObjectCode objectCode, byte pool_code, byte id, Vector3 position, Vector3 rotation)
    {
        NetObject netObject;
        switch (objectCode)
        {
            case NetObjectCode.Player:
                {
                    netObject = Instantiate(player_prefab, position, Quaternion.Euler(rotation));
                }
                break;


            default:
                {
                    netObject = null;
                }
                break;
        }
        netObject.Set_netObject_info(ownerCode, pool_code, id);
        netObject.After_Set_start();

        if (!pools_pool.ContainsKey(pool_code))
        {
            pools_pool[pool_code] = new Dictionary<byte, NetObject>();
        }
        pools_pool[pool_code][id] = netObject;

        Debug.Log($"NetOBjectManager Debug__ ownerCode : {ownerCode}, objectCode : {objectCode}, pool_code : {pool_code}, id : {id}, position : {position}, rotation : {rotation}");
    }

    public NetObject Get_netObject(byte pool_code, byte id)
    {
        return pools_pool[pool_code][id];
    }
}
