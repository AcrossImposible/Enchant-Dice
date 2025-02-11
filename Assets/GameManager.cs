using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using static Settings;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(50)]
public class GameManager : MonoBehaviour
{
    [SerializeField] List<Dice> allDices;

    [Space]

    [SerializeField] Enemy enemyPrefab;
    [SerializeField] MiniBoss miniBossPrefab;
    [SerializeField] float spawnDelay;

    [Space]

    [SerializeField] bool spawnCreesp;

    [SerializeField] MultiplayerManager multiplayer;
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] public Transform[] spawnEnemyPoints;
    [Header(" ŒŒœ◊» ")] [Space(3)]
    [SerializeField] public Transform[] trajectoryPoints;
    [Header("›œ¿ÿ»ÀŒ¬Œ")] [Space(3)]
    [SerializeField] public Transform[] trajectoryMinePoints;
    [SerializeField] public Transform[] trajectoryOtherPoints;

    [field: SerializeField]
    public bool IsPVP { get; private set; }
    public int Wave { get; set; }

    public static GameManager Instance { get; private set; }

    
    public List<Player> allPlayers = new();
    public List<RespawnData> respawnQueue = new();
    public List<Enemy> allEnemies = new();

    public bool complete;
    public bool spawning;

    

    private void Awake()
    {
        Instance = this;

        multiplayer.onPlayerSpawned += Player_Spawned;

        Application.targetFrameRate = 60;

        EventsHolder.onEnemyAnniged.AddListener(Enemy_Anniged);
    }

    
    private void Start()
    {
        SpawnEnemies();

        if (IsPVP)
            spawnDelay /= 1.2f;

        SetCameraSize();
    }

    private void Enemy_Anniged(Enemy enemy)
    {
        allEnemies.Remove(enemy);

        if (allEnemies.Count == 0 && !spawning)
        {
            Wave++;

            SpawnEnemies();
            // HOT FIX
            UIStarter.Single.ActiveUI.ShowPanelWave();

            foreach (var player in allPlayers)
            {
                player.Coins += 30 * Wave;
            }
        }
    }

    void SpawnEnemies()
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            spawning = true;

            yield return new WaitForSeconds(3);

            for (int i = 0; i < (IsPVP ? 38 : 30); i++)
            {
                if (i == 10 && (Wave + 1) % 5 == 0)
                {
                    yield return new WaitForSeconds(spawnDelay);

                    Spawn(miniBossPrefab, Team.Mine, i);

                    yield return new WaitForSeconds(spawnDelay);

                    Spawn(miniBossPrefab, Team.Other, i);
                }
                    

                yield return new WaitForSeconds(spawnDelay);

                Spawn(enemyPrefab, Team.Mine, i);

                yield return new WaitForSeconds(spawnDelay);

                Spawn(enemyPrefab, Team.Other, i);
            }

            
            spawning = false;
        }

        void Spawn(Enemy prefab, Team team, int number)
        {
            var pos = spawnEnemyPoints[(int)team].position + new Vector3(0, 0, Random.Range(-50, -1));
            var enemy = Instantiate(prefab, pos, Quaternion.identity);
            if (IsPVP)
            {
                var trajectory = team == Team.Mine ? trajectoryMinePoints : trajectoryOtherPoints;
                enemy.Init(trajectory, number, team);
            }
            else
            {
                enemy.Init(trajectoryPoints, number, team);
            }

            allEnemies.Add(enemy);
        }
    }
    

    private void Player_Spawned(GameObject player)
    {
        var p = player.GetComponent<Player>();
        EventsHolder.playerSpawnedMine?.Invoke(p);
    }

    private void SetCameraSize()
    {
#if !UNITY_EDITOR
        if (Application.isMobilePlatform)
#endif
        {
            var resFactor = (float)Screen.height / (float)Screen.width;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(1.7f, 11);
            curve.AddKey(2.3f, 13.9f);

            Camera.main.orthographicSize = curve.Evaluate(resFactor);
        }
    }
    
}

[Serializable]
public class RespawnData
{
    public string name;
    public int viewID;
    public bool isAI;
    public int countAnniges;
    public int countAnniged;
    public int score;

    public float timeAnniged;

    //public int baseDamage;
    //public int critChance;
    //public int maxHealth;
    //public int evasion;
    //public float rateIfFire;
}

public enum CompleteStatus
{
    Victory,
    Defeat
}