using System;
using System.Collections;
using System.Collections.Generic;
using FreeNet;
using Unity.VisualScripting;
using UnityEngine;

public class NetObjectManager : MonoBehaviour
{
    public static NetObjectManager instance;
    private Dictionary<byte, Dictionary<byte, NetObject>> pools_pool = new Dictionary<byte, Dictionary<byte, NetObject>>();
    public PlayerHealth player_prefab;
    public Enemy_Skeleton enemy_skeleton_prefab;
    public Item_health item_health_prefab;
    public PlayerMissile playersMissile_prefab;

    private byte scene_object_index = 0;
    
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

        pools_pool[0] = new Dictionary<byte, NetObject>();
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
    private void OnDestroy()
    {
        instance = null;
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
            case NetObjectCode.Enemy_skeleton:
                {
                    netObject = Instantiate(enemy_skeleton_prefab, position, Quaternion.Euler(rotation));
                }
                break;
            case NetObjectCode.Item_health:
                {
                    netObject = Instantiate(item_health_prefab, position, Quaternion.Euler(rotation));
                }
                break;
            case NetObjectCode.PlayersMissile:
                {
                    netObject = Instantiate(playersMissile_prefab, position, Quaternion.Euler(rotation));
                }
                break;
            default:
                {
                    Debug.LogError("NetObjectManager : 코드에 대응하는 오브젝트가 할당되지 않았습니다");
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

        //Debug.Log($"NetOBjectManager Debug__ ownerCode : {ownerCode}, objectCode : {objectCode}, pool_code : {pool_code}, id : {id}, position : {position}, rotation : {rotation}");
    }
    public void Remove_object(byte pool_code, byte id)
    {
        if (pools_pool.ContainsKey(pool_code) && pools_pool[pool_code].ContainsKey(id))
        {
            Destroy(pools_pool[pool_code][id].gameObject);

            pools_pool[pool_code].Remove(id);
            if (pools_pool[pool_code].Count <= 0 && pools_pool.Count - pool_code > 0) // 갓 생성된 풀에 오브젝트가 하나밖에 없는 상황에서, 그 오브젝트가 Destroy됐을 때 풀이 삭제 되는 것을 방지
            {
                pools_pool.Remove(pool_code);
            }
        }
    }

    public NetObject Get_netObject(byte pool_code, byte id)
    {
        try
        {
            return pools_pool[pool_code][id];
        }
        catch(Exception e)
        {
            Debug.Log($"NetObjectManager : {e.Message} // {pool_code} - {id}");
            return null;
        }
    }

    public byte Register_scene_object(NetObject netObject)
    {
        pools_pool[0].Add(++scene_object_index, netObject);
        return scene_object_index;
    }
    public void UnRegister_scene_object(byte id)
    {
        pools_pool[0].Remove(id);
    }
}
