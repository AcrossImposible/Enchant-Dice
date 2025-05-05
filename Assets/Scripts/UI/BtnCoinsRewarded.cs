using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


public class BtnCoinsRewarded : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image adIcon;
    [SerializeField] Image watchedMarker;
    [SerializeField] int idx;

    public State state;

    public int Idx => idx;

    [HideInInspector] public UnityEvent<BtnCoinsRewarded> onClick;

    public void Init()
    {
        button.onClick.AddListener(Button_Clicked);
    }

    private void Button_Clicked()
    {
        onClick?.Invoke(this);
    }

    public void Unavailable()
    {
        if (state is State.Unavailable)
            return;

        state = State.Unavailable;
        LeanTweanTool.HideImage(watchedMarker);
        LeanTweanTool.SetTransparencyImage(adIcon, 0.35f);
        LeanTweanTool.SetTransparencyImage(button.image, 0.8f);
        //print(gameObject);
    }

    public void Available()
    {
        if (state is State.Available)
            return;

        state = State.Available;

        LeanTweanTool.SetTransparencyImage(adIcon, 1f);
        LeanTweanTool.SetTransparencyImage(button.image, 1f);
    }

    public void Watched()
    {
        if (state is State.Watched)
            return;

        state = State.Watched;

        watchedMarker.gameObject.SetActive(true);
        LeanTweanTool.HideImage(adIcon);
        LeanTweanTool.ShowImage(watchedMarker);
    }

    public enum State
    {
        None,
        Unavailable,
        Available,
        Watched
    }
}
