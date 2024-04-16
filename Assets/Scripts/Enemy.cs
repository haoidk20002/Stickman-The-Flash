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
    private float moveDelay =-1f;
    [SerializeField] private float setMoveDelay;

    private bool playerInSight = false;
    private float lastAttackedAt = 0;
    private Character player;
    private Vector2 playerLocation, oldPos, newPos;
    //Rigidbody2D body;




    private void Awake()
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

    protected override Character findTarget()
    {
        var main_player = GameManager.Instance.MainPlayer;
        return main_player;
    }

    private bool inRange()
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
        isMoving = true;
        Run();
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
        //Debug.Log(onGround);

        if (player != null)
        {
            // Run or Jump
            if (playerHealth.IsDead == false && playerInSight == false)
            {
                // check if player is in attack range
                playerInSight = inRange();
                // Move and Jump
                // only move or jump when attack anim finished and is_jumping false
                if (Time.time > lastAttackedAt + 0.5f && onGround == true)
                {
                    if (moveDelay > 0)
                    {
                        moveDelay -= Time.deltaTime;
                        Debug.Log(moveDelay);
                    }
                    else
                    {
                        if (transform.position.x == playerLocation.x)
                        {
                            //Debug.Log("Jump");
                            TriggerJump();
                            moveDelay = setMoveDelay;
                        }
                        else
                        {
                            Move();
                        }
                    }
                }
            }
            // Attack
            else if (playerHealth.IsDead == false && playerInSight == true)
            {
                if (Time.time > lastAttackedAt + 0.5f)
                {
                    Idle();
                }
                delayLeft -= Time.deltaTime;
                if (delayLeft <= 0)
                {

                    if (inRange() == true)
                    {
                        Attack();
                        Evt_MeleeAttack?.Invoke(damage);
                        lastAttackedAt = Time.time;
                    }
                    else
                    {
                        EmptyAttack();
                        lastAttackedAt = Time.time;
                    }
                    delayLeft = delay;
                    playerInSight = false;
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





