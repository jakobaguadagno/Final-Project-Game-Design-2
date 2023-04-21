using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class cameraControlScript : MonoBehaviour
{

    private Vector2 camDirection = Vector2.zero;
    public Camera mainCamera;
    public InputActionAsset MyMap;
    public float cameraSpeed = 5f;
    public GameObject disconnectUI;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public LobbyManagerScript[] lbm;
    public NetworkCore core;
    private bool spawnLocFound = false;
    private int spawnNumber = 0;

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

    void Start()
    {
        if(disconnectUI != null)
        {
            disconnectUI.SetActive(false);
        }
    }

    void Update()
    {
        if(!spawnLocFound)
        {
            core = GameObject.FindObjectOfType<NetworkCore>();
            {
                if(core != null)
                {
                    if(core.IsServer)
                    {
                        Debug.Log("Server");
                        transform.position = new Vector3(GameObject.Find("Spawn 1").transform.position.x,GameObject.Find("Spawn 1").transform.position.y,-5);
                        spawnLocFound = true;
                    }
                    if(core.IsClient)
                    {
//                        Debug.Log("Client");
                        lbm = GameObject.FindObjectsOfType<LobbyManagerScript>();
                        foreach(LobbyManagerScript l in lbm)
                        {
                            if(l.GetComponent<NetworkID>()!=null)
                            {
                                if(l.GetComponent<NetworkID>().IsLocalPlayer)
                                {
                                    spawnNumber = l.GetComponent<NetworkID>().Owner;
                                    switch(spawnNumber)
                                    {
                                        case 0:
                                            transform.position = new Vector3(GameObject.Find("Spawn 1").transform.position.x,GameObject.Find("Spawn 1").transform.position.y,-5);
                                            
                                            break;
                                        case 1:
                                            transform.position = new Vector3(GameObject.Find("Spawn 2").transform.position.x,GameObject.Find("Spawn 2").transform.position.y,-5);
                                            break;
                                        case 2:
                                            transform.position = new Vector3(GameObject.Find("Spawn 3").transform.position.x,GameObject.Find("Spawn 3").transform.position.y,-5);
                                            break;
                                        case 3:
                                            transform.position = new Vector3(GameObject.Find("Spawn 4").transform.position.x,GameObject.Find("Spawn 4").transform.position.y,-5);
                                            break;
                                        default:
                                            break;
                                    }
                                    spawnLocFound = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        if(disconnectUI == null)
        {
            if(SceneManager.GetActiveScene().buildIndex == 2)
            {
                disconnectUI = GameObject.Find("LanNetworkManager");
                disconnectUI = disconnectUI.transform.GetChild(0).GetChild(1).gameObject;
            }
            if(SceneManager.GetActiveScene().buildIndex == 1)
            {
                disconnectUI = GameObject.Find("WANNetworkManager");
                disconnectUI = disconnectUI.transform.GetChild(0).GetChild(1).gameObject;
            }
            
            Debug.Log(disconnectUI.name);
            disconnectUI.SetActive(false);
        }
        if(camDirection != Vector2.zero)
        {
            mainCamera.transform.position += new Vector3(camDirection.x,camDirection.y,0) * cameraSpeed * Time.deltaTime;
        }
        Vector3 currentPosition = transform.position;
        float clampedX = Mathf.Clamp(currentPosition.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(currentPosition.y, minBounds.y, maxBounds.y);
        transform.position = new Vector3(clampedX, clampedY, currentPosition.z);
    }

    public void DisconnectUIToggle()
    {
        if(disconnectUI != null)
        {
            if(disconnectUI.activeSelf)
            {
                disconnectUI.SetActive(false);
            }
            else
            {
                disconnectUI.SetActive(true);
            }
        }
    }
    
}
