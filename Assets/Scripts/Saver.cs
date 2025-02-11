using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saver : MonoBehaviour
{
    public static void Save()
    {
#if UNITY_WEBGL
        ConvertToYG();
        YG.YandexGame.SaveProgress();
#else
        var jsonStr = Json.Serialize(User.Data);

        PlayerPrefs.SetString("data", jsonStr);
        PlayerPrefs.Save();
#endif
    }

    public static User Load()
    {
        if (PlayerPrefs.HasKey("data"))
        {
            var data = Json.Deserialize<User>(PlayerPrefs.GetString("data"));

            User.Data = data;
        }

        return User.Data;
    }

#if UNITY_WEBGL
    public static void ConvertToYG()
    {
        var data = YG.YandexGame.savesData;
        data.countCards = User.Data.countCards;
        data.countStones = User.Data.countStones;
        data.tutorCompleted = User.Data.tutorCompleted;
        data.money = User.Data.golda;
        data.deck = User.Data.deck;
        data.inventory = User.Data.inventory;
        data.exp = User.Data.exp;
        data.lvl = User.Data.lvl;
        YG.YandexGame.savesData = data;
    }

    public static void ConvertToUserData()
    {
        var data = YG.YandexGame.savesData;
        User.Data.countStones = data.countStones;
        User.Data.countCards = data.countCards;
        User.Data.tutorCompleted = data.tutorCompleted;
        User.Data.lvl = data.lvl;
        User.Data.exp = data.exp;
        User.Data.golda = data.money;

        if (data.deck != null)
        {
            User.Data.deck = data.deck;
            User.Data.inventory = data.inventory;
        }
    }
#endif
}
