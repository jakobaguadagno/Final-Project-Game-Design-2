using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.UI;

public class healthScript : NetworkComponent
{

    public bool isAlive = true;
    private int unitHealth = 100;
    public Slider UnitHealth;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "UNITHEALTH")
        {
            unitHealth = int.Parse(value);
            if(IsClient)
            {
                Debug.Log("Unit Health Client: " + unitHealth);
            }
            UnitHealth.value = unitHealth;
            if(IsServer)
            {
                SendUpdate("UNITHEALTH", value);
            }
        }
    }

    public override void NetworkedStart()
    {
        if(IsDirty)
        {
            SendUpdate("UNITHEALTH", unitHealth.ToString());
            IsDirty = false;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
    }

    public void TakeDamage(int d)
    {
        if(IsServer)
        {
            if(unitHealth>0)
            {
                unitHealth -= d;
                UnitHealth.value = unitHealth;
                isAlive = true;
            }
            else
            {
                isAlive = false;
                MyCore.NetDestroyObject(gameObject.GetComponent<NetworkID>().NetId);
            }
            SendUpdate("UNITHEALTH", unitHealth.ToString());
        }
    }
}
