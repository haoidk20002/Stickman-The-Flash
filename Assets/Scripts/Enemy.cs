using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.AI;
using Unity.VisualScripting;
using Spine.Unity.Examples;


public class Enemy : Character
{
    protected float moveSpeed = 20f, jumpForce = 50f;
    protected float direction;
    protected float delay = 0.5f, delayLeft = 0f;
    [SerializeField] protected float detectRange = 4.5f;
    protected float moveDelay = -1f, recoverTimer = 0.5f;
    [SerializeField] protected float setMoveDelay;

    protected bool playerInSight = false;
    protected float lastAttackedAt = 0;
    protected Character player;
    protected Vector2 playerLocation, oldPos, newPos;

    protected float playerVerticalLocation;


    protected void Awake() // setting stats
    {
        health = 10;
        damage = 1;
    }
    protected override void start2()
    {
        delayLeft = delay;
        gameObject.tag = "Enemy";
        gameObject.layer = 6;


    }

    protected override Character findTarget() // following player's pos
    {
        var main_player = GameManager.Instance.MainPlayer;
        return main_player;
    }

    protected bool inRange() // check if the player is in enemy's range
    {
        RaycastHit2D hitColliders = Physics2D.Raycast(transform.position, directionSign* Vector2.right, detectRange, playerLayerMask);
        return hitColliders;
    }

    protected void Move()
    {
        Run();
        isMoving = true;
        oldPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(playerLocation.x, transform.position.y), moveSpeed * Time.deltaTime);
        newPos = transform.position;
        direction = newPos.x - oldPos.x;
        Turn(direction);
    }

    protected void TriggerJump()
    {
        isMoving = false;
        isJumping =true;
        body.velocity = new Vector2(body.velocity.x, jumpForce);
        Debug.Log("Jump force: " + body.velocity.y);
    }
    protected override void update2()
    {
        player = findTarget();
        playerLocation = player.gameObject.transform.position;


        if (body.velocity.y < 0)
        {
            moveDelay = setMoveDelay;
        }
        if (player != null)
        {
            if (playerHealth.IsDead == false)
            {
                if (!isDamaged) // when damaged can't do anything until the damaged anim is inactive
                {
                    if (playerInSight == false)
                    {
                        // Run or Jump
                        playerInSight = inRange();  // check if player is in attack range
                        // only move or jump when attack anim finished and is_jumping false
                        if (Time.time > lastAttackedAt + 0.5f && isGrounded == true)
                        {
                            if (moveDelay > 0)
                            {
                                moveDelay -= Time.deltaTime;
                                Debug.Log(moveDelay);
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
                    else
                    {
                        // Attack
                        // Damaged then idle
                        // 0.5s cooldown before continue attacking
                        Idle();
                        delayLeft -= Time.deltaTime;
                        if (delayLeft <= 0)
                        {
                            Attack();
                            Evt_MeleeAttack?.Invoke(damage);
                            lastAttackedAt = Time.time;
                            delayLeft = delay;
                            playerInSight = false;
                        }
                        // }
                    }
                }
            }
        }
        else
        {
            Idle();
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + detectRange,transform.position.y,transform.position.z));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x - detectRange,transform.position.y,transform.position.z));
    }
}





