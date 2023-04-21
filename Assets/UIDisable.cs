using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIDisable : MonoBehaviour
{
    GameObject lan;
    GameObject wan;

    public void DisableUI()
        {
            if(SceneManager.GetActiveScene().buildIndex == 2)
            {
                lan.SetActive(false);
            }
            if(SceneManager.GetActiveScene().buildIndex == 1)
            {
                wan.SetActive(false);
            }
        }
}
