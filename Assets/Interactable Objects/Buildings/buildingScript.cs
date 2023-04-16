using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class buildingScript : NetworkComponent
{
    public LayerMask noOverlapLayerMask;
    public Vector2 overlapBoxSize, worldPos2D;
    private bool canCreate = true;
    public GameObject buildingPrefab;
    private bool buildingUIF = false;
    private bool buildingEnabled = false;
    private bool build = false;
    private bool buildUI = false;
    private bool buildSendCommand = false;
    private bool buildServer = false;
    private int buildingOwner = -1;
    private Vector3 mousePosition, worldPos;
    public Image buildBoxOutLine;
    public RectTransform vanillaBuildPanel, villagerBuildPanel, horseBuildPanel, swordsmanBuildPanel, archerBuildPanel, villagerCreate, horseCreate, swordsmanCreate, archerCreate;
    public int woodCost = 0;
    public int ironCost = 10;
    public int goldCost = 10;
    public int typeToCreate = -2;
    private bool uICheck = false;

    public Vector2 ParseV2(string v)
    {
        Vector2 temp = new Vector2();
        string[] args = v.Trim('(').Trim(')').Split(',');
        temp.x = float.Parse(args[0]);
        temp.y = float.Parse(args[1].Trim(')'));
        return temp;
    }

    public int ParseOAV2(string v)
    {
        int temp = -1;
        string[] args = v.Trim('(').Trim(')').Split(',');
        temp = int.Parse(args[2]);
        return temp;
    }

    public int ParseTAOAV2(string v)
    {
        int temp = -1;
        string[] args = v.Trim('(').Trim(')').Split(',');
        temp = int.Parse(args[3]);
        return temp;
    }

    public override void HandleMessage(string flag, string value)
    {
        if(IsServer && flag == "BUILD")
        {
            worldPos2D = ParseV2(value);
            buildingOwner = ParseOAV2(value);
            typeToCreate = ParseTAOAV2(value);
            Debug.Log("Server Build Information: " + "\nWorld Position: " + worldPos2D + "\nBuilding Owner: " + buildingOwner + "\nBuilding Type: " + typeToCreate);
            buildServer = true;
        }
    }

    public override void NetworkedStart()
    {
        buildBoxOutLine = gameObject.transform.GetChild(4).GetChild(0).GetComponent<Image>();
        if(!IsLocalPlayer)
        {
            this.transform.GetChild(3).gameObject.SetActive(false);
            this.transform.GetChild(4).gameObject.SetActive(false);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected)
        {
            if(IsServer)
            {
                if(buildServer)
                {
                    if(typeToCreate != -2)
                    {
                        PlayerCharacter[] allP = FindObjectsOfType<PlayerCharacter>();
                        foreach(PlayerCharacter pc in allP)
                        {
                            if(pc.GetComponent<NetworkComponent>().Owner == buildingOwner)
                            {
                                if((pc.playerWood>=woodCost)&&(pc.playerIron>=ironCost)&&(pc.playerGold>=goldCost))
                                {
                                    CreateObjectAt(worldPos2D, buildingOwner, typeToCreate, pc);
                                }
                            }
                        }
                    }
                    buildServer = false;
                    typeToCreate = -2;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
        
    }

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        overlapBoxSize = spriteRenderer.bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        if(buildingEnabled)
        {
            mousePosition = Mouse.current.position.ReadValue();
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z));
            worldPos2D = new Vector2 (worldPos.x, worldPos.y);
            if(buildBoxOutLine.gameObject != null)
            {
                buildBoxOutLine.gameObject.SetActive(true);
            }
            uICheck = false;
            BuildUpdate();
        }
        if(!buildingEnabled)
        {
            if(!uICheck)
            {
                if(buildBoxOutLine.gameObject != null)
                {
                    buildBoxOutLine.gameObject.SetActive(false);
                }
                uICheck = true;
            }
        }
    }

    

    

    public void BuildClick(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            if(buildingEnabled)
            {
                build = true;
            }
        }
    }
    
    public void BuildRClick(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            TurnOffAllUI();
            buildingEnabled = false;
            build = false;
            TurnOnAllUI();
        }
    }

    private void BuildFalse()
    {
        buildUI=false;
        buildingUIF = false;
        buildSendCommand = false;
        buildingEnabled = false;
        build = false;
        TurnOnAllUI();
    }

    public void CreateObjectAt(Vector2 position, int owner, int type, PlayerCharacter pc)
    {
        canCreate = true;
        Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(position, overlapBoxSize*3, 0f, noOverlapLayerMask);
        if (overlappingColliders.Length > 0)
        {
            Debug.Log("Cannot place over object.");
            canCreate = false;
        }
        if (canCreate)
        {
            Debug.Log("Create");
            MyCore.NetCreateObject(type, owner, position);
            pc.RemoveResources(woodCost, ironCost, goldCost);
        }
    }

    private void BuildUpdate()
    {
        if(buildingUIF)
            {
                buildBoxOutLine.transform.position = worldPos2D;
            }
            if(build)
            {
                if(!buildSendCommand)
                {
                    mousePosition = Mouse.current.position.ReadValue();
                    worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z));
                    worldPos2D = new Vector2 (worldPos.x, worldPos.y);
                    TurnOffAllUI();
                    SendCommand("BUILD", worldPos2D.ToString() + ", " + MyCore.LocalPlayerId.ToString() + ", " + typeToCreate);
                    typeToCreate=-2;
                    TurnOffAllUI();
                    buildSendCommand = true;
                }
                BuildFalse();
            }
            if(!buildUI)
            {
                RectTransform rectTransform = buildBoxOutLine.GetComponent<RectTransform>();
                rectTransform.sizeDelta = overlapBoxSize*3;
                buildBoxOutLine.gameObject.SetActive(true);
                buildingUIF = true;
                buildUI = true;
            }
    }

    private void TurnOffAllUI()
    {
        if(buildBoxOutLine.gameObject != null)
        {
            buildBoxOutLine.gameObject.SetActive(false);
        }
        TurnUIOffSwordsman();
        TurnUIOffHorse();
        TurnUIOffArcher();
        TurnUIOffVillager();
        TurnUIOffSwordsmanCreate();
        TurnUIOffHorseCreate();
        TurnUIOffArcherCreate();
        TurnUIOffVillagerCreate();
    }

    private void TurnOnAllUI()
    {
        TurnUIOnSwordsman();
        TurnUIOnHorse();
        TurnUIOnArcher();
        TurnUIOnVillager();
    }

    public void BuildingCreator()
    {
        buildingEnabled = false;
        buildingEnabled = true;
        TurnOffAllUI();
        TurnUIOnVanilla();
    }

    public void VillagerBuildingCreator()
    {
        buildingEnabled = false;
        buildingEnabled = true;
        TurnOffAllUI();
        TurnUIOnVillager();
        typeToCreate = 9;
    }

    public void ArcherBuildingCreator()
    {
        buildingEnabled = false;
        buildingEnabled = true;
        TurnOffAllUI();
        TurnUIOnArcher();
        typeToCreate = 11;
    }

    public void HorseBuildingCreator()
    {
        buildingEnabled = false;
        buildingEnabled = true;
        TurnOffAllUI();
        TurnUIOnHorse();
        typeToCreate = 12;
    }

    public void SwordsmanBuildingCreator()
    {
        buildingEnabled = false;
        buildingEnabled = true;
        TurnOffAllUI();
        TurnUIOnSwordsman();
        typeToCreate = 10;
    }
    
    public void TurnUIOnVillagerCreate()
    {
        if(villagerCreate.gameObject != null)
        {
            villagerCreate.gameObject.SetActive(true);
        }
    }

    public void TurnUIOnArcherCreate()
    {
        if(archerCreate.gameObject != null)
        {
            archerCreate.gameObject.SetActive(true);
        }
    }

    public void TurnUIOnHorseCreate()
    {
        if(horseCreate.gameObject != null)
        {
            horseCreate.gameObject.SetActive(true);
        }
    }

    public void TurnUIOnSwordsmanCreate()
    {
        if(swordsmanCreate.gameObject != null)
        {
            swordsmanCreate.gameObject.SetActive(true);
        }
    }

    public void TurnUIOffVillagerCreate()
    {
        if(villagerCreate.gameObject != null)
        {
            villagerCreate.gameObject.SetActive(false);
        }
    }

    public void TurnUIOffArcherCreate()
    {
        if(archerCreate.gameObject != null)
        {
            archerCreate.gameObject.SetActive(false);
        }
    }

    public void TurnUIOffHorseCreate()
    {
        if(horseCreate.gameObject != null)
        {
            horseCreate.gameObject.SetActive(false);
        }
    }

    public void TurnUIOffSwordsmanCreate()
    {
        if(swordsmanCreate.gameObject != null)
        {
            swordsmanCreate.gameObject.SetActive(false);
        }
    }

    public void TurnUIOnVanilla()
    {
        if(vanillaBuildPanel.gameObject != null)
        {
            vanillaBuildPanel.gameObject.SetActive(true);
        }
    }

    public void TurnUIOnVillager()
    {
        if(villagerBuildPanel.gameObject != null)
        {
            villagerBuildPanel.gameObject.SetActive(true);
        }
    }

    public void TurnUIOnArcher()
    {
        if(archerBuildPanel.gameObject != null)
        {
            archerBuildPanel.gameObject.SetActive(true);
        }
    }

    public void TurnUIOnHorse()
    {
        if(horseBuildPanel.gameObject != null)
        {
            horseBuildPanel.gameObject.SetActive(true);
        }
    }

    public void TurnUIOnSwordsman()
    {
        if(swordsmanBuildPanel.gameObject != null)
        {
            swordsmanBuildPanel.gameObject.SetActive(true);
        }
    }
    
    public void TurnUIOffVanilla()
    {
        if(vanillaBuildPanel.gameObject != null)
        {
            vanillaBuildPanel.gameObject.SetActive(false);
        }
    }

    public void TurnUIOffVillager()
    {
        if(villagerBuildPanel.gameObject != null)
        {
            villagerBuildPanel.gameObject.SetActive(false);
        }
    }

    public void TurnUIOffArcher()
    {
        if(archerBuildPanel.gameObject != null)
        {
            archerBuildPanel.gameObject.SetActive(false);
        }
    }

    public void TurnUIOffHorse()
    {
        if(horseBuildPanel.gameObject != null)
        {
            horseBuildPanel.gameObject.SetActive(false);
        }
    }

    public void TurnUIOffSwordsman()
    {
        if(swordsmanBuildPanel.gameObject != null)
        {
            swordsmanBuildPanel.gameObject.SetActive(false);
        }
    }
}
