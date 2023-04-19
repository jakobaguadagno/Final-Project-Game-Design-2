using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class volumeBarScript : MonoBehaviour
{
    public Slider slider;
    
    void Start()
    {
        slider.value = 50f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
