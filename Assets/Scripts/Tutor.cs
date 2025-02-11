using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class Tutor : MonoBehaviour
{
    [SerializeField] TMP_Text labelInfo;
    
    [SerializeField] Button tutorPanel;
    [SerializeField] Button panelIncreaseTutor;
    [SerializeField] TMP_Text labelInfoIncreaseTutor;

    [Space]

    [SerializeField] List<TutorInfo> tutorInfos;

    GameObject labelCoins;
    GameObject labelStones;
    Button btnSplitting;
    Button btnSpawnDice;

    Player player;
    int idxStage;
    float timer;
    bool isRunTimer;

    public void Init()
    {
        var ui = UIStarter.Single.ActiveUI;
        labelCoins = ui.labelCountPoints.gameObject;
        labelStones = ui.labelCountPowerStones.gameObject;
        btnSplitting = ui.btnSplitting;
        btnSpawnDice = ui.btnSpawnDice;

        labelCoins.SetActive(false);
        labelStones.SetActive(false);
        tutorPanel.gameObject.SetActive(true);
        btnSplitting.gameObject.SetActive(false);
        panelIncreaseTutor.gameObject.SetActive(false);

        btnSpawnDice.onClick.AddListener(BtnSpawnDice_Clicked);
        tutorPanel.onClick.AddListener(TutorPanel_Clicked);
        panelIncreaseTutor.onClick.AddListener(TutorIncrease_Clicked);

        EventsHolder.playerSpawnedMine.AddListener(MinePlayer_Spawned);
        EventsHolder.onDiceMerged.AddListener(Dice_Merged);
        EventsHolder.onEnemySkiped.AddListener(Enemy_Skiped);
        EventsHolder.onDiceClicked.AddListener(Dice_Clicked);
        EventsHolder.onDiceIncreased.AddListener(Dice_Increased);

        UpdateTutor();
    }

    private void TutorIncrease_Clicked()
    {
        if (idxStage >= 7)
            panelIncreaseTutor.gameObject.SetActive(false);
    }

    private void Dice_Increased(Dice dice)
    {
        if (idxStage == 6)
        {
            idxStage++;
            UpdateTutor();
        }
    }

    private void Dice_Clicked(Dice dice)
    {
        if(idxStage == 5 && dice.IncreaseStage > 0)
        {
            idxStage++;
            UpdateTutor();

            panelIncreaseTutor.gameObject.SetActive(true);
            tutorPanel.gameObject.SetActive(false);
            labelStones.SetActive(true);
            btnSplitting.gameObject.SetActive(true);
        }
    }

    private void Enemy_Skiped(Enemy enemy)
    {
        if (idxStage >= 7)
        {
            User.Data.tutorCompleted = true;
#if UNITY_ANDROID
            Saver.Save();
#endif

#if UNITY_WEBGL
            Saver.ConvertToYG();
            YG.YandexGame.SaveProgress();
#endif
        }
    }

    private void TutorPanel_Clicked()
    {
        if (idxStage == 3)
        {
            tutorPanel.gameObject.SetActive(false);
        }
    }

    private void Dice_Merged(Dice dice)
    {
        if (idxStage == 2)
        {
            idxStage++;
            UpdateTutor();
            isRunTimer = true;
        }

        if (idxStage == 4 && dice.IncreaseStage == 1)
        {
            idxStage++;
            UpdateTutor();
        }
    }

    private void MinePlayer_Spawned(Player player)
    {
        this.player = player;
    }

    private void BtnSpawnDice_Clicked()
    {
        if(idxStage == 0)
        {
            idxStage++;
            UpdateTutor();
        }

        if(idxStage == 1 && player.allDices.Count >= 4)
        {
            idxStage++;
            UpdateTutor();
            labelCoins.SetActive(true);
        }
    }

    void UpdateTutor()
    {
        string info = tutorInfos[idxStage].info;
        if (!Language.Rus)
            info = TutorInfo.GetEngText(idxStage);

        labelInfo.text = info;
        // HOT FIX
        labelInfoIncreaseTutor.text = info;
    }

    private void Update()
    {
        if (isRunTimer)
        {
            timer += Time.deltaTime;

            if(timer > 38)
            {
                tutorPanel.gameObject.SetActive(true);
                idxStage++;//4
                UpdateTutor();

                for (int i = 0; i < 2; i++)
                {
                    player.allDices.RemoveAll(d => !d);

                    var cell = player.allDices[i].Cell;
                    cell.IsEmpty = true;

                    Destroy(player.allDices[i].gameObject);

                    var newDice = player.SpawnDice(player.Prefabs[1]);
                    newDice.Init(player.user, Team.Mine, 5);
                }

                isRunTimer = false;
                timer = 0;
            }
        }

    }

    [Serializable]
    public class TutorInfo
    {
        [TextArea(8, 18)]
        public string info;

        public static string GetEngText(int idxTutor)
        {
            switch (idxTutor)
            {
                case 0:
                    return "Hi!\nLet me show you how to play.\n\n\nTo start, click on the green button to create a dice";

                case 1:
                    return "Great!\n\nBattle points are spent on creating a dice -BP.Create more dices.\n\nYou will get battle points for every enemy killed.";

                case 2:
                    return "To the left of the green button, your current number of battle points is displayed.\n\nNow let's improve your dice. Click on the red dice and connect it with the same dice.";

                case 3:
                    return "After connecting two identical cubes, a random dice with the number of dots +1 appears.\n\nPlay a little and I'll show you the main feature of this game";

                case 4:
                    return "Now let's imagine that you have two identical dice and each has 6 dots. Connect two of these dice.";

                case 5:
                    return "Excellent!\nNow you've got the awakened dice. Click on it to open the gain menu";
                
                case 6:
                    return "This is the dice enhancement menu.\nTo strengthen the dice, you need power stones.Power stones fall from enemies in PvP mode, you need to click on the stone to pick it up.\n\nClick the \"Increase\" button to try to improve the dice. The dice increases with a certain probability, with each level this probability is less.If an unsuccessful attempt to strengthen the dice, the chance of its improvement increases.";
                
                case 7:
                    return "Great, now you've learned the basics of the game!\n\nKeep fighting to win.\nClick on this message to close it";
            }

            return "";
        }
    }
}
