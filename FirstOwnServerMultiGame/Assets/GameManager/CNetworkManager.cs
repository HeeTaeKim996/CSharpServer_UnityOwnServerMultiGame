using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeNet;
using FreeNetUnity;
using UnityEngine.SceneManagement;
using System.Threading;



public class CNetworkManager : MonoBehaviour
{
    public static CNetworkManager instance;
    private CNetUnityService cNetUnityService;
    private bool isOnGame = false;
    private bool is_scene_load_completed = true;
    private NetLobbyActionAdmin netLobbyActionAdmin;
    public string remote_endPoint;
    public bool isDevelopMode;
    public bool isMobiletest; 

    public byte room_id { get; private set; }
    public bool isMasterClient { get; private set; }// �켱, �����ʹ� room_id �� ���� 1�� ������ ����. �����Ͱ� ������ ������ ����Ǵ� ������(���� room_id===1�� ���� ���� ����� ������ ������� ������ �켱 ó��)

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

        if (instance != this) return; // Destroy �� ����ǵ�, ���� ������Ʈ�� LateUpdate ����, ������ ���� �������� �ؼ�, instance != null�̶� �ص�, awake�� start�� ������ ����ȴ� �Ѵ�. ���� if(instance != this) return; ���� ����

        if (isDevelopMode)
        {
            Set_room_id(1);
        }
        SceneManager.activeSceneChanged += On_scene_changed;
        cNetUnityService = gameObject.AddComponent<CNetUnityService>();
    
        Connect();

        if (SceneManager.GetActiveScene().name == "MainGame") // �׽�Ʈ �뵵
        {
            StartCoroutine(ForDevelop_ownStart_coroutine());
        }
    }
    private void Start()
    {
        if (instance != this) return;

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
            // �̺�Ʈ�� �ߵ��ϴ� ������ ���� �����ǰ�, Awake�� �ߵ��ϱ� ���̶�, �������� ����� ���� ������ �ʹ�.
        }
    }
    


    public void Set_net_lobby_Action_Admin(NetLobbyActionAdmin netLobbyActionAdmin)
    {
        this.netLobbyActionAdmin = netLobbyActionAdmin;
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
                    Debug.Log("CNetworkManager : ������ �����");
                    if (isDevelopMode) return;  // ���� �뵵. ���� �� MainGame���� �ٷ� ������ ����, LobbyManager�� �����Ƿ�, LobbyScene������ Ȱ��ȭ�ǵ��� ó��
                    
                    LobbyManager.instance.lobby_start();
                    
                }
                break;
            case NetworkEvent.disconnected:
                {
                    Debug.Log("CNetworkManager : ������ ������ �����ƽ��ϴ�");
                }
                break;
        }
    }
    public void On_message(CPacket msg)
    {
        if (!isOnGame)
        {
            if (!is_scene_load_completed) return;

            Pr_client_action client_action = (Pr_client_action)msg.Pop_byte();
            switch (client_action) 
            {
                case Pr_client_action.lobby_actin:
                    {
                        if (isDevelopMode) return;  // ���� �뵵. ���� �� MainGame���� �ٷ� ������ ����, LobbyManager�� �����Ƿ�, LobbyScene������ Ȱ��ȭ�ǵ��� ó��
                        netLobbyActionAdmin.Net_lobbyAction_task(msg);
                    }
                    break;
                case Pr_client_action.room_action:
                    {
                        if (CNetworkManager.instance.isDevelopMode) return;  // ���� �뵵. ���� �� MainGame���� �ٷ� ������ ����, LobbyManager�� �����Ƿ�, LobbyScene������ Ȱ��ȭ�ǵ��� ó��
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

                        if(NetObjectManager.instance == null)
                        {
                            Debug.Log("@@@@@@@@@@@@DEEEBBBUGUG00");
                        }

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
                case InGameAction_client.ETC:
                    {
                        Ga_c_Etc_action etcAction = (Ga_c_Etc_action)msg.Pop_byte();
                        switch (etcAction)
                        {
                            case Ga_c_Etc_action.BackToLobby:
                                {
                                    isOnGame = false;
                                    is_scene_load_completed = false;
                                    SceneManager.LoadScene("LobbyScene");
                                    Debug.Log("CetworkMAnager : �� ������Դϴ�. ������ ������ �����Ͽ� �κ�� ���ƿɴϴ�.");
                                }
                                break;
                            default:
                                {
                                    Debug.LogError($"Switch�� ����Ʈ ����. {etcAction.ToString()}");
                                }
                                break;
                        }

                    }
                    break;
                default:
                    {
                        Debug.LogError($"Switch�� ����Ʈ ����. {inGameAction.ToString()}");
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

    public void Get_Scene_Ready()
    {
        is_scene_load_completed = true;
    }    
}
