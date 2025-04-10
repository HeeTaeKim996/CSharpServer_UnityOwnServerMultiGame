using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Mobile : PlayerController
{

    private int movingTouchId = -1;
    private bool didMovingTouch = false;

    private void Update()
    {
        for(int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if( movingTouchId == -1 || movingTouchId == touch.fingerId)
            {
                if(touch.phase == TouchPhase.Began)
                {
                    movingTouchId = touch.fingerId;
                    didMovingTouch = true;
                    playerMovement.On_touch_start(touch.position);
                }
                else if( (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && didMovingTouch)
                {

                }
                else if( ( touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && didMovingTouch)
                {
                    movingTouchId = -1;
                    didMovingTouch = false;
                }
            }
            
        }    
    }




    private bool IsTouhcWithinRect(RectTransform rectTransform, Vector2 touchPosition)
    {
        Vector2 localPoint;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touchPosition, null, out localPoint))
        {
            return rectTransform.rect.Contains(localPoint);
        }
        return false;
    }
    private bool IsTouchWithinCircle(RectTransform rectTransform, Vector2 touchPosition, float radius)
    {
        Vector2 localPoint;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touchPosition, null, out localPoint))
        {
            return localPoint.magnitude <= radius;
        }
        return false;
    }
}
