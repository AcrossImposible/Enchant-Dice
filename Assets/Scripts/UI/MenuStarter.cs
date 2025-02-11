using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MenuStarter : MonoBehaviour
{
    [SerializeField] Menu menuPortrait;
    [SerializeField] Menu menuLandscape;

    private void Awake()
    {
        if(Screen.height > Screen.width)
        {
            menuPortrait.Init();
            menuLandscape.gameObject.SetActive(false);
        }
        else
        {
            menuLandscape.Init();
            menuPortrait.gameObject.SetActive(false);
        }
    }
}
