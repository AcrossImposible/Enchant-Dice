using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class DiceStorage : MonoBehaviour
{
    public List<DiceGroup> groups;

    public List<Dice> allDices;

    static DiceStorage single;

    public static DiceStorage Single
    {
        get
        {
            if (single)
            {
                return single;
            }
            else
            {
                var prefab = Resources.Load<DiceStorage>("Dice Storage");
                single = Instantiate(prefab);
                single.InitDices();
                return single;
            }
        }
    }
    

    private void Awake()
    {
        single = single != null ? single : this;

        InitDices();
    }

    void InitDices()
    {
        if (allDices == null || allDices.Count == 0)
        {
            allDices = GetAllPrefabs();
        }
    }

    public List<Dice> GetAllPrefabs()
    {
        return groups
            .Where(g => g.prefabsInGroup != null)
            .SelectMany(g => g.prefabsInGroup)
            .OrderBy(d => d.idxInInventory)
            .ToList();
    }

    public Dice GetRandomDicePrefab()
    {
        // 1) Выбираем группу по весам groupWeight (как раньше)
        float totalGroupWeight = groups.Sum(g => Mathf.Max(0f, g.groupWeight));
        float r = Random.Range(0f, totalGroupWeight);
        DiceGroup chosenGroup = groups.Last();
        float accum = 0f;
        foreach (var g in groups)
        {
            float w = Mathf.Max(0f, g.groupWeight);
            if (r < accum + w)
            {
                chosenGroup = g;
                break;
            }
            accum += w;
        }

        // 2) Изнутри группы — равновероятный выбор префаба
        if (chosenGroup.prefabsInGroup.Count == 0) return null;
        int idx = Random.Range(0, chosenGroup.prefabsInGroup.Count);
        return chosenGroup.prefabsInGroup[idx];
    }

    [Serializable]
    public struct DiceGroup
    {
        public string groupName;
        public float groupWeight;
        public List<Dice> prefabsInGroup;
    }
}
