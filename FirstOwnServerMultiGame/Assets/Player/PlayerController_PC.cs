using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController_PC : PlayerController
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            playerMovement.On_touch_start(Input.mousePosition);
        }
    }
}
