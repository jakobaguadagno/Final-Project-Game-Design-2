using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainMenuMusicScript : MonoBehaviour
{

    public AudioSource audioSource;
    public volumeScript userSound;
    private bool setSound = false;
    
    void Start()
    {
        userSound = GameObject.FindObjectOfType<volumeScript>();
    }

    void Update()
    {
        if(userSound == null)
        {
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
    }
}
