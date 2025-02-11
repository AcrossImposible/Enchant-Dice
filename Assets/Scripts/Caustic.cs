using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caustic : MonoBehaviour
{
    [SerializeField] GameObject subCaustic;

    private void Start()
    {
        subCaustic.SetActive(!Application.isMobilePlatform);
    }
}
