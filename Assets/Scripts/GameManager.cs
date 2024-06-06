using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class CharacterStatsContainer
{
    public CharacterStats player_stats;
    public CharacterStats enemy_stats;
    public CharacterStats boss_stats;
    public void BuffEnemiesHealthAndDamage(int multiplier)
    {
        enemy_stats.health = enemy_stats.health * multiplier;
        enemy_stats.damage = enemy_stats.damage * multiplier;
        boss_stats.health = boss_stats.health * multiplier;
        boss_stats.damage = boss_stats.damage * multiplier;
    }
}
[System.Serializable]
public class PlayerProfile
{
    public int HighScore;
}
[System.Serializable]
public class WaveData
{
    public Wave[] Waves;
}

[System.Serializable]
public class Wave
{
    public string WaveName;
    public int StatsMultiplier;
    public int NumberOfEnemiesSpawn;
    public int MaxEnemiesCount;
    public float SpawnSpeed;
    //public Enemy[] Enemies;
}

public class GameManager : MonoBehaviour
{
    // Character Instances
    public Player player;
    public Enemy enemy;
    public Boss boss;

    //
    private static GameManager instance; // Singleton instance
    private int score = 0;
    private int highScore = 0;
    // wave info
    private int statsMultiplier;
    private int numberOfEnemiesSpawn;
    private int maxEnemiesCount;
    private float spawnSpeed;
    //private Enemy[] enemies;
    private bool allEnemiesSpawned = true;
    private int enemiesCount = 0;
    private bool bossSpawned = false;
    private int waveNumber = 0;
    private bool isSpawning = false;
    private int enemiesTypesCount;
    // camera boundaries
    private float minX, maxX, minY, maxY;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI HighScoreText;
    // JSON file handling objects and variables
    private PlayerProfile playerProfile = new PlayerProfile();
    private WaveData wavedata = new WaveData();
    private CharacterStatsContainer statsContainer = new CharacterStatsContainer();
    private string statsPath = Path.Combine(Application.streamingAssetsPath, "CharacterStats.json");
    private string wavePath = Path.Combine(Application.streamingAssetsPath, "WaveData.json");
    private string profilePath;

    public Canvas GameOverScreen;
    public Character MainPlayer
    {
        get;
        private set;

    } // Reference to the player
    public Character Boss
    {
        get;
        private set;

    } // Reference to the boss

    public int Score
    {
        get { return score; }
    }

    public int HighScore
    {
        get { return highScore; }
    }

    public Enemy[] enemyPrefab;  // Prefab of the enemy to spawn
    // Getter for the singleton instance
    public static GameManager Instance { get; private set; }

    public void EnemiesCountDecrease()
    {
        enemiesCount--;
    }

    private void Awake()
    {

        if (Instance == null)
        {
            //profilePath = Path.Combine(Application.persistentDataPath, "PlayerProfile.json"); // for build version
            profilePath = Path.Combine(Application.dataPath, "PlayerProfile.json"); // for testing
            Debug.Log(profilePath);

            Instance = this;
            LoadCharacterStats();
            LoadWaveData();
        }
    }

    [field: SerializeField] public HealthBar[] HealthBars { get; private set; } // Health Bars
    private void LoadWaveData()
    {
        if (File.Exists(wavePath))
        {
            string jsonText = File.ReadAllText(wavePath);
            wavedata = JsonUtility.FromJson<WaveData>(jsonText);
        }
        else
        {
            Debug.LogError("JSON file not found!");
        }
    }
    private void LoadNewWave(int number)
    {
        statsMultiplier = wavedata.Waves[number].StatsMultiplier;
        Debug.Log("Stats Multiplier: " + statsMultiplier);
        // buff enemies stats
        statsContainer.BuffEnemiesHealthAndDamage(statsMultiplier);
        enemy.LoadStats(statsContainer.enemy_stats);
        boss.LoadStats(statsContainer.boss_stats);
        numberOfEnemiesSpawn = wavedata.Waves[number].NumberOfEnemiesSpawn;
        maxEnemiesCount = wavedata.Waves[number].MaxEnemiesCount;
        spawnSpeed = wavedata.Waves[number].SpawnSpeed;
        // sth here

    }

