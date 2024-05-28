using System;
using UnityEngine;
using Spine.Unity;
using System.Collections;
using Unity.VisualScripting;
// detectRange = 4.5 (normal) 20 (special)
// speed = 35 (normal) 85 (dash)
public class Boss : Enemy
{ // boss immune to knockback and his attack can't be disabled
    private int attackCount = 0;
    [SerializeField] private int SpecialAttackCondition;
    private Vector2 dashDestination;
    [SerializeField] private float dashSpeed;
    private HealthBar _bossHealth;
    private float normalMoveSpeed;

    //protected new float waitTimer = 1f;
    private bool specialAttack = false;
    private int specialAttackNumber;

    private bool isJumpingParabolic = false;
    private Vector2 jumpTargetPosition;
    [Header("Parabol Jump")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpDuration;


    protected override IEnumerator WaitToAttack(float waitSecs)
    { // after 3 normal attack, special attack is triggered
        attackState = true;
        if (specialAttack)
        {
            Prepare();
        }
        else { Idle(); }
        attackWarning = StartCoroutine(AttackWarning());
        // calculate destination
        dashDestination.x = transform.position.x + directionSign * 200f;
        // get player's x pos (attack special 2)
        yield return new WaitForSeconds(waitSecs);
        warningEffect = false;
        meleeHitBoxSprite.color = transparent;
        dashDestination.y = transform.position.y;
        if (!specialAttack)
        {
            Attack();
            attackCount++;
            // Normal Attack 
            yield return new WaitForSeconds(0.5f);
            attackState = false;
            playerInSight = false;
        }
        else
        {
            Debug.Log(specialAttackNumber);
            switch (specialAttackNumber)
            {
                case 1:
                    DashAttack();
                    break;
                case 2:
                    //Debug.Break();
                    TriggerParabolicJump(playerLocation);
                    SpinningAttack();
                    break;
            }
            attackCount = 0;
        }
    }


    protected override IEnumerator AttackWarning()
    {
        if (specialAttack && specialAttackNumber == 1) // change warning
        {
            dashAttackHitbox.GetComponent<SpriteRenderer>().size = new Vector2(90f, 10f);
        }
        else if (!specialAttack)
        {
            basicAttackHitBox.GetComponent<SpriteRenderer>().size = new Vector2(5.9f, 10f);
        }
        warningEffect = true;
        flashupColor = lightRed;
        flashoutColor = transparent;
        Color temp;
        // flash in 0.5s
        //
        if (!specialAttack || (specialAttack && specialAttackNumber == 1))
        {
            while (warningEffect)
            {
                lerpValue += Time.fixedDeltaTime;
                // red to nothing originally, then keeps inversing until the state is out. Flash in and out in 1s totally
                meleeHitBoxSprite.color = Color.Lerp(flashupColor, flashoutColor, lerpValue / 1f);
                if (lerpValue / 1f > 1)
                {
                    //Debug.Break();
                    lerpValue = 0f;
                    temp = flashupColor;
                    flashupColor = flashoutColor;
                    flashoutColor = temp;
                    yield return null;
                }
            }
        }
        //
        if (!warningEffect) { lerpValue = 0f; }
    }
    private void DisableDash()
    {
        dashAttackHitbox.GetComponent<BoxCollider2D>().size = new Vector2(5.9f, 10f);
        dashAttackHitbox.GetComponent<BoxCollider2D>().offset = new Vector2(0f, 0f);
        isDashing = false;
        isAttacking = false;

        attackState = false;
        playerInSight = false;
    }

    private void Move2()
    {
        transform.position = Vector2.MoveTowards(transform.position, dashDestination, stats.moveSpeed * Time.deltaTime);
    }
    public void AddBossHealth(HealthBar bossHealth)
    {
        _bossHealth = bossHealth;
    }

    private void UpdateBossAttackState()
    {
        if (attackCount == SpecialAttackCondition && !specialAttack)
        {
            specialAttackNumber = UnityEngine.Random.Range(1, 3);
            Debug.Log("Special Number: " + specialAttackNumber);
            if (specialAttackNumber == 1)
            {
                stats.detectRange = 20f;
                stats.moveSpeed = dashSpeed;
                waitTimer = 3f;
            }
            else
            {
                stats.detectRange = 20f;
                waitTimer = 3f;
            }
            specialAttack = true;

        }
        else if (!specialAttack)
        {
            stats.detectRange = 5f;
            stats.moveSpeed = normalMoveSpeed;
            waitTimer = 1f;
        }
    }

    private void TriggerParabolicJump(Vector2 targetPosition)
    {
        isJumpingParabolic = true;
        jumpTargetPosition = targetPosition;
        Turn(jumpTargetPosition.x - transform.position.x);
        Vector2 jumpVelocity = CalculateJumpVelocity(transform.position, jumpTargetPosition, jumpHeight, jumpDuration);
        body.velocity = jumpVelocity;
    }

    private Vector2 CalculateJumpVelocity(Vector2 startPoint, Vector2 endPoint, float height, float time)
    {
        float gravity = Physics2D.gravity.y;
        float verticalVelocity = 2 * height / time - 0.5f * gravity * time;
        float horizontalVelocity = (endPoint.x - startPoint.x) / time;
        return new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void HandleParabolicJump()
    {
        if (isJumpingParabolic && body.velocity.y <= 0 && Mathf.Abs(transform.position.y - jumpTargetPosition.y) < 0.1f)
        {
            isJumpingParabolic = false;
            body.velocity = Vector2.zero;
        }
    }

    private void HandleCharacterInteraction()
    {
        //if (!isDamaged) // when damaged can't do anything until the damaged anim is inactive
        //{
        if (!playerInSight && !attackState)
        {
            // Run or Jump
            playerInSight = InRange();  // check if player is in attack range (for boss attack range is random depend on the attack)
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
        }
        else
        {
            // Handle Dash
            if (dashAttack.IsPerforming)
            {
                Move2();
            }
        }
    }


    // Boss immune to knockback and only flash when damaged
    protected override void start2()
    {
        GameManager.Instance.RegisterBoss(this);
        AddBossHealth(GameManager.Instance.HealthBars[1]);
        normalMoveSpeed = stats.moveSpeed;
        base.start2(); // extends from start2 of enemy class (base class)
        gameObject.tag = "Boss";
        dashAttack.OnEnd = () =>
        {
            attackState = false;
            playerInSight = false;
            DisableDash();
            specialAttack = false;
        };
        // This clause is  not called 
        spinningAttack.OnEnd = () =>
        {
            attackState = false;
            playerInSight = false;
            specialAttack = false;
        };
        //
    }
    // Boss will have 3 attack (kicking and spinning attack and dashing attack)

    protected override void update2()
    {
        //  attackCount = 3 => change detect range
        UpdateBossAttackState();
        // Boss Health
        ratio = characterHealth.RatioHealth;
        _bossHealth.UpdateHealthBar(ratio);
        // Character Interaction
        player = FindTarget();
        if (player != null)
        {
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
                HandleCharacterInteraction();
                HandleParabolicJump();
            }
            else if (attackState) // handle death
            {
                StopAllCoroutines();
                meleeHitBoxSprite.color = transparent;
            }
        }
        else
        {
            Idle();
        }
        // Boss is bascially enemy with high hitpoints (HP) and high damages
    }
}
