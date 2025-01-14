using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;

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
    public int[] Enemies;
}
[System.Serializable]
public class Settings
{
    public float MusicLevel;
    public float SFXLevel;
}

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyPrefabData
    {
        public int id;
        public string name;
        public string prefabPath;
    }
    [System.Serializable]
    public class EnemyList
    {
        public List<EnemyPrefabData> enemyPrefabs;

    }
    // Character Instances
    public Player player;
    public Enemy enemy;
    public Boss boss;

    //private static GameManager instance; // Singleton instance
    public Slider SFXSlider, MusicSlider;
    private int score = 0;
    private int highScore = 0;
    // wave info
    private int statsMultiplier = 1;
    private int numberOfEnemiesSpawn;
    private int maxEnemiesCount;
    private float spawnSpeed;
    private bool allEnemiesSpawned = true;
    private int enemiesCount = 0;
    private bool bossSpawned = false;
    private int waveNumber = 0;
    private bool isSpawning = false;
    private int enemiesTypesCount;
    private List<Enemy> loadedEnemyPrefabs = new List<Enemy>();
    private float minX, maxX, minY, maxY;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI HighScoreText;
    // JSON file handling objects and variables
    private PlayerProfile playerProfile = new PlayerProfile();
    public Settings _settings = new Settings();
    private WaveData wavedata = new WaveData();
    private EnemyList enemyList = new EnemyList();
    private CharacterStatsContainer statsContainer = new CharacterStatsContainer();
    private string statsPath = Path.Combine(Application.streamingAssetsPath, "CharacterStats.json");
    private string wavePath = Path.Combine(Application.streamingAssetsPath, "WaveData.json");
    private string enemyListPath = Path.Combine(Application.streamingAssetsPath, "EnemyList.json");
    private string profilePath;
    private string settingsPath;
    // show dam pop up
    public GameObject damagePopUpPrefab;
    public TextMeshProUGUI WaveNotification;
    public TextMeshProUGUI Tutorial;
    public Canvas GameOverScreen;
    private int enemyIndex;
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
    // Getter for the singleton instance
    public static GameManager Instance { get; private set; }
    [field: SerializeField] public HealthBar[] HealthBars { get; private set; } // Health Bars

    private void Awake()
    {

        if (Instance == null)
        {
            //Purpose: Files placed in the Resources folder are included in the build and can be loaded at runtime using Resources.Load.
            //Usage: Ideal for assets that need to be dynamically loaded.

            //Purpose: Files in the StreamingAssets folder are copied as-is to a specific folder in the build and can be accessed using file paths. Useful for files that need to be read directly or updated post-build.
            //Usage: Great for configuration files, external data, or media files.

            //Purpose: The persistentDataPath is used for storing files that need to be persistent across sessions. Files saved here are not included in the initial build but can be created or modified at runtime.
            //Usage: Ideal for saving game progress, player settings, or downloaded content.

            profilePath = Path.Combine(Application.persistentDataPath, "PlayerProfile.json"); // for build version
            //profilePath = Path.Combine(Application.dataPath, "PlayerProfile.json"); // for testing
            Debug.Log(profilePath);

            settingsPath = Path.Combine(Application.persistentDataPath, "Settings.json"); // for build version
            //settingsPath = Path.Combine(Application.dataPath, "Settings.json"); // for testing
            Debug.Log(settingsPath);

            Instance = this;
            LoadCharacterStats();
            LoadWaveData();
            //DontDestroyOnLoad(Instance);
        }
    }
    private void Start()
    {
        StartCoroutine(ShowTutorial(3f));
        ReadSettings();
        ReadProfile();
        ScoreText.text = "Score: " + score.ToString();
        HighScoreText.text = "High Score: " + highScore.ToString();
    }
    private void Update()
    {
        if (!PauseAndContinue.gameIsPaused)
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
            //Spawn next wave
            if (enemiesCount == 0 && allEnemiesSpawned)
            {
                bossSpawned = false;
                waveNumber++;
                WaveNotification.text = "Wave " + waveNumber.ToString(); // Update wave notification
                if (waveNumber <= 12)
                {
                    LoadNewWave(waveNumber - 1);
                }
                else { LoadNewWave(11); }
                StartCoroutine(NotifyWave(5f));
            }
            //
            if (enemiesCount < maxEnemiesCount && !allEnemiesSpawned && !isSpawning)
            {
                isSpawning = true;
                StartCoroutine(SpawnEnemies(spawnSpeed));
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
    }
    // Register Player and Boss
    public void RegisterPlayer(Character player)
    {
        MainPlayer = player;
    }
    public void RegisterBoss(Character boss)
    {
        Boss = boss;
    }
    // Save and read profile
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
    // Load from JSON
    private void LoadEnemyPrefabs(int number)
    {
        if (loadedEnemyPrefabs.Count > 0)
        {
            loadedEnemyPrefabs.Clear();
        }
        // if (File.Exists(enemyListPath))
        // {
        //     try
        //     {
        //         string jsonText = File.ReadAllText(enemyListPath);
        //         enemyList = JsonUtility.FromJson<EnemyList>(jsonText);
        //         foreach (EnemyPrefabData a in enemyList.enemyPrefabs)
        //         {
        //             if (wavedata.Waves[number].Enemies.Contains(a.id))
        //             {
        //                 loadedEnemyPrefabs.Add(Resources.Load<Enemy>(a.prefabPath));
        //             }
        //             // Load Prefabs if enemies ID exist in Enemies[] of WaveData
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError(e);
        //     }
        // }
        var jsonFile = Resources.Load<TextAsset>("EnemyList");
        if (jsonFile != null)
        {
            enemyList = JsonUtility.FromJson<EnemyList>(jsonFile.text);
            foreach (EnemyPrefabData a in enemyList.enemyPrefabs)
            {
                if (wavedata.Waves[number].Enemies.Contains(a.id))
                {
                    loadedEnemyPrefabs.Add(Resources.Load<Enemy>(a.prefabPath));
                }
                // Load Prefabs if enemies ID exist in Enemies[] of WaveData
            }
        }
        else
        {
            Debug.LogError("Could not find the JSON file!");
        }
    }
    private void LoadWaveData()
    {
        // if (File.Exists(wavePath))
        // {
        //     string jsonText = File.ReadAllText(wavePath);
        //     wavedata = JsonUtility.FromJson<WaveData>(jsonText);
        // }
        // else
        // {
        //     Debug.LogError("JSON file not found!");
        // }
        var jsonFile = Resources.Load<TextAsset>("WaveData");
        if (jsonFile != null)
        {
            wavedata = JsonUtility.FromJson<WaveData>(jsonFile.text);
        }
        else
        {
            Debug.LogError("JSON file not found!");
        }
    }
    private void LoadNewWave(int number)
    {
        LoadEnemyPrefabs(number);
        // getting original stats
        LoadCharacterStats();
        statsMultiplier = wavedata.Waves[number].StatsMultiplier;
        //Debug.Log("Stats Multiplier: " + statsMultiplier);
        // buff enemies stats
        statsContainer.BuffEnemiesHealthAndDamage(statsMultiplier);
        // enemy.LoadStats(statsContainer.enemy_stats);
        // boss.LoadStats(statsContainer.boss_stats);
        numberOfEnemiesSpawn = wavedata.Waves[number].NumberOfEnemiesSpawn;
        maxEnemiesCount = wavedata.Waves[number].MaxEnemiesCount;
        spawnSpeed = wavedata.Waves[number].SpawnSpeed;
    }
    private void LoadCharacterStats()
    {
        // if (File.Exists(statsPath))
        // {
        //     string jsonText = File.ReadAllText(statsPath);
        //     statsContainer = JsonUtility.FromJson<CharacterStatsContainer>(jsonText);
        //     player.LoadStats(statsContainer.player_stats);


        // }
        // else
        // {
        //     Debug.LogError("Character stats JSON file not found!");
        // }
        var jsonFile = Resources.Load<TextAsset>("CharacterStats");
        if (jsonFile != null)
        {
            statsContainer = JsonUtility.FromJson<CharacterStatsContainer>(jsonFile.text);
            player.LoadStats(statsContainer.player_stats);
        }
        else
        {
            Debug.LogError("Character stats JSON file not found!");
        }
    }

    // Spawn Enemies Couroutine
    private IEnumerator SpawnEnemies(float spawnSpeed)
    {
        yield return new WaitForSeconds(spawnSpeed);
        if (!bossSpawned)
        {
            SpawnWithBoss();
        }
        else
        {
            SpawnWithoutBoss();
        }
        enemiesCount++;
        numberOfEnemiesSpawn--;
        isSpawning = false;
        //yield return null;
    }

    private void SpawnWithoutBoss()
    {
        // Filter out boss enemies from the selection
        List<Enemy> nonBossEnemies = new List<Enemy>();
        foreach (var enemyPrefab in loadedEnemyPrefabs)
        {
            if (!enemyPrefab.CompareTag("Boss")) // Assuming bosses have a "Boss" tag
            {
                nonBossEnemies.Add(enemyPrefab);
            }
        }
        if (nonBossEnemies.Count > 0)
        {
            enemyIndex = UnityEngine.Random.Range(0, nonBossEnemies.Count);
            var newcharacter = Instantiate(nonBossEnemies[enemyIndex], GetRandomSpawnPosition(), Quaternion.identity);
            newcharacter.LoadStats(statsContainer.enemy_stats);
        }
    }
    private void SpawnWithBoss()
    {
        enemyIndex = UnityEngine.Random.Range(0, loadedEnemyPrefabs.Count);
        var newcharacter = Instantiate(loadedEnemyPrefabs[enemyIndex], GetRandomSpawnPosition(), Quaternion.identity);
        if (newcharacter.gameObject.CompareTag("Enemy"))
        {
            newcharacter.LoadStats(statsContainer.enemy_stats);
        }
        else
        {
            bossSpawned = true;
            newcharacter.LoadStats(statsContainer.boss_stats);
        }
    }
    // Other simple functions
    public void EnemiesCountDecrease()
    {
        enemiesCount--;
    }
    private Vector2 GetRandomSpawnPosition()
    {
        // Return a random position within some bounds (adjust to your needs)
        return new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(0f, 30f));
    }
    public void ShowDamagePopUp(Vector3 position, string damage)
    {
        GameObject damagePopUp = Instantiate(damagePopUpPrefab, position, Quaternion.identity);
        DamagePopUp popUpScript = damagePopUp.GetComponent<DamagePopUp>();
        popUpScript.SetDamageText(damage);
    }
    public void AddScore(int amount)
    {
        score += amount;
    }
    private IEnumerator NotifyWave(float duration)
    {
        WaveNotification.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        WaveNotification.gameObject.SetActive(false);
    }
    private IEnumerator ShowTutorial(float duration)
    {
        Tutorial.gameObject.SetActive(true);
        // Load the tutorial text file from the Resources folder
        TextAsset tutorialTextAsset = Resources.Load<TextAsset>("Tutorial");
        if (tutorialTextAsset == null)
        {
            Debug.LogError("Tutorial.txt not found in Resources");
            yield break;
        }
        // Read the text asset line by line
        using (StringReader reader = new StringReader(tutorialTextAsset.text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Tutorial.text = line;
                // Delay for the specified duration before moving on to the next line
                yield return new WaitForSeconds(duration);
            }
        }
        Tutorial.gameObject.SetActive(false);
    }
    private void ReadSettings()
    {
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            _settings = JsonUtility.FromJson<Settings>(json);
            SFXSlider.value = _settings.SFXLevel;
            MusicSlider.value = _settings.MusicLevel;
        }
        else
        {
            _settings.SFXLevel = 1f;
            _settings.MusicLevel = 1f;
            SaveSettings();
        }
    }
    public void SaveSettings()
    {
        string settingJSON = JsonUtility.ToJson(_settings);
        File.WriteAllText(settingsPath, settingJSON);
    }

}




