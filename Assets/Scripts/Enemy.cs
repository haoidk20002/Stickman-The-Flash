using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.AI;
using Unity.VisualScripting;
using Spine.Unity.Examples;
//using System.Numerics;

public class Enemy : Character
{
    // Logic
    private float moveSpeed = 20f;
    Vector2 oldPos, newPos;

    private float jumpForce = 50f; // Force applied to the enemy when jumping
    private float direction;
    private float delay = 1f;
    private float delayLeft = 0f;
    private float radius = 4f; //old: 5f
    //public LayerMask layerMask;
    private bool playerInSight;
    private float lastAttackedAt = 0;
    private bool onGround;
    private Character player;
    private Vector2 playerLocation;
    Rigidbody2D body;
    //private Transform player;
    protected override void start2()
    {
        playerInSight = false;
        onGround = false;
        delayLeft = delay;
        gameObject.tag = "Enemy";
        gameObject.layer = 6;
        body = GetComponent<Rigidbody2D>();
        Idle();

        // get melee hitbox
        Transform meleeHitboxTransform = transform.Find("MeleeHitbox");
        hitboxOffset = new Vector2(6,2.5f);
    }

    protected override Character findTarget()
    {
        var main_player = GameManager.Instance.MainPlayer;
        return main_player;
    }

    private bool inRange()
    {
        //ignore enemy layer (layer 6)
        int enemyLayerMask = 1 << 6;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius, ~enemyLayerMask);
        if (hitColliders.Length > 0)
        {
            //Debug.Log(hitColliders.Length);
            return true;
        }
        else return false;
    }

    private void Move()
    {
        Run();
        oldPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(playerLocation.x, transform.position.y), moveSpeed * Time.deltaTime);
        newPos = transform.position;
        direction = newPos.x - oldPos.x;
        Turn(direction);
    }



    private void TriggerJump()
    {
        body.velocity = new Vector2(body.velocity.x, jumpForce);
        Jump();
        //Idle(0);
    }
    private bool CheckLanding()
    {
        int enemyLayerMask = 1 << 6;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 6f,~enemyLayerMask);
        Debug.DrawRay(transform.position, Vector2.down * 6f,Color.green);
        //Debug.Log(hit);
        if(hit.collider != null && hit.collider.CompareTag("Ground")){
            
            return true;
        }
        else{
            return false;
        } 
    }
    protected override void update2()
    {

        player = findTarget();
        playerLocation = player.gameObject.transform.position;


        onGround = CheckLanding();
        //Debug.Log(on_ground);
        if (player != null)
        {
            if (playerHealth.IsDead == false && playerInSight == false)
            {
                // check if player is in attack range
                playerInSight = inRange();
                // Move and Jump
                // only move or jump when attack anim finished and is_jumping false
                if (Time.time > lastAttackedAt + 0.5f && onGround == true)
                {
                    if (transform.position.x == playerLocation.x)
                    {
                        TriggerJump();
                    }
                    else
                    {
                        Move();
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
                        lastAttackedAt = Time.time;
                        Run(0);
                    }
                    else
                    {
                        EmptyAttack();
                        lastAttackedAt = Time.time;
                        Run(0);
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
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}





