using System.Collections;
using UnityEngine;
using Spine.Unity;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;
using System;


public class Enemy : Character
{
    [SerializeField] protected float jumpForce; //jumpforce = 50
    //protected float moveSpeed;
    protected float direction;
    protected float delay = 1f, delayLeft = 0f;
    [SerializeField] protected float specialDetectRange;
    protected float moveDelay = -1f, waitTimer = 1f;
    [SerializeField] protected float setMoveDelay;
    protected bool playerInSight = false;
    protected float lastAttackedAt = 0;
    protected Character player;
    protected Vector2 playerLocation, oldPos, newPos;
    protected float playerVerticalLocation;
    protected float lerpValue = 0f;
    protected bool attackState = false, warningEffect;

    protected Color flashupColor, flashoutColor;

    protected override void start2()
    {
        delayLeft = delay;
        gameObject.tag = "Enemy";
        gameObject.layer = 6;
    }
    protected override Character FindTarget() // following player's pos
    {
        var main_player = GameManager.Instance.MainPlayer;
        return main_player;
    }
    protected bool InRange() // check if the player is in enemy's range
    {
        RaycastHit2D hitColliders = Physics2D.Raycast(transform.position, directionSign * Vector2.right, stats.detectRange, playerLayerMask);
        return hitColliders;
    }
    protected void Move()
    {
        Run();
        isMoving = true;
        oldPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(playerLocation.x, transform.position.y), stats.moveSpeed * Time.deltaTime);
        newPos = transform.position;
        direction = newPos.x - oldPos.x;
        Turn(direction);
    }

    protected void TriggerJump()
    {
        Jump();
        isMoving = false;
        isJumping = true;
        body.velocity = new Vector2(body.velocity.x, jumpForce);
    }

    protected virtual IEnumerator WaitToAttack(float waitSecs)
    {// when he attacks, the warning become transparent
        attackState = true;
        Idle();
        attackWarning = StartCoroutine(AttackWarning(waitSecs)); // called once
        yield return new WaitForSeconds(waitSecs);
        meleeHitBoxSprite.color = transparent;
        Attack();
        yield return new WaitForSeconds(0.5f);
        attackState = false;
        playerInSight = false;
    }
    protected virtual IEnumerator AttackWarning(float timer)
    {
        flashupColor = lightRed;
        flashoutColor = transparent;
        Color temp;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            lerpValue += Time.deltaTime*2;
             // Red to nothing originally, then keeps inversing until the timer (attack waiting time) is out. Flash in and out in 1s totally
            meleeHitBoxSprite.color = Color.Lerp(flashupColor, flashoutColor, lerpValue);
            if (lerpValue > 1)
            {
                lerpValue = 0f;
                temp = flashupColor;
                flashupColor = flashoutColor;
                flashoutColor = temp;
            }
            yield return null;
        }
        //
        if (timer < 0) { lerpValue = 0f; }
    }
    protected override void update2()
    {
        player = FindTarget();
        if (player != null)
        {
            playerLocation = player.gameObject.transform.position;
        }
        if (body.velocity.y < 0)
        {
            landingAnimTime = 0.5f;
        }
        if (player != null)
        {
            if (characterHealth.IsDead == false)
            {
                //if (!isDamaged) // when damaged can't do anything until the damaged anim is inactive
                //{
                if (!playerInSight && !attackState)
                {
                    // Run or Jump
                    playerInSight = InRange();  // check if player is in attack range (for boss attack range is random depend on the attack)
                                                // only move or jump when attack anim (0.5f) finished and isGround == true
                    if (isGrounded == true)
                    {
                        if (!isLanding)
                        {
                            if (Math.Abs(transform.position.x - playerLocation.x) > 2)
                            {
                                Move();
                            }
                            else
                            {
                                Idle();
                            }
                        }
                    }
                }
                else if (!attackState)
                {
                    characterWaitForAttack = StartCoroutine(WaitToAttack(waitTimer));
                }
            }
            else if (attackState)
            {
                StopAllCoroutines();
                meleeHitBoxSprite.color = transparent;
            }
        }
        else
        {
            Idle();
        }
    }

    protected void OnDestroy()
    {
        GameManager.Instance.EnemiesCountDecrease();
    }
}





