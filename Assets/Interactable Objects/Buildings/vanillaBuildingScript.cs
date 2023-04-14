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

    public override void HandleMessage(string flag, string value)
    {
        if(IsServer && flag == "VANILLAOBJ")
        {
            spawn = bool.Parse(value);
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
                            }
                        }
                    }
                    spawn = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    public void SpawnVanilla()
    {
        SendCommand("VANILLAOBJ", true.ToString());
    }
}
