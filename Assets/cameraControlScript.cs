using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.InputSystem;

public class cameraControlScript : MonoBehaviour
{

    private Vector2 camDirection = Vector2.zero;
    public Camera mainCamera;
    public InputActionAsset MyMap;
    public float cameraSpeed = 5f;

    public void OnDirectionChanged(InputAction.CallbackContext context)
    {
        if(context.action.phase == InputActionPhase.Started || context.action.phase == InputActionPhase.Performed )
        {
            camDirection = context.ReadValue<Vector2>();
        }
        if(context.action.phase == InputActionPhase.Canceled)
        {
            camDirection = Vector2.zero;
        }
    }

    void Update()
    {
        if(camDirection != Vector2.zero)
        {
            mainCamera.transform.position += new Vector3(camDirection.x,camDirection.y,0) * cameraSpeed * Time.deltaTime;
        }
    }
}
