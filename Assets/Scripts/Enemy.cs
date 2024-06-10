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
        attackWarning = StartCoroutine(AttackWarning());
        yield return new WaitForSeconds(waitSecs);
        warningEffect = false;
        meleeHitBoxSprite.color = transparent;
        Attack();
        //Evt_MeleeAttack?.Invoke(damage);
        yield return new WaitForSeconds(0.5f);
        attackState = false;
        playerInSight = false;
    }
    protected virtual IEnumerator AttackWarning()
    {
        warningEffect = true;
        flashupColor = lightRed;
        flashoutColor = transparent;
        Color temp;
        // flash in 0.5s
        //
        while (warningEffect)
        {
            lerpValue += Time.deltaTime;
            // red to nothing originally, then keeps inversing until the state is out. Flash in and out in 0.5s totally
            meleeHitBoxSprite.color = Color.Lerp(flashupColor, flashoutColor, lerpValue / 0.25f);
            if (lerpValue / 0.25f > 1)
            {
                //Debug.Break();
                lerpValue = 0f;
                temp = flashupColor;
                flashupColor = flashoutColor;
                flashoutColor = temp;
                yield return null;
            }
        }
        //
        if (!warningEffect) { lerpValue = 0f; }
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





