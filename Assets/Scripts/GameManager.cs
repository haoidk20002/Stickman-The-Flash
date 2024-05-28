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
    public CharacterStats player;
    public CharacterStats enemy;
    public CharacterStats boss;
}
[System.Serializable]
public class PlayerProfile{
    public int HighScore;
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
    private int enemiesCount = 0, enemiesSpawnNumber = 0, waveNumber = 4;
    private bool spawningWave = false;

    private float minX, maxX, minY, maxY;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI HighScoreText;
    private PlayerProfile playerProfile;
    public Canvas GameOverScreen;

    private string statsPath = Path.Combine(Application.streamingAssetsPath, "CharacterStats.json");
    private string profilePath;

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

    public GameObject[] enemyPrefab;  // Prefab of the enemy to spawn
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
            profilePath = Path.Combine(Application.persistentDataPath, "PlayerProfile.json");
            Instance = this;
            LoadCharacterStats();
            //DontDestroyOnLoad(Instance);
            //DontDestroyOnLoad(HealthBars.gameObject);
        }
    }

    [field: SerializeField] public HealthBar[] HealthBars { get; private set; } // Health Bars

    private void LoadCharacterStats()
    {
        if (File.Exists(statsPath))
        {
            string jsonText = File.ReadAllText(statsPath);
            CharacterStatsContainer statsContainer = JsonUtility.FromJson<CharacterStatsContainer>(jsonText);
            player.LoadStats(statsContainer.player);
            enemy.LoadStats(statsContainer.enemy);
            boss.LoadStats(statsContainer.boss);
        }
        else
        {
            Debug.LogError("Character stats JSON file not found!");
        }
    }


    private void Start()
    {
        //ReadProfile();
        ScoreText.text = "Score: " + score.ToString();
        HighScoreText.text = "High Score: " + highScore.ToString();
    }

    private void Update()
    {
        CameraBounds.GetCameraBoundsLocation(Camera.main, out minX, out maxX, out minY, out maxX);
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

        if (!PauseAndContinue.gameIsPaused)
        {
            // get enemy count
            //if enemy count < 0 then spawn next wave
            if (enemiesCount == 0 && !spawningWave)
            {
                waveNumber++;
                enemiesSpawnNumber++;
                StartCoroutine(SpawnEnemiesWave());
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
    IEnumerator SpawnEnemiesWave()
    {
        spawningWave = true;
        //Debug.Log("Wait for 5s");
        //Debug.Log("Enemies Spawn Number: " + enemiesSpawnNumber);

        yield return new WaitForSeconds(5f);
        // Spawn an enemy
        while (enemiesCount < enemiesSpawnNumber)
        {
            if (waveNumber % 5 == 0)
            {
                //boss.AddBossHealth(HealthBars[1]);
                Instantiate(enemyPrefab[1], GetRandomSpawnPosition(), Quaternion.identity);
                enemiesCount++;
                enemiesSpawnNumber = 0;
                break;
            } //boss every 5 wave
            else
            {
                Instantiate(enemyPrefab[0], GetRandomSpawnPosition(), Quaternion.identity);
                enemiesCount++;
                yield return new WaitForSeconds(0.5f);
            }
            // Instantiate(enemyPrefab, GetRandomSpawnPosition(), Quaternion.identity);
            // yield return new WaitForSeconds(1f);
        }
        spawningWave = false;
        // Wait for a certain amount of time before spawning the next enemy
        //yield return new WaitForSeconds(1f);
    }
    Vector2 GetRandomSpawnPosition()
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
        string highScoreJSON = JsonUtility.ToJson(playerProfile.HighScore);
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




