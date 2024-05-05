using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeBullet : MonoBehaviour
{   
    private int damage;
    private float knockbackForce = 150f;
    public Character character;
    private void HandleDamage(int value)
    {
        damage = value;
    }
    private void Awake()
    {
        character.Evt_MeleeAttack += HandleDamage;
    }
    private void ApplyKnockback(int direction, Character other)
    {
        //Debug.Log("Knockback");
        Rigidbody2D otherRigidbody = other.GetComponent<Rigidbody2D>();
        if (other.isImmune == true) return;
        otherRigidbody.AddForce(new Vector2(direction * knockbackForce,0), ForceMode2D.Impulse);
    }
    public Action<Character, int> OnHit;
    private void OnTriggerEnter2D(Collider2D other)
    {
        var character = other.GetComponent<Character>();
        //Debug.Log("Character1: " + character);
        if (character != null)
        { 
            int direction = (transform.position.x > character.transform.position.x)? -1:1;
            if (character.CompareTag("Enemy") || character.CompareTag("Player")){
                //ApplyKnockback(direction,character);
            }
            OnHit(character, damage); // OnHit invoked b4 checking => player's immunity is already set as true
        }
    }

}
