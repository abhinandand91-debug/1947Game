// Author:  Abhinandan Dharmadhikari
// Created: 2017-12-13

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the basics of the game such as spawning and communicating with the user interface
/// </summary>
public class GameManager : MonoBehaviour
{

    [Header("General")]
    public UserInterfaceManager UserInterface;

    [Header("Spawning")]
    public bool SpawnEnemies;
    public GameObject Player;
    public List<GameObject> EnemyPrefabs = new List<GameObject>();
    public List<GameObject> SpawnPoints = new List<GameObject>();

    private int _score;
    private int _kills;
    private int _waveKills;
    private int _wave;
    private bool _bossAlive;
    private int _combo;
    private float _lastKillTime;

    private const float ComboWindow = 2.5f; // seconds allowed between kills to keep a combo
    private const int KillsPerLevel = 3;    // kills in a combo for each extra multiplier
    private const int MaxMultiplier = 5;

    /// <summary>
    /// Shows the name of the current wave and warns when the boss arrives.
    /// </summary>
    private void AnnounceWave()
    {
        WaveDefinition wave = WaveConfig.Get(_wave);
        if (wave.IsBoss)
        {
            ScreenFx.Banner("BOSS INCOMING!", new Color(1f, 0.4f, 0.4f));
            Sfx.Play("boss_alarm");
        }
        else
        {
            ScreenFx.Banner("WAVE " + (_wave + 1) + ": " + wave.Name, Color.white);
        }
    }

    void Update()
    {
        if (_combo == 0)
        {
            return;
        }

        float remaining = 1f - (Time.time - _lastKillTime) / ComboWindow;
        if (remaining <= 0f)
        {
            _combo = 0;
            ScreenFx.SetCombo(1, 0f);
            return;
        }
        ScreenFx.SetCombo(Mathf.Min(MaxMultiplier, 1 + _combo / KillsPerLevel), remaining);
    }

    private void SpawnShip(GameObject prefab, Vector3 position, WaveDefinition stats, bool boss)
    {
        GameObject spawned = (GameObject)GameObject.Instantiate(prefab, position, Quaternion.identity);
        Enemy spawnedEnemy = spawned.GetComponent<Enemy>();
        if (spawnedEnemy != null)
        {
            spawnedEnemy.ApplyTier(stats, boss);
        }
    }

    /// <summary>
    /// Spawns a V of three or a line of four ships of the same kind.
    /// </summary>
    private void SpawnFormation(Vector3 origin, WaveDefinition stats)
    {
        // Offsets are (sideways, behind) in world units
        Vector2[] offsets = Random.value < 0.5f
            ? new Vector2[] { new Vector2(0f, 0f), new Vector2(-1.3f, 1f), new Vector2(1.3f, 1f) }
            : new Vector2[] { new Vector2(-2.4f, 0f), new Vector2(-0.8f, 0f), new Vector2(0.8f, 0f), new Vector2(2.4f, 0f) };

        GameObject prefab = EnemyPrefabs[Random.Range(0, EnemyPrefabs.Count)];
        foreach (Vector2 offset in offsets)
        {
            Vector3 position = origin + Vector3.right * offset.x + Vector3.forward * offset.y;

            // Snap onto the navigation mesh so every ship can move
            UnityEngine.AI.NavMeshHit hit;
            if (!UnityEngine.AI.NavMesh.SamplePosition(position, out hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            {
                continue; // too far from the play field, skip this ship
            }

            position = new Vector3(hit.position.x, origin.y, hit.position.z);
            SpawnShip(prefab, position, stats, false);
        }
    }

    /// <summary>
    /// Beating a boss refills health and adds a bonus heart (up to five).
    /// </summary>
    private void RewardBossKill()
    {
        var player = Player.GetComponent<Player>();
        if (player.AddMaxHeart())
        {
            UserInterface.AddHeart();
        }
        UserInterface.RestoreAllHearts();
        ScreenFx.Popup(player.transform.position, "FULL HEALTH + HEART!", new Color(1f, 0.4f, 0.5f), 1.2f);
    }

    void Start()
    {
        TogglePlayer(true);
        Player.GetComponent<Player>().ResetHearts();

        _score = 0;
        _kills = 0;
        _waveKills = 0;
        _wave = 0;
        _combo = 0;
        ScreenFx.SetCombo(1, 0f);
        _bossAlive = false;

        // Start() also runs from OnEnable, so make sure only one spawn loop is ever active
        CancelInvoke();
        Player.GetComponent<Player>().SetWeaponLevel(1, false);
        AnnounceWave();
        if (SpawnEnemies)
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// Executes the beginning game logic 
    /// </summary>
    public void OnEnable()
    {
        Start();
    }

    /// <summary>
    /// Spawns an enemy AI and recusively invokes itself to ensure spawning continues. Automatically stops running when the script has been disabled.
    /// </summary>
    public void SpawnEnemy()
    {
        int enemy = Random.Range(0, EnemyPrefabs.Count);
        int spawnLocation = Random.Range(0, SpawnPoints.Count);

        WaveDefinition wave = WaveConfig.Get(_wave);
        bool spawnBoss = wave.IsBoss && !_bossAlive;
        WaveDefinition stats = spawnBoss ? wave : (wave.IsBoss ? WaveConfig.Escort(_wave) : wave);
        _bossAlive |= spawnBoss;

        Vector3 origin = SpawnPoints[spawnLocation].transform.position;
        if (!wave.IsBoss && Random.value < wave.FormationChance)
        {
            SpawnFormation(origin, stats);
        }
        else
        {
            SpawnShip(EnemyPrefabs[enemy], origin, stats, spawnBoss);
        }

        Invoke("SpawnEnemy", Random.Range(wave.SpawnMin, wave.SpawnMax));
    }



    /// <summary>
    /// Toggles the player game object.
    /// </summary>
    /// <param name="visible"></param>

    public void TogglePlayer(bool visible)
    {
        Player.SetActive(visible);
    }

    /// <summary>
    /// Increments the score and calls the UI for display.
    /// </summary>
    public int IncrementScore(int points = 1, bool wasBoss = false)
    {
        // Kills in quick succession build a combo that multiplies the points
        _combo = (Time.time - _lastKillTime <= ComboWindow) ? _combo + 1 : 1;
        _lastKillTime = Time.time;
        int multiplier = Mathf.Min(MaxMultiplier, 1 + _combo / KillsPerLevel);
        points = Mathf.RoundToInt(points * multiplier * Difficulty.ScoreMultiplier);

        _score += points;

        UserInterface.AdjustScore(_score);

        // Every kill can upgrade the player's gun and move the game on to the next wave
        _kills++;
        _waveKills++;
        Player.GetComponent<Player>().RegisterKill();

        WaveDefinition current = WaveConfig.Get(_wave);
        bool waveCleared = current.IsBoss ? wasBoss : _waveKills >= current.KillsToAdvance;
        if (waveCleared)
        {
            if (wasBoss)
            {
                RewardBossKill();
            }

            _wave++;
            _waveKills = 0;
            _bossAlive = false;
            AnnounceWave();
        }

        return points;
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    public void EndGame()
    {
        TogglePlayer(false);
        ScreenFx.SetCombo(1, 0f);

        CancelInvoke();
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }

        UserInterface.EndGame(_score); //Adjust the UI
    }

}
