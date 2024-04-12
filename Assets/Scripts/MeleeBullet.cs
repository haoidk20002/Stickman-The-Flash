using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class MeleeBullet : MonoBehaviour
{   //public int damage = 2;
    private int damage;

    public Character character;
    private void HandleDamage(int value)
    {
        damage = value;
    }
    void Awake()
    {
        character.Evt_MeleeAttack += HandleDamage;
    }

    public Action<Character, int> OnHit;
    private void OnTriggerEnter2D(Collider2D other)
    {
        var character = other.GetComponent<Character>();
        Debug.Log("Character1: " + character);
        if (character != null)
        { 
            OnHit(character, damage);
        }
    }

}
