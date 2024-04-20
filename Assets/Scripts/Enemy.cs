using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.AI;
using Unity.VisualScripting;
using Spine.Unity.Examples;


public class Enemy : Character
{
    private float moveSpeed = 20f, jumpForce = 50f;
    private float direction;
    private float delay = 0.5f, delayLeft = 0f, radius = 4f;
    private float moveDelay = -1f, recoverTimer = 0.5f;
    [SerializeField] private float setMoveDelay;

    private bool playerInSight = false;
    private float lastAttackedAt = 0;
    private Character player;
    private Vector2 playerLocation, oldPos, newPos;

    private void Awake() // setting stats
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

    private bool inRange() // check if the player is in enemy's range
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius, playerLayerMask);
        if (hitColliders.Length > 0)
        {
            return true;
        }
        else return false;
    }

    private void Move()
    {
        Run();
        isMoving = true;
        oldPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(playerLocation.x, transform.position.y), moveSpeed * Time.deltaTime);
        newPos = transform.position;
        direction = newPos.x - oldPos.x;
        Turn(direction);
    }

    private void TriggerJump()
    {
        isMoving = false;
        body.velocity = new Vector2(body.velocity.x, jumpForce);
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
                if (playerInSight == false)
                {
                    // Run or Jump
                    playerInSight = inRange();  // check if player is in attack range
                    // only move or jump when attack anim finished and is_jumping false
                    if (Time.time > lastAttackedAt + 0.5f  && isGrounded == true)
                    {
                        if (moveDelay > 0)
                        {
                            moveDelay -= Time.deltaTime;
                            Debug.Log(moveDelay);
                        }
                        else
                        {
                            if (Mathf.Abs(transform.position.x - playerLocation.x) < 2)
                            {
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
                    Debug.Log("Idle");
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
        else
        {
            Idle();
        }
    }
    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, radius);
    // }
}





