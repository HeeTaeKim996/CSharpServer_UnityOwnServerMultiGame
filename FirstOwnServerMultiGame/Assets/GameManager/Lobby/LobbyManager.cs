using System.Collections;
using System.Collections.Generic;
using FreeNet;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    [SerializeField]
    private GameObject lobby_background;
    public Button create_room_button;
    public RectTransform[] room_rects;
    public Room room_prefab;

    public Button create_Room_button;
    public TMP_InputField tmp_inputField;

    private List<Room> active_rooms = new List<Room>();



    [SerializeField]
    private GameObject room_background;
    [SerializeField]
    private Text room_name_text;
    [SerializeField]
    private Text rooms_playerCount_text;
    [SerializeField]
    private Button start_game_button;

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
        create_room_button.onClick.AddListener(Create_room);

        lobby_background.GetComponent<CanvasGroup>().alpha = 1;
        room_background.GetComponent<CanvasGroup>().alpha = 1;
        lobby_background.gameObject.SetActive(false);
        room_background.gameObject.SetActive(false);
        start_game_button.onClick.AddListener(Invoke_start_game);
    }

    public void lobby_start()
    {
        lobby_background.gameObject.SetActive(true);
        room_background.gameObject.SetActive(false);
    }
    public void room_start()
    {
        lobby_background.gameObject.SetActive(false);
        room_background.gameObject.SetActive(true);
    }

    public void Lobby_list_update(CPacket msg)
    {
        foreach(Room room in active_rooms)
        {
            Destroy(room.gameObject);
        }

        int room_count = msg.Pop_byte();
        for(int i = 0; i < room_count; i++)
        {
            Debug.Log("Check");

            string room_name = msg.Pop_string();
            int rooms_user_count = msg.Pop_byte();
            Room room = Instantiate(room_prefab);
            room.Initialize(room_name, rooms_user_count);

            RectTransform roomRect = room.GetComponent<RectTransform>();
            roomRect.SetParent(room_rects[i]);
            roomRect.anchoredPosition = Vector2.zero;

            active_rooms.Add(room);

            Debug.Log("LobbyManager : Lobby room updated ");
        }
    }
    public void Create_room()
    {
        CPacket msg = CPacket.Pop_forCreate();
        msg.Push((byte)Pr_target.lobby);
        msg.Push((byte)Pr_ta_lobby_action.create_room);
        msg.Push((string)tmp_inputField.text);
        CNetworkManager.instance.Send(msg);
    }


    public void Join_room(string room_name)
    {
        CPacket msg = CPacket.Pop_forCreate();
        msg.Push((byte)Pr_target.lobby);
        msg.Push((byte)Pr_ta_lobby_action.enter_room);
        msg.Push(room_name);
        CNetworkManager.instance.Send(msg);
    }



    public void Update_room_info(CPacket msg)
    {
        room_name_text.text = msg.Pop_string();
        rooms_playerCount_text.text = msg.Pop_byte().ToString();
        if(msg.Pop_byte() == 0)
        {
            start_game_button.gameObject.SetActive(true);
        }
        else
        {
            start_game_button.gameObject.SetActive(false);
        }
    }
    
    private void Invoke_start_game()
    {
        Debug.Log("LobbyManager : Invoke_start_game Check");
        CPacket packet = CPacket.Pop_forCreate();
        packet.Push((byte)Pr_target.room);
        packet.Push((byte)Pr_ta_room_target.all);
        packet.Push((byte)Pr_ta_room_action.game_start_masterClient);
        CNetworkManager.instance.Send(packet);
    }
}
