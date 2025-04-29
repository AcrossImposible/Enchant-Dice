using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using static Settings;
using YG.Utils.LB;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject panelEbash;
    [SerializeField] PanelDeck panelDeck;
    [SerializeField] GameObject panelChestResult;
    [SerializeField] PanelUpdateGame panelUpdateGame;

    [Space]

    [SerializeField] Button btnEbash;
    [SerializeField] public Button btnPVP;
    [SerializeField] public Button btnCoop;
    [SerializeField] public GameObject connectingInfo;
    [SerializeField] Button btnDeck;
    [SerializeField] Button btnOpenChest;
    [SerializeField] Button btnTutor;

    [Space]

    [SerializeField] TMP_Text labelExp;
    [SerializeField] TMP_Text labelGold;
    [SerializeField] TMP_Text labelCountCards;
    [SerializeField] TMP_Text labelMaxWave;

    [Space]

    [SerializeField] Transform chestResultParent;

    [Space]

    [SerializeField] CardChestResult chestResultPrefab;
    [SerializeField] List<Dice> allDices;

    [SerializeField] Button tutorReset;
    [SerializeField] TMP_InputField inputNickname;

    static bool loaded;

    public void Init()
    {
        //print(YG.YandexGame.savesData.tutorCompleted);

        gameObject.SetActive(true);

#if UNITY_ANDROID
        Saver.Load();
        loaded = true;
#endif

#if UNITY_WEBGL
        YG.YandexGame.GetDataEvent += Data_Geted;
        YG.YandexGame.onGetLeaderboard += LB_geted;
#endif
        inputNickname.onValueChanged.AddListener(Nickname_Changed);
        inputNickname.onSubmit.AddListener(Nickname_Submited);

        panelChestResult.SetActive(false);
        panelDeck.gameObject.SetActive(false);
        panelEbash.SetActive(true);
        panelUpdateGame.gameObject.SetActive(false);

        btnTutor.gameObject.SetActive(false);

        btnDeck.onClick.AddListener(BtnDeck_Clicked);
        btnEbash.onClick.AddListener(BtnEbash_Clicked);
        btnPVP.onClick.AddListener(BtnPVP_Clicked);
        btnOpenChest.onClick.AddListener(BtnChest_Clicked);

        //checkGameVersion.versionNotMatch += GameVersion_NotMatched;
        if (loaded)
        {
#if UNITY_WEBGL

            if (YG.YandexGame.savesData != null && string.IsNullOrEmpty(YG.YandexGame.savesData.newPlayerName))
            {
                var str = Language.Rus ? "Игрок " : "Player ";
                inputNickname.text = $"{str}{Random.Range(100, 999)}";
            }
            else
            {
                inputNickname.text = YG.YandexGame.savesData?.newPlayerName;
            }
#endif

            UpdateMaxWaveUIView();
        }

        tutorReset.onClick.AddListener(() => 
        { 
            User.Data.tutorCompleted = false;
#if UNITY_ANDROID
            Saver.Save();
#endif

#if UNITY_WEBGL
            Saver.ConvertToYG();
            YG.YandexGame.SaveProgress();
#endif
            Advertising.ShowVideoAd(); 
        });

#if UNITY_WEBGL
        //if (YG.YandexGame.savesData.deck != null)
        //{
        //    Saver.ConvertToUserData();
        //}
#endif
        UpdateUI();

    }

    

    private void Nickname_Submited(string value)
    {
        Nickname_Changed(value);
    }

    private void Nickname_Changed(string value)
    {
#if UNITY_WEBGL
        if (YG.YandexGame.savesData.newPlayerName == value)
            return;

        YG.YandexGame.savesData.newPlayerName = value;
        YG.YandexGame.SaveProgress();
#endif
        Photon.Pun.PhotonNetwork.NickName = value;

        DelaySave();
    }

    private void DelaySave()
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(3);

#if UNITY_WEBGL
            YG.YandexGame.savesData.newPlayerName = inputNickname.text;
            YG.YandexGame.SaveProgress();
