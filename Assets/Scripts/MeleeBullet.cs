using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeBullet : MonoBehaviour
{   
    private int damage, direction;
    [SerializeField] private float knockbackForce = 150f;
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
        if (other.CheckImmunity == true) return;
        otherRigidbody.AddForce(new Vector2(direction * knockbackForce,0), ForceMode2D.Impulse);
    }
    public Action<Character, int> OnHit;
    private void OnTriggerEnter2D(Collider2D other)
    {
        var otherCharacter = other.GetComponent<Character>();
        //Debug.Log("Character1: " + character);
        if (otherCharacter != null && !otherCharacter.characterHealth.IsDead)
        { 
            direction = (character.transform.position.x > otherCharacter.transform.position.x)? -1:1; 
            //direction = Character.directionSign;
            // opposite knockback direction happens when the character's pos relative to melee pos is opposite for the attacker's facing direction
            if (otherCharacter.CompareTag("Enemy") || otherCharacter.CompareTag("Player")){
                ApplyKnockback(direction,otherCharacter);
            }
            OnHit(otherCharacter, damage); // OnHit invoked b4 checking => player's immunity is already set as true
        }
    }

}
