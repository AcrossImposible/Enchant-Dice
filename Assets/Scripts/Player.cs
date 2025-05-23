using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(100)]
public class Player : MonoBehaviour
{
    [SerializeField] List<Cell> cells;

    [HideInInspector]
    public List<Dice> allDices = new();
    public int Coins { get; set; } = 300;
    public int PriceDice { get; set; } = 50;
    public Dice[] Prefabs => dicesPrefab;

    protected Dice[] dicesPrefab;

    public Action onCoinsUpdated;
    public Action<Dice> onDiceSpawned;
    public User user;

    public Team team;
    List<int> idxDices = new();

    private void Awake()
    {
        if (!GameManager.Instance)
            return;

        dicesPrefab = DiceStorage.Single.allDices.ToArray();

        var isAI = GetComponent<AIBehaviour>();
        var isOffline = Photon.Pun.PhotonNetwork.CurrentRoom == null;
        if (isOffline)
        {
            if (!isAI)
            {
                EventsHolder.onSpawnDiceClicked.AddListener(SpawnDice_Clicked);
                EventsHolder.playerSpawnedMine?.Invoke(this);
                team = Team.Mine;
                user = User.Data;
            }
            else
            {
                team = Team.Other;
                user = CreateRandomBotProfile();
            }
        }
        else
        {
            if (this.IsMine())
            {
                EventsHolder.onSpawnDiceClicked.AddListener(SpawnDice_Clicked);
                EventsHolder.playerSpawnedMine?.Invoke(this);
                team = Team.Mine;
                user = User.Data;
            }
            else
            {
                team = Team.Other;
                if (isAI)
                    user = CreateRandomBotProfile();
            }
        }

        EventsHolder.playerSpawnedAny?.Invoke(this);

        EventsHolder.onEnemyAnniged.AddListener(Enemy_Anniged);
        EventsHolder.onEnemySkiped.AddListener(Enemy_Skiped);
        EventsHolder.onDiceDestroyed.AddListener(Dice_Destroyed);

        user.CalculateCrit();

        InitMineDeck();
    }

    private void Dice_Destroyed(Dice dice)
    {
        if (dice is Victim)
        {
            if (dice.Team == team)
            {
                Coins += 80 * (dice.Stage + 1);
            }
        }
    }

    void InitMineDeck()
    {
        foreach (var item in user.deck)
        {
            idxDices.Add(item.idx);
        } 
    }

    private void Enemy_Skiped(Enemy enemy)
    {
        allDices.ForEach(d => d.Stop());
    }

    private void Enemy_Anniged(Enemy enemy)
    {
        Coins += enemy.reward;
        onCoinsUpdated?.Invoke();
    }

    private void SpawnDice_Clicked()
    {
        SpawnDice();
    }

    public virtual Dice SpawnDice(Cell cell = null, int stage = 0)
    {
        if (!cell)
        {
            if (PriceDice > Coins)
                return null;

            cell = GetCell();

            if (cell == null)
                return null;

            Coins -= PriceDice;
            PriceDice += 10;

            onCoinsUpdated?.Invoke();
        }

        var idx = user.deck[Random.Range(0, user.deck.Count)].idx;
        var randomDice = dicesPrefab[idx];

        var dice = Instantiate(randomDice, cell.transform.position + Vector3.back, Quaternion.identity);
        dice.Init(user, team, stage);
        dice.Cell = cell;

        cell.IsEmpty = false;

        allDices.Add(dice);
        allDices.RemoveAll(d => !d);

        onDiceSpawned?.Invoke(dice);
        EventsHolder.onDiceSpawned?.Invoke(dice);

        dice.transform.localScale = Vector3.one * 0.3f;
        dice.transform.LeanScale(Vector3.one * 0.95f, 0.18f).setEaseOutQuad();

        return dice;
    }

    public virtual Dice SpawnDice(Dice prefab)
    {
        var cell = GetCell();

        return SpawnDice(prefab, cell);
    }

    public virtual Dice SpawnDice(Dice prefab, Cell cell, int stage = 0)
    {
        var randomDice = prefab;

        var dice = Instantiate(randomDice, cell.transform.position + Vector3.back, Quaternion.identity);
        dice.Init(user, team, stage);
        dice.Cell = cell;

        cell.IsEmpty = false;

        allDices.Add(dice);
        allDices.RemoveAll(d => !d);

        onDiceSpawned?.Invoke(dice);
        EventsHolder.onDiceSpawned?.Invoke(dice);

        return dice;
    }

    public Cell GetCell()
    {
        var emptyes = cells.FindAll(c => c.IsEmpty);
        if (emptyes.Count > 0)
            return emptyes[Random.Range(0, emptyes.Count)];
        else
            return null;
    }

    public Cell GetCell(int idx)
    {
        return cells.Find(c => c.Idx == idx);
    }

    User CreateRandomBotProfile()
    {
        User result = new();

        result.isBot = true;

        if (!User.Data.tutorCompleted)
        {
            return result;
        }

        List<int?> idxDeck = new();

        while(idxDeck.Count < 5)
        {
            var idx = Random.Range(0, dicesPrefab.Length);
            var found = idxDeck.Find(i => i.Value == idx);
            if(found == null)
            {
                idxDeck.Add(idx);
                //print(idx);
            }
        }

        for (int i = 0; i < idxDeck.Count; i++)
        {
            result.deck[i].idx = idxDeck[i].Value;
        }

        var minLvl = User.Data.inventory.Min(d => d.lvl);
        var maxLvl = User.Data.inventory.Max(d => d.lvl);

        for (int i = 0; i < idxDeck.Count; i++)
        {
            var diceData = result.inventory.Find(d => d.idx == idxDeck[i]);
            if(diceData == null)
            {
                diceData = new() { idx = idxDeck[i].Value };
                result.inventory.Add(diceData);
            }
            diceData.lvl = Random.Range(minLvl, maxLvl + 1);
        }

        return result;
    }

    
}
