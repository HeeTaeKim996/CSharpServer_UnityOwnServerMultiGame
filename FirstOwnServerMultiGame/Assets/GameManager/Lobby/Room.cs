using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public Text room_name;
    public Text rooms_user_count;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(string room_name, int room_user_count)
    {
        this.room_name.text = room_name;
        this.rooms_user_count.text = "Player : " + rooms_user_count.ToString();
        button.onClick.AddListener(() => LobbyManager.instance.Join_room(room_name));
    }
}
