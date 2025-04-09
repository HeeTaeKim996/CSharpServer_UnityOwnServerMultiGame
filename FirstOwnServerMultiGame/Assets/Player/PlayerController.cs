using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    protected PlayerMovement playerMovement;

    public void Set_PlayerMovement(PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;
    }
}