    private void LoadCharacterStats()
    {
        if (File.Exists(statsPath))
        {
            string jsonText = File.ReadAllText(statsPath);
            statsContainer = JsonUtility.FromJson<CharacterStatsContainer>(jsonText);
            player.LoadStats(statsContainer.player_stats);
            enemy.LoadStats(statsContainer.enemy_stats);
            boss.LoadStats(statsContainer.boss_stats);
        }
        else
        {
            Debug.LogError("Character stats JSON file not found!");
        }
    }


    private void Start()
    {
        ReadProfile();
        ScoreText.text = "Score: " + score.ToString();
        HighScoreText.text = "High Score: " + highScore.ToString();
    }

    private void Update()
    {
        CameraBounds.GetCameraBoundsLocation(Camera.main, out minX, out maxX, out minY, out maxX);
        // managing allEnemiesSpawned variable
        if (numberOfEnemiesSpawn == 0)
        {
            allEnemiesSpawned = true;
        }
        else allEnemiesSpawned = false;
        // if player or boss dies, its respective health bar toggles off
        // Toggle on when it appears.
        if (MainPlayer == null)
        {
            HealthBars[0].gameObject.SetActive(false);
            GameOverScreen.gameObject.SetActive(true);
            PauseAndContinue.gameIsPaused = true;
            SaveProfile();
        }
        else
        {
            HealthBars[0].gameObject.SetActive(true);
        }
        if (Boss == null)
        {
            HealthBars[1].gameObject.SetActive(false);
        }
        else { HealthBars[1].gameObject.SetActive(true); }

        if (!PauseAndContinue.gameIsPaused || !PauseAndContinue.gameIsStopped)
        {
            //spawn next wave
            if (enemiesCount == 0 && allEnemiesSpawned)
            {
                waveNumber++;
                if (waveNumber <= 12){
                    LoadNewWave(waveNumber - 1);
                } else { LoadNewWave(11);}
            }
            //
            if (enemiesCount < maxEnemiesCount && !allEnemiesSpawned && !isSpawning)
            {
                isSpawning = true;

                StartCoroutine(SpawnEnemies(spawnSpeed));
            }
        }

        // show score and high score
        ScoreText.text = "Score: " + score.ToString();
        HighScoreText.text = "High Score: " + highScore.ToString();
        // Check for high score update
        if (score > highScore)
        {
            highScore = score;
        }
    }

    public void RegisterPlayer(Character player)
    {
        MainPlayer = player;
    }
    public void RegisterBoss(Character boss)
    {
        Boss = boss;
    }

    // Method to spawn an enemy
    private IEnumerator SpawnEnemies(float spawnSpeed)
    {
        int enemyIndex = Random.Range(0,2);
        if (bossSpawned){
            enemyIndex = 1;
        }
        yield return new WaitForSeconds(spawnSpeed);
        Instantiate(enemyPrefab[0], GetRandomSpawnPosition(), Quaternion.identity);
        if (enemyIndex == 1){ bossSpawned = true;}
        enemiesCount++;
        isSpawning = false;
    }
    private Vector2 GetRandomSpawnPosition()
    {
        // Return a random position within some bounds (adjust to your needs)
        return new Vector2(Random.Range(minX, maxX), Random.Range(0f, 30f));
    }


    // show dam pop up
    public GameObject damagePopUpPrefab;

    public void ShowDamagePopUp(Vector3 position, string damage)
    {
        GameObject damagePopUp = Instantiate(damagePopUpPrefab, position, Quaternion.identity);
        DamagePopUp popUpScript = damagePopUp.GetComponent<DamagePopUp>();
        popUpScript.SetDamageText(damage);
    }

    // Method to update score
    public void AddScore(int amount)
    {
        score += amount;
    }
    // Method to update high score
    private void SaveProfile()
    {
        playerProfile.HighScore = highScore;
        string highScoreJSON = JsonUtility.ToJson(playerProfile);
        File.WriteAllText(profilePath, highScoreJSON);
    }
    private void ReadProfile()
    {
        if (File.Exists(profilePath))
        {
            string json = File.ReadAllText(profilePath);
            playerProfile = JsonUtility.FromJson<PlayerProfile>(json);
            highScore = playerProfile.HighScore;
        }
        else
        {
            SaveProfile();
        }
    }
}




