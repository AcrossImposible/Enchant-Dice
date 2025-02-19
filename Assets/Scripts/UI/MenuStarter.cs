using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MenuStarter : MonoBehaviour
{
    [SerializeField] Menu menuPortrait;
    [SerializeField] Menu menuLandscape;

    public Menu ActiveMenu { get; private set; }

    public static MenuStarter Single;

    private void Awake()
    {
        Single = this;

        if (Screen.height > Screen.width)
        {
            menuLandscape.gameObject.SetActive(false);
            menuPortrait.gameObject.SetActive(true);

            menuPortrait.Init();
            ActiveMenu = menuPortrait;
        }
        else
        {
            menuPortrait.gameObject.SetActive(false);
            menuLandscape.gameObject.SetActive(true);
            menuLandscape.Init();
            ActiveMenu = menuLandscape;
        }
    }
}
