using UnityEngine;
using Spine.Unity;
using Unity.Collections;
using UnityEditor.U2D.Animation;
using System;
using System.ComponentModel;
using System.Collections;
using Unity.VisualScripting;


// detectRange 1.25f
public class Player : Character
{
    private HealthBar _playerHealth;
    private float cooldown = 0.5f, lastAttackedAt = 0f;
    //[SerializeField] private float moveSpeed;
    [SerializeField] private float attackRange; 
    [SerializeField] private float minSwipeDistance;
    private float minX, maxX, maxY, minY;
    private float swipeMagnitude, playerWidth, playerHeight, direction;
    [SerializeField] private float SetInvincibilityTime;
    private float invincibilityTime;
    // Coordinates
    private Vector2 oldPos, newPos, clickPosition, cameraBounds, cameraPos;
    private Vector2 startPos, endPos, dashDestination, teleportDestination, swipeDirection, swipeDirectionOnScreen;

    // get camera bonuds first, then lock player in bounds
    //private Rigidbody2D body;
    public LayerMask DetectLayerMask;
    public GameObject bullet;
    //private Vector3 bulletStartPos;
    private Vector2 startScreenPos, endScreenPos;

    public void AddPlayerHealth(HealthBar playerHealth)
    {
        _playerHealth = playerHealth;
    }
    protected override void start2()
    {
        AddPlayerHealth(GameManager.Instance.HealthBars[0]);
        SettingMainCharacterValue1();
        SettingMainCharacterValue2();
        //bulletStartPos = transform.position;
        invincibilityTime = SetInvincibilityTime;

    }
    private void SettingMainCharacterValue1()
    {
        clickPosition = transform.position;
        gameObject.tag = "Player";
        gameObject.layer = 3;
        //GetCameraBounds();
        GameManager.Instance.RegisterPlayer(this);
        hitboxOffset = new Vector2(6, 2.5f);
    }
    private void SettingMainCharacterValue2()
    {
        dashDestination = new Vector2(0, 0);
        dashDestination = transform.position;
    }


