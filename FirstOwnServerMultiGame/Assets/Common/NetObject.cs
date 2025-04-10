using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;

public abstract class NetObject : MonoBehaviour
{
    public byte owner { get; protected set; }
    public byte pool_code { get; protected set; }
    public byte id { get; protected set; }
    public bool isMine { get; protected set; }

    private enum InstanType
    {
        Instantiated,
        ScenePlaced_each,
        ScenePlaced_Sync
    }
    [SerializeField]
    private InstanType instanType;
    [SerializeField]
    private NetObjectCode netObjectCode_forScenePlaced_Sync;

    protected byte byteNetEnum;

    protected virtual void Awake()
    {

    }
    protected virtual void Start()
    {


        if (instanType == InstanType.ScenePlaced_each)
        {
            owner = CNetworkManager.instance.room_id;
            pool_code = 0;
            id = NetObjectManager.instance.Register_scene_object(this);

            isMine = true;
        }

        GameManager.instance.event_lateStart += Late_start;
    }
    protected virtual void Late_start()
    {
        if (instanType == InstanType.ScenePlaced_Sync)
        {
            if (CNetworkManager.instance.isMasterClient)
            {
                CommonMethods.Instantiate_netObject(CNetworkManager.instance.room_id, netObjectCode_forScenePlaced_Sync, transform.position, transform.eulerAngles);
            }
            Destroy(gameObject);
        }
    }

    public void Set_netObject_info(byte owner, byte pool_code, byte id)
    {
        this.owner = owner;
        this.pool_code = pool_code;
        this.id = id;

        isMine = CNetworkManager.instance.room_id == owner ? true : false; 
    }

    public virtual void After_Set_start() { }

    public enum NetEnum__0_30 : byte
    {
        // 0부터 30 까지의 byte를 사용해야 한다. (상속하는 클래스들은 31부터..)

    }

    public virtual void NetMethod(CPacket msg)
    {
        byteNetEnum = (byte)msg.Pop_byte();
    }


    public void OnDestroy()
    {
        GameManager.instance.event_lateStart -= Late_start;
        if (instanType == InstanType.ScenePlaced_each)
        {
            NetObjectManager.instance.UnRegister_scene_object(id);
        }
    }
}
