using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;

public class vanillaCharacterScript : NetworkComponent
{

    public int unitDamage = 1;
    public bool attackCooldown = false;
    public float attackCooldownTime = 2;
    public float mineCooldownTime = 1;
    public float unitSpeed = 10;
    private bool unitMove = false;
    private bool mineCooldown = false;
    private bool unitAP = false;
    private bool unitMine = false;
    public bool isArcher = false;
    public bool isHorse = false;
    public bool isVillager = false;
    public bool isSwordsman = false;
    public bool canMine = true;
    private bool isMining = false;
    private bool unitAttack = false;
    private Vector2 unitTargetLocation;
    private PlayerCharacter enemyTarget;
    public float attackRange = 1f;
    private PlayerCharacter leader = null;
    private GameObject mineTarget;
    public GameObject arrowPrefab;
    private healthScript hsTarget;
    private healthScript selfTarget;
    private Vector2 hsTargetLoc;
    public bool clientShootArrow = false;
    public Vector2 clientShootArrowPOS;
    public Animator MyAnime;
    private bool clientAttack = false;
    private bool clientMoving = false;
    private bool clientMiningBuilding = false;
    private bool clientSpriteFlip = false;
    private bool clientCheckSpriteFlip = false;
    private bool serverSetTeam = false;
    private bool clientUnitColorSet = false;
    private Color unitColor = new Color(1,1,1,1);
    private bool unitSoundEnabled = false;
    public AudioSource audioSource;
    public AudioClip soundSword;
    public AudioClip soundHit;
    public AudioClip soundArcher;
    public AudioClip soundHorseAttack;
    public AudioClip soundUnitMine;
    private bool loopSet = false;
    public float soundVolume = 0.5f;
    public volumeScript userSound;
    private bool setSound = false;

    public Vector2 ParseV2(string v)
    {
        Vector2 temp = new Vector2();
        string[] args = v.Trim('(').Trim(')').Split(',');
        temp.x = float.Parse(args[0]); 
        temp.y = float.Parse(args[1]);
        return temp;
    }

    public Color ParseCV4(string v)
    {
        Color temp = new Color();
        string[] args = v.Trim('(').Trim(')').Split(',');
        Debug.Log("ARGS: " + args[0] + " / " + args[1] + " / " + args[2] + " / " + args[3]);
        temp.r = float.Parse(args[0]);
        Debug.Log("1: " + temp.r);
        temp.g = float.Parse(args[1]);
        Debug.Log("2: " + temp.g);
        temp.b = float.Parse(args[2]);
        Debug.Log("3: " + temp.b);
        temp.a = float.Parse(args[3]);
        Debug.Log("4: " + temp.a);
        return temp;
    }

    public override void HandleMessage(string flag, string value)
    {
        if(IsClient && flag == "SHOOTARROW")
        {
            Debug.Log(value);
            PlayUnitSound(soundArcher);
            clientShootArrowPOS = ParseV2(value);
            clientShootArrow = true;
            clientAttack = true;
        }
        if((IsClient || IsServer) && flag == "ATTACK")
        {
            if(IsClient)
            {
                if(isVillager)
                {
                    PlayUnitSound(soundHit);
                }
                if(isSwordsman)
                {
                    PlayUnitSound(soundSword);
                }
                if(isHorse)
                {
                    PlayUnitSound(soundHorseAttack);
                }
            }
            clientAttack = true;
        }
        if((IsClient || IsServer) && flag == "MOVING")
        {
            clientMoving = bool.Parse(value);
        }
        if((IsClient || IsServer) && flag == "MININGBUILDING")
        {
            clientMiningBuilding = bool.Parse(value);
        }
        if((IsClient) && flag == "SPRITEFLIP")
        {
            clientSpriteFlip = bool.Parse(value);
            clientCheckSpriteFlip = true;
        }
        if(IsClient && flag == "UNITCOLOR")
        {
            unitColor = ParseCV4(value);
            clientUnitColorSet = true;
        }
        if(IsClient && flag == "MINESOUND")
        {
            PlayUnitSound(soundUnitMine);
        }
    }

