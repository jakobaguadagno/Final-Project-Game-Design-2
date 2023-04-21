using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;


public class LobbyManagerScript : NetworkComponent
{
    public string playerName;
    public int character;
    public bool ready;
    

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "READY")
        {
            ready = bool.Parse(value);
            if(IsServer)
            {
                SendUpdate("READY", value);
            }
        }

        if(flag == "NAME")
        {
            playerName = value;
            if(IsServer)
            {
                SendUpdate("NAME", value);
            }
        }

        if(flag == "CHARACTER")
        {
            character = int.Parse(value);
            if(IsServer)
            {
                SendUpdate("CHARACTER", value);
            }
        }
    }

    public override void NetworkedStart()
    {
        
       if(!IsLocalPlayer)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void ToggleReady(bool r)
    {
        if(IsLocalPlayer)
        {
            SendCommand("READY", r.ToString());
        }
    }

    public void DisableCanvasLMS()
    {
        if(IsLocalPlayer)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void SetCharacterSelect(int c)
    {
        if(IsLocalPlayer)
        {
            SendCommand("CHARACTER", c.ToString());
        }
    }

    public void SetPlayerName(string n)
    {
        if(IsLocalPlayer)
        {
            string myString = n;
            if(myString.Length>=66)
            {
                myString.Substring(0, 65);
            }
            SendCommand("NAME", myString);
        }

    }


    public override IEnumerator SlowUpdate()
    {
        while(IsConnected)
        {
            if(IsServer)
            {

                if(IsDirty)
                {
                    SendUpdate("NAME", playerName);
                    SendUpdate("CHARACTER", character.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }
}
