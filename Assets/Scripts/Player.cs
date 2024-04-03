using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Unity.Collections;
using UnityEditor.U2D.Animation;
using System;
using Unity.VisualScripting;

public class Player : Character
{
    [SerializeField] private UnitAttack dashAttack;

    
    private float cooldown = 0.5f;
    private float lastAttackedAt = 0;
    private float radius = 1f;
    private float moveDistance = 75f;
    private float moveSpeed = 50f;
    private float ratio = 0;
    private float attackRange = 6f;
    private float timer;
    private float minSwipeDistance = 20f; // Adjust as needed
    private bool enemyDetected;
    private bool isMoving;
    private bool onGround;
    // Coordinates
    private Vector2 oldPos, newPos, target, clickPosition;
    private Vector2 startPos, destination;
    private Rigidbody2D body;
    private HealthBar health;
    public LayerMask DetectLayerMask;

    protected override void start2()
    {
        target = transform.position;
        clickPosition = transform.position;
        gameObject.tag = "Player";
        gameObject.layer = 3;
        body = GetComponent<Rigidbody2D>();
        Idle();
        GameManager.Instance.RegisterPlayer(this);

        hitboxOffset = new Vector2(6, 2.5f);

        enemyDetected = false;
        isMoving = false;
        onGround = false;
        timer = 1f;
        destination = new Vector2(0, 0);
        health = GameObject.Find("PlayerHealth").GetComponentInChildren<HealthBar>();
        destination = transform.position;

        dashAttack.Init();
    }

    protected override Character findTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(target, radius, DetectLayerMask);

        if (hitColliders.Length > 0)
        {
            // Return the first character found
            return hitColliders[0].GetComponent<Character>();
        }
        else
        {
            // No character found, return null or handle the absence of a target accordingly
            return null;
        }
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

        if (enemies != null)
        {
            //Debug.Log(enemies);
            var enemy = enemies.gameObject.transform.position;
            var destination = target;
            oldPos = transform.position;
            transform.position = target;
            float direction = transform.position.x - oldPos.x;
            Turn(direction);

            if (transform.position.x < enemy.x)
            {
                destination.x = enemy.x - attackRange;

            }
            else
            {
                destination.x = enemy.x + attackRange;

            }
            TriggerAttack();
            transform.position = destination;

        }
        else
        {
            oldPos = transform.position;
            transform.position = target;
            float direction = transform.position.x - oldPos.x;
            Turn(direction);
        }
        // reseting falling velocity
        body.velocity = Vector3.zero;
    }
    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed*Time.deltaTime);
    }
    private void DashAttack()
    {
        PlayAnimation(dashAttack.AttackAnim, dashAttack.AttackTime ,false);
        basicAttackHitBox.enabled = true;
    }


    protected override void update2()
    {
        dashAttack.TakeTime(Time.deltaTime);
        if (isMoving == true)
        {
            //dashAttack.Init();
            Move();
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Debug.Log("Stop");
                isMoving = false;
                basicAttackHitBox.enabled = false;
            }
        }
        else
        {
            timer = 1f;
        }
        // Teleport: Click to desired destination and the player teleports after releasing click
        // Move: Click, swipe, then release to move according to the swipe direction
        // Teleportation by mouse click, move by mouse swipe
        if (Input.GetMouseButtonDown(0))
        {
            // target: actual destination
            // click_position:  desired destination
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startPos = Input.mousePosition; // Detect the start of the swipe
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
                        Turn(swipeDirection.x);
                        destination.x = transform.position.x + moveDistance;
                        destination.y = transform.position.y;
                        isMoving = true;
                        DashAttack();
                        Idle(0);
                    }
                    else
                    {
                        Turn(swipeDirection.x);
                        destination.x = transform.position.x - moveDistance;
                        destination.y = transform.position.y;
                        isMoving = true;
                        DashAttack();
                        Idle(0);
                    }
                }
            }
            else
            {
                Teleport();
                isMoving = false;
            }
        }
        // Track Player's Health
        ratio = playerHealth.RatioHealth;
        health.UpdateHealthBar(ratio);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(clickPosition, radius);
    }
}

