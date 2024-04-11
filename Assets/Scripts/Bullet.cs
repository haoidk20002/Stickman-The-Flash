using Unity.VisualScripting;
using UnityEngine;
using System;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;
    private int damage;

    private float timer = 3f;

    //public Character character;

    private void HandleDamage(int value)
    {
        damage = value;
    }
    // void Awake(){
    //     character.Evt_ShootingAttack += HandleDamage;
    // }

    public Action <Character, int> OnHit;

    private void Update()
    {
        // Destroy itself 1. after leaving camera view
        // 2. hit opponent
        // after 3s (temporary)
        Debug.Log("..");
        transform.Translate(new Vector3(1,0,0) * speed * Time.deltaTime); // 1 or -1
        timer -= Time.deltaTime;
        if(timer <= 0){
            Destroy(gameObject);
        }
        // 
    }

    private void OnCollisionEnter2D(Collision2D other){
        // var character = other.GetComponent<Character>();
        // if(character != null){
        //     OnHit(character, damage);
        // } 
        Destroy(gameObject);
    }
}
