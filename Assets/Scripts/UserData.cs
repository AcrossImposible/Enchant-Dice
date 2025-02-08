using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class User
{
    static User data;
    public static User Data
    {
        get
        {
            if (data == null)
            {
                data = new();
            }

            return data;
        }

        set => data = value;
    }

    public int countCards;
    public int lvl = 1;
    public int exp;
    public int countStones = 100;
    public int golda;
    public int ExpToNextLvl => ((int)(Mathf.Pow(lvl * 18, Settings.EXP_EXPONENTIATION) / 10)) * 10;
    public int crit;
    

    public List<Dice> deck = new()
    {
        new() { idx = 0 },
        new() { idx = 1 },
        new() { idx = 2 },
        new() { idx = 3 },
        new() { idx = 4 },
    };

    public List<Dice> inventory = new()
    {
        new() { idx = 0 },
        new() { idx = 1 },
        new() { idx = 2 },
        new() { idx = 3 },
        new() { idx = 4 },
    };

    public bool tutorCompleted;
    public bool isBot;

    public void CalculateCrit()
    {
        crit = 100;
        inventory.ForEach(d => crit += (d.lvl - 1) * 3);
    }

    [System.Serializable]
    public class Dice
    {
        public int idx;
        public int count;
        public int lvl = 1;
    }
}
