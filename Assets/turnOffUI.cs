using System.Collections;
using System.Collections.Generic;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.SceneManagement;

public class turnOffUI : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {
    }

    public override void NetworkedStart()
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            Debug.Log("Turn Off Panel");
            GameObject wan = GameObject.Find("WANNetworkManager");
            wan.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(10f);
    }
}