    public override void NetworkedStart()
    {
        if(IsServer)
        {
            IEnumerator unitAttackIE = UnitCooldown(attackCooldownTime);
            StartCoroutine(unitAttackIE);
            IEnumerator unitMineIE = UnitMineCooldown(mineCooldownTime);
            StartCoroutine(unitMineIE);
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
            if(gameObject.GetComponent<healthScript>() != null)
            {
                selfTarget = gameObject.GetComponent<healthScript>();
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
                if(selfTarget.isAlive)
                {
                    if(unitMove)
                    {
                        if(clientMiningBuilding)
                        {
                            SendUpdate("MININGBUILDING", false.ToString());
                            clientMiningBuilding = false;
                        } 
                        float distanceFromGoal = Vector3.Distance(gameObject.transform.position, unitTargetLocation);
                        SpriteFlipper(new Vector2 (unitTargetLocation.x, unitTargetLocation.y));
                        bool setVar = false;
                        while(distanceFromGoal >= .01f && !unitAP && !unitMine && !unitAttack)
                        {
                            if(!setVar)
                            {
                                clientMoving = true;
                                SendUpdate("MOVING", clientMoving.ToString());
                                setVar = true;
                            }
                            SpriteFlipper(new Vector2 (unitTargetLocation.x, unitTargetLocation.y));
                            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, unitTargetLocation, unitSpeed*Time.deltaTime);
                            distanceFromGoal = Vector3.Distance(gameObject.transform.position, unitTargetLocation);
                            yield return new WaitForSeconds(.1f);
                        }
                        clientMoving = false;
                        SendUpdate("MOVING", clientMoving.ToString());
                        unitMove = false;
                    }
                    if(unitAP)
                    {
                        if(enemyTarget != null)
                        {
                            bool setVar = false;
                            float distanceFromGoal = Vector3.Distance(gameObject.transform.position, enemyTarget.gameObject.transform.position);
                            SpriteFlipper(new Vector2 (enemyTarget.gameObject.transform.position.x, enemyTarget.gameObject.transform.position.y));
                            while(distanceFromGoal >= attackRange && !unitMove && !unitMine && !unitAttack && (enemyTarget != null))
                            {
                                if(!setVar)
                                {
                                    clientMoving = true;
                                    SendUpdate("MOVING", clientMoving.ToString());
                                    setVar = true;
                                }
                                SpriteFlipper(new Vector2 (enemyTarget.gameObject.transform.position.x, enemyTarget.gameObject.transform.position.y));
                                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, enemyTarget.gameObject.transform.position, unitSpeed*Time.deltaTime);
                                distanceFromGoal = Vector3.Distance(gameObject.transform.position, enemyTarget.gameObject.transform.position);
                                yield return new WaitForSeconds(.1f);
                            }
                            clientMoving = false;
                            SendUpdate("MOVING", clientMoving.ToString());
                            if(enemyTarget.isAlive)
                            {
                                UnitAttackPlayer(enemyTarget);
                            }
                            else
                            {
                                unitAP = false;
                            }
                        }
                        else
                        {
                            unitAP = false;
                        }
                    }
                    if(unitMine)
                    {
                        float distanceFromGoal = Vector3.Distance(gameObject.transform.position, mineTarget.gameObject.transform.position);
                        SpriteFlipper(new Vector2 (mineTarget.gameObject.transform.position.x, mineTarget.gameObject.transform.position.y));
                        while(distanceFromGoal >= 1 && !unitMove && !unitAP && !unitAttack)
                        {
                            bool setVar = false;
                            if(!setVar)
                            {
                                clientMoving = true;
                                SendUpdate("MOVING", clientMoving.ToString());
                                setVar = true;
                            }
                            SpriteFlipper(new Vector2 (mineTarget.gameObject.transform.position.x, mineTarget.gameObject.transform.position.y));
                            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, mineTarget.gameObject.transform.position, unitSpeed*Time.deltaTime);
                            distanceFromGoal = Vector3.Distance(gameObject.transform.position, mineTarget.gameObject.transform.position);
                            yield return new WaitForSeconds(.1f);
                        }
                        clientMoving = false;
                        SendUpdate("MOVING", clientMoving.ToString());
                        if(distanceFromGoal < 1)
                        {
                            if(!clientMiningBuilding)
                            {
                                SendUpdate("MININGBUILDING", true.ToString());
                                clientMiningBuilding = true;
                            }    
                            UnitMining(mineTarget);
                        }
                        else
                        {
                            if(clientMiningBuilding)
                            {
                                SendUpdate("MININGBUILDING", false.ToString());
                                clientMiningBuilding = false;
                            } 
                            unitMine = false;
                        }
                    }
                    else
                    {
                        if(clientMiningBuilding)
                        {
                            SendUpdate("MININGBUILDING", false.ToString());
                            clientMiningBuilding = false;
                        } 
                    }
                    if(unitAttack && hsTarget.isAlive && (hsTarget.gameObject != null))
                    {
                        if((hsTarget.gameObject != this.gameObject))
                        {
                            if(hsTarget.gameObject != null)
                            {
                                hsTargetLoc = hsTarget.transform.position;
                            }
                            float distanceFromGoal = Vector3.Distance(gameObject.transform.position, hsTargetLoc);
                            SpriteFlipper(hsTargetLoc);
                            while(distanceFromGoal >= attackRange && !unitMove && !unitMine && !unitAP && (hsTarget != null))
                            {
                                bool setVar = false;
                                if(!setVar)
                                {
                                    clientMoving = true;
                                    SendUpdate("MOVING", clientMoving.ToString());
                                    setVar = true;
                                }
                                if(hsTarget.gameObject != null)
                                {
                                    hsTargetLoc = hsTarget.transform.position;
                                }
                                SpriteFlipper(hsTargetLoc);
                                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, hsTargetLoc, unitSpeed*Time.deltaTime);
                                distanceFromGoal = Vector3.Distance(gameObject.transform.position, hsTargetLoc);
                                yield return new WaitForSeconds(.1f);
                            }
                            clientMoving = false;
                            SendUpdate("MOVING", clientMoving.ToString());
                            UnitAttackHSUnit(hsTarget);
                        }
                        else
                        {
                            if(clientMoving)
                            {
                                clientMoving = false;
                                SendUpdate("MOVING", clientMoving.ToString());
                            }
                            unitAttack = false;
                        }
                    }
                    else
                    {
                        if(clientMoving)
                        {
                            clientMoving = false;
                            SendUpdate("MOVING", clientMoving.ToString());
                        }
                        if(unitAttack)
                        {
                            unitAttack = false;
                        }
                    }
                }
                if(IsDirty)
                {
                    SendUpdate("SPRITEFLIP", clientSpriteFlip.ToString());
                    SendUpdate("MOVING", clientMoving.ToString());
                    SendUpdate("MININGBUILDING", clientMiningBuilding.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    void Start()
    {
        MyAnime = GetComponent<Animator>();
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
            if(gameObject.GetComponent<AudioSource>() != null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
            }
        }
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
            soundVolume = userSound.volume/100;
            setSound = true;
        }
        if(userSound != null && soundVolume != (userSound.volume/100))
        {
            soundVolume = userSound.volume/100;
        }
        if(selfTarget == null && (gameObject != null))
        {
            selfTarget = gameObject.GetComponent<healthScript>();
        }
        if(IsClient || IsServer)
        {
            if(MyAnime==null)
            {
                MyAnime = GetComponent<Animator>();
            }
        }
        if(IsClient)
        {
            if(audioSource == null)
            {
                if(gameObject.GetComponent<AudioSource>() != null)
                {
                    audioSource = gameObject.GetComponent<AudioSource>();
                }
            }
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
            if(clientCheckSpriteFlip)
            {
                SpriteRenderer spriteTransform = GetComponent<SpriteRenderer>();
                if(spriteTransform != null)
                {
                    if (clientSpriteFlip) 
                    {
                        spriteTransform.flipX = true;
                    } 
                    else if(!clientSpriteFlip)
                    {
                        spriteTransform.flipX = false;
                    }
                }
                clientCheckSpriteFlip = false;
            }
            if(clientShootArrow)
            {
                GameObject instance = Instantiate(arrowPrefab, gameObject.transform);
                instance.GetComponent<arrowScript>().targetPoint = new Vector3(clientShootArrowPOS.x, clientShootArrowPOS.y, -1);
                instance.GetComponent<arrowScript>().startingPoint = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -1);
                
                clientShootArrow = false;
            }
            if(MyAnime != null)
            {
                if(clientAttack)
                {
                    MyAnime.SetTrigger("attack");
                    clientAttack = false;
                }
                if(clientMoving && !MyAnime.GetBool("moving"))
                {
                    MyAnime.SetBool("moving",true);
                }
                else if(!clientMoving && MyAnime.GetBool("moving"))
                {
                    MyAnime.SetBool("moving",false);
                }
                if(clientMiningBuilding && !MyAnime.GetBool("miningbuilding"))
                {
                    MyAnime.SetBool("miningbuilding",true);
                }
                else if(!clientMiningBuilding && MyAnime.GetBool("miningbuilding"))
                {
                    MyAnime.SetBool("miningbuilding",false);
                }
            }
        }
    }

    //Unit Basic Move Script

    public void UnitMove(Vector2 finish)
    {
        int UnitNetID = GetComponent<NetworkID>().NetId;
        unitTargetLocation = finish;
        
        unitAP = false;
        unitMine = false;
        unitAttack = false;
        unitMove = true;

        Debug.Log("Moving Unit " + UnitNetID + ": " + gameObject.name);
        Debug.Log("Moving From: " + gameObject.transform.position);
        Debug.Log("Moving To: " + finish);
        
    }

    //EOS Unit Basic Move Script

    //Unit Basic Attack Script

    public void UnitAttackPlayer(PlayerCharacter pc)
    {
        if(!attackCooldown && pc.isAlive && !isArcher)
        {
            leader.AddScore(5);
            SendUpdate("ATTACK", true.ToString());
            StartCoroutine(UnitAttackRangedPlayer(.5f,pc));
            attackCooldown = true;
        }
        if(!attackCooldown && pc.isAlive && isArcher)
        {
            leader.AddScore(5);
            Vector2 tar = new Vector2(enemyTarget.transform.position.x,enemyTarget.transform.position.y);
            SendUpdate("SHOOTARROW", tar.ToString("F2"));
            StartCoroutine(UnitAttackRangedPlayer(1.1f,pc));
            attackCooldown = true;
        }
    }

    public void UnitAttackingPlayer(PlayerCharacter pc)
    {
        int UnitNetID = GetComponent<NetworkID>().NetId;
        enemyTarget = pc;

        unitMove = false;
        unitMine = false;
        unitAttack = false;
        unitAP = true;

        Debug.Log("Moving Unit " + UnitNetID + ": " + gameObject.name);
        Debug.Log("Moving From: " + gameObject.transform.position);
        Debug.Log("Moving To: " + enemyTarget.gameObject.name);
    }

    public IEnumerator UnitCooldown(float s)
    {
        while(true)
        {
            if(attackCooldown)
            {
                yield return new WaitForSeconds(s);
                attackCooldown = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    public IEnumerator UnitAttackRangedPlayer(float s, PlayerCharacter pc)
    {
        yield return new WaitForSeconds(s);
        if(pc != null)
        {
            pc.DamageTaken(unitDamage);
        }
        yield break;
    }


    //EOS Unit Basic Attack Script

    //Unit Mining Script

    public void UnitMine(GameObject mine)
    {
        int UnitNetID = GetComponent<NetworkID>().NetId;
        mineTarget = mine;

        unitMove = false;
        unitAP = false;
        unitAttack = false;
        unitMine = true;

        Debug.Log("Moving Unit " + UnitNetID + ": " + gameObject.name);
        Debug.Log("Moving From: " + gameObject.transform.position);
        Debug.Log("Moving To: " +  gameObject.name);
    }

    public void UnitMining(GameObject mine)
    {
        if(!mineCooldown && canMine)
        {
            SendUpdate("MINESOUND",true.ToString());
            leader.MinedResource(mine.tag);
            mineCooldown = true;
        }
    }

    public IEnumerator UnitMineCooldown(float s)
    {
        while(true)
        {
            if(mineCooldown)
            {
                Debug.Log("Mine Cooldown");
                yield return new WaitForSeconds(s);
                mineCooldown = false;
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    //EOS Mining Script

    //Unit Damage

    

    public void UnitAttackHSUnit(healthScript hs)
    {
        if(!attackCooldown && hs.isAlive && !isArcher)
        {
            leader.AddScore(5);
            SendUpdate("ATTACK", true.ToString());
            StartCoroutine(UnitAttackRangedUnit(.5f,hs));
            attackCooldown = true;
        }
        if(!attackCooldown && hs.isAlive && isArcher)
        {
            leader.AddScore(5);
            SendUpdate("SHOOTARROW", hsTargetLoc.ToString("F2"));
            StartCoroutine(UnitAttackRangedUnit(1.1f,hs));
            attackCooldown = true;
        }
    }

    public IEnumerator UnitAttackRangedUnit(float s, healthScript hs)
    {
        yield return new WaitForSeconds(s);
        if(hs != null)
        {
            hs.TakeDamage(unitDamage);
        }
        yield break;
    }

    public void UnitAttackingHSUnit(healthScript hs)
    {
        int UnitNetID = GetComponent<NetworkID>().NetId;
        hsTarget = hs;
        hsTargetLoc = hs.transform.position;

        unitMove = false;
        unitMine = false;
        unitAP = false;
        unitAttack = true;

        Debug.Log("Moving Unit " + UnitNetID + ": " + gameObject.name);
        Debug.Log("Moving From: " + gameObject.transform.position);
        Debug.Log("Moving To: " + hsTarget.gameObject.name);
    }

    //EOS Unit Damage

    public void SpriteFlipper(Vector2 targetPos)
    {
        SpriteRenderer spriteTransform = GetComponent<SpriteRenderer>();
        if(spriteTransform != null)
        {
            if (targetPos.x < gameObject.transform.position.x) 
            {
                clientSpriteFlip = true;
                SendUpdate("SPRITEFLIP", clientSpriteFlip.ToString());
                spriteTransform.flipX = true;
            } 
            else 
            {
                clientSpriteFlip = false;
                SendUpdate("SPRITEFLIP", clientSpriteFlip.ToString());
                spriteTransform.flipX = false;
            }
        }
    }
    
    //Sound Scripting

    private void OnBecameVisible()
    {
        if (audioSource != null && Camera.main != null)
        {
            unitSoundEnabled = true;
        }
    }

    private void OnBecameInvisible()
    {
        if (audioSource != null && Camera.main != null)
        {
            unitSoundEnabled = false;
        }
    }

    public void PlayUnitSound(AudioClip sound)
    {
        if(IsClient)
        {
            if(audioSource != null && Camera.main != null && unitSoundEnabled)
            {
                audioSource.PlayOneShot(sound, soundVolume);
            }
        }
    }

    


    //EOS Sound Scripting
}
