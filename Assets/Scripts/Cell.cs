using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool IsEmpty { get; set; } = true;

    [field: SerializeField]
    public int Idx { get; set; }

    private void Start()
    {
        Idx = transform.GetSiblingIndex();
    }
}
