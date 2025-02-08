using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using static Settings;

public class MultiplayerManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject aiPrefab;
    [SerializeField] bool useAI;

    [Header("*DEV*")]

    [SerializeField]
    TMPro.TMP_Text networkRole;

    bool applicationIsQuit;

    Dictionary<EventCode, EventAction> eventActions = new Dictionary<EventCode, EventAction>();

    public static MultiplayerManager Instance { get; set; }

    public static bool IsMaster => PhotonNetwork.IsMasterClient;
    public static int ServerTime => PhotonNetwork.ServerTimestamp;
    public static List<Player> Players { get; } = new List<Player>();

    public Action<GameObject> onPlayerSpawned;

    float currentGameTime = 0;

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }

        if (Instance != null)
        {
            Debug.LogError("Больше 1-го MultiplayerManager ");
        }

        Instance = this;
    }


    void Start()
    {
        SpawnPlayer();
        SpawnAI();

        PhotonPeer.RegisterType(typeof(EventHpContent), 012, SerializeEventHpContent, DeserializeEventHpContent);
        PhotonPeer.RegisterType(typeof(EventTurnProjectileContent), 011, SerializeEventTurnContent, DeserializeEventTurnContent);
        PhotonPeer.RegisterType(typeof(DirectionNetworkData), 016, SerializeDirectionNetworkData, DeserializeDirectionNetworkData);
        PhotonPeer.RegisterType(typeof(IntOwnerNetworkData), 019, SerializeWeaponPickedNetworkData, DeserializeWeaponPickedNetworkData);
        PhotonPeer.RegisterType(typeof(IntIntOwnerNetworkData), 020, SerializeIIONetworkData, DeserializeIIONetworkData);
        PhotonPeer.RegisterType(typeof(IntIntIntOwnerNetworkData), 021, SerializeIIIONetworkData, DeserializeIIIONetworkData);

        //PhotonPeer.RegisterType(typeof(CreateDiceParams), 017, SerializeCreateDiceParams, DeserializeCreateDiceParams);

        if (networkRole)
            networkRole.text = $"{PhotonNetwork.NetworkClientState}";

        currentGameTime = 0;

    }

    private void Update()
    {
        currentGameTime += Time.deltaTime;
        
        //if(currentGameTime > 60 * 5 && PhotonNetwork.CurrentRoom != null)
        //{
        //    PhotonNetwork.CurrentRoom.IsOpen = false;
        //    PhotonNetwork.CurrentRoom.IsVisible = false;
        //}
    }

    public static void SpawnPlayer()
    {
        //int number = PhotonNetwork.LocalPlayer.ActorNumber;
        //int idx = number % 2;
        
        var playerPos = GameManager.Instance.spawnPoints[0].position;

        //PhotonNetwork.Instantiate(Instance.playerPrefab.name, playerPos, Quaternion.identity);

        GameObject player;
        if (PhotonNetwork.CurrentRoom == null)
        {
            player = FindObjectOfType<Player>()?.gameObject ?? null;
            if (!player)
                player = Instantiate(Instance.playerPrefab, playerPos, Quaternion.identity);
        }
        else
        {
            player = PhotonNetwork.Instantiate(Instance.playerPrefab.name, playerPos, Quaternion.identity);
        }

        //Instance.StartCoroutine(Delay());

        //IEnumerator Delay()
        //{
        //    yield return new WaitForSeconds(0.3f);

        //}

        Instance.onPlayerSpawned?.Invoke(player);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

    }

    void SpawnAI()
    {
        if (PhotonNetwork.PlayerList.Length > 1)
            return;

        List<int> usedProfiles = new();

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1f);

            var ai = Instantiate(aiPrefab);
            ai.transform.position = GameManager.Instance.spawnPoints[1].position;
        }
    }

    public static void AddPlayer(Player player) => Players.Add(player);
    

    public static T Spawn<T>(T prefab, Vector3 position)
    {
        var prefabName = (prefab as MonoBehaviour).name;
        var spawned = PhotonNetwork.Instantiate(prefabName, position, Quaternion.identity);

        return spawned.GetComponent<T>();
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position)
    {
        return PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity);
    }

    public static void DestroyByPhoton(GameObject target)
    {
        PhotonNetwork.Destroy(target);
    }

    public static void RaiseEvent<T>(EventCode eventCode, T data, int targetActor)
    {
        RaiseEventOptions option = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        option.TargetActors = new int[] { targetActor };
        PhotonNetwork.RaiseEvent((byte)eventCode, data, option, sendOptions);
    }

    public static void RaiseEvent<T>(EventCode eventCode, T data)
    {
        RaiseEventOptions option = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)eventCode, data, option, sendOptions);
    }

    public static void RaiseEvent(PhotonView actor, EventCode eventCode)
    {
        RaiseEventOptions option = new() { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new() { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)eventCode, actor.Owner.ActorNumber, option, sendOptions);
    }

    public static void RaiseEvent<T>(PhotonView actor, EventCode eventCode, T data)
    {
        RaiseEventOptions option = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        option.TargetActors = new int[] { actor.OwnerActorNr };
        PhotonNetwork.RaiseEvent((byte)eventCode, data, option, sendOptions);
    }

    //Dictionary<PhotonView, Dictionary<EventCode, Action>> ActorMethods = new Dictionary<PhotonView, Dictionary<EventCode, Action>>();

    Dictionary<byte, List<ActorMethods>> actorMethods = new Dictionary<byte, List<ActorMethods>>();

    public class ActorMethods
    {
        public PhotonView actor;
        public Action method;
        public bool allActors;
        //public Action<object> objectMethod;
    }

    public class ParamMethods<T> : ActorMethods
    {
        public Action<T> paramMethod;
    }

    //public static void RegisterMethod(PhotonView actor, EventCode eventCode, Action<object> method)
    //{
    //    var actorMethods = Instance.actorMethods;
    //    byte code = (byte)eventCode;
    //    if (actorMethods.ContainsKey(code))
    //    {
    //        var actors = actorMethods[code];
    //        ActorMethods am = null;
    //        foreach (var item in actors)
    //        {
    //            if (item.actor == actor) am = item as ActorMethods;
    //        }
    //        if (am == null)
    //        {
    //            var newActor = new ActorMethods { actor = actor, objectMethod = method };
    //            actors.Add(newActor);
    //        }
    //    }
    //    else
    //    {
    //        var newActor = new ActorMethods { actor = actor, objectMethod = method };
    //        actorMethods.Add(code, new List<ActorMethods> { newActor });
    //    }
    //}

    public static void RegisterMethod<T>(PhotonView actor, EventCode eventCode, Action<T> method, bool allActros)
    {
        var actorMethods = Instance.actorMethods;
        byte code = (byte)eventCode;
        if (actorMethods.ContainsKey(code))
        {
            var actors = actorMethods[code];
            ParamMethods<T> am = null;
            foreach (var item in actors)
            {
                if (item.actor == actor) am = item as ParamMethods<T>;
            }
            if (am == null)
            {
                var newActor = new ParamMethods<T> { actor = actor, paramMethod = method, allActors = allActros };
                actors.Add(newActor);
            }
        }
        else
        {
            var newActor = new ParamMethods<T> { actor = actor, paramMethod = method, allActors = allActros };
            actorMethods.Add(code, new List<ActorMethods> { newActor });
        }
    }

    public static void RegisterMethod(PhotonView actor, EventCode eventCode, Action method)
    {
        var actorMethods = Instance.actorMethods;
        byte code = (byte)eventCode;
        if (actorMethods.ContainsKey(code))
        {
            var actors = actorMethods[code];
            ActorMethods am = null;
            foreach (var item in actors)
            {
                if (item.actor == actor) am = item;
            }
            if (am == null)
            {
                var newActor = new ActorMethods { actor = actor, method = method };
                actors.Add(newActor);
            }
        }
        else
        {
            var newActor = new ActorMethods { actor = actor, method = method };
            actorMethods.Add(code, new List<ActorMethods> { newActor });
        }
    }

    //public static void RaiseEvent(EventCode code, object content)
    //{
    //    RaiseEventOptions option = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
    //    SendOptions sendOptions = new SendOptions { Reliability = true };
    //    PhotonNetwork.RaiseEvent((byte)code, content, option, sendOptions);
    //}

    public static void RaiseEvent(EventCode code, object content, ReceiverGroup receiverGroup)
    {
        RaiseEventOptions option = new RaiseEventOptions { Receivers = receiverGroup };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)code, content, option, sendOptions);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //Player player = GameManager.Instance.allPlayers.First(p => p.GetComponent<PhotonView>().Owner == null);
        //GameManager.Instance.allPlayers.Remove(player);

        //if (player.GetComponent<PhotonView>().IsMine == true && PhotonNetwork.IsConnected == true)//возможно проверка лишняя но все работает норм
        //{
        //    PhotonNetwork.Destroy(player.gameObject);
        //}

        //    EventSystem.TriggerEvent(EventKey.PlayerLeave);
    }

    

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        if (!applicationIsQuit)
        {
            SceneManager.LoadScene(0);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        EventsHolder.onBlyatDisconect?.Invoke();
        print($"це пязда {cause}");
    }

    private void OnApplicationQuit()
    {
        applicationIsQuit = true;
    }

    public void OnEvent(EventData photonEvent)
    {
        if (actorMethods.ContainsKey(photonEvent.Code))
        {
            var actors = actorMethods[photonEvent.Code];

            foreach (var actor in actors)
            {
                if (actor.actor == null)
                {
                    continue;
                }

                if (actor.actor.Owner.ActorNumber == photonEvent.Sender || actor.allActors)
                {
                    actor.method?.Invoke();

                    var data = photonEvent.CustomData;

                    if (data is float)
                    {
                        (actor as ParamMethods<float>).paramMethod?.Invoke((float)photonEvent.CustomData);
                    }
                    if (data is int)
                    {
                        (actor as ParamMethods<int>).paramMethod?.Invoke((int)photonEvent.CustomData);
                    }
                    if (data is string)
                    {
                        (actor as ParamMethods<string>).paramMethod?.Invoke((string)photonEvent.CustomData);
                    }
                    if (data is Vector2)
                    {
                        (actor as ParamMethods<Vector2>).paramMethod?.Invoke((Vector2)photonEvent.CustomData);
                    }
                    else if (data is DirectionNetworkData)
                    {
                        (actor as ParamMethods<DirectionNetworkData>).paramMethod?.Invoke((DirectionNetworkData)photonEvent.CustomData);
                    }
                    else if (data is IntOwnerNetworkData)
                    {
                        (actor as ParamMethods<IntOwnerNetworkData>).paramMethod?.Invoke((IntOwnerNetworkData)photonEvent.CustomData);
                    }
                    else if (data is IntIntOwnerNetworkData)
                    {
                        (actor as ParamMethods<IntIntOwnerNetworkData>).paramMethod?.Invoke((IntIntOwnerNetworkData)photonEvent.CustomData);
                    }
                    else if (data is IntIntIntOwnerNetworkData)
                    {
                        (actor as ParamMethods<IntIntIntOwnerNetworkData>).paramMethod?.Invoke((IntIntIntOwnerNetworkData)photonEvent.CustomData);
                    }
                }

            }
        }
      
    }


    #region --== Serialize / Deserialize Photon Data ==--
    public static object DeserializeEventHpContent(byte[] data)
    {
        EventHpContent result = new EventHpContent();
        result.viewId = BitConverter.ToInt32(data, 0);
        result.hp = BitConverter.ToInt32(data, 4);

        return result;
    }

    public static byte[] SerializeEventHpContent(object obj)
    {
        EventHpContent ehc = (EventHpContent)obj;
        byte[] result = new byte[8];

        BitConverter.GetBytes(ehc.viewId).CopyTo(result, 0);
        BitConverter.GetBytes(ehc.hp).CopyTo(result, 4);

        return result;
    }

    public static object DeserializeEventTurnContent(byte[] data)
    {
        EventTurnProjectileContent result = new EventTurnProjectileContent
        {
            ActorNumber = BitConverter.ToInt32(data, 0),
            zAngle = BitConverter.ToInt32(data, 4)
        };
        return result;
    }

    public static byte[] SerializeEventTurnContent(object obj)
    {
        EventTurnProjectileContent ehc = (EventTurnProjectileContent)obj;
        byte[] result = new byte[8];
        BitConverter.GetBytes(ehc.ActorNumber).CopyTo(result, 0);
        BitConverter.GetBytes(ehc.zAngle).CopyTo(result, 4);
        return result;
    }

    //public static object DeserializeCreateDiceParams(byte[] data)
    //{
    //    CreateDiceParams result = new CreateDiceParams
    //    {
    //        stage = BitConverter.ToInt32(data, 0),
    //        kind = BitConverter.ToInt32(data, 4),
    //        pos = new Vector2
    //        {
    //            x = BitConverter.ToSingle(data, 8),
    //            y = BitConverter.ToSingle(data, 12),
    //        }
    //    };
    //    return result;
    //}

    //public static byte[] SerializeCreateDiceParams(object obj)
    //{
    //    CreateDiceParams cdp = (CreateDiceParams)obj;
    //    byte[] result = new byte[16];
    //    BitConverter.GetBytes(cdp.stage).CopyTo(result, 0);
    //    BitConverter.GetBytes(cdp.kind).CopyTo(result, 4);
    //    BitConverter.GetBytes(cdp.pos.x).CopyTo(result, 8);
    //    BitConverter.GetBytes(cdp.pos.y).CopyTo(result, 12);
    //    return result;
    //}

    public static object DeserializeIIONetworkData(byte[] data)
    {
        IntIntOwnerNetworkData result = new()
        {
            value_1 = BitConverter.ToInt32(data, 0),
            value_2 = BitConverter.ToInt32(data, 4),
            viewID  = BitConverter.ToInt32(data, 8)
        };
        return result;
    }

    public static byte[] SerializeIIONetworkData(object obj)
    {
        IntIntOwnerNetworkData data = (IntIntOwnerNetworkData)obj;
        byte[] result = new byte[12];
        BitConverter.GetBytes(data.value_1).CopyTo(result, 0);
        BitConverter.GetBytes(data.value_2).CopyTo(result, 4);
        BitConverter.GetBytes(data.viewID).CopyTo(result, 8);
        return result;
    }

    public static object DeserializeIIIONetworkData(byte[] data)
    {
        IntIntIntOwnerNetworkData result = new()
        {
            value_1 = BitConverter.ToInt32(data, 0),
            value_2 = BitConverter.ToInt32(data, 4),
            value_3 = BitConverter.ToInt32(data, 8),
            viewID  = BitConverter.ToInt32(data, 12)
        };
        return result;
    }

    public static byte[] SerializeIIIONetworkData(object obj)
    {
        var data = (IntIntIntOwnerNetworkData)obj;
        byte[] result = new byte[16];
        BitConverter.GetBytes(data.value_1).CopyTo(result, 0);
        BitConverter.GetBytes(data.value_2).CopyTo(result, 4);
        BitConverter.GetBytes(data.value_3).CopyTo(result, 8);
        BitConverter.GetBytes(data.viewID).CopyTo(result, 12);
        return result;
    }
    #endregion

    #region Direction/Vector network data S/D
    public static object DeserializeDirectionNetworkData(byte[] data)
    {
        DirectionNetworkData result = new DirectionNetworkData
        {

            value = new Vector2
            {
                x = BitConverter.ToSingle(data, 0),
                y = BitConverter.ToSingle(data, 4),
            },
            viewID = BitConverter.ToInt32(data, 8)
        };
        return result;
    }

    public static byte[] SerializeDirectionNetworkData(object obj)
    {
        DirectionNetworkData dir = (DirectionNetworkData)obj;
        byte[] result = new byte[12];
        BitConverter.GetBytes(dir.value.x).CopyTo(result, 0);
        BitConverter.GetBytes(dir.value.y).CopyTo(result, 4);
        BitConverter.GetBytes(dir.viewID).CopyTo(result, 8);
        return result;
    }
    #endregion

    #region WeaponPicked network data S/D
    public static object DeserializeWeaponPickedNetworkData(byte[] data)
    {
        IntOwnerNetworkData result = new()
        {
            value = BitConverter.ToInt32(data, 0),
            viewID = BitConverter.ToInt32(data, 4)
        };
        return result;
    }

    public static byte[] SerializeWeaponPickedNetworkData(object obj)
    {
        IntOwnerNetworkData data = (IntOwnerNetworkData)obj;
        byte[] result = new byte[8];
        BitConverter.GetBytes(data.value).CopyTo(result, 0);
        BitConverter.GetBytes(data.viewID).CopyTo(result, 4);
        return result;
    }
    #endregion

}

public class EventTurnProjectileContent
{
    public float zAngle;
    public int ActorNumber;
}

public class EventHpContent
{
    public int viewId;
    public int hp;
}

public enum EventCode
{
    DiceSpawned = 1,
    DiceMerged = 3,
    DiceDestroyed = 4,
    UserData = 5,
    UserCrit = 6,
    Anniged    = 11,
    PerkPick   = 12,
    RuneTaked = 15,
    RuneEnded = 16,
    Profile    = 20,
    RespawnPlayer  = 19,
    RespawnProfiles = 21,
    RespawnAnniges  = 22,
    RespawnAnniged  = 23,
    RespawnDataEnd  = 29,
    RespawnPerk     = 27,
    RespawnTeam     = 26,
    ClientCompleteData   = 25,
 
}

public class EventAction
{
    public int actorNumber;
    public Action action;
}


