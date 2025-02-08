using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class PanelUpdateGame : MonoBehaviour
{
    [SerializeField] Button btnDownload;

    private void Start()
    {
        btnDownload.onClick.AddListener(Download_Clicekd);
    }

    private void Download_Clicekd()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.WogerGames.RandomIncreaseDice");
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.H))
        {
            gameObject.SetActive(false);
        }
#endif
    }
}
