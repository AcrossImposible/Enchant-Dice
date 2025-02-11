using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorStarter : MonoBehaviour
{
    [SerializeField] Tutor tutorPortrait;
    [SerializeField] Tutor tutorLandscape;

    private void Start()
    {
        if (Screen.height > Screen.width)
        {
            tutorPortrait.Init();
            tutorPortrait.gameObject.SetActive(true);
            tutorLandscape.gameObject.SetActive(false);
        }
        else
        {
            tutorLandscape.Init();
            tutorLandscape.gameObject.SetActive(true);
            tutorPortrait.gameObject.SetActive(false);
        }
    }
}
