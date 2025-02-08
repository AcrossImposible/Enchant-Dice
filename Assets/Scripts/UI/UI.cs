using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class UI : MonoBehaviour
{
    [SerializeField] Button btnSpawnDice;
    [SerializeField] Button btnMenu;
    [SerializeField] Button btnSplitting;
    [SerializeField] Button btnOkSplit;
    [SerializeField] Button btnCancelSplit;
    [SerializeField] GameObject panelWave;
    [SerializeField] GameObject panelComplete;
    [SerializeField] GameObject statsCoop;
    [SerializeField] GameObject statsPVP;
    [SerializeField] GameObject panelSplittingTooltip;
    [SerializeField] GameObject panelConfirmSplitting;
    [SerializeField] TMP_Text labelWave;
    [SerializeField] TMP_Text labelWaveTopPanel;
    [SerializeField] TMP_Text labelResult;
    [SerializeField] TMP_Text labelCountWaves;
    [SerializeField] TMP_Text labelCountCards;
    [SerializeField] TMP_Text labelCountPoints;
    [SerializeField] TMP_Text labelCountPowerStones;
    [SerializeField] TMP_Text labelGolda;
    [SerializeField] TMP_Text labelExp;
    [SerializeField] TMP_Text txtMineNickname;
    [SerializeField] TMP_Text txtOtherNickname;
    [SerializeField] PanelIncrease panelIncrease;

    Player minePlayer;
    Dice diceToSplit;

    private void Awake()
    {
        btnSpawnDice.onClick.AddListener(SpawnDice_Clicked);
        btnMenu.onClick.AddListener(BtnMenu_Clicked);
        btnSplitting.onClick.AddListener(BtnSplitting_Clicked);
        btnOkSplit.onClick.AddListener(OkSplit_Clicked);
        btnCancelSplit.onClick.AddListener(CancelSplit_Clicked);

        EventsHolder.onEnemySkiped.AddListener(Enemy_Skiped);
        EventsHolder.onPowerStoneClicked.AddListener(PowerStone_Clicked);
        EventsHolder.playerSpawnedMine.AddListener(MinePlayer_Spawned);
        EventsHolder.onDiceToSplitClicked.AddListener(DiceToSplit_Clicked);
        EventsHolder.onBlyatDisconect.AddListener(Ebychiy_Slychayai);

        panelIncrease.Init();

        PowerStone_Clicked();

        txtMineNickname.text = LobbyManager.GetNickname();
        var room = Photon.Pun.PhotonNetwork.CurrentRoom;
        if (room != null)
            txtOtherNickname.text = room.Players.ToList().Find(p => !p.Value.IsLocal).Value.NickName;
        else
            txtOtherNickname.text = "������ �������";
    }

    private void Ebychiy_Slychayai()
    {
        print($"������ �����.. ���������� ���� {GameManager.Instance.Wave}");
    }

    private void CancelSplit_Clicked()
    {
        diceToSplit = null;
        panelConfirmSplitting.SetActive(false);
    }

    private void OkSplit_Clicked()
    {
        EventsHolder.onDiceDestroyed?.Invoke(diceToSplit);

        User.Data.countStones += diceToSplit.Stage + 1;
        diceToSplit.Cell.IsEmpty = true;
        Destroy(diceToSplit.gameObject);
        minePlayer.Coins += minePlayer.PriceDice / 2;

        panelConfirmSplitting.SetActive(false);
        panelSplittingTooltip.SetActive(false);
        EventsHolder.onSplittingModeUpdated.Invoke(false);

        PowerStone_Clicked();

#if UNITY_ANDROID
        Saver.Save();
#endif

#if UNITY_WEBGL
        Saver.ConvertToYG();
        YG.YandexGame.SaveProgress();
#endif
    }

    private void DiceToSplit_Clicked(Dice dice)
    {
        diceToSplit = dice;
        panelConfirmSplitting.SetActive(true);
    }

    private void BtnSplitting_Clicked()
    {
        var splittingMode = !panelSplittingTooltip.activeSelf;
        panelSplittingTooltip.SetActive(splittingMode);
        EventsHolder.onSplittingModeUpdated?.Invoke(splittingMode);
    }

    private void MinePlayer_Spawned(Player player)
    {
        minePlayer = player;
        minePlayer.onCoinsUpdated += Coins_Updated;

        Coins_Updated();
        UpdateBtnSpawnDice();
    }

    private void Coins_Updated()
    {
        labelCountPoints.text = $"{minePlayer.Coins}";
    }

    private void PowerStone_Clicked()
    {
        var txt = Language.Rus ? "����� ��������" : "Power Stones";
        labelCountPowerStones.text = $"{txt} x{User.Data.countStones}";
    }

    private void BtnMenu_Clicked()
    {
        Advertising.ShowVideoAd();

        if (Photon.Pun.PhotonNetwork.CurrentRoom == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);

        }
        else
        {
            Photon.Pun.PhotonNetwork.LeaveRoom();
        }
    }

    private void Start()
    {
        ShowPanelWave();

        panelComplete.SetActive(false);
        panelSplittingTooltip.SetActive(false);
        panelConfirmSplitting.SetActive(false);
    }

    void SpawnDice_Clicked()
    {
        EventsHolder.onSpawnDiceClicked?.Invoke();

        UpdateBtnSpawnDice();
    }

    void UpdateBtnSpawnDice()
    {
        btnSpawnDice.GetComponentInChildren<TMP_Text>().text = $"{minePlayer.PriceDice}";
    }

    private void Enemy_Skiped(Enemy enemy)
    {
        if (panelComplete.activeSelf)
            return;

        int waves = GameManager.Instance.Wave + 1;

        panelComplete.SetActive(true);

        var txt = Language.Rus ? "�����" : "Wave";
        labelCountWaves.text = $"{txt} {waves}";


        if (GameManager.Instance.IsPVP)
        {
            statsPVP.SetActive(true);
            statsCoop.SetActive(false);

            var txtGolda = Language.Rus ? "������" : "Gold";
            var txtExp = Language.Rus ? "����" : "Exp";

            if (enemy.Team == Team.Mine)
            {
                var txtDefeat = Language.Rus ? "���������" : "Defeat";
                labelResult.text = txtDefeat;

                User.Data.golda += waves;
                User.Data.exp -= 5;
                User.Data.exp = Mathf.Clamp(User.Data.exp, 0, int.MaxValue);

                labelGolda.text = $"{txtGolda} +{waves}";
                labelExp.text = $"{txtExp} -{5}";
            }
            else
            {
                var txtVictory = Language.Rus ? "������" : "Victory";
                labelResult.text = txtVictory;

                User.Data.exp += 8 + waves;
                User.Data.golda += waves * 10;

                labelGolda.text = $"{txtGolda} +{waves * 10}";
                labelExp.text = $"{txtExp} +{8 + waves}";

                if(User.Data.exp >= User.Data.ExpToNextLvl)
                {
                    User.Data.exp -= User.Data.ExpToNextLvl;
                    User.Data.lvl++;
                }
            }
                       
        }
        else
        {
            statsPVP.SetActive(false);
            statsCoop.SetActive(true);

            var txtCardReceived = Language.Rus ? "�������� ��������" : "Cards received";
            labelCountCards.text = $"{txtCardReceived} {waves}";

            User.Data.countCards += waves;
        }

#if UNITY_ANDROID
        Saver.Save();
#endif

#if UNITY_WEBGL
        Saver.ConvertToYG();
        YG.YandexGame.SaveProgress();
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            User.Data.countStones += 100;
            
            Saver.Save();
        }
#endif

        var txtWave = Language.Rus ? "�����" : "Wave";
        string wave = $"{txtWave} {GameManager.Instance.Wave + 1}";
        labelWave.text = wave;
        labelWaveTopPanel.text = wave;

        var txtPowerStones = Language.Rus ? "����� ��������" : "Power Stones";
        labelCountPowerStones.text = $"{txtPowerStones} x{User.Data.countStones}";
    }

    public void ShowPanelWave()
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            panelWave.SetActive(true);

            yield return new WaitForSeconds(1.8f);

            panelWave.SetActive(false);
        }
    }
}
