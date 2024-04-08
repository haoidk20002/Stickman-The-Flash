using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Bullet : MonoBehaviour
{   public int damage = 2;
    //Character character;
    //private int damage;
    //public event Action Hit = delegate{};
    private void OnTriggerEnter2D(Collider2D other){
        var target = other.GetComponent<Character>();
        target.BeingHit(damage);
    }

}
