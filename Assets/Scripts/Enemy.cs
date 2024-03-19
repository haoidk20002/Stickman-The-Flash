using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.AI;
using Unity.VisualScripting;
//using System.Numerics;

public class Enemy : Character
{
    // Logic
    private float moveSpeed = 20f;
    Vector2 old_pos, new_pos;

    private float direction;
    public float delay = 2f;
    private float delayLeft = 0f;
    public float radius = 5;
    //public LayerMask layerMask;
    private bool player_in_sight;

    private bool is_Attacking;

    private List<Character> player;
    Vector2 player_location;

    GameObject target;



    //private Transform player;
    protected override void start2()
    {
        player_in_sight = false;
        is_Attacking = false;
        delayLeft = delay;
        // Assuming the player has a tag "Player"
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        gameObject.tag = "Enemy";
        gameObject.layer = 6;
        Run();


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
            Debug.Log(hitColliders.Length);
            return true;
        }
        else return false;
    }


// Enemy change to attack state when player is in range,
//  After delay, attacks and attackanim
// After that, change back to find state

    void Update()
    {

        player = findTarget();
        player_location = player[0].gameObject.transform.position;
        target = player[0].gameObject;
        player_in_sight = inRange();

        // If player available then run, else idle
        // If player is range, attack after 2s delay unless the player jumps out of range during delay
        if (player != null)
        {
            if (playerHealth.IsDead == false)
            {
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
                // if (player_in_sight == true)
                // {
                //     delayLeft -= Time.deltaTime;
                //     Debug.Log(delayLeft);
                //     if (delayLeft <= 0)
                //     {
                //         TriggerAttack();
                //         delayLeft = delay;
                //     }
                // }else delayLeft = delay;
            }
        }
        else Idle();

    }

    void TriggerAttack()
    {
        Attack();
        target.GetComponent<Player>().DamagedAnim();
        
    }

    // IEnumerator TriggerAttack()
    // {
    //     Attack();
    //     target.GetComponent<Player>().DamagedAnim();
        
    // }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}




// IEnumerator HitPlayerAfterDelay(float hitDelay)
// {
//     yield return new WaitForSeconds(hitDelay);
//     if (player_in_sight == true) // Check if the player is still within range after the delay
//     {
//         // Perform hit action here, for example:
//         target.GetComponent<Player>().DamagedAnim();
//         Attack();
//         //StopCoroutine();
//     }
// }




