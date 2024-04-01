using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Unity.Collections;
using UnityEditor.U2D.Animation;
using System;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

public class Player : Character
{
    public float cooldown = 0.5f;
    private float lastAttackedAt = 0;
    private float radius = 1f;
    private float moveSpeed = 50f;
    private float ratio = 0;
    public LayerMask layerMask;

    [SerializeField]
    private float attackRange = 5f;
    private bool enemy_detected;
    private bool isMoving;
    private bool on_ground;
    // Coordinates
    private Vector2 old_pos, new_pos, target, click_position;

    private Vector2 startPos;

    private float minSwipeDistance = 20f; // Adjust as needed
    private Rigidbody2D body;

    private HealthBar health;
    protected override void start2()
    {
        target = transform.position;
        click_position = transform.position;
        gameObject.tag = "Player";
        gameObject.layer = 3;
        body = GetComponent<Rigidbody2D>();
        Idle();
        GameManager.Instance.RegisterPlayer(this);

        enemy_detected = false;
        isMoving = false;
        on_ground = false;

        //HealthBar health = new HealthBar();
        health = GameObject.Find("PlayerHealth").GetComponentInChildren<HealthBar>();
    }

    protected override List<Character> findTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(target, radius);
        List<Character> result = new List<Character>();
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                result.Add(collider.gameObject.GetComponent<Character>());
            }
        }
        return result;
    }
    private void TriggerAttack()
    {
        if (Time.time > lastAttackedAt + cooldown)
        {
            lastAttackedAt = Time.time;

            Attack();
            Idle(0);

        }
    }

    private void Teleport()
    {
        var enemies = findTarget();
        // if enemy is found, player teleports close to it then attack
        // else teleport to the clicked point

        if (enemies.Count > 0)
        {
            Debug.Log(enemies);
            var enemy = enemies[0].gameObject.transform.position;
            var destination = target;
            if (transform.position.x < enemy.x)
            {
                destination.x = enemy.x - attackRange;
                skeleton.ScaleX = 1;
            }
            else
            {
                destination.x = enemy.x + attackRange;
                skeleton.ScaleX = -1;
            }

            TriggerAttack();
            //enemy_detected = false;
            transform.position = destination;

        }
        else
        {
            old_pos = transform.position;
            transform.position = target;
            float direction = transform.position.x - old_pos.x;
            Idle();
            if (direction > 0)
            {
                skeleton.ScaleX = 1;
            }
            else if (direction < 0)
            {
                skeleton.ScaleX = -1;
            }
        }
        // reseting falling velocity
        body.velocity = Vector3.zero;
    }

    private void Move(float direction)
    {
        if (direction > 0)
        {
            skeleton.ScaleX = 1;
            // Apply a constant force to the player's Rigidbody2D
            direction = 1;
            body.velocity = new Vector2(direction * moveSpeed, 0f);
        }
        else if (direction < 0)
        {
            skeleton.ScaleX = -1;
            // Apply a constant force to the player's Rigidbody2D
            direction = -1;
            body.velocity = new Vector2(direction * moveSpeed, 0f);
        }
    }
    private void MoveAttack()
    {
        int playerLayerMask = 1 << 3;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius, ~playerLayerMask);
        if (hitColliders.Length > 0)
        {   
            Debug.Log("Found");
            TriggerAttack();
        }
    }

    private void Update()
    {
        // Detect if the player is moving horizontally

        isMoving = Mathf.Abs(body.velocity.x) > 0.1f;
        if (isMoving == true)
        {
            MoveAttack();
        }
        else
        {
            Idle();
        }

        // Teleport: Click to desired destination and the player teleports after releasing click
        // Move: Click, swipe, then release to move according to the swipe direction
        // Teleportation by mouse click, move by mouse swipe
        if (Input.GetMouseButtonDown(0))
        {
            // target: actual destination
            // click_position:  desired destination
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            click_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startPos = Input.mousePosition; // Detect the start of the swipe

            //isMoving = true;
        }
        // Detect the end of the swipe
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = Input.mousePosition;
            Vector2 swipeDirection = endPos - startPos;
            // Determine if the swipe was long enough (you can set a threshold)
            float swipeMagnitude = swipeDirection.magnitude;
            if (swipeMagnitude > minSwipeDistance) // Adjust the threshold as needed
            {
                // Check the direction of the swipe
                if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                {
                    // Horizontal swipe
                    if (swipeDirection.x > 0)
                    {
                        Move(swipeDirection.x);
                        //Debug.Log("Right swipe!");
                    }
                    else
                    {
                        Move(swipeDirection.x);
                        //Debug.Log("Left swipe!");
                    }
                }
            }
            else
            {
                Teleport();
            }
        }
        // Track Player's Health
        ratio = playerHealth.RatioHealth;
        health.UpdateHealthBar(ratio);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(click_position, radius);
    }
}

