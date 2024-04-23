using System;
using UnityEngine;

public class Boss : Enemy
{
    private bool healthBarAvailable = false;
    //new private float detectRange = 8f;
    new private void Awake() // setting stats
    {
        health = 30;
        damage = 1;
    }

    protected override void start2()
    {
        GameManager.Instance.RegisterBoss(this);
        base.start2(); // extends from start2 of enemy class (base class)
        //Debug.Log("Assign Health Bars"); 
        //healthBar = GameObject.Find("BossHealth").GetComponentInChildren<HealthBar>(); 
        // this statement execute before the health bar being available
    }

    protected override void update2()
    {
        base.update2();
        if (!healthBarAvailable)
        {
            healthBar = GameObject.Find("BossHealth").GetComponentInChildren<HealthBar>();
            healthBarAvailable = true;
        }
        else
        {
            ratio = playerHealth.RatioHealth;
            healthBar.UpdateHealthBar(ratio);
        }
    }
}
