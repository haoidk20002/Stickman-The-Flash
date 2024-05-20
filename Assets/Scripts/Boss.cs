using System;
using UnityEngine;
using Spine.Unity;
using System.Collections;
using Unity.VisualScripting;

public class Boss : Enemy
{ // boss immune to knockback and his attack can't be disabled
    private int attackCount = 0;
    // [SerializeField] protected UnitAttack specialAttack1;
    // [SerializeField] protected UnitAttack specialAttack2;
    private HealthBar _bossHealth;

    protected new float waitTimer = 1f;

    // private void SpecialAttack1() // dashing
    // {
    //     // increased detect range (10f), double damage, 
    //     isAttacking = true;
    //     attackAnimTime = specialAttack1.AttackTime;
    //     PlayAnimation(specialAttack1.AttackAnim, specialAttack1.AttackTime, false);
    //     specialAttack1.Trigger();
    //     AddAnimation(idleAnimationName, true, 0f);
    // }

    // private void SpecialAttack2() // spinning vertically attack
    // {
    //     isAttacking = true;
    //     attackAnimTime = basicAttack.AttackTime;
    //     PlayAnimation(basicAttack.AttackAnim, basicAttack.AttackTime, false);
    //     basicAttack.Trigger();
    //     AddAnimation(idleAnimationName, true, 0f);
    //     AddAnimation(idleAnimationName, true, 0f);
    // }

    protected override IEnumerator WaitToAttack(float waitSecs)
    { // after 3 normal attack, special attack is triggered
        attackState = true;
        Idle();
        attackWarning = StartCoroutine(AttackWarning());
        yield return new WaitForSeconds(waitSecs);
        warningEffect = false;
        meleeHitBoxSprite.color = transparent;
        Attack();
        //DashAttack();
        //attackCount++;

        // Normal Attack 
        yield return new WaitForSeconds(0.5f);
        attackState = false;
        playerInSight = false;

        // Dash Attack
        // condition here b4 setting these value

        // attackCount = 0;
        attackState = false;
        playerInSight = false;
    }
    public void AddBossHealth(HealthBar bossHealth)
    {
        _bossHealth = bossHealth;
    }
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
        //  attackCount = 3 => change detect range
        // if (attackCount == 3){
        detectRange = 20f;
        //}
        // else {detectRange = 4.5f;}
        // tracking boss health
        ratio = characterHealth.RatioHealth;
        _bossHealth.UpdateHealthBar(ratio);
        // Modify here
        // player = FindTarget();
        // if (player != null)
        // {
        //     playerLocation = player.gameObject.transform.position;
        // }
        // if (body.velocity.y < 0)
        // {
        //     moveDelay = setMoveDelay;
        // }
        // if (player != null)
        // {
        //     if (characterHealth.IsDead == false)
        //     {
        //         //if (!isDamaged) // when damaged can't do anything until the damaged anim is inactive
        //         //{
        //         if (!playerInSight && !attackState)
        //         {
        //             // Run or Jump
        //             playerInSight = InRange();  // check if player is in attack range (for boss attack range is random depend on the attack)
        //                                         // only move or jump when attack anim (0.5f) finished and isGround == true
        //             if (isGrounded == true)
        //             {
        //                 if (moveDelay > 0)
        //                 {
        //                     moveDelay -= Time.deltaTime;
        //                 }
        //                 else
        //                 {
        //                     // if close to player by 3 x points and the player is above this character more than 8 y points => Jump
        //                     if (Mathf.Abs(transform.position.x - playerLocation.x) < 3f && (playerLocation.y - transform.position.y > 8f) && isJumping == false) // change this jump condition
        //                     {
        //                         TriggerJump();
        //                     }
        //                     else
        //                     {
        //                         Move();
        //                     }
        //                 }
        //             }
        //         }
        //         else if (!attackState)
        //         {
        //             characterWaitForAttack = StartCoroutine(WaitToAttack(waitTimer));
        //         }
        //         else
        //         {
        //             // if (isDashing == true)
        //             // {
        //             //     Move();
        //             //     currentTimer -= Time.deltaTime;
        //             //     //Debug.Log(currentTimer);

        //             //     if (currentTimer <= 0f)
        //             //     {
        //             //         DisableDash();
        //             //     }
        //             // }
        //         }
        //         //}
        //     }
        //     else if (attackState)
        //     {
        //         StopAllCoroutines();
        //         meleeHitBoxSprite.color = transparent;
        //     }
        // }
        // else
        // {
        //     Idle();
        // }
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
