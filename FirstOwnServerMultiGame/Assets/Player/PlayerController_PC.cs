using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController_PC : PlayerController
{
    [SerializeField]
    private RectTransform playersUsingImos;
    [SerializeField]
    private RectTransform missileImo;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsClickWithinRect(playersUsingImos, Input.mousePosition))
            {
                playerMovement.On_touch_start(Input.mousePosition);
            }
            else
            {
                if(IsClickWithinRect(missileImo, Input.mousePosition))
                {
                    playerMovement.Switch_PlayerActionState(PlayerMovement.PlayerActionState.Missle);
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if(!IsClickWithinRect(playersUsingImos, Input.mousePosition))
            {
                playerMovement.On_touching(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!IsClickWithinRect(playersUsingImos, Input.mousePosition))
            {
                playerMovement.On_touch_end(Input.mousePosition);
            }
        }
    }

    private bool IsClickWithinRect(RectTransform rectTransform, Vector2 screenPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, null);
    }
}
