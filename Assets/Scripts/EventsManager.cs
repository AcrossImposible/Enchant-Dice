using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.Events;

public class EventsHolder
{

    public class PlayerSpawned : UnityEvent<Player> { }

    public static PlayerSpawned playerSpawnedMine = new();

    //-----------------------------------------------------------------------

    public class BtnPvPClicked : UnityEvent<Button> { }

    public static BtnPvPClicked onBtnPvPClicked = new();

    //-----------------------------------------------------------------------

    public class PlayerSpawnedAny : UnityEvent<Player> { }

    public static PlayerSpawnedAny playerSpawnedAny = new PlayerSpawnedAny();

    //-----------------------------------------------------------------------

    public class DiceDestroyed : UnityEvent<Dice> { }

    public static DiceDestroyed onDiceDestroyed = new();

    //-----------------------------------------------------------------------

    public class RightJoystickMoved : UnityEvent<Vector2> { }

    public static RightJoystickMoved rightJoystickMoved = new RightJoystickMoved();

    //-----------------------------------------------------------------------

    public class RightJoystickUp : UnityEvent { }

    public static RightJoystickUp rightJoystickUp = new RightJoystickUp();

    //-----------------------------------------------------------------------

    public class JumpClicked : UnityEvent { }

    public static JumpClicked jumpClicked = new JumpClicked();

    //-----------------------------------------------------------------------

    public class PlayerAnniged : UnityEvent<Player> { }

    public static PlayerAnniged playerAnniged = new ();

    ////-----------------------------------------------------------------------

    public class SpawnDiceClicked : UnityEvent { }

    public static SpawnDiceClicked onSpawnDiceClicked = new();

    ////-----------------------------------------------------------------------


    public class DiceClicked : UnityEvent<Dice> { }

    public static DiceClicked onDiceClicked = new();

    ////-----------------------------------------------------------------------

    public class EnemyAnniged : UnityEvent<Enemy> { }

    public static EnemyAnniged onEnemyAnniged = new();

    ////-----------------------------------------------------------------------

    public class EnemySkiped : UnityEvent<Enemy> { }

    public static EnemySkiped onEnemySkiped = new();

    ////-----------------------------------------------------------------------

    public class PowerStoneClicked : UnityEvent { }

    public static PowerStoneClicked onPowerStoneClicked = new();

    ////-----------------------------------------------------------------------

    public class ClientGameCompleted : UnityEvent { }

    public static ClientGameCompleted clientGameCompleted = new();

    ////-----------------------------------------------------------------------

    public class SplittingModeUpdated : UnityEvent<bool> { }

    public static SplittingModeUpdated onSplittingModeUpdated = new();

    ////-----------------------------------------------------------------------

    public class DiceToSplitClicked : UnityEvent<Dice> { }

    public static DiceToSplitClicked onDiceToSplitClicked = new();

    //-----------------------------------------------------------------------

    public class DiceSpawned : UnityEvent<Dice> { }

    public static DiceSpawned onDiceSpawned = new();

    //-----------------------------------------------------------------------

    public class ClientGameCompleteDataReceived : UnityEvent { }

    public static ClientGameCompleteDataReceived clientCompleteDataReceived = new();

    ////-----------------------------------------------------------------------

    
    public class CamClicked : UnityEvent { }

    public static CamClicked onCamClicked = new();

    ////-----------------------------------------------------------------------

    public class DiceMerged : UnityEvent<Dice> { }

    public static DiceMerged onDiceMerged = new();

    ////-----------------------------------------------------------------------

    public class DiceIncreased : UnityEvent<Dice> { }

    public static DiceIncreased onDiceIncreased = new();

    ////-----------------------------------------------------------------------


    public class RuneTaked : UnityEvent<Player> { }

    public static RuneTaked runeTaked = new();

    ////-----------------------------------------------------------------------

    public class RuneEnded : UnityEvent<Player> { }

    public static RuneEnded runeEnded = new();

    ////-----------------------------------------------------------------------

    public class DisconectBlat : UnityEvent { }

    public static DisconectBlat onBlyatDisconect = new();
}
