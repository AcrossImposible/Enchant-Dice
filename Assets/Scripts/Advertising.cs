using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID && APPODEAL
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
#endif

public class Advertising : MonoBehaviour
#if UNITY_ANDROID && APPODEAL
, IRewardedVideoAdListener
#endif
{
    public System.Action onVideoClosed;

    public static int countGameSession = 1;

    private void Start()
    {
#if UNITY_ANDROID && APPODEAL
        Appodeal.disableLocationPermissionCheck();
        Appodeal.setAutoCache(Appodeal.REWARDED_VIDEO, false);
        Appodeal.initialize("2e904cde7e6339ea65baa5bac3793b0443f88852042c5bd1", Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO, false);
        Appodeal.setRewardedVideoCallbacks(this);
#endif
    }

    public static void ShowVideoAd()
    {
        countGameSession++;
        if (countGameSession >= 3)
        {
#if UNITY_ANDROID && APPODEAL
            if (Appodeal.isLoaded(Appodeal.REWARDED_VIDEO))
            {
                Appodeal.show(Appodeal.REWARDED_VIDEO);
                countGameSession = 1;
            }
            else if (Appodeal.isLoaded(Appodeal.INTERSTITIAL))
            {
                Appodeal.show(Appodeal.INTERSTITIAL);
                countGameSession = 1;
            }

            Appodeal.cache(Appodeal.REWARDED_VIDEO);

#endif
        }
    }

    public void onRewardedVideoLoaded(bool precache)
    {
        
    }

    public void onRewardedVideoFailedToLoad()
    {
        
    }

    public void onRewardedVideoShowFailed()
    {
        
    }

    public void onRewardedVideoShown()
    {
        
    }

    public void onRewardedVideoFinished(double amount, string name)
    {
        
    }

    public void onRewardedVideoClosed(bool finished)
    {
        onVideoClosed?.Invoke();
    }

    public void onRewardedVideoExpired()
    {
        
    }

    public void onRewardedVideoClicked()
    {
        
    }
}
