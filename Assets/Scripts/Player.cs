using UnityEngine;
using Spine.Unity;
using Unity.Collections;
using UnityEditor.U2D.Animation;
using System;
using System.ComponentModel;



public class Player : Character
{
    [Header("Dash Attack")]
    [SerializeField] private UnitAttack dashAttack;
    private float cooldown = 0.5f, lastAttackedAt = 0f, radius = 1.25f;
    private float moveDistance = 5f;
    [SerializeField] private float moveSpeed = 50f;
    private float attackRange = 6f, minSwipeDistance = 8f;
    private float minX, maxX;
    [SerializeField] private float timer;
    private float currentTimer;
    private float multiplier = 10f;
    private float swipeMagnitude, playerWidth, direction;
    [SerializeField] private float SetInvincibilityTime;
    private float invincibilityTime;

    // Coordinates
    private Vector2 oldPos, newPos, target, clickPosition, cameraBounds, cameraPos;
    private Vector2 startPos, dashDestination, teleportDestination, swipeDirection;

    // get camera bonuds first, then lock player in bounds
    //private Rigidbody2D body;
    public LayerMask DetectLayerMask;
    public GameObject bullet;
    private Vector3 bulletStartPos;
    private void Awake()
    {
        health = 20;
        damage = 4;
    }
    protected override void start2()
    {
        SettingMainCharacterValue1();
        SettingMainCharacterValue2();
        bulletStartPos = transform.position;
        //isImmune = true;
        invincibilityTime = SetInvincibilityTime;

    }
    private void SettingMainCharacterValue1()
    {
        target = transform.position;
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
        healthBar = GameObject.Find("PlayerHealth").GetComponentInChildren<HealthBar>();
        dashDestination = transform.position;
        dashAttack.Init();
        dashAttack.Evt_EnableBullet += value => basicAttackHitBox.GetComponent<BoxCollider2D>().enabled = value;
        currentTimer = timer;
        //playerLayerMask = 1 << LayerMask.NameToLayer("Enemy");
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
    private void GetPlayerStat()
    {
        playerWidth = transform.GetComponent<BoxCollider2D>().size.x;
    }
    protected override Character findTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(target, radius, DetectLayerMask);
        if (hitColliders.Length > 0) { return hitColliders[0].GetComponent<Character>(); } /* Return the first character found*/
        else { return null; }
    }
    private void TriggerAttack()
    {
        if (Time.time > lastAttackedAt + cooldown)
        {
            //Debug.Log("Hit");
            lastAttackedAt = Time.time;
            Attack();
            Evt_MeleeAttack?.Invoke(damage);
        }
    }
    private void Teleport()
    {
        var enemies = findTarget();

        // if enemy is found, player teleports close to it then attack, else teleport to the clicked point
        if (enemies != null)
        {
            var enemy = enemies.gameObject.transform.position;
            teleportDestination = target;
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
            Idle();
            oldPos = transform.position; transform.position = target;
            direction = transform.position.x - oldPos.x;
            Turn(direction);
        }
        // reseting falling velocity
        body.velocity = Vector3.zero;
    }
    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, dashDestination, moveSpeed * Time.deltaTime);
        Vector2 verlocity = body.velocity; verlocity.y = 0;
        body.velocity = verlocity;
    }

    private void SwipeOrTeleport()
    {
        Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        swipeDirection = endPos - startPos;
        // Determine if the swipe was long enough (you can set a threshold)
        swipeMagnitude = swipeDirection.magnitude;
        //Debug.Log(swipeMagnitude*multiplier);
        if (swipeMagnitude > minSwipeDistance)
        {
            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y)) // Check the direction of the swipe
            {
                CalculateMove();
            }
        }
        else
        {
            isMoving = false;
            Teleport();
        }
    }

    private void CalculateMove()
    {   // Distance depend on swipe distance, get to destination in time duration
        currentTimer = timer;
        dashDestination.y = transform.position.y;
        int value;
        if (swipeDirection.x > 0)
        {
            value = 1;
        }
        else
        {
            value = -1;
        }
        Turn(swipeDirection.x);
        dashDestination.x = transform.position.x + value * swipeMagnitude * multiplier;
        //Debug.Break();
        DashAttack();

        //Debug.Log("Swipe Distance:" + swipeMagnitude + " Move Distance: " + swipeMagnitude);
    }
    private void DashAttack()
    {
        isMoving = true;
        isAttacking = true;
        attackAnimTime = dashAttack.AttackTime;
        PlayAnimation(dashAttack.AttackAnim, dashAttack.AttackTime, false);
        dashAttack.Trigger();
        Evt_MeleeAttack?.Invoke((int)damage / 2);
        AddAnimation(idleAnimationName, true, 0f);
    }
    // may be set as protected in Character class
    private void GetShootPosAndDirection()
    {
        int bulletDirection;
        // if(direction > 0){
        //     bulletDirection = 1;
        // }else {bulletDirection = -1;}
        bulletDirection = direction > 0 ? 1 : -1; // forward (direction > 0) => 1 // short typing
        bulletStartPos.x = transform.position.x + bulletDirection * 5;
        bulletStartPos.y = transform.position.y;
    }

    protected override void BeingHit2(){
        Debug.Log("isImmune: "+ isImmune);
        isImmune = true;
    }

    protected override void update2()
    {
        // if got hit, immune to damage for duration
        // if (isDamaged){
        //     isImmune = true;
        //     Debug.Log("Hit");
        // }
        if(isImmune){
            invincibilityTime -= Time.deltaTime;
            //gameObject.GetComponentInChildren<SkeletonAnimation>().skeleton.SetSkin(invincibleColor);

            if (invincibilityTime < 0){
                
                isImmune = false;
                //gameObject.GetComponentInChildren<SkeletonAnimation>().skeleton.SetSkin(originalColor);
                invincibilityTime = SetInvincibilityTime;
            }
        }
        CameraBounds.GetCameraBoundsLocation(Camera.main, out minX, out maxX);
        //CalculateBoundsLocation();
        GetPlayerStat();
        GetShootPosAndDirection();
        dashAttack.TakeTime(Time.deltaTime);
        if (playerHealth.IsDead == false)
        {
            if (isMoving == true)
            {
                Move();
                currentTimer -= Time.deltaTime;
                if (currentTimer <= 0f)
                {
                    isMoving = false;
                    isAttacking = false;
                    basicAttackHitBox.enabled = false;
                }
            }
            else
            {
                currentTimer = timer;
            }
            // Teleport: Click to desired destination and the player teleports after releasing click
            // Move: Click, swipe, then release to move according to the swipe direction
            if (Input.GetMouseButtonDown(0))
            {
                // target: actual destination, click_position:  desired destination
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // swipe
                startPos = target; // Detect the start of the swipe
            }
            if (Input.GetMouseButtonUp(0))
            {
                SwipeOrTeleport();
            }
            // Track Player's Health
            ratio = playerHealth.RatioHealth;
            healthBar.UpdateHealthBar(ratio);

            // lock player in camera, reset velocity when hit camera bound
            if (transform.position.x < minX + playerWidth / 2 + 5f|| transform.position.x > maxX - playerWidth / 2 - 5f){
                body.velocity = Vector3.zero;
            }
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX + playerWidth / 2, maxX - playerWidth / 2);
            transform.position = clampedPosition;

            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     Debug.Log("pressed");
            //     Instantiate(bullet, bulletStartPos, bullet.transform.rotation);
            // }
        }
    }
}



