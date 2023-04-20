using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NETWORK_ENGINE;


public class hoverScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public string textToPut;
    public Text textBox;
    private PlayerCharacter[] players;
    private NetworkCore core;
    private bool textSet = false;
    private bool isServer = false;

    void Start()
    {
        if(textBox == null && !textSet)
        {
            core = FindObjectOfType<NetworkCore>();
            if(core != null)
            {
                if(core.IsServer)
                {
                    isServer = true;
                }
                else
                {
                    players = FindObjectsOfType<PlayerCharacter>();
                    if(players != null)
                    {
                        foreach(PlayerCharacter p in players)
                        {
                            if(p.IsLocalPlayer && p != null && p.Owner != -1)
                            {
                                textBox = p.gameObject.transform.GetChild(3).GetChild(13).GetChild(0).GetComponent<Text>();
                                textSet = true;
                            }
                        }
                    }
                }
            }
        }
        if(textBox == null && !textSet && !isServer)
        {
            StartCoroutine(TextBoxFinder());
        }
    }

    public IEnumerator TextBoxFinder()
    {
        while(textBox == null && !textSet && !isServer)
        {
            core = FindObjectOfType<NetworkCore>();
            if(core != null)
            {
                if(core.IsServer)
                {
                    isServer = true;
                }
                else
                {
                    players = FindObjectsOfType<PlayerCharacter>();
                    if(players != null)
                    {
                        foreach(PlayerCharacter p in players)
                        {
                            if(p.IsLocalPlayer && p != null && p.Owner != -1)
                            {
                                textBox = p.gameObject.transform.GetChild(3).GetChild(13).GetChild(0).GetComponent<Text>();
                                textSet = true;
                            }
                        }
                    }
                }
            }
            yield return new WaitForSeconds(.5f);
        }
        yield break;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(textBox != null && textToPut != "" && !isServer)
        {
            //Debug.Log("Entering");
            textBox.text = textToPut;
        }
        else if(textToPut == "" && !isServer)
        {
//            Debug.Log("No text");
        }
        else if(!isServer)
        {
//            Debug.Log("Text box is null");
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if(textBox != null && !isServer)
        {
            //Debug.Log("Exiting");
            textBox.text = "";
        }
        else if(!isServer)
        {
//            Debug.Log("Text box is null");
        }
    }
}
