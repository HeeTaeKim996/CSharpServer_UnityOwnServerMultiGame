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

    public bool isScenePlaced;

    protected byte byteNetEnum;

    protected virtual void Awake()
    {

    }
    protected virtual void Start()
    {
        if (isScenePlaced)
        {
            owner = CNetworkManager.instance.room_id;
            pool_code = 0;
            id = NetObjectManager.instance.Register_scene_object(this);

            isMine = true;
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
        if (isScenePlaced)
        {
            NetObjectManager.instance.UnRegister_scene_object(id);
        }
    }
}
