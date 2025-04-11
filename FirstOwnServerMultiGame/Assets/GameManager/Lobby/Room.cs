using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    [SerializeField]
    private Text room_name;
    [SerializeField]
    private Text rooms_user_count;
    [SerializeField]
    private Text text_is_room_sealed;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(string room_name, int room_user_count, bool is_room_sealed)
    {
        this.room_name.text = room_name;
        this.rooms_user_count.text = "Player : " + rooms_user_count.ToString();

        this.text_is_room_sealed.text =  "Is Room Sealed : " + is_room_sealed.ToString();
        if (!is_room_sealed)
        {
            button.onClick.AddListener(() => LobbyManager.instance.Join_room(room_name));
        }
    }
}
