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

    private float moveSpeed = 20f;
    private float ratio = 0;
    public LayerMask layerMask;

    [SerializeField]
    private float attackRange = 5f;
    private bool enemy_detected = false;

    private bool isTeleporting = false; // Flag to track teleportation
    // Coordinates
    private Vector2 old_pos, new_pos, target, click_position;

    private Vector2 startPos;

    private float minSwipeDistance = 50f; // Adjust as needed
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
    // public void DamagedAnim()
    // {
    //     Debug.Log("Hit");
    //     BeingHit();
    //     Debug.Log("Hit");
    //     Idle(0);
    // }
    private void Teleport()
    {
        var enemies = findTarget();
        //old_pos = transform.position;

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
            enemy_detected = false;
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

    private void MovePlayer(Vector2 direction)
    {
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void Update()
    {
        // this is a mobile game, but for easy development, use pc control (mouse click and swipe) for now
        // tap to teleport
        if (Input.GetMouseButtonDown(0))
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            click_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Teleport();
        }
        // Track Player's Health
        ratio = playerHealth.RatioHealth;
        health.UpdateHealthBar(ratio);

        // swipe to move
        if (Input.GetMouseButtonDown(0))
        {
            // Record the start position of the swipe
            startPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Calculate the end position of the swipe
            Vector2 endPos = Input.mousePosition;
            Vector2 swipeDirection = endPos - startPos;

            // Check if the swipe is horizontal and long enough
            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y) && swipeDirection.magnitude > minSwipeDistance)
            {
                // Move player based on swipe direction
                MovePlayer(swipeDirection.normalized);
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(click_position, radius);
    }
}

