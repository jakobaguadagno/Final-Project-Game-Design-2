using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMasterScript : NetworkComponent
{
    public bool GameStarted = false;
    public LobbyManagerScript[] lobbyManager;
    public GenericCore_Web[] serverManager;
    public GameObject endGameScreen;
    public AudioSource audioSource;
    public AudioClip endOfGame;
    public AudioClip buttonPlay;
    public AudioClip gameMusic;
    public volumeScript userSound;
    public Text egsPlayer1Score;
    public Text egsPlayer2Score;
    public Text egsPlayer3Score;
    public Text egsPlayer4Score;
    public GameObject[] playerScoreUI;
    public GameObject[] winnerUI;
    public bool paused = true;
    public bool end = true;
    public int[] playerScores = {0,0,0,0};
    public int[] playerScoresActiveCheck = {0,0,0,0};
    public bool[] playerAlive = {false,false,false,false};
    private int playerCount;
    private bool endTime = false;
    private bool endScreenHidden = false;
    private bool endScreenSet = false;
    private bool setAudioStart = false;
    private bool setAudioMid = false;
    private bool setAudioEnd = false;
    private bool setSound = false;
    public int winner = -1;

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
                audioSource.PlayOneShot(buttonPlay);
            }
        }
        if(flag == "ENDSCREEN")
        {
            if(!paused)
            {
                paused = false;
            }
            endTime = true;
            Debug.Log(endTime);
            if(IsClient)
            {
                Debug.Log(value);
                string[] args = value.Split(',');
                playerScores[0] = int.Parse(args[0]); 
                playerScores[1] = int.Parse(args[1]);
                playerScores[2] = int.Parse(args[2]);
                playerScores[3] = int.Parse(args[3]);
                winner = int.Parse(args[4]);
                if(winner>=0||winner<=3)
                {
                    winnerUI[winner].SetActive(true);
                }
                Debug.Log(playerScores[0].ToString() + ", " + playerScores[1].ToString() + ", " + playerScores[2].ToString() + ", " + playerScores[3].ToString());
                Debug.Log("Winner: " + winner);
            }
        }
        if(flag == "PLAYERSCOREUI" && IsClient)
        {
//            Debug.Log(value);
            string[] args = value.Split(',');
            playerScoresActiveCheck[0] = int.Parse(args[0]); 
            playerScoresActiveCheck[1] = int.Parse(args[1]);
            playerScoresActiveCheck[2] = int.Parse(args[2]);
            playerScoresActiveCheck[3] = int.Parse(args[3]);
            int count = 0;
            foreach(int p in playerScoresActiveCheck)
            {
                if(p == 1)
                {
                    playerScoreUI[count].SetActive(true);
                }
                count++;
            }
            
        }
    }

    void Start()
    {
        userSound = GameObject.FindObjectOfType<volumeScript>();
    }

    void Update()
    {
        if(userSound == null)
        {
//            Debug.Log("Finding Sound");
            userSound = GameObject.FindObjectOfType<volumeScript>();
        }
        if(userSound != null && !setSound)
        {
            audioSource.volume = userSound.volume/100;
            setSound = true;
        }
        if(userSound != null && audioSource.volume != (userSound.volume/100))
        {
            audioSource.volume = userSound.volume/100;
        }
        if(IsServer || IsClient)
        {
            if(endTime && endGameScreen!=null && !endScreenSet)
            {
                Debug.Log("EndTime");
                egsPlayer1Score.text = "Player 1: " + playerScores[0];
                egsPlayer2Score.text = "Player 2: " + playerScores[1];
                egsPlayer3Score.text = "Player 3: " + playerScores[2];
                egsPlayer4Score.text = "Player 4: " + playerScores[3];
                endGameScreen.SetActive(true);
                endScreenSet = true;
            }
        }
        if(IsClient)
        {
            if(!GameStarted)
            {
                if(!setAudioStart)
                {
                        Debug.Log("Start Song");
                        audioSource.clip = endOfGame;
                        audioSource.loop = true;
                        audioSource.Play();
                        setAudioStart = true;
                }
            }
            if(GameStarted && !endTime)
            {
                if(!setAudioMid)
                {
                        Debug.Log("Mid Song");
                        audioSource.clip = gameMusic;
                        audioSource.loop = true;
                        audioSource.Play();
                        setAudioMid = true;
                }
            }
            if(endTime)
            {
                if(!setAudioEnd)
                {
                        Debug.Log("End Song");
                        audioSource.clip = endOfGame;
                        audioSource.loop = true;
                        audioSource.Play();
                        setAudioEnd = true;
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
    
            while (!GameStarted)
            {
                lobbyManager = GameObject.FindObjectsOfType<LobbyManagerScript>();
                if(MyCore.Connections.Count == lobbyManager.Length)
                {
                    if (MyCore.Connections.Count > 1)
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
                playerCount++;
                GameObject temp;
                PlayerCharacter pc;
                switch (player.Owner)
                {
                    case 0:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 1").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        pc.teamColor = new Color(1,0,0,1);
                        playerAlive[0] = true;
                        playerScoreUI[0].SetActive(true);
                        playerScoresActiveCheck[0] = 1;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    case 1:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 2").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        pc.teamColor = new Color(0,1,0,1);
                        playerAlive[1] = true;
                        playerScoreUI[1].SetActive(true);
                        playerScoresActiveCheck[1] = 1;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    case 2:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 3").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        pc.teamColor = new Color(0,0,1,1);
                        playerAlive[2] = true;
                        playerScoreUI[2].SetActive(true);
                        playerScoresActiveCheck[2] = 1;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    case 3:
                        temp = MyCore.NetCreateObject(player.character, player.Owner, GameObject.Find("Spawn 4").transform.position);
                        pc = temp.GetComponent<PlayerCharacter>();
                        pc.playerName = player.playerName;
                        pc.teamColor = new Color(1,1,0,1);
                        playerAlive[3] = true;
                        playerScoreUI[3].SetActive(true);
                        playerScoresActiveCheck[3] = 1;
                        Debug.Log("Player: " + (1+player.Owner));
                        Debug.Log(pc.playerName);
                        break;
                    default:
                        break;
                }
            }
            SendUpdate("PLAYERSCOREUI", playerScoresActiveCheck[0].ToString() + ", " + playerScoresActiveCheck[1].ToString() + ", " + playerScoresActiveCheck[2].ToString() + ", " + playerScoresActiveCheck[3].ToString());
            Debug.Log("Ready");

            //Game Start

            SendUpdate("GAMESTART", GameStarted.ToString());

            //Game Timer

            IEnumerator gameTime = PauseGame(3600f);

            StartCoroutine(gameTime);

            int tempCC = MyCore.Connections.Count;
            PlayerCharacter[] pcTemp = FindObjectsOfType<PlayerCharacter>();

            while(paused)
            {
                PlayerCharacter[] pcCheck = FindObjectsOfType<PlayerCharacter>();
                if(pcTemp != pcCheck)
                {
                    int[] tempAliveCheck = {0,0,0,0};
                    foreach(PlayerCharacter p in pcCheck)
                    {
                        tempAliveCheck[p.Owner] = 1;
                        playerScores[p.Owner] = p.playerScore;
                    }
                    for(int i = 0; i < 4; i++)
                    {
                        if(tempAliveCheck[i] == 0)
                        {
                            playerAlive[i] = false;
                        }
                    }
                }
                if(pcCheck.Length < 2)
                {
                    foreach(PlayerCharacter p in pcCheck)
                    {
                        if(p!=null)
                        {
                            winner = p.GetComponent<NetworkID>().Owner;
                        }
                    }
                    StopCoroutine(gameTime);
                    paused = false;
                }
                pcTemp = FindObjectsOfType<PlayerCharacter>();
                yield return new WaitForSeconds(1f);
            }

            StopCoroutine(gameTime);

            Debug.Log("Unpaused");

            //End Screen

            

            Debug.Log("End Screen");

            SendUpdate("ENDSCREEN", playerScores[0].ToString() + ", " + playerScores[1].ToString() + ", " + playerScores[2].ToString() + ", " + playerScores[3].ToString() + ", " + winner.ToString());

            //End Screen Logic

            IEnumerator endScreen = EndGame(30f);

            StartCoroutine(endScreen);

            PlayerCharacter[] delete = FindObjectsOfType<PlayerCharacter>();
            foreach(PlayerCharacter p in delete)
            {
                MyCore.NetDestroyObject(p.NetId);
            }

            while(end)
            {
                
                yield return new WaitForSeconds(1f);
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
        paused = false;
    }

    public IEnumerator EndGame(float s)
    {
        yield return new WaitForSeconds(s);
        end = false;
    }
}