    private void GetCameraBounds() //camera's bounds depending on aspect ratio
    {
        // way 1
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect; // aspect ratio * height => width*height /height 

        cameraBounds = new Vector2(width, height);

        // way 2
        // var leftBounds = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0));
        // var rightBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0));
        // cameraBounds = rightBounds - leftBounds;

        // way 3
        // var leftBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        // var rightBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        // cameraBounds = rightBounds - leftBounds;

        //Debug.Log("Camera Bounds: " + cameraBounds);
    }
    private void CalculateBoundsLocation()
    {
        cameraPos = Camera.main.transform.position; //cam's pos

        minX = cameraPos.x - cameraBounds.x / 2f; maxX = cameraPos.x + cameraBounds.x / 2f;


        // // way 2 
        // minX = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0)).x;
        // maxX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0)).x;
    }
    private void GetPlayerHurtBoxSize()
    {
        playerWidth = transform.GetComponent<BoxCollider2D>().size.x;
        playerHeight = transform.GetComponent<BoxCollider2D>().size.y;
    }
    protected override Character FindTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(endPos, stats.detectRange, DetectLayerMask);
        if (hitColliders.Length > 0) { return hitColliders[0].GetComponent<Character>(); } /* Return the first character found*/
        else { return null; }
    }
    private void TriggerAttack()
    {
        if (Time.time > lastAttackedAt + cooldown)
        {
            lastAttackedAt = Time.time;
            Attack();
            Evt_MeleeAttack?.Invoke(stats.damage);
        }
    }
    private void Teleport()
    {
        attackAnimTime = basicAttack.AttackTime;
        var enemies = FindTarget();
        // if enemy is found, player teleports close to it then attack, else teleport to the clicked point
        if (enemies != null)
        {
            var enemy = enemies.gameObject.transform.position;
            teleportDestination = endPos;
            oldPos = transform.position;
            direction = teleportDestination.x - oldPos.x;
            Turn(direction);
            if (transform.position.x < enemy.x) { teleportDestination.x = enemy.x - attackRange; }
            else { teleportDestination.x = enemy.x + attackRange; }
            TriggerAttack();
            transform.position = teleportDestination;
        }
        else
        {
            Run();
            //Idle();
            oldPos = transform.position;
            transform.position = endPos;
            direction = transform.position.x - oldPos.x;
            Turn(direction);
        }
        // reseting falling velocity
        body.velocity = Vector3.zero;
    }
    private void Move()
    {
        verlocity.y = 1f;
        transform.position = Vector2.MoveTowards(transform.position, dashDestination, stats.moveSpeed * Time.deltaTime);
    }

    private void SwipeOrTeleport()
    {
        isFalling = false;
        swipeDirection = endPos - startPos;
        swipeDirectionOnScreen = endScreenPos - startScreenPos;
        //Debug.Log("Swipe Direction On Screen: " + swipeDirectionOnScreen);
        // Determine if the swipe was long enough (you can set a threshold)
        //swipeMagnitude = swipeDirection.magnitude;
        swipeMagnitude = (startScreenPos - endScreenPos).magnitude;
        // Debug.Log("Start: " + startPos + "End: " + endPos);
        //Debug.Log("Swipe direction: " + swipeDirection);
        //Debug.Log(swipeMagnitude*multiplier);
        if (swipeMagnitude > minSwipeDistance)
        {
            body.velocity = Vector2.zero;
            CalculateMove();
        }
        else
        {
            isDashing = false;
            isAttacking = true;
            Teleport();
        }
    }

    private void CalculateMove()
    {   // Distance depend on swipe distance, get to destination in time duration
        // if he is on ground, whether he dashes or not depending on the swipe direction on screen (different coordinate)
        // if that swipe direction's y is low enough, he won't do that 
        if (isGrounded && swipeDirectionOnScreen.y < -0.1f) { return; }
        if (isGrounded && (-0.1f <= swipeDirectionOnScreen.y) && (swipeDirectionOnScreen.y < 0f))
        {
            swipeDirection.y = 0f;
        }
        //currentTimer = timer;
        Turn(swipeDirection.x);
        //Debug.Log("Swipe direction y: " + swipeDirection.y + "Dash y destination: " + dashDestination.y);
        dashDestination.x = transform.position.x + swipeDirection.x * 10;
        dashDestination.y = transform.position.y + swipeDirection.y * 5;
        //Debug.Break();
        DashAttack(); // Dash can be disabled when touching ground from above
    }
    // Unused concept (Shooting)
    // may be set as protected in Character class
    // private void GetShootPosAndDirection()
    // {
    //     int bulletDirection;
    //     // if(direction > 0){
    //     //     bulletDirection = 1;
    //     // }else {bulletDirection = -1;}
    //     bulletDirection = direction > 0 ? 1 : -1; // forward (direction > 0) => 1 // short typing
    //     bulletStartPos.x = transform.position.x + bulletDirection * 5;
    //     bulletStartPos.y = transform.position.y;
    // }

    protected override void BeingHit2()
    {
        isImmune = true;
    }
    protected override IEnumerator Flickering()
    {
        while (isImmune)
        {
            FillPhase(0.4f);
            yield return new WaitForSeconds(0.05f);
            FillPhase(0f);
            yield return new WaitForSeconds(0.05f);
        }
    }
    private void DisableDash()
    {
        dashAttack.CancelAttack();
        dashAttackHitbox.GetComponent<BoxCollider2D>().size = new Vector2(5.9f, 10f);
        dashAttackHitbox.GetComponent<BoxCollider2D>().offset = new Vector2(0f, 0f);
        isAttacking = false;
        // fall distance after disabled is > 14.5f => set isFalling = true to show full animation (Land then Idle)
        if (swipeDirectionOnScreen.y < 0f) // condition not suit
        {
            isFalling = true;
        }
        else { isFalling = false; }
        if (isGrounded) { Land(); }
    }

    protected override void update2()
    {
        // Track Player's Health
        ratio = characterHealth.RatioHealth;
        _playerHealth.UpdateHealthBar(ratio);
        // immunity
        if (isImmune)
        {
            invincibilityTime -= Time.deltaTime;
            if (invincibilityTime < 0)
            {
                isImmune = false;
                invincibilityTime = SetInvincibilityTime;
            }
        }
        CameraBounds.GetCameraBoundsLocation(Camera.main, out minX, out maxX, out maxY, out minY);
        //CalculateBoundsLocation();
        GetPlayerHurtBoxSize();
        //GetShootPosAndDirection();
        // dashAttack.TakeTime(Time.deltaTime);
        if (characterHealth.IsDead == false)
        {
            if (dashAttack.IsPerforming)
            {
                Move();
            }
            // Teleport: Click to desired destination and the player teleports after releasing click
            // Move: Click, swipe, then release to move according to the swipe direction
            if (Input.GetMouseButtonDown(0))
            {
                // target: actual destination, click_position:  desired destination
                startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startScreenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                endScreenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                SwipeOrTeleport();
            }

            // lock player in camera, reset velocity when hit camera bound 
            // to do that, get half player hurtbox's size  and limiting pos relative to the bound (sideway only) (5f)
            if (transform.position.x < minX + playerWidth / 2 + 5f || transform.position.x > maxX - playerWidth / 2 - 5f
            || transform.position.y > maxY - playerHeight / 2 || transform.position.y < minY + playerHeight / 2 + 1f)
            {
                // Debug.Log("Blocked");
                body.velocity = Vector3.zero;
                if (dashAttack.IsPerforming)
                {
                    DisableDash();
                }

            }
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX + playerWidth / 2, maxX - playerWidth / 2);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY + playerHeight / 2 + 1f, maxY - playerHeight / 2);
            //Debug.Log(clampedPosition.y); // maxY = 26.5f => to block him, clampedPosition.y < 26.5f
            transform.position = clampedPosition;
        }
    }
}



