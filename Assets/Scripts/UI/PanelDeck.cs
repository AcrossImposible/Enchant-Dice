using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using System;

public class PanelDeck : MonoBehaviour
{
    [SerializeField] Transform inventorySlotPrefab;
    [SerializeField] InventoryDice dicePrefab;
    [SerializeField] CardDice cardDicePrefab;
    [SerializeField] Transform inventoryParent;
    [SerializeField] Transform deckParent;
    [SerializeField] DiceInfo diceInfo;
    [SerializeField] TMP_Text labelCrit;
    
    List<Dice> allDices;
    List<InventoryDice> dicesDeck = new();
    InventoryDice movedDice;

    float timeMoveDice;

    public void Init(List<Dice> allDices)
    {
        this.allDices = allDices;

        for (int i = 0; i < 5; i++)
        {
            var slot = Instantiate(inventorySlotPrefab, deckParent);
            var dice = Instantiate(dicePrefab, slot);
            var diceData = allDices[User.Data.deck[i].idx];
            dice.Init(diceData);
            dicesDeck.Add(dice);
        }

        diceInfo.gameObject.SetActive(false);

        UpdateInventoryCards();
    }


    public void UpdateInventoryCards()
    {
        foreach (Transform item in inventoryParent) Destroy(item.gameObject);

        List<int> notInInventory = new();
        for (int i = 0; i < allDices.Count; i++)
        {
            var dice = User.Data.inventory.Find(d => d.idx == i);
            if(dice == null)
            {
                notInInventory.Add(i);
            }
        }

        var inventory = User.Data.inventory;
        for (int i = 0; i < inventory.Count; i++)
        {
            var card = Instantiate(cardDicePrefab, inventoryParent);
            card.Init(inventory[i], allDices[inventory[i].idx]);
            card.onUpgraded += Dice_Upgraded;
        }

        for (int i = 0; i < notInInventory.Count; i++)
        {
            Instantiate(cardDicePrefab, inventoryParent);
        }

        CreateFakeCard();

        UpdateCriticalLabel();

#if UNITY_ANDROID
        Saver.Save();
#endif
    }

    void CreateFakeCard()
    {
        for (int i = 0; i < 8; i++)
        {
            Instantiate(cardDicePrefab, inventoryParent);
        }
    }

    private void Dice_Upgraded()
    {
        UpdateCriticalLabel();
    }

    void UpdateCriticalLabel()
    {
        User.Data.CalculateCrit();
        string txt = Language.Rus ? "Критический урон" : "Critical damage";
        labelCrit.text = $"{txt} {User.Data.crit}%";
    }

    private void Update()
    {
        List<RaycastResult> hits = new();
        PointerEventData ped = new(EventSystem.current) { position = Input.mousePosition };
        EventSystem.current.RaycastAll(ped, hits);


        if (Input.GetMouseButtonDown(0))
        {
            if (!diceInfo.gameObject.activeSelf)
            {
                foreach (var item in hits)
                {
                    var dice = item.gameObject.GetComponent<InventoryDice>();
                    if (dice && !dice.transform.parent.parent.GetComponent<Deck>())
                    {
                        movedDice = dice;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (movedDice)
            {
                movedDice.TransformUI.position = Input.mousePosition;
                timeMoveDice += Time.deltaTime;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!movedDice)
                return;

            if(timeMoveDice < 0.3f)
            {
                diceInfo.Init(allDices.Find(d => d.Color == movedDice.Color));
                DiceMoveBack(movedDice);
                movedDice = null;
                timeMoveDice = 0;
                return;
            }

            timeMoveDice = 0;

            foreach (var hit in hits)
            {
                var dice = hit.gameObject.GetComponent<InventoryDice>();

                if (dice && dice != movedDice && dice.transform.GetComponentInParent<Deck>())
                {
                    var slot = dice.transform.parent;

                    var sameDice = dicesDeck.Find(d => d.Color == movedDice.Color);
                    if (sameDice)
                    {
                        var prefabSame = allDices.Find(d => d.Color == sameDice.Color);
                        var idxSame = allDices.IndexOf(prefabSame);

                        var prefabOther = allDices.Find(d => d.Color == dice.Color);
                        var idxOther = allDices.IndexOf(prefabOther);
                        
                        User.Data.deck[IndexInUiDeck(dice)].idx = idxSame;
                        User.Data.deck[IndexInUiDeck(sameDice)].idx = idxOther; 

                        var p = sameDice.transform.parent;
                        dice.transform.parent = p;
                        DiceMoveBack(dice, 1000);
                        dicesDeck.Remove(sameDice);
                        Destroy(sameDice.gameObject);

                        var newDice = Instantiate(movedDice, slot);
                        newDice.TransformUI.localPosition = Vector3.zero;
                        dicesDeck.Add(newDice);

#if UNITY_ANDROID
                        Saver.Save();
#endif

#if UNITY_WEBGL
                        Saver.ConvertToYG();
                        YG.YandexGame.SaveProgress();
#endif
                    }
                    else
                    {
                        var prefab = allDices.Find(d => d.Color == movedDice.Color);
                        var idx = allDices.IndexOf(prefab);

                        User.Data.deck[IndexInUiDeck(dice)].idx = idx;

                        dicesDeck.Remove(dice);
                        Destroy(dice.gameObject);
                        var newDice = Instantiate(movedDice, slot);
                        newDice.TransformUI.localPosition = Vector3.zero;
                        dicesDeck.Add(newDice);

#if UNITY_ANDROID
                        Saver.Save();
#endif

#if UNITY_WEBGL
                        Saver.ConvertToYG();
                        YG.YandexGame.SaveProgress();
#endif
                    }

                    break;
                }
            }
            DiceMoveBack(movedDice);
            movedDice = null;
        }
    }

    void DiceMoveBack(InventoryDice dice, int speed = 3000)
    {
        StartCoroutine(Duration());

        IEnumerator Duration()
        {
            while (dice.TransformUI.localPosition.magnitude > 0.1f)
            {
                var t = dice.TransformUI;
                t.localPosition = Vector2.MoveTowards(t.localPosition, Vector2.zero, Time.deltaTime * speed);

                yield return null;
            }

            dice.TransformUI.localPosition = Vector2.zero;
        }
    }

    int IndexInUiDeck(InventoryDice dice)
    {
        for (int i = 0; i < deckParent.childCount; i++)
        {
            if (deckParent.GetChild(i).GetComponentInChildren<InventoryDice>() == dice)
                return i;
        }

        return -1;
    }
}
