using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public class NetObjectManager : MonoBehaviour
{
    public static NetObjectManager instance;
    private Dictionary<NetObjectCode, Dictionary<short, NetObject>> pools_pool = new Dictionary<NetObjectCode, Dictionary<short, NetObject>>();
    public Player player_prefab;

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

        Inform_client_is_ready(); // �� ���� �Ŵ����� ó���ϴ� �� ������ �ؼ� ���⼭, ������ ������ �ڵ� �ۼ�

    }

    private void Inform_client_is_ready()
    {
        CPacket msg = CPacket.Pop_forCreate();
        msg.Push((byte)Pr_target.room);
        msg.Push((byte)Pr_ta_room_target.room);
        msg.Push((byte)Pr_ta_room_action.game_load_completed);

        CNetworkManager.instance.Send(msg);
    }

    public void Instantiate_object_pool(NetObjectCode objectCode)
    {
        if (pools_pool.ContainsKey(objectCode))
        {
            Debug.LogError("NetObjectManager : �̹� ������ ������Ʈ �ڵ� Ǯ�� ���� �õ�");
        }

        pools_pool[objectCode] = new Dictionary<short, NetObject>();
    }

    public void Instantiate_object(NetObjectCode objectCode, short id, Vector3 position, Vector3 rotation)
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

        pools_pool[objectCode][id] = netObject;
    }
}
