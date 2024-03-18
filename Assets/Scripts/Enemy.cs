using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.AI;
//using System.Numerics;

public class Enemy : Character
{
    // Logic
    private float moveSpeed = 10f;
    Vector2 old_pos, new_pos;
    public float cooldown = 0.5f;
    private float lastAttackedAt = 0;
    public float radius = 5;
    //private Transform player;
    protected override void start2()
    {
        // Assuming the player has a tag "Player"
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        gameObject.tag = "Enemy";
        gameObject.layer = 6;
        Idle();
    }

    protected override List<Character> findTarget()
    {
        var player = GameManager.Instance.MainPlayer;
        return new List<Character> { player };
    }

    
    void Update()
    {
        var player = findTarget();
        var players_location = player[0].gameObject.transform.position;
        if (player != null)
        {
            //Run();
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(players_location.x, transform.position.y), moveSpeed * Time.deltaTime);
        }
        else
        {
            //Idle();
        }
    }
}



