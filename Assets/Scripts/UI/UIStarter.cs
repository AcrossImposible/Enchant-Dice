using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-101)]
public class UIStarter : MonoBehaviour
{
    [SerializeField] UI uiPortrait;
    [SerializeField] UI uiLandscape;

    public UI ActiveUI { get; private set; }

    public static UIStarter Single;


    private void Start()
    {
        Single = this;

        if (Screen.height > Screen.width)
        {
            uiPortrait.gameObject.SetActive(true);
            uiLandscape.gameObject.SetActive(false);
            uiPortrait.Init();
            ActiveUI = uiPortrait;
        }
        else
        {
            uiLandscape.gameObject.SetActive(true);
            uiPortrait.gameObject.SetActive(false);
            uiLandscape.Init();
            ActiveUI = uiLandscape;
        }
    }
}
