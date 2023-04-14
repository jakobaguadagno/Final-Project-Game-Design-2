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
    public bool canMine = true;
    private bool isMining = false;
    private bool unitAttack = false;
    private Vector2 unitTargetLocation;
    private PlayerCharacter enemyTarget;
    public float attackRange = 1f;
    private PlayerCharacter leader = null;
    private GameObject mineTarget;
    private healthScript hsTarget;
    private Vector2 hsTargetLoc;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        if(IsServer)
        {
            
            IEnumerator unitAttackIE = UnitCooldown(attackCooldownTime);
            StartCoroutine(unitAttackIE);
            IEnumerator unitMineIE = UnitMineCooldown(mineCooldownTime);
            StartCoroutine(unitMineIE);
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
                if(unitMove)
                {
                    float distanceFromGoal = Vector3.Distance(gameObject.transform.position, unitTargetLocation);
                    while(distanceFromGoal >= .01f && !unitAP && !unitMine && !unitAttack)
                    {
                        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, unitTargetLocation, unitSpeed*Time.deltaTime);
                        distanceFromGoal = Vector3.Distance(gameObject.transform.position, unitTargetLocation);
                        yield return new WaitForSeconds(.1f);
                    }
                    unitMove = false;
                }
                if(unitAP)
                {
                    float distanceFromGoal = Vector3.Distance(gameObject.transform.position, enemyTarget.gameObject.transform.position);
                    while(distanceFromGoal >= attackRange && !unitMove && !unitMine && !unitAttack)
                    {
                        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, enemyTarget.gameObject.transform.position, unitSpeed*Time.deltaTime);
                        distanceFromGoal = Vector3.Distance(gameObject.transform.position, enemyTarget.gameObject.transform.position);
                        yield return new WaitForSeconds(.1f);
                    }
                    if(enemyTarget.isAlive)
                    {
                        UnitAttackPlayer(enemyTarget);
                    }
                    else
                    {
                        unitAP = false;
                    }
                }
                if(unitMine)
                {
                    float distanceFromGoal = Vector3.Distance(gameObject.transform.position, mineTarget.gameObject.transform.position);
                    while(distanceFromGoal >= 1 && !unitMove && !unitAP && !unitAttack)
                    {
                        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, mineTarget.gameObject.transform.position, unitSpeed*Time.deltaTime);
                        distanceFromGoal = Vector3.Distance(gameObject.transform.position, mineTarget.gameObject.transform.position);
                        yield return new WaitForSeconds(.1f);
                    }
                    if(distanceFromGoal < 1)
                    {
                        UnitMining(mineTarget);
                    }
                    else
                    {
                        unitMine = false;
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
                        while(distanceFromGoal >= attackRange && !unitMove && !unitMine && !unitAP && (hsTarget != null))
                        {
                            if(hsTarget.gameObject != null)
                            {
                                hsTargetLoc = hsTarget.transform.position;
                            }
                            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, hsTargetLoc, unitSpeed*Time.deltaTime);
                            distanceFromGoal = Vector3.Distance(gameObject.transform.position, hsTargetLoc);
                            yield return new WaitForSeconds(.1f);
                        }
                    
                        UnitAttackHSUnit(hsTarget);
                    }
                    else
                    {
                        unitAttack = false;
                    }
                }
                else
                {
                    unitAttack = false;
                }
            }
            yield return new WaitForSeconds(.1f);
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
        if(!attackCooldown && pc.isAlive)
        {
            pc.DamageTaken(unitDamage);
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
        if(!attackCooldown && hs.isAlive)
        {
            hs.TakeDamage(unitDamage);
            attackCooldown = true;
        }
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

}
