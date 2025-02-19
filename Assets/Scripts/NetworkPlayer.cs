using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    Player player;
    int idxUserDiceData;

    private void Start()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        player = GetComponent<Player>();

        EventsHolder.onDiceMerged.AddListener(Dice_Merged);
        EventsHolder.onDiceDestroyed.AddListener(Dice_Destroyed);
        
        if (!photonView.IsMine)
        {
            var playerPos = GameManager.Instance.spawnPoints[1].position;
            transform.position = playerPos;

            photonView.RegisterMethod<IntIntIntOwnerNetworkData>(EventCode.DiceSpawned, SpawnDice_Received);
            photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.DiceDestroyed, DiceDestroy_Received);
            photonView.RegisterMethod<IntIntOwnerNetworkData>(EventCode.DiceMerged, DiceMerge_Received);
            photonView.RegisterMethod<IntIntOwnerNetworkData>(EventCode.UserData, UserData_Received);
            photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.UserCrit, UserCrit_Received);
        }
        else
        {
            player.onDiceSpawned += Dice_Spawned;
        }

        SendUserData();
    }

    private void UserCrit_Received(IntOwnerNetworkData data)
    {
        if(photonView.ViewID == data.viewID)
        {
            print(data.value);
            player.user.crit = data.value;
        }
    }

    private void UserData_Received(IntIntOwnerNetworkData data)
    {
        if (data.viewID == photonView.ViewID)
        {
            player.user.deck[idxUserDiceData].idx = data.value_1;
            var dice = player.user.inventory.Find(d => d.idx == data.value_1);
            if (dice == null)
            {
                dice = new() { idx = data.value_1, lvl = data.value_2 };
                player.user.inventory.Add(dice);
            }
            else
            {
                dice.lvl = data.value_2;
            }
        }

        idxUserDiceData++;
    }

    private void DiceDestroy_Received(IntOwnerNetworkData data)
    {
        if (data.viewID == photonView.ViewID)
        {
            player.allDices.RemoveAll(d => !d);

            print(player.team);

            var dice = player.allDices.Find
            (
                d =>
                {
                    //print(d);
                    //print(d.Cell);
                    //print(data.value);
                    return d.Cell.Idx == data.value; 
                }
            );
            dice.Cell.IsEmpty = true;
            Destroy(dice.gameObject);
        }
    }

    private void Dice_Destroyed(Dice dice)
    {
        if(dice.Team == player.team)
        {
            IntOwnerNetworkData data = new()
            {
                value = dice.Cell.Idx,
                viewID = photonView.ViewID,
            };

            photonView.RaiseEvent(EventCode.DiceDestroyed, data);
        }
    }

    private void DiceMerge_Received(IntIntOwnerNetworkData data)
    {
        if (photonView.ViewID == data.viewID)
        {
            StartCoroutine(Delay());
            // HOT FIX, решение отпралять мердж дайсов одним пакетом
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(0.15f);

                var dice = player.allDices.Find(d => d && d.Cell.Idx == data.value_2);
                dice.Init(player.user, player.team, data.value_1);
            }
        }
    }

    private void Dice_Merged(Dice dice)
    {
        if(dice.Team == player.team)
        {
            IntIntOwnerNetworkData data = new()
            {
                value_1 = dice.Stage,
                value_2 = dice.Cell.Idx,
                viewID = photonView.ViewID,
            };

            photonView.RaiseEvent(EventCode.DiceMerged, data);
        }
    }

    private void SpawnDice_Received(IntIntIntOwnerNetworkData data)
    {
        if (photonView.ViewID == data.viewID)
        {
            var cell = player.GetCell(data.value_2);
            var prefab = player.Prefabs[data.value_1];
            player.SpawnDice(prefab, cell, data.value_3);
        }
    }

    private void Dice_Spawned(Dice dice)
    {
        IntIntIntOwnerNetworkData data = new() 
        { 
            value_1 = dice.idxInInventory, 
            value_2 = dice.Cell.Idx,
            value_3 = dice.Stage,
            viewID = photonView.ViewID 
        };
        
        photonView.RaiseEvent(EventCode.DiceSpawned, data);
    }

    private void SendUserData()
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.18f);

            foreach (var item in player.user.deck)
            {
                yield return new WaitForSeconds(0.18f);

                IntIntOwnerNetworkData data = new()
                {
                    value_1 = item.idx,
                    value_2 = player.user.inventory.Find(d => d.idx == item.idx).lvl,
                    viewID = photonView.ViewID
                };

                photonView.RaiseEvent(EventCode.UserData, data);
            }

            yield return new WaitForSeconds(0.18f);

            IntOwnerNetworkData critData = new()
            {
                value = player.user.crit,
                viewID = photonView.ViewID
            };

            photonView.RaiseEvent(EventCode.UserCrit, critData);
        }
    }
}
