using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    private static GameManager instance; // Singleton instance
    private int score;
    public Character MainPlayer
    {
        get;
        private set;

    } // Reference to the player
    public GameObject enemyPrefab;  // Prefab of the enemy to spawn
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

    private void Start()
    {
        // Start spawning enemies
        StartCoroutine(SpawnEnemy());
    }

    private void Update()
    {
    }

    public void RegisterPlayer(Character player)
    {
        MainPlayer = player;
    }

    // Method to spawn an enemy
    IEnumerator SpawnEnemy()
    {
        while (true)
        {
            // Spawn an enemy
            Instantiate(enemyPrefab, GetRandomSpawnPosition(), Quaternion.identity);

            // Wait for a certain amount of time before spawning the next enemy
            yield return new WaitForSeconds(5f);
        }
    }
    Vector2 GetRandomSpawnPosition()
    {
        // Return a random position within some bounds (adjust to your needs)
        return new Vector2(Random.Range(-5f, 5f), Random.Range(0f, 10f));
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




