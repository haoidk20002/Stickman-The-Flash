using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Boss : Enemy
{ // boss immune to knockback and his attack can't be disabled

    private HealthBar _bossHealth;

    public void AddBossHealth(HealthBar bossHealth){
        _bossHealth = bossHealth;
    }
    //new private float detectRange = 8f;
    // Boss immune to knockback and only flash when damaged

    protected override IEnumerator WaitToAttack()
    {
        Idle();
        yield return new WaitForSeconds(1f);
        Attack();
        Evt_MeleeAttack?.Invoke(damage);
        lastAttackedAt = Time.time;
        playerInSight = false;
        attackState = false;

    }
    protected override void start2()
    {
        GameManager.Instance.RegisterBoss(this);
        AddBossHealth(GameManager.Instance.HealthBars[1]);
        base.start2(); // extends from start2 of enemy class (base class)
        gameObject.tag = "Boss";
    }
    protected override void update2()
    {
        // Player's location
        player = findTarget();
        if (player != null)
        {
            playerLocation = player.gameObject.transform.position;
        }

        if (body.velocity.y < 0)
        {
            moveDelay = setMoveDelay;
        }

        // tracking boss health
        ratio = playerHealth.RatioHealth;
        _bossHealth.UpdateHealthBar(ratio);

        if (player != null)
        {
            if (playerHealth.IsDead == false)
            {
                if (!playerInSight && !attackState)
                {
                    // Run or Jump
                    playerInSight = inRange();  // check if player is in attack range
                    // only move or jump when attack anim (0.5f) finished and isGround == true
                    if (Time.time > lastAttackedAt + 0.5f && isGrounded == true)
                    {
                        if (moveDelay > 0)
                        {
                            moveDelay -= Time.deltaTime;
                            //Debug.Log(moveDelay);
                        }
                        else
                        {
                            if (Mathf.Abs(transform.position.x - playerLocation.x) < 3f && (playerLocation.y - transform.position.y > 8f) && isJumping == false) // change this jump condition
                            {// TriggerJump called more than once
                                TriggerJump();
                            }
                            else
                            {
                                Move();
                            }
                        }
                    }
                }
                else if (!attackState && Time.time > lastAttackedAt + 0.5f)
                {
                    StartCoroutine(WaitToAttack());
                    attackState = true;
                }
            }
        }
        else
        {
            Idle();
        }
    }
}
