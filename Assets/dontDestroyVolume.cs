using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dontDestroyVolume : MonoBehaviour
{
    void Awake()
    {
        dontDestroyVolume[] objs = FindObjectsOfType<dontDestroyVolume>();
        if (objs.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
