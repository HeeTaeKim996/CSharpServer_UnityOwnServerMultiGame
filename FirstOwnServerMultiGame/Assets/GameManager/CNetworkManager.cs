using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeNet;
using FreeNetUnity;
using UnityEngine.SceneManagement;
using TMPro;

public class CNetworkManager : MonoBehaviour
{
    public static CNetworkManager instance;
    private CNetUnityService cNetUnityService;
    private bool isOnGame = false;
    private NetLobbyActionAdmin netLobbyActionAdmin;
    public string remote_endPoint;
    public bool isDevelopMode;
    public bool isMobiletest; 

    public byte room_id { get; private set; }
    public bool isMasterClient { get; private set; }// 우선, 마스터는 room_id 가 고정 1일 때에로 하자. 마스터가 나가면 게임은 종료되는 것으로(방을 room_id===1인 방을 만든 사람이 나가도 사라지는 것으로 우선 처리)

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (isDevelopMode)
        {
            Set_room_id(1);
        }
        SceneManager.activeSceneChanged += On_scene_changed;
        cNetUnityService = GetComponent<CNetUnityService>();
        On_lobby_scene_start();
        Connect();

        if (SceneManager.GetActiveScene().name == "MainGame") // 테스트 용도
        {
            StartCoroutine(ForDevelop_ownStart_coroutine());
        }
    }

    private IEnumerator ForDevelop_ownStart_coroutine()
    {
        yield return new WaitForSeconds(0.1f);

        CPacket msg = CPacket.Pop_forCreate();
        msg.Push((byte)Pr_target.lobby);
        msg.Push((byte)Pr_ta_lobby_action.create_room);
        msg.Push("For Develop Room132465");
        CNetworkManager.instance.Send(msg);

        yield return new WaitForSeconds(0.1f);

        CPacket packet = CPacket.Pop_forCreate();
        packet.Push((byte)Pr_target.room);
        packet.Push((byte)Pr_ta_room_target.all);
        packet.Push((byte)Pr_ta_room_action.game_start_masterClient);
        CNetworkManager.instance.Send(packet);
    }

    private void On_scene_changed(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
    {
        if(newScene.name == "MainGame")
        {
            // 이벤트가 발동하는 시점이 씬이 생성되고, Awake가 발동하기 전이라, 생각보다 쓸모는 없지 않을까 싶다.
        }
    }
    


    private void On_lobby_scene_start()
    {
        netLobbyActionAdmin = FindAnyObjectByType<NetLobbyActionAdmin>();
    }

    private void Connect()
    {
        cNetUnityService.Connect(remote_endPoint, 7979);
    }
    public bool is_connected()
    {
        return cNetUnityService.is_connected();
    }


    public void On_status_changed(NetworkEvent netEvent)
    {
        switch (netEvent)
        {
            case NetworkEvent.connected:
                {
                    Debug.Log("CNetworkManager : 서버와 연결됨");
                    if (isDevelopMode) return;  // 개발 용도. 개발 때 MainGame에서 바로 시작할 때에, LobbyManager가 없으므로, LobbyScene에서만 활성화되도록 처리
                    
                    LobbyManager.instance.lobby_start();
                    
                }
                break;
            case NetworkEvent.disconnected:
                {
                    Debug.Log("CNetworkManager : 서버와 연결이 해제됐습니다");
                }
                break;
        }
    }
    public void On_message(CPacket msg)
    {
        if (!isOnGame)
        {
            Pr_client_action client_action = (Pr_client_action)msg.Pop_byte();
            switch (client_action) 
            {
                case Pr_client_action.lobby_actin:
                    {
                        if (isDevelopMode) return;  // 개발 용도. 개발 때 MainGame에서 바로 시작할 때에, LobbyManager가 없으므로, LobbyScene에서만 활성화되도록 처리
                        netLobbyActionAdmin.Net_lobbyAction_task(msg);
                    }
                    break;
                case Pr_client_action.room_action:
                    {
                        if (CNetworkManager.instance.isDevelopMode) return;  // 개발 용도. 개발 때 MainGame에서 바로 시작할 때에, LobbyManager가 없으므로, LobbyScene에서만 활성화되도록 처리
                        netLobbyActionAdmin.Net_room_action_task(msg);   
                    }
                    break;
                case Pr_client_action.game_late_start:
                    {
                        GameManager.instance.Invoke_lateStart();
                        isOnGame = true;
                    }
                    break;
            }
        }
        else
        {
            InGameAction_client inGameAction = (InGameAction_client)msg.Pop_byte();
            switch (inGameAction) 
            {
                case InGameAction_client.Intantaite_object:
                    {
                        byte owner_code = msg.Pop_byte();
                        NetObjectCode objectCode = (NetObjectCode)msg.Pop_byte();
                        byte pool_code = msg.Pop_byte();
                        byte id = msg.Pop_byte();
                        Vector3 position = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
                        Vector3 rotation = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
                        NetObjectManager.instance.Instantiate_object(owner_code, objectCode, pool_code, id, position, rotation);
                    }
                    break;
                case InGameAction_client.Delete_object:
                    {
                        NetObjectManager.instance.Remove_object(msg.Pop_byte(), msg.Pop_byte());
                    }
                    break;
                case InGameAction_client.Object_transfer:
                    {
                        NetObject netObject = NetObjectManager.instance.Get_netObject(msg.Pop_byte(), msg.Pop_byte());
                        netObject.NetMethod(msg);
                    }
                    break;
            }
        }
    }

    public void Send(CPacket msg)
    {
        cNetUnityService.Send(msg);
    }
    public void Set_room_id(byte room_id)
    {
        this.room_id = room_id;
        isMasterClient = room_id == 1 ? true : false;
    }

    
}
