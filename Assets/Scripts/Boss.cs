using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Boss : Enemy
{ // boss immune to knockback and his attack can't be disabled

    private HealthBar _bossHealth;

    protected new float waitTimer = 1f;

    private void Attack2()
    {
        isAttacking = true;
        attackAnimTime = basicAttack.AttackTime;
        PlayAnimation(basicAttack.AttackAnim, basicAttack.AttackTime, false);
        basicAttack.Trigger();
        AddAnimation(idleAnimationName, true, 0f);
    }

    // protected virtual IEnumerator WaitToAttack(float waitSecs){

    // }



    public void AddBossHealth(HealthBar bossHealth)
    {
        _bossHealth = bossHealth;
    }
    //new private float detectRange = 8f;
    // Boss immune to knockback and only flash when damaged
    protected override void start2()
    {
        GameManager.Instance.RegisterBoss(this);
        AddBossHealth(GameManager.Instance.HealthBars[1]);
        base.start2(); // extends from start2 of enemy class (base class)
        gameObject.tag = "Boss";
    }
    // Boss will have 3 attack (kicking and spinning attack and dashing attack)
    protected override void update2()
    {
        // tracking boss health
        ratio = characterHealth.RatioHealth;
        _bossHealth.UpdateHealthBar(ratio);
        // Boss is bascially enemy with high hitpoints (HP) and high damages
        base.update2();

        // old code (don't delete)
        /*
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
        if (player != null)
        {
            if (characterHealth.IsDead == false)
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
            else
            {
                // this clause should be called when he is dead, but didn't ?
                StopCoroutine(WaitToAttack());
                Debug.Log("Stop");
                Debug.Break();
            }
        }
        else
        {
            Idle();
        }*/
    }
}
