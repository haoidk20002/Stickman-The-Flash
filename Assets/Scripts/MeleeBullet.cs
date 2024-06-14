using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeBullet : MonoBehaviour
{
    private int damage, Xdirection, Ydirection;
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
    private void ApplyKnockback(int xdirection, int ydirection, Character other)
    {
        //Debug.Log("Knockback");
        Rigidbody2D otherRigidbody = other.GetComponent<Rigidbody2D>();
        if (other.CheckImmunity == true) return;
        otherRigidbody.AddForce(new Vector2(xdirection * knockbackForce, ydirection*knockbackForce), ForceMode2D.Impulse);
    }
    public Action<Character, int> OnHit;
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject.name);
        var otherCharacter = other.GetComponent<Character>();
        //Debug.Log("Character1: " + character);
        if (otherCharacter != null && !otherCharacter.characterHealth.IsDead)
        {
            Xdirection = (character.transform.position.x > otherCharacter.transform.position.x) ? -1 : 1;
            Ydirection = (character.transform.position.y > otherCharacter.transform.position.y) ? -1 : 1;
            if (Mathf.Abs(character.transform.position.x - otherCharacter.transform.position.x) < 3f){
                Xdirection = 0;
            }
            if (Mathf.Abs(character.transform.position.y - otherCharacter.transform.position.y) < 3f){
                Ydirection = 0;
            }
                if (otherCharacter.CompareTag("Enemy") || otherCharacter.CompareTag("Player"))
                {
                    ApplyKnockback(Xdirection,Ydirection, otherCharacter);
                    //Debug.Log(Xdirection + ", "+ Ydirection);
                    //Debug.Break();
                }
            OnHit(otherCharacter, damage); // OnHit invoked b4 checking => player's immunity is already set as true
        }
    }
    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     //Gizmos.DrawWireCube(transform.position, hurtboxSize);
    // }

}
