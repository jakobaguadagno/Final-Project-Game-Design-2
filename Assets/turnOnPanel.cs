using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class turnOnPanel : MonoBehaviour
{
    public void TurnOnPanel()
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            //Debug.Log("Turn Off Panel");
            GameObject wan = GameObject.Find("WANNetworkManager");
            if(wan!=null)
            {
                if(wan.transform.GetChild(0).GetChild(0).gameObject!=null)
                {
                    wan.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }
}
