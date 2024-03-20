using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Unity.Collections;
using UnityEditor.U2D.Animation;
using System;
using UnityEngine.UIElements;

public class Player : Character
{
    public float cooldown = 0.5f;
    private float lastAttackedAt = 0;
    private float radius = 1f;

    public LayerMask layerMask;

    [SerializeField]
    private float attackRange = 5f;
    bool enemy_detected = false;
    // Coordinates
    Vector2 old_pos, new_pos, target, click_position;
    Rigidbody2D body;
    protected override void start2()
    {
        target = transform.position;
        click_position = transform.position;
        gameObject.tag = "Player";
        body = GetComponent<Rigidbody2D>();
        Idle();
        GameManager.Instance.RegisterPlayer(this);
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
    void TriggerAttack()
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
    void Teleport()
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

    // protected override void BeingHit(){

    // }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            click_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Teleport();
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(click_position, radius);
    }
}

