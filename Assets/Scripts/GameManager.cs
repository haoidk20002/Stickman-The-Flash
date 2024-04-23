using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    private static GameManager instance; // Singleton instance
    private int score;
    private int enemiesCount, enemiesSpawnNumber = 0, waveNumber = 4;
    private bool spawningWave = false;

    private float minX,maxX;
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

    public GameObject[] enemyPrefab;  // Prefab of the enemy to spawn
    // Getter for the singleton instance
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject singleton = new GameObject("GameManager");
                    instance = singleton.AddComponent<GameManager>();
                }
            }

            return instance;
        }
    }
    private void Awake()
    {
        // Ensure there is only one instance of the GameManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public GameObject[] HealthBars;

    private void Start()
    {

    }

    private void Update()
    {
        CameraBounds.GetCameraBoundsLocation(Camera.main, out minX, out maxX);
        // if player or boss dies, its respective health bar toggles off
        // Toggle on when it appears.
        if (MainPlayer == null)
        {
            HealthBars[0].SetActive(false);
        } else HealthBars[0].SetActive(true);
        if (Boss == null)
        {
            HealthBars[1].SetActive(false);
        } else {HealthBars[1].SetActive(true);}
        // get enemy count
        enemiesCount = FindObjectsOfType<Enemy>().Length;
        //Debug.Log(enemiesCount);
        //if enemy count < 0 then spawn next wave
        if (enemiesCount == 0 && !spawningWave)
        {
            waveNumber++;
            enemiesSpawnNumber++;
            StartCoroutine(SpawnEnemiesWave());
        }
        //StartCoroutine(SpawnEnemiesWave());
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
        Debug.Log("Wait for 5s");
        Debug.Log("Enemies Spawn Number: " + enemiesSpawnNumber);

        yield return new WaitForSeconds(5f);
        // Spawn an enemy
        while (enemiesCount < enemiesSpawnNumber)
        {
            if (waveNumber % 5 == 0)
            {
                Instantiate(enemyPrefab[1], GetRandomSpawnPosition(), Quaternion.identity);
                enemiesSpawnNumber = 0;
                break;
            } //boss every 5 wave
            else
            {
                Instantiate(enemyPrefab[0], GetRandomSpawnPosition(), Quaternion.identity);
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
    // Method to update score
    public void UpdateScore(int amount)
    {
        score += amount;
        Debug.Log("Score: " + score);
    }


    // show dam pop up
    public GameObject damagePopUpPrefab;

    public void ShowDamagePopUp(Vector3 position, string damage)
    {
        GameObject damagePopUp = Instantiate(damagePopUpPrefab, position, Quaternion.identity);
        DamagePopUp popUpScript = damagePopUp.GetComponent<DamagePopUp>();
        popUpScript.SetDamageText(damage);
    }

}




