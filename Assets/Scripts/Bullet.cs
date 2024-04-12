using Unity.VisualScripting;
using UnityEngine;
using System;
// Bullet's ownership needed
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;
    private int damage, direction;

    private float timer = 3f;

    private void HandleDamage(int value)
    {
        damage = value;
    }
    private void HandleDirection(int value){
        direction = value;
    }
    void Awake(){
    
    }

    public Action <Character, int> OnHit;

    private void Update()
    {
        // Destroy itself 1. after leaving camera view
        // 2. hit opponent
        Debug.Log("..");
        transform.Translate(new Vector3(direction,0,0) * speed * Time.deltaTime); // 1 or -1

        // after 3s (temporary)
        timer -= Time.deltaTime;
        if(timer <= 0){
            Destroy(gameObject);
        }
        // 
    }

    private void OnTriggerEnter2D(Collider2D other){
        // var character = other.GetComponent<Character>(); 
        // if(character != null){         
        //     OnHit(character, damage);
        // } 
        Destroy(gameObject);
    }
}
