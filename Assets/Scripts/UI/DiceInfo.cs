using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DiceInfo : MonoBehaviour
{
    [SerializeField] InventoryDice inventoryDice;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text labelInfo;
    [SerializeField] Transform statsParent;
    [SerializeField] TMP_Text statPrefab;

    public void Init(Dice dice)
    {
        gameObject.SetActive(true);

        foreach (Transform item in statsParent) Destroy(item.gameObject);

        inventoryDice.Init(dice);

        title.text = GetLocalizationTitle(dice.title);
        labelInfo.text = GetLocalizationInfo(dice.info);

        var statDamage = Instantiate(statPrefab, statsParent);
        var diceData = User.Data.inventory.Find(d => d.idx == dice.idxInInventory);
        var damage = dice.baseDamage + (dice.stepDamage * diceData.lvl);
        string txtDmg = Language.Rus ? "Урон" : "Damage";
        statDamage.text = $"{txtDmg} {damage}";

        var statFireRate = Instantiate(statPrefab, statsParent);
        string txt = Language.Rus ? "Скорость стрельбы" : "Fire rate";
        string unit = Language.Rus ? "сек" : "sec";
        statFireRate.text = $"{txt} {dice.fireRate} {unit}";

        switch (dice)
        {
            case Fire f:
                txt = Language.Rus ? "Огненный урон" : "Fire damage";

                var statFire = Instantiate(statPrefab, statsParent);
                statFire.text = $"{txt} {f.fireDamage}";
                break;

            case Poison p:
                txt = Language.Rus ? "Урон от яда" : "Poison damage";

                var statPoison = Instantiate(statPrefab, statsParent);
                statPoison.text = $"{txt} {p.poisonDamage}";
                break;

            case Energy e:
                txt = Language.Rus ? "Электрический урон" : "Energy damage";

                var statEnergy = Instantiate(statPrefab, statsParent);
                statEnergy.text = $"{txt} {e.energyDamage}";
                break;
        }
    }

    string GetLocalizationTitle(string title)
    {
        if (!Language.Rus)
            return title;

        switch (title)
        {
            case "Iron":
                return "Железо";

            case "Poison":
                return "Яд";

            case "Adventurer":
                return "Авантюрист";

            case "Trooper":
                return "Штурмовик";

            case "Breakage":
                return "Поломка";

            case "Chameleon":
                return "Хамелеон";

            case "Energy":
                return "Электро";

            case "Fire":
                return "Огонь";

            case "Ice":
                return "Лед";

            //case "Energy":
            //    return "железо";

            default:
                return string.Empty;
        }
    }

    string GetLocalizationInfo(string info)
    {
        if (!Language.Rus)
            return info;

        switch (info)
        {
            case "Attacks the first enemy. Deals random damage from basic to critical":
                return "Атакует первого врага. Наносит случайный урон от базового до критического";

            case "Has a high rate of fire":
                return "Имеет повышенную скорость стрельбы";

            case "Attacks a random enemy.":
                return "Атакует случайного врага";

            case "Attacks the first enemy. Can be combined with any other dice with the same number of points.":
                return "Атакует первого врага. Может быть объединëн с любым другим кубиком с тем же количеством точек.";

            case "Attacks the first enemy, dealing base damage. The second enemy is dealt 70% of the damage from electricity, and 30% of the damage from electricity to the third.":
                return "Атакует первого врага, нанося базовый урон. Второму врагу наносится 70% урона от электричества, и 30% урона от электричества третьему.";

            case "Nearby enemies take fire damage across the area.":
                return "Ближайшие враги получают урон от огня по области.";

            case "Attacks the first enemy. When attacking, it freezes the enemy, slowing down his movement speed. Freezing is summed up to three times.":
                return "Атакует первого врага. При атаке замораживает противника, замедляя его скорость передвижения. Заморозка суммируется до трех раз.";

            case "Attacks the opponent with the most life points. Deals double damage to bosses and mini-bosses.":
                return "Атакует противника с самым большим количеством ОЗ. Наносит двойной урон боссам и мини-боссам.";

            case "Attacks the first undefeated enemy. When attacking, it puts poisoning on the monster, causing damage to it every second.":
                return "Атакует первого неотравленного врага. При атаке накладывает на монстра отравление, ежесекундно наносящее ему урон.";

            default:
                return string.Empty;
        }
    }
}
