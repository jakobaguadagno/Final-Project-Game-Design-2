using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainMenuScript : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;
    private int menuOn = 0;

    public void ToggleOptions()
    {
        switch(menuOn)
        {
            case 0:
                optionsMenu.SetActive(true);
                mainMenu.SetActive(false);
                menuOn = 1;
                break;
            case 1:
                optionsMenu.SetActive(false);
                mainMenu.SetActive(true);
                menuOn = 0;
                break;
        }
    }
}
