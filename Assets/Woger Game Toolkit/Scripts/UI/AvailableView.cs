using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AvailableView : MonoBehaviour
{
    [SerializeField] Graphic[] oAnims;
    [SerializeField] float transparency = 0.5f;

    public State state;

    public void Unavailable()
    {
        if (state is State.Unavailable)
            return;

        state = State.Unavailable;

        foreach (var item in oAnims)
        {
            LeanTweanTool.SetTransparencyImage(item, transparency);
        }

    }

    public void Available()
    {
        if (state is State.Available)
            return;

        state = State.Available;

        foreach (var item in oAnims)
        {
            LeanTweanTool.SetTransparencyImage(item, 1);
        }
    }

    public enum State
    {
        None,
        Unavailable,
        Available,
        Watched
    }
}
