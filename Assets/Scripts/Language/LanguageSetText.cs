using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LanguageSetText : MonoBehaviour
{
    [TextArea(5,10)]
    [SerializeField] string value;

    private void Start()
    {
        if (Language.Rus)
            GetComponent<TMP_Text>().text = value;
    }
}
