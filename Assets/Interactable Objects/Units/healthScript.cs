using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.UI;

public class healthScript : NetworkComponent
{

    public bool isAlive = true;
    public int unitHealth = 100;
    public Slider UnitHealth;
    public Animator MyAnime;
    private bool clientDeath = false;
    public AudioSource audioSource;
    public AudioClip soundDying;
    public AudioClip soundGrunt;
    private float soundVolume = 0.5f;
    public volumeScript userSound;
    private bool unitSoundEnabled = false;
    private bool setSound = false;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "UNITHEALTH")
        {
            if(IsClient)
            {
                if(unitHealth != int.Parse(value) && (int.Parse(value) >= 1))
                {
                    PlayUnitSound(soundGrunt);
                }
            }
            unitHealth = int.Parse(value);
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
                PlayUnitSound(soundDying);
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
        if(IsClient)
        {
            if(gameObject.GetComponent<AudioSource>() != null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
            }
        }
        userSound = GameObject.FindObjectOfType<volumeScript>();
    }

    public void Update()
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
        if(IsClient)
        {
            if(audioSource == null)
            {
                if(gameObject.GetComponent<AudioSource>() != null)
                {
                    audioSource = gameObject.GetComponent<AudioSource>();
                }
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

    
}
