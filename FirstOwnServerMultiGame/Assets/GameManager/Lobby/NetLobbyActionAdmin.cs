using System.Collections;
using System.Collections.Generic;
using FreeNet;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetLobbyActionAdmin : MonoBehaviour
{
    private LobbyManager lobbyManager;

    private void Awake()
    {
        lobbyManager = GetComponent<LobbyManager>();
    }

    public void Net_lobbyAction_task(CPacket msg)
    {
        Pr_ca_lobby__action lobby_action = (Pr_ca_lobby__action)msg.Pop_byte();
        switch (lobby_action)
        {
            case Pr_ca_lobby__action.lobby_list_info:
                {
                    lobbyManager.Lobby_list_update(msg);
                }
                break;
        }

    }

    public void Net_room_action_task(CPacket msg)
    {
        Pr_ca_room_action room_action = (Pr_ca_room_action)msg.Pop_byte();
        switch (room_action) 
        {
            case Pr_ca_room_action.room_start:
                {
                    CNetworkManager.instance.Set_room_id(msg.Pop_byte());
                    lobbyManager.room_start();
                }
                break;
            case Pr_ca_room_action.room_info:
                {
                    lobbyManager.Update_room_info(msg);
                }
                break;
            case Pr_ca_room_action.game_start:
                {
                    SceneManager.LoadScene("MainGame");
                }
                break;
        }
    }
}
