using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeNet;
using FreeNetUnity;
using UnityEngine.SceneManagement;

public class CNetworkManager : MonoBehaviour
{
    public static CNetworkManager instance;
    private CNetUnityService cNetUnityService;
    private bool isOnGame = false;
    private NetLobbyActionAdmin netLobbyActionAdmin;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("두개의 CNetworkManager가 있습니다. 새로 추가된 것을 삭제합니다");
            Destroy(gameObject);
        }
        SceneManager.activeSceneChanged += On_scene_changed;
        cNetUnityService = GetComponent<CNetUnityService>();
        On_lobby_scene_start();
        Connect();
    }
    private void On_scene_changed(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
    {
        if(newScene.name == "MainGame")
        {
            Debug.Log("MainGAme Laoded Check"); // 이벤트가 발동하는 시점이 씬이 생성되고, Awake가 발동하기 전이라, 생각보다 쓸모는 없지 않을까 싶다.
        }
    }
    


    private void On_lobby_scene_start()
    {
        netLobbyActionAdmin = FindAnyObjectByType<NetLobbyActionAdmin>();
        if(netLobbyActionAdmin == null)
        {
            Debug.Log("Null??");
        }
    }

    private void Connect()
    {
        cNetUnityService.Connect("127.0.0.1", 7979);
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
                        netLobbyActionAdmin.Net_lobbyAction_task(msg);
                    }
                    break;
                case Pr_client_action.room_action:
                    {
                        netLobbyActionAdmin.Net_room_action_task(msg);   
                    }
                    break;
                case Pr_client_action.game_late_start:
                    {
                        GameManager.instance.Invoke_lateStart();
                        isOnGame = true;
                        Debug.Log("CNetworkManager : DebugCheck");
                    }
                    break;
            }
        }
        else
        {
            InGameAction_client inGameAction = (InGameAction_client)msg.Pop_byte();
            switch (inGameAction) 
            {
                case InGameAction_client.Instantiate_object_pool:
                    {
                        NetObjectManager.instance.Instantiate_object_pool((NetObjectCode)msg.Pop_byte());
                    }
                    break;
                case InGameAction_client.Intantaite_object:
                    {
                        NetObjectCode objectCode = (NetObjectCode)msg.Pop_byte();
                        short id = msg.Pop_int16();
                        Vector3 position = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
                        Vector3 rotation = new Vector3(msg.Pop_float(), msg.Pop_float(), msg.Pop_float());
                        NetObjectManager.instance.Instantiate_object(objectCode, id, position, rotation);
                    }
                    break;
            }

        }
    }

    public void Send(CPacket msg)
    {
        cNetUnityService.Send(msg);
    }
   
}
