using System.Collections;
using UnityEngine;
using Spine.Unity;
using UnityEngine.SocialPlatforms.Impl;


public class Enemy : Character
{
    protected float moveSpeed = 20f, jumpForce = 50f;
    protected float direction;
    protected float delay = 1f, delayLeft = 0f;
    [SerializeField] protected float detectRange = 4.5f;
    protected float moveDelay = -1f, waitTimer = 0.6f;
    [SerializeField] protected float setMoveDelay;
    protected bool playerInSight = false;
    protected float lastAttackedAt = 0;
    protected Character player;
    protected Vector2 playerLocation, oldPos, newPos;
    protected float playerVerticalLocation;

    protected bool attackState = false;

    //public BoxCollider2D meleeHitBox;
    // protected void Awake() // setting stats
    // {
    //     health = 10;
    //     damage = 1;
    // }
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
    protected bool inRange() // check if the player is in enemy's range
    {
        RaycastHit2D hitColliders = Physics2D.Raycast(transform.position, directionSign * Vector2.right, detectRange, playerLayerMask);
        return hitColliders;
    }
    protected void Move()
    {
        Run();
        isMoving = true;
        oldPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(playerLocation.x, transform.position.y), moveSpeed * Time.deltaTime);
        newPos = transform.position;
        direction = newPos.x - oldPos.x;
        Turn(direction);
    }

    protected void TriggerJump()
    {
        Jump();
        isMoving = false;
        isJumping = true;
        body.velocity = new Vector2(body.velocity.x, jumpForce);
        Debug.Log("Jump force: " + body.velocity.y);
    }

    protected virtual IEnumerator WaitToAttack(float waitSecs)
    {
        attackState = true;
        attackWarning = StartCoroutine(AttackWarning());
        Idle();
        // meleeHitBoxSpriteColor = basicAttackHitBox.GetComponent<SpriteRenderer>().color; but melleHitBoxSriteColor not updated?
        //basicAttackHitBox.GetComponent<SpriteRenderer>().color = lightRed;
        yield return new WaitForSeconds(waitSecs);
        Attack();
        Evt_MeleeAttack?.Invoke(damage); 
        //StopCoroutine(attackWarning);
        yield return new WaitForSeconds(0.5f);
        basicAttackHitBox.GetComponent<SpriteRenderer>().color = transparent;
        playerInSight = false;
        attackState = false;
    }
    protected IEnumerator AttackWarning()
    {
        //Debug.Log("Flash");
        while (attackState)
        {
            basicAttackHitBox.GetComponent<SpriteRenderer>().color = lightRed;
            yield return new WaitForSeconds(0.05f);
            basicAttackHitBox.GetComponent<SpriteRenderer>().color = transparent;
            yield return new WaitForSeconds(0.05f);
        }
    }
    protected override void update2()
    {
        player = findTarget();
        if (player != null){
            playerLocation = player.gameObject.transform.position;
        }
        if (body.velocity.y < 0)
        {
            moveDelay = setMoveDelay;
        }
        if (player != null)
        {
            if (characterHealth.IsDead == false)
            {
                //if (!isDamaged) // when damaged can't do anything until the damaged anim is inactive
                //{
                    if (!playerInSight && !attackState)
                    {
                        // Run or Jump
                        playerInSight = inRange();  // check if player is in attack range
                        // only move or jump when attack anim (0.5f) finished and isGround == true
                        if (isGrounded == true)
                        {
                            if (moveDelay > 0)
                            {
                                moveDelay -= Time.deltaTime;
                            }
                            else
                            {
                                // if close to player by 3 x points and the player is above this character more than 8 y points => Jump
                                if (Mathf.Abs(transform.position.x - playerLocation.x) < 3f && (playerLocation.y - transform.position.y > 8f) && isJumping == false) // change this jump condition
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
                    else if (!attackState)
                    {
                        characterWaitForAttack = StartCoroutine(WaitToAttack(waitTimer));
                        //attackState = true;
                    }
                //}
            }else if (attackState){
                StopCoroutine(characterWaitForAttack);
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
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + detectRange, transform.position.y, transform.position.z));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x - detectRange, transform.position.y, transform.position.z));
    }
    protected void OnDestroy(){
        GameManager.Instance.EnemiesCountDecrease();
    }
}





