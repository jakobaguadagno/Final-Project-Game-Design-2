using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.SceneManagement;

public class GameMasterScript : NetworkComponent
{
    public bool GameStarted = false;
    public LobbyManagerScript[] lobbyManager;
    public GenericCore_Web[] serverManager;
    public bool paused = true;
    public bool end = true;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "GAMESTART")
        {
            GameStarted = bool.Parse(value);
            if(GameStarted && IsClient)
            {
                lobbyManager = GameObject.FindObjectsOfType<LobbyManagerScript>();
                foreach (LobbyManagerScript player in lobbyManager)
                {
                    player.DisableCanvasLMS();
                }

                PlayerCharacter[] playerS = GameObject.FindObjectsOfType<PlayerCharacter>();

                foreach (PlayerCharacter player in playerS)
                {
                    player.EnableCanvasIG();
                }
            }
        }
        if(flag == "ENDSCREEN")
        {
            paused = bool.Parse(value);
            if(!paused && IsClient)
            {
                PlayerCharacter[] playerS = GameObject.FindObjectsOfType<PlayerCharacter>();

                foreach (PlayerCharacter player in playerS)
                {
                    player.EnableCanvasES();
                }
            }
        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {

            GameObject lan = GameObject.Find("LanNetworkManager");
            lan.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);

            serverManager = GameObject.FindObjectsOfType<GenericCore_Web>();

            int[] pScore = new int[4];
            for(int i = 0; i<4; i++)
            {
                pScore[i] = Random.Range(1,10000);
            }
    
            while (!GameStarted)
            {
                lobbyManager = GameObject.FindObjectsOfType<LobbyManagerScript>();
                if(MyCore.Connections.Count == lobbyManager.Length)
                {
                    if (MyCore.Connections.Count >= 1)
                    {
                        GameStarted = true;
                        foreach (LobbyManagerScript player in lobbyManager)
                        {
                            if (!player.ready)
                            {
                                GameStarted = false;
                                break;
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(1);
            }

            lobbyManager = GameObject.FindObjectsOfType<LobbyManagerScript>();
            foreach (LobbyManagerScript player in lobbyManager)
            {
                GameObject temp;
                PlayerCharacter pc;
                switch (player.Owner)
                {
                    case 0:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 1").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        MyCore.NetCreateObject(5, 0, GameObject.Find("Test").transform.position);
                        MyCore.NetCreateObject(5, 0, GameObject.Find("Test 2").transform.position);
                        //pc.ColorSelected = player.colorSelect;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    case 1:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 2").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    case 2:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 3").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    case 3:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 4").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    default:
                        break;
                }
            }

            Debug.Log("Ready");

            //Game Start

            SendUpdate("GAMESTART", GameStarted.ToString());

            //Game Timer

            IEnumerator pausePlay = PauseGame(5f);

            StartCoroutine(pausePlay);

            while(paused)
            {
                yield return new WaitForSeconds(.1f);
            }

            StopCoroutine(pausePlay);
            Debug.Log("Unpaused");

            //End Screen

            PlayerCharacter[] playerS = GameObject.FindObjectsOfType<PlayerCharacter>();
            int count = 0;

            foreach (PlayerCharacter player in playerS)
            {
                player.playerScore = pScore[count];
                count++;
            }

            Debug.Log("End Screen");
            SendUpdate("ENDSCREEN", paused.ToString());

            //End Screen Logic

            IEnumerator endScreen = EndGame(5f);

            StartCoroutine(endScreen);

            while(end)
            {
                yield return new WaitForSeconds(.1f);
            }

            StopCoroutine(endScreen);

            //Server Shutdown

            foreach (GenericCore_Web server in serverManager)
            {
                StartCoroutine(server.DisconnectServer());
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    public IEnumerator PauseGame(float s)
    {
        yield return new WaitForSeconds(s);
        //paused = false;
    }

    public IEnumerator EndGame(float s)
    {
        yield return new WaitForSeconds(s);
        end = false;
    }
}
