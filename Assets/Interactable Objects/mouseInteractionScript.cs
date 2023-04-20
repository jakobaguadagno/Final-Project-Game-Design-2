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
    private bool alternateSpace = false;
    private float unitSpacing = .25f;
    
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
                        SendUpdate("REMOVESELECTUNITUI", value);
                        Debug.Log("Removing unit: " + id.gameObject.name);
                        int count = 0;
                        foreach(GameObject obj in selectedUnits.ToArray())
                        {
                            count++;
//                          Debug.Log("Unit " + count + ": " + obj.name);
                        }
                        if(count == 0)
                        {
//                            Debug.Log("No units in the list.");
                        }
                    }
                    else
                    {
                        selectedUnits.Add(id.gameObject);
                        SendUpdate("SELECTUNITUI", value);
//                        Debug.Log("Adding unit: " + id.gameObject.name);
                        int count = 0;
                        foreach(GameObject obj in selectedUnits.ToArray())
                        {
                            count++;
//                            Debug.Log("Unit " + count + ": " + obj.name);
                        }
                        if(count == 0)
                        {
//                           Debug.Log("No units in the list.");
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
                NetworkID tempR = su.GetComponent<NetworkID>();
                if(tempR != null)
                {
                    int tempInt = tempR.NetId;
                    SendUpdate("REMOVESELECTUNITUI", tempInt.ToString());
                }
                
            }
        }
        if(IsClient && flag == "SELECTUNITUI")
        {
            NetworkID[] temp = GameObject.FindObjectsOfType<NetworkID>();
            if(temp!=null)
            {
                foreach(NetworkID id in temp)
                {
                    if(id.NetId == int.Parse(value))
                    {
                        vanillaCharacterScript unitTemp = id.gameObject.GetComponent<vanillaCharacterScript>();
                        if(unitTemp!=null)
                        {
                            unitTemp.TurnOnSelectedUI();
                        }
                    }
                }
            }
        }
        if(IsClient && flag == "REMOVESELECTUNITUI")
        {
            NetworkID[] temp = GameObject.FindObjectsOfType<NetworkID>();
            if(temp!=null)
            {
                foreach(NetworkID id in temp)
                {
                    if(id.NetId == int.Parse(value))
                    {
                        vanillaCharacterScript unitTemp = id.gameObject.GetComponent<vanillaCharacterScript>();
                        if(unitTemp!=null)
                        {
                            unitTemp.TurnOffSelectedUI();
                        }
                    }
                }
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
                            if(unitSpacing == .25f)
                            {
                                vCS.UnitMove(worldPos2D);
                            }
                            else
                            {
                                vCS.UnitMove(new Vector2 (worldPos2D.x, worldPos2D.y+unitSpacing));
                            }
                            if(unitSpacing > 0)
                            {
                                unitSpacing += .25f;
                            }
                            unitSpacing *= -1f;
                        }
                        else
                        {
                            selectedUnits.Remove(obj);
                        }
                    }
                    if(count == 0)
                    {
//                        Debug.Log("Nothing to do.");
                    }
                    unitSpacing = .25f;
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
//                                Debug.Log("Nothing to do.");
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
//                            Debug.Log("Nothing to do.");
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
//                                Debug.Log("Nothing to do.");
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
                

//                Debug.Log("Unit's Name: " + selectedObject.name);
//                Debug.Log("Unit's Owner: " + netIDObject.Owner);

                if(netIDObject.Owner == MyCore.LocalPlayerId)
                {
                    SendCommand("SELECTUNIT", netIDObject.NetId.ToString());
                }
                else
                {
                    Debug.Log("Unit " + selectedObject.name + " does not belong to you.");
                }
            }
            if (hit.collider != null && (hit.collider.gameObject.CompareTag("Building") || hit.collider.gameObject.CompareTag("Player")))
            {

                NetworkCore MyCore = GameObject.FindObjectOfType<NetworkCore>();
                NetworkID netIDObject = hit.collider.gameObject.GetComponent<NetworkID>();
                GameObject selectedObject = hit.collider.gameObject;
                

//                Debug.Log("Building's Name: " + selectedObject.name);
//                Debug.Log("Building's Owner: " + netIDObject.Owner);

                if(netIDObject.Owner == MyCore.LocalPlayerId)
                {
                    if(selectedObject.GetComponent<vanillaBuildingScript>() != null)
                    {
                        gameObject.GetComponent<buildingScript>().TurnUIOnVanilla();
                        buildingTarget = selectedObject;
                    }
                    else if(selectedObject.GetComponent<archerBuildScript>() != null)
                    {
                        gameObject.GetComponent<buildingScript>().TurnOffAllUICreate();
                        gameObject.GetComponent<buildingScript>().TurnUIOnArcher();
                        gameObject.GetComponent<buildingScript>().TurnUIOnArcherCreate();
                        buildingTarget = selectedObject;
                    }
                    else if(selectedObject.GetComponent<horseBuildScript>() != null)
                    {
                        gameObject.GetComponent<buildingScript>().TurnOffAllUICreate();
                        gameObject.GetComponent<buildingScript>().TurnUIOnHorse();
                        gameObject.GetComponent<buildingScript>().TurnUIOnHorseCreate();
                        buildingTarget = selectedObject;
                    }
                    else if(selectedObject.GetComponent<swordsManBuildScript>() != null)
                    {
                        gameObject.GetComponent<buildingScript>().TurnOffAllUICreate();
                        gameObject.GetComponent<buildingScript>().TurnUIOnSwordsman();
                        gameObject.GetComponent<buildingScript>().TurnUIOnSwordsmanCreate();
                        buildingTarget = selectedObject;
                    }
                    else if(selectedObject.GetComponent<villagerBuildScript>() != null)
                    {
                        gameObject.GetComponent<buildingScript>().TurnOffAllUICreate();
                        gameObject.GetComponent<buildingScript>().TurnUIOnVillager();
                        gameObject.GetComponent<buildingScript>().TurnUIOnVillagerCreate();
                        buildingTarget = selectedObject;
                    }
                    else
                    {
//                        Debug.Log("No script attached");
                    }
                }
                else
                {
//                    Debug.Log("Building " + selectedObject.name + " does not belong to you.");
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
//                Debug.Log("Attack/Mine!");
                SendCommand("ATTACKUNIT", netIDObject.NetId.ToString());
            }
            else
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z));
                worldPos2D = new Vector2 (worldPos.x, worldPos.y);
//                Debug.Log("Move!");
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
        if(buildingTarget.GetComponent<archerBuildScript>() != null)
        {
            buildingTarget.GetComponent<archerBuildScript>().SpawnArcher();
        }
        if(buildingTarget.GetComponent<horseBuildScript>() != null)
        {
            buildingTarget.GetComponent<horseBuildScript>().SpawnHorse();
        }
        if(buildingTarget.GetComponent<swordsManBuildScript>() != null)
        {
            buildingTarget.GetComponent<swordsManBuildScript>().SpawnSwordsman();
        }
        if(buildingTarget.GetComponent<villagerBuildScript>() != null)
        {
            buildingTarget.GetComponent<villagerBuildScript>().SpawnVillager();
        }
    }

    public void ClearSelectedUnits()
    {
        SendCommand("CLEARUNIT", true.ToString());
    }

}