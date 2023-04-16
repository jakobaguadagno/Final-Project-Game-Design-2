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
    public Animator MyAnime;
    private bool clientDeath = false;

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
        if(flag == "DEATH")
        {
            if(IsClient)
            {
                clientDeath = true;
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
                if(unitHealth>0)
                {
                    isAlive = true;
                }
                else
                {
                    if(isAlive)
                    {
                        clientDeath = true;
                        SendUpdate("DEATH", clientDeath.ToString());
                        StartCoroutine(DeathDelay(3f));
                    }
                    isAlive = false;
                }
                if(IsDirty)
                {
                    SendUpdate("UNITHEALTH", unitHealth.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    void Start()
    {
        MyAnime = GetComponent<Animator>();
    }

    public void Update()
    {
        if(IsClient || IsServer)
        {
            if(MyAnime==null)
            {
                MyAnime = GetComponent<Animator>();
            }
        }
        if(IsClient || IsServer)
        {
            if(clientDeath && (MyAnime!=null))
            {
                MyAnime.SetTrigger("death");
                clientDeath=false;
            }
        }
    }



    public void TakeDamage(int d)
    {
        if(IsServer)
        {
            unitHealth -= d;
            UnitHealth.value = unitHealth;
            SendUpdate("UNITHEALTH", unitHealth.ToString());
        }
    }

    public IEnumerator DeathDelay(float s)
    {
        yield return new WaitForSeconds(s);
        MyCore.NetDestroyObject(gameObject.GetComponent<NetworkID>().NetId);
        yield break;
    }
   

    
}
