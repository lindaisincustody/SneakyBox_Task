using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollow : MonoBehaviour
{
    public Vector3 offset;
    public bool isFollowingCursor;
    private bool isCtrlPressed = false;
     private Vector3 initialPosition;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isCtrlPressed = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
        {
            isCtrlPressed = false;
        }

        // Debug.Log(isFollowingCursor);
        if (isFollowingCursor)
        {
            if(isCtrlPressed)
            {
                Vector3 pos = BuildManager.GetMouseWorldPosition();
                pos.y = initialPosition.y;
                transform.position = pos;
                
            }
            else
            {
            Vector3 pos = BuildManager.GetMouseWorldPosition();
            transform.position = BuildManager.current.SnapCoordinateToGrid(pos);
            }
        }
       
    }

    private void OnMouseDown()
    {
        isFollowingCursor = true;
       
        initialPosition = transform.position;
        
    }
}