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
    public RectTransform vanillaBuildPanel;
    public int woodCost = 0;
    public int ironCost = 10;
    public int goldCost = 10;

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

    public override void HandleMessage(string flag, string value)
    {
        if(IsServer && flag == "BUILD")
        {
            worldPos2D = ParseV2(value);
            buildingOwner = ParseOAV2(value);
            Debug.Log("Server Build Information: " + "\nWorld Position: " + worldPos2D + "\nBuilding Owner: " + buildingOwner);
            buildServer = true;
        }
    }

    public override void NetworkedStart()
    {
        buildBoxOutLine = gameObject.transform.GetChild(4).GetChild(0).GetComponent<Image>();
        vanillaBuildPanel = gameObject.transform.GetChild(3).GetChild(4).GetComponent<RectTransform>();
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
                    PlayerCharacter[] allP = FindObjectsOfType<PlayerCharacter>();
                    foreach(PlayerCharacter pc in allP)
                    {
                        if(pc.GetComponent<NetworkComponent>().Owner == buildingOwner)
                        {
                            if((pc.playerWood>=woodCost)&&(pc.playerIron>=ironCost)&&(pc.playerGold>=goldCost))
                            {
                                CreateObjectAt(worldPos2D, buildingOwner, 9, pc);
                            }
                        }
                    }
                    
                    buildServer = false;
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
            BuildUpdate();
        }
    }

    

    public void BuildingCreator()
    {
        buildingEnabled = false;
        buildingEnabled = true;
        TurnOffAllUI();
        buildBoxOutLine.gameObject.SetActive(true);
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
        }
    }

    private void BuildFalse()
    {
        buildUI=false;
        buildingUIF = false;
        buildSendCommand = false;
        buildingEnabled = false;
        build = false;
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
    public void TurnUIOnVanilla()
    {
        vanillaBuildPanel.gameObject.SetActive(true);
    }

    private void TurnOffAllUI()
    {
        if(buildBoxOutLine.gameObject != null)
        {
            buildBoxOutLine.gameObject.SetActive(false);
        }
        if(vanillaBuildPanel.gameObject != null)
        {
            vanillaBuildPanel.gameObject.SetActive(false);
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
                    SendCommand("BUILD", worldPos2D.ToString() + ", " + MyCore.LocalPlayerId.ToString());
                    TurnOffAllUI();
                    buildSendCommand = true;
                }
                BuildFalse();
            }
            if(!buildUI)
            {
                RectTransform rectTransform = buildBoxOutLine.GetComponent<RectTransform>();
                rectTransform.sizeDelta = overlapBoxSize;
                buildBoxOutLine.gameObject.SetActive(true);
                buildingUIF = true;
                buildUI = true;
            }
    }
}
