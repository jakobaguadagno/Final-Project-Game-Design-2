using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class volumeScript : MonoBehaviour
{
    public Slider slider;
    public float volume = 50f;


    void Update()
    {
        if(slider==null)
        {
            if(GameObject.FindGameObjectWithTag("VolumeSlider")!=null)
            {
                slider = GameObject.FindGameObjectWithTag("VolumeSlider").GetComponent<Slider>();
                slider.value = volume;
            }
        }
        if(slider!=null && volume != slider.value)
        {
            volume = slider.value;
        }
        
    }
}
