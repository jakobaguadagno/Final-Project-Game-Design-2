using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;

public class vanillaBuildingScript : NetworkComponent
{

    public GameObject spawnPoint;
    public GameObject spawnObject;
    public int woodCost = 10;
    public int ironCost = 0;
    public int goldCost = 0;
    private bool spawn = false;
    private PlayerCharacter leader = null;
    private bool serverSetTeam = false;
    private bool clientUnitColorSet = false;
    private Color unitColor = new Color(1,1,1,1);

    public Color ParseCV4(string v)
    {
        Color temp = new Color();
        string[] args = v.Trim('(').Trim(')').Split(',');
        //Debug.Log("ARGS: " + args[0] + " / " + args[1] + " / " + args[2] + " / " + args[3]);
        temp.r = float.Parse(args[0]);
        //Debug.Log("1: " + temp.r);
        temp.g = float.Parse(args[1]);
        //Debug.Log("2: " + temp.g);
        temp.b = float.Parse(args[2]);
        //Debug.Log("3: " + temp.b);
        temp.a = float.Parse(args[3]);
        //Debug.Log("4: " + temp.a);
        return temp;
    }

    public override void HandleMessage(string flag, string value)
    {
        if(IsServer && flag == "VANILLAOBJ")
        {
            spawn = bool.Parse(value);
        }
        if(IsClient && flag == "UNITCOLOR")
        {
            unitColor = ParseCV4(value);
            clientUnitColorSet = true;
        }
    }

    public override void NetworkedStart()
    {
        if(IsServer)
        {
            if(leader == null)
            {
                PlayerCharacter[] temp = FindObjectsOfType<PlayerCharacter>();
                foreach (PlayerCharacter tempLead in temp)
                {
                    if(tempLead.gameObject.GetComponent<NetworkID>().Owner == gameObject.GetComponent<NetworkID>().Owner)
                    {
                        leader = tempLead;
                    }
                }
            }
            else
            {
                if(!serverSetTeam)
                {
                    if(gameObject.GetComponent<SpriteRenderer>()!=null)
                    {
                        gameObject.GetComponent<SpriteRenderer>().color = leader.teamColor;
                        SendUpdate("UNITCOLOR", leader.teamColor.r.ToString() + ", " + leader.teamColor.g.ToString() + ", " + leader.teamColor.b.ToString() + ", " + leader.teamColor.a.ToString());
                        serverSetTeam = true;
                    }
                }
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while(IsConnected)
        {
            if(IsServer)
            {
                if(leader == null)
                {
                    PlayerCharacter[] temp = FindObjectsOfType<PlayerCharacter>();
                    foreach (PlayerCharacter tempLead in temp)
                    {
                        if(tempLead.gameObject.GetComponent<NetworkID>().Owner == gameObject.GetComponent<NetworkID>().Owner)
                        {
                            leader = tempLead;
                        }
                    }
                }
                else
                {
                    if(!serverSetTeam)
                    {
                        if(gameObject.GetComponent<SpriteRenderer>()!=null)
                        {
                            gameObject.GetComponent<SpriteRenderer>().color = leader.teamColor;
                            SendUpdate("UNITCOLOR", leader.teamColor.r.ToString() + ", " + leader.teamColor.g.ToString() + ", " + leader.teamColor.b.ToString() + ", " + leader.teamColor.a.ToString());
                            serverSetTeam = true;
                        }
                    }
                }
                if(spawn)
                {
                    PlayerCharacter[] allP = FindObjectsOfType<PlayerCharacter>();
                    foreach(PlayerCharacter pc in allP)
                    {
                        if(pc.GetComponent<NetworkComponent>().Owner == gameObject.GetComponent<NetworkComponent>().Owner)
                        {
                            if((pc.playerWood>=woodCost)&&(pc.playerIron>=ironCost)&&(pc.playerGold>=goldCost))
                            {
                                MyCore.NetCreateObject(spawnObject.GetComponent<NetworkID>().Type, gameObject.GetComponent<NetworkComponent>().Owner, spawnPoint.transform.position);
                                pc.RemoveResources(woodCost, ironCost, goldCost);
                                pc.AddScore(10);
                            }
                        }
                    }
                    spawn = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    void Start()
    {
        if(IsClient)
        {
            if(leader != null)
            {
                SpriteRenderer temp = gameObject.GetComponent<SpriteRenderer>();
                if(temp!=null)
                {
                    temp.color = leader.teamColor;
                }
            }
        }
        
    }

    void Update()
    {
        if(IsClient)
        {
            if(clientUnitColorSet)
            {
                Debug.Log("Color Loop");
                SpriteRenderer temp = gameObject.GetComponent<SpriteRenderer>();
                if(temp!=null)
                {
                    temp.color = unitColor;
                    Debug.Log("Unit Color: " + unitColor);
                    Debug.Log("Sprite Color: " + temp);
                    clientUnitColorSet = false;
                }
            }
        }
    }

    public void SpawnVanilla()
    {
        SendCommand("VANILLAOBJ", true.ToString());
        Debug.Log("Vanilla Spawn");
    }
}
