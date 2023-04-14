using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.InputSystem;

public class mouseInteractionScript : NetworkComponent
{

    public List<GameObject> selectedUnits = new List<GameObject>();
    private Vector2 mousePosition;
    private Vector2 worldPos2D;
    private GameObject unitTarget;
    private bool movingObject = false;
    private bool attackingObject = false;
    private bool DebugLog = false;
    private GameObject buildingTarget;
    
    public Vector2 ParseV2(string v)
    {
        Vector2 temp = new Vector2();
        string[] args = v.Trim('(').Trim(')').Split(',');
        temp.x = float.Parse(args[0]); 
        temp.y = float.Parse(args[1]);
        return temp;
    }

    public override void HandleMessage(string flag, string value)
    {
        if(IsServer && flag == "MOVEUNIT")
        {
            worldPos2D = ParseV2(value);
            movingObject = true;
            attackingObject = false;
        }
        if(IsServer && flag == "ATTACKUNIT")
        {
            NetworkID[] temp = GameObject.FindObjectsOfType<NetworkID>();
            foreach(NetworkID id in temp)
            {
                if(id.NetId == int.Parse(value))
                {
                    unitTarget = id.gameObject;
                }
            }
            movingObject = false;
            attackingObject = true;
        }
        if(IsServer && flag == "SELECTUNIT")
        {
            NetworkID[] temp = GameObject.FindObjectsOfType<NetworkID>();
            foreach(NetworkID id in temp)
            {
                if(id.NetId == int.Parse(value))
                {
                    if (selectedUnits.Contains(id.gameObject))
                    {
                        selectedUnits.Remove(id.gameObject);
                        Debug.Log("Removing unit: " + id.gameObject.name);
                        int count = 0;
                        foreach(GameObject obj in selectedUnits.ToArray())
                        {
                            count++;
                            Debug.Log("Unit " + count + ": " + obj.name);
                        }
                        if(count == 0)
                        {
                            Debug.Log("No units in the list.");
                        }
                    }
                    else
                    {
                        selectedUnits.Add(id.gameObject);
                        Debug.Log("Adding unit: " + id.gameObject.name);
                        int count = 0;
                        foreach(GameObject obj in selectedUnits.ToArray())
                        {
                            count++;
                            Debug.Log("Unit " + count + ": " + obj.name);
                        }
                        if(count == 0)
                        {
                            Debug.Log("No units in the list.");
                        }
                    }
                }
            }
        }
        if(IsServer && flag == "CLEARUNIT")
        {
            foreach(GameObject su in selectedUnits)
            {
                selectedUnits.Remove(su);
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected)
        {
            if(IsServer)
            {
                if(movingObject)
                {
                    int count = 0;
                    foreach(GameObject obj in selectedUnits.ToArray())
                    {
                        count++;
                        if((obj != null)&&(obj.GetComponent<vanillaCharacterScript>() != null))
                        {
                            vanillaCharacterScript vCS = obj.GetComponent<vanillaCharacterScript>();
                            vCS.UnitMove(worldPos2D);
                        }
                        else
                        {
                            selectedUnits.Remove(obj);
                        }
                    }
                    if(count == 0)
                    {
                        Debug.Log("Nothing to do.");
                    }
                    movingObject = false;
                }
                if(attackingObject)
                {
                    //Attacking Player Object
                    if(unitTarget!=null)
                    {
                        if(unitTarget.CompareTag("Player"))
                        {
                            PlayerCharacter temp = unitTarget.GetComponent<PlayerCharacter>();
                            int count = 0;
                            foreach(GameObject obj in selectedUnits.ToArray())
                            {
                                count++;
                                if((obj != null)&&(obj.GetComponent<vanillaCharacterScript>() != null))
                                {
                                    vanillaCharacterScript unit = obj.GetComponent<vanillaCharacterScript>();
                                    unit.UnitAttackingPlayer(temp);
                                }
                                else
                                {
                                    selectedUnits.Remove(obj);
                                }
                            }
                            if(count == 0)
                            {
                                Debug.Log("Nothing to do.");
                            }
                            attackingObject=false;
                        }
                    }
                    //"Attacking" Mine Object
                    if(unitTarget.CompareTag("Gold")||unitTarget.CompareTag("Iron")||unitTarget.CompareTag("Wood") && unitTarget!=null)
                    {
                        int count = 0;
                        foreach(GameObject obj in selectedUnits.ToArray())
                        {
                            count++;
                            if((obj != null)&&(obj.GetComponent<vanillaCharacterScript>() != null))
                            {
                                vanillaCharacterScript unit = obj.GetComponent<vanillaCharacterScript>();
                                unit.UnitMine(unitTarget);
                            }
                            else
                            {
                                selectedUnits.Remove(obj);
                            }
                        }
                        if(count == 0)
                        {
                            Debug.Log("Nothing to do.");
                        }
                        attackingObject=false;
                    }
                    //Attacking Unit
                    if(unitTarget.CompareTag("Unit") && unitTarget!=null)
                    {
                        if(unitTarget.GetComponent<healthScript>() != null)
                        {
                            healthScript target = unitTarget.GetComponent<healthScript>();
                            int count = 0;
                            foreach(GameObject obj in selectedUnits.ToArray())
                            {
                                count++;
                                if((obj != null)&&(obj.GetComponent<vanillaCharacterScript>() != null))
                                {
                                    vanillaCharacterScript unit = obj.GetComponent<vanillaCharacterScript>();
                                    unit.UnitAttackingHSUnit(target);
                                }
                                else
                                {
                                    selectedUnits.Remove(obj);
                                }
                            }
                            if(count == 0)
                            {
                                Debug.Log("Nothing to do.");
                            }
                            attackingObject=false;
                        }
                    }
                    attackingObject=false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Unit"))
            {

                NetworkCore MyCore = GameObject.FindObjectOfType<NetworkCore>();
                NetworkID netIDObject = hit.collider.gameObject.GetComponent<NetworkID>();
                GameObject selectedObject = hit.collider.gameObject;
                

                Debug.Log("Unit's Name: " + selectedObject.name);
                Debug.Log("Unit's Owner: " + netIDObject.Owner);

                if(netIDObject.Owner == MyCore.LocalPlayerId)
                {
                    SendCommand("SELECTUNIT", netIDObject.NetId.ToString());
                }
                else
                {
                    Debug.Log("Unit " + selectedObject.name + " does not belong to you.");
                }
            }
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Building"))
            {

                NetworkCore MyCore = GameObject.FindObjectOfType<NetworkCore>();
                NetworkID netIDObject = hit.collider.gameObject.GetComponent<NetworkID>();
                GameObject selectedObject = hit.collider.gameObject;
                

                Debug.Log("Building's Name: " + selectedObject.name);
                Debug.Log("Building's Owner: " + netIDObject.Owner);

                if(netIDObject.Owner == MyCore.LocalPlayerId)
                {
                    if(selectedObject.GetComponent<vanillaBuildingScript>() != null)
                    {
                        gameObject.GetComponent<buildingScript>().TurnUIOnVanilla();
                        buildingTarget = selectedObject;
                    }
                    else
                    {
                        Debug.Log("No script attached");
                    }
                }
                else
                {
                    Debug.Log("Building " + selectedObject.name + " does not belong to you.");
                }
            }
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null && (hit.collider.gameObject.CompareTag("Player")||hit.collider.gameObject.CompareTag("Unit")||hit.collider.gameObject.CompareTag("Gold")||hit.collider.gameObject.CompareTag("Iron")||hit.collider.gameObject.CompareTag("Wood")))
            {
                NetworkID netIDObject = hit.collider.gameObject.GetComponent<NetworkID>();
                Debug.Log("Attack/Mine!");
                SendCommand("ATTACKUNIT", netIDObject.NetId.ToString());
            }
            else
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z));
                worldPos2D = new Vector2 (worldPos.x, worldPos.y);
                Debug.Log("Move!");
                SendCommand("MOVEUNIT", worldPos2D.ToString("F2"));
            }
        }
    }

    public void SpawnUnit()
    {
        if(buildingTarget.GetComponent<vanillaBuildingScript>() != null)
        {
            buildingTarget.GetComponent<vanillaBuildingScript>().SpawnVanilla();
        }
    }

    public void ClearSelectedUnits()
    {
        SendCommand("CLEARUNIT", true.ToString());
    }

}

/* 

                if (selectedUnits.Contains(selectedObject))
                    {
                        selectedUnits.Remove(selectedObject);
                        Debug.Log("Removing object: " + selectedObject.name);
                        int count = 0;
                        foreach(GameObject obj in selectedUnits)
                        {
                            count++;
                            Debug.Log("Object " + count + ": " + obj.name);
                        }
                        if(count == 0)
                        {
                            Debug.Log("No objects in the list.");
                        }
                    }

                else
                {
                    selectedUnits.Add(selectedObject);
                        Debug.Log("Adding object: " + selectedObject.name);
                        int count = 0;
                        foreach(GameObject obj in selectedUnits)
                        {
                            count++;
                            Debug.Log("Object " + count + ": " + obj.name);
                        }
                        if(count == 0)
                        {
                            Debug.Log("No objects in the list.");
                        }
                }
*/