#endif

            Saver.Save();
        }
    }

    private void Data_Geted()
    {
#if UNITY_WEBGL
        Saver.ConvertToUserData();

        var data = YG.YandexGame.savesData;
        inputNickname.text = data.newPlayerName;

        Photon.Pun.PhotonNetwork.NickName = data.newPlayerName;
#endif
        YG.YandexGame.GetLeaderboard("wave", 20, 3, 6, "nonePhoto");
        loaded = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menuha");
        print($"Данные загружены {data.maxWave} ===");
    }

    void UpdateUI()
    {
        //print(YG.YandexGame.savesData.countCards);
        panelDeck.Init(allDices);
        var txtCards = Language.Rus ? "Карточки" : "Cards";
        labelCountCards.text = $"{txtCards} {User.Data.countCards}/{CARDS_TO_OPEN_CHEST}";

        if (User.Data.tutorCompleted == false)
        {
            btnTutor.gameObject.SetActive(true);
            btnPVP.gameObject.SetActive(false);
            btnCoop.gameObject.SetActive(false);

            btnTutor.onClick.AddListener(BtnTutor_Clicked);
        }
    }

    private void GameVersion_NotMatched()
    {
        panelUpdateGame.gameObject.SetActive(true);
    }

    private void BtnTutor_Clicked()
    {
        print("Загружаю тутор");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Tutor Epta");
    }

    private void BtnPVP_Clicked()
    {
        EventsHolder.onBtnPvPClicked?.Invoke(btnPVP);
    }

    private void BtnChest_Clicked()
    {
        if (User.Data.countCards >= CARDS_TO_OPEN_CHEST)
        {
            panelChestResult.SetActive(true);

            foreach (Transform item in chestResultParent) Destroy(item.gameObject);

            int countTypesDice = Random.Range(2, 4);
            for (int i = 0; i < countTypesDice; i++)
            {
                var randomIdx = Random.Range(0, allDices.Count);
                var dice = allDices[randomIdx];
                int countDices = Random.Range(1, 10);

                var diceUI = Instantiate(chestResultPrefab, chestResultParent);
                diceUI.Init(countDices, dice);

                var inInventory = User.Data.inventory.Find(d => d.idx == randomIdx);
                if(inInventory == null)
                {
                    User.Data.inventory.Add(new() { idx = randomIdx, count = countDices });
                }
                else
                {
                    inInventory.count += countDices;
                }
            }

            // Golda 
            var cardReward = Instantiate(chestResultPrefab, chestResultParent);
            var golda = Random.Range(User.Data.lvl * 8, User.Data.lvl * 88);
            cardReward.Init(golda, null);
            User.Data.golda += golda;

            User.Data.countCards -= CARDS_TO_OPEN_CHEST;
            string txt = Language.Rus ? "Карточки" : "Cards";
            labelCountCards.text = $"{txt} {User.Data.countCards}/{CARDS_TO_OPEN_CHEST}";

#if UNITY_ANDROID
            Saver.Save();
#endif

#if UNITY_WEBGL
            Saver.ConvertToYG();
            YG.YandexGame.SaveProgress();
#endif
        }
    }

    void UpdateMaxWaveUIView()
    {
        labelMaxWave.text = Language.Rus ? $"Максимально прожито волн в кооперативе {User.Data.maxWave}" : $"Max surved waves in cooperative {User.Data.maxWave}";
    }

    private void Update()
    {
#if UNITY_EDITOR
   
        if (Input.GetKeyDown(KeyCode.P))
        {
            User.Data.countCards += 3;
            labelCountCards.text = $"Cards {User.Data.countCards}/{CARDS_TO_OPEN_CHEST}";

            Saver.Save();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            User.Data.exp += Random.Range(5, 30);

            if (User.Data.exp >= User.Data.ExpToNextLvl)
            {
                User.Data.exp -= User.Data.ExpToNextLvl;
                User.Data.lvl++;
            }
        }

        

        if (Input.GetKeyDown(KeyCode.C))
        {
            User.Data.tutorCompleted = true;
            Saver.Save();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            print("Загружаю тутор");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutor Epta");
        }
#endif

        if (Input.GetKeyDown(KeyCode.R))
            PlayerPrefs.DeleteAll();

        if (Input.GetKeyDown(KeyCode.B))
            YG.YandexGame.GetLeaderboard("wave", 20, 3, 6, "nonePhoto");

        string txt = Language.Rus ? "Уровень" : "LvL";
        labelExp.text = $"{txt} {User.Data.lvl} ({User.Data.exp}/{User.Data.ExpToNextLvl})";
        txt = Language.Rus ? "Золото" : "Gold";
        labelGold.text = $"{txt} {User.Data.golda}";
    }

    private void LB_geted(LBData obj)
    {
        if (obj == null || obj.thisPlayer == null)
            return;

        print($"Запись в лдиерборде {obj.thisPlayer.score}");
        if (User.Data.maxWave < obj.thisPlayer.score)
        {
            User.Data.maxWave = obj.thisPlayer.score;
            Saver.Save();

            UpdateMaxWaveUIView();
        }
    }

    private void OnDestroy()
    {
        YG.YandexGame.GetDataEvent -= Data_Geted;
        YG.YandexGame.onGetLeaderboard -= LB_geted;
    }

    private void BtnEbash_Clicked()
    {
        panelDeck.gameObject.SetActive(false);
        panelEbash.SetActive(true);
    }

    private void BtnDeck_Clicked()
    {
        panelDeck.gameObject.SetActive(true);
        panelDeck.UpdateInventoryCards();

        panelEbash.SetActive(false);
    }
}
