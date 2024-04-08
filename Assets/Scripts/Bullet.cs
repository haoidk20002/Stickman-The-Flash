using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Bullet : MonoBehaviour
{   public int damage = 2;
    public Action <Character, int> OnHit;
    private void OnTriggerEnter2D(Collider2D other){
        Debug.Log("Result: " + other.name);
        // var target = other.GetComponent<Character>();
        // target.BeingHit(damage);
        var character = other.GetComponent<Character>();
        // get other characters except the main one 
        if(character != null){
            OnHit(character, damage);
        } 
    }

}
