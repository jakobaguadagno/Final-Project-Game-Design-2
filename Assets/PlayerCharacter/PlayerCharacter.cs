using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class PlayerCharacter : NetworkComponent
{
    public Text PlayerLabel;
    public Text PlayerScore;
    public string playerName = "Blank";
    public int playerScore = 0;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "NAME")
        {
            playerName = value;
            PlayerLabel.text = playerName;
        }
        if(flag == "SCORE")
        {
            playerScore = int.Parse(value);
            PlayerScore.text = playerScore.ToString();
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
                if(IsDirty)
                {
                    SendUpdate("NAME", playerName);
                    SendUpdate("SCORE", playerScore.ToString());
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }
    public void EnableCanvasES()
    {
        if(IsLocalPlayer)
        {
            this.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
