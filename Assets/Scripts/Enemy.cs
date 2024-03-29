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
    Vector2 old_pos, new_pos;

    private float jumpForce = 50f; // Force applied to the enemy when jumping
    private float jumpCooldown = 2f; // Cooldown between jumps
    private float direction;
    private float delay = 1f;
    private float delayLeft = 0f;
    private float radius = 4f; //old: 5f
    //public LayerMask layerMask;
    private bool player_in_sight;
    private float lastAttackedAt = 0;

    private bool on_ground;

    private List<Character> player;
    Vector2 player_location;
    Vector2 jumping_location;

    Rigidbody2D body;

    //private Transform player;
    protected override void start2()
    {
        player_in_sight = false;
        on_ground = false;
        delayLeft = delay;
        gameObject.tag = "Enemy";
        gameObject.layer = 6;
        body = GetComponent<Rigidbody2D>();
        Idle();


    }

    protected override List<Character> findTarget()
    {
        var player = GameManager.Instance.MainPlayer;
        return new List<Character> { player };
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
        old_pos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(player_location.x, transform.position.y), moveSpeed * Time.deltaTime);
        new_pos = transform.position;
        direction = new_pos.x - old_pos.x;
        if (direction > 0)
        {
            skeleton.ScaleX = 1;
        }
        else if (direction < 0)
        {
            skeleton.ScaleX = -1;
        }
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
    void Update()
    {

        player = findTarget();
        player_location = player[0].gameObject.transform.position;


        on_ground = CheckLanding();
        //Debug.Log(on_ground);
        if (player != null)
        {
            if (playerHealth.IsDead == false && player_in_sight == false)
            {
                // check if player is in attack range
                player_in_sight = inRange();
                // Move and Jump
                // only move or jump when attack anim finished and is_jumping false
                if (Time.time > lastAttackedAt + 0.5f && on_ground == true)
                {
                    if (transform.position.x == player_location.x)
                    {
                        TriggerJump();
                    }
                    else
                    {
                        Move();
                    }
                }// else if(on_ground == false){
                //     Idle();
                // }
            }
            // Attack
            else if (playerHealth.IsDead == false && player_in_sight == true)
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
                    player_in_sight = false;
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





