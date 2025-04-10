using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerManager : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerController_PC playerController_PC;
    private PlayerController_Mobile playerController_mobile;


    private void Awake()
    {
        playerController_PC = GetComponentInChildren<PlayerController_PC>();
        playerController_mobile = GetComponentInChildren<PlayerController_Mobile>();

        if(CNetworkManager.instance.isMobiletest || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            playerController_PC.gameObject.SetActive(false);
            playerController = playerController_mobile;
        }
        else
        {
            playerController_mobile.gameObject.SetActive(false);
            playerController = playerController_PC;
        }
    }

    public PlayerController Set_PlayerMovement(PlayerMovement playerMovement)
    {
        playerController.Set_PlayerMovement(playerMovement);
        return playerController;
    }
}
