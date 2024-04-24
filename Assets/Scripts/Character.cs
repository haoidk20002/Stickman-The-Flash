using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;
using Unity.VisualScripting;

public abstract class Character : MonoBehaviour
{
    // Animation
    [SpineAnimation][SerializeField] protected string idleAnimationName;
    [Header("Basic Attack")]
    [SerializeField] private UnitAttack basicAttack;
    [SerializeField] protected MeleeBullet basicAttackHitBox;

    [Header("Other Anim")]
    [SpineAnimation][SerializeField] private string fallAnimationName;
    [SpineAnimation][SerializeField] private string landAnimationName;
    [SpineAnimation][SerializeField] private string runAnimationName;
    [SpineAnimation][SerializeField] private string hurtAnimationName;
    [SpineAnimation][SerializeField] private string dieAnimationName;
    [SpineAnimation][SerializeField] private string jumpAnimationName1;
    [SpineAnimation][SerializeField] private string jumpAnimationName2;
    // access skeleton animation from outside
    [SerializeField] SkeletonAnimation skeletonAnimation;
    public Spine.AnimationState spineAnimationState => skeletonAnimation.AnimationState;
    public Spine.Skeleton skeleton => skeletonAnimation.Skeleton;
    protected HealthLogic playerHealth = new HealthLogic();
    protected Vector2 hitboxOffset;
    protected Coroutine attackCoroutine;
    [SerializeField] protected string playingAnim;

    protected int enemyLayerMask, groundLayerMask, playerLayerMask;

    // Check all state using boolean
    protected bool isMoving = false, isAttacking = false, isJumping = false, isDamaged = false, isFalling = false;
    protected int directionSign;
    protected float damagedAnimTime;
    [SerializeField] protected float setdamagedAnimTime = 0.5f;

    protected float ratio;
    protected HealthBar healthBar;

    protected float attackAnimTime;
    protected Vector3 groundPos, backgroundPos, groundLocalPos;
    protected float groundYPos;
    protected bool wasGrounded, isGrounded;
    protected bool isImmune = false;

    public Action<int> Evt_MeleeAttack;
    public Action<Character, int> Evt_ShootingAttack;
    protected Rigidbody2D body;

    protected BoxCollider2D characterHurtBox;
    protected Vector2 offset;
    protected Vector3 offset3D;

    [SerializeField] protected string flashColor;
    [SerializeField] protected string invincibleColor;
    protected string originalColor; //skeleton animation's initial color




    [Header("Stats")]
    [SerializeField] protected int health;
    [SerializeField] protected int damage;

    void Start()
    {
        originalColor = gameObject.GetComponentInChildren<SkeletonAnimation>().initialSkinName;

        //Debug.Log("Color: " + originalColor + "Flash Color: " + flashColor); // get inital color
        body = GetComponent<Rigidbody2D>(); // get character's Rigidbody
        playerHealth.Init(health); // Initialize HP
        basicAttack.Init(); // Initialize Basic Attack Hitbox
        basicAttack.Evt_EnableBullet += value => basicAttackHitBox.GetComponent<BoxCollider2D>().enabled = value;
        basicAttackHitBox.OnHit += DealDmg;
        // Specifying layer masks
        enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");
        groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        // Setting Hurtbox's offset
        characterHurtBox = GetComponent<BoxCollider2D>();
        offset = characterHurtBox.offset; offset3D = offset; offset3D.x = 0;
        start2();
    }
    protected virtual void start2() { }
    protected virtual void update2() { }

    protected void PlayAnimation(string name, float durationInSeconds, bool isLoop)
    {
        if (name == playingAnim) return;
        var spineAnimation = spineAnimationState.Data.SkeletonData.FindAnimation(name);
        if (spineAnimation == null) return;

        if (durationInSeconds > 0)
        {
            skeletonAnimation.timeScale = spineAnimation.Duration / durationInSeconds;
        }
        else
        {
            skeletonAnimation.timeScale = 1;
        }

        spineAnimationState.SetAnimation(0, spineAnimation, isLoop);
        playingAnim = name;
    }
    protected void AddAnimation(string anim_name, bool is_loop, float delay)
    {
        spineAnimationState.AddAnimation(0, anim_name, is_loop, delay);
        // only set value playingAnim after the added anim started playing

        playingAnim = anim_name;
    }
    protected abstract Character findTarget();
    protected void Attack()
    {
        isAttacking = true;
        attackAnimTime = basicAttack.AttackTime;
        PlayAnimation(basicAttack.AttackAnim, basicAttack.AttackTime, false);
        basicAttack.Trigger();
        Debug.Log("Triggered");
        AddAnimation(idleAnimationName, true, 0f);
    }
    protected void Turn(float direction)
    {
        directionSign = direction < 0 ? -1 : 1;
        transform.rotation = Quaternion.Euler(0, direction < 0 ? -180 : 0, 0);
    }
    protected void Run()
    {
        if (isGrounded == true)
        {
            PlayAnimation(runAnimationName, 0f, true);
        }
    }
    protected void Idle()
    {
        // if(isDamaged == false){
        //     PlayAnimation(idleAnimationName, 0f, true);
        // }
        PlayAnimation(idleAnimationName, 0f, true);
    }
    protected void Die()
    {
        PlayAnimation(dieAnimationName, 0f, false);
        Destroy(gameObject, 1f);
    }

    protected void Hurt()
    {
        //gameObject.GetComponentInChildren<SkeletonAnimation>().skeleton.SetSkin(flashColor);
        PlayAnimation(hurtAnimationName, 0f, false);
        AddAnimation(idleAnimationName, true, 0);
    }
    protected void DealDmg(Character enemy, int value)
    {
        if (enemy != this)
        {
            enemy.BeingHit(value);
        }
    }
    private void BeingHit(int damage)
    {
        if (isImmune) return;
        
        isDamaged = true;
        playerHealth.TakeDamage(damage);
        //show damage pop up
        GameManager.Instance.ShowDamagePopUp(transform.position, damage.ToString());
        // different anim whether character is dead or not
        if (playerHealth.IsDead)
        {
            Die();
        }
        else
        {
            Hurt();
        }
    }

    protected void Jump()
    {
        if (isGrounded == true)
        {
            PlayAnimation(jumpAnimationName1, 0f, false);
            AddAnimation(jumpAnimationName2, false, 0);
        }
    }
    protected void Fall()
    {
        if (!isAttacking && Mathf.Abs(transform.position.y - groundYPos) > 14.5f)
        {
            PlayAnimation(fallAnimationName, 0f, false);
            isFalling = true;
        }
    }

    protected void Land()
    {
        if (!isFalling) return;
        if (isAttacking == false && isDamaged == false) // vertical distance > 6
        {
            PlayAnimation(landAnimationName, 0f, false);
            AddAnimation(idleAnimationName, true, 0);
            isFalling = false;
        }
    }
    protected void SettingDamagedEffect()
    {
        if (isDamaged == true)
        {
            damagedAnimTime -= Time.deltaTime;
            if (damagedAnimTime < 0)
            {
                //gameObject.GetComponentInChildren<SkeletonAnimation>().skeleton.SetSkin(originalColor);
                damagedAnimTime = setdamagedAnimTime;
                isDamaged = false;
            }
        }
    }
    void Update()
    {
        groundLocalPos = GameObject.Find("Ground").transform.localPosition; // ground's local pos relative to parent (LvlBackground)
        Debug.Log("Local: "+ groundLocalPos);
        groundPos = GameObject.Find("Ground").transform.parent.TransformPoint(groundLocalPos); // Convert local pos to global pos
        Debug.Log("Global: "+ groundPos);
        groundYPos = groundPos.y; // only care ground Y pos
        basicAttack.TakeTime(Time.deltaTime);
        // controlling isAttacking
        if (isAttacking == true)
        {
            attackAnimTime -= Time.deltaTime;
            if (attackAnimTime < 0)
            {
                isAttacking = false;
            }
        }
        // Controlling isDamaged
        SettingDamagedEffect();
        update2();
    }
    void FixedUpdate()
    {

        // The old frame data (old isGounded value) is stored in wasGrounded
        wasGrounded = isGrounded;
        // The new frame data is collected using raycast and put in isGrounded
        RaycastHit2D hit = Physics2D.Raycast(transform.position + offset3D, Vector2.down, characterHurtBox.size.y / 2 + 0.1f, groundLayerMask);
        Debug.DrawLine(transform.position, new Vector2(transform.position.x + offset3D.x, transform.position.y + offset3D.y - characterHurtBox.size.y / 2), Color.green);
        if (hit.collider != null)
        {
            isGrounded = true;
            isJumping = false;
        }
        else isGrounded = false;
        // Land not called during attack and damaged
        if (isGrounded == true && wasGrounded == false)
        {
            Land();
        }
        if (body.velocity.y < 0 && !isGrounded)
        {
            Fall();
        }
    }

    public void CallMeleeAttack(int damage)
    {
        Evt_MeleeAttack?.Invoke(damage);
    }
}


// Unit Basic Attack
[Serializable]
public class UnitAttack
{
    [SpineAnimation][SerializeField] private string attackAnimationName;
    public string AttackAnim => attackAnimationName;
    [SerializeField] private float attackTime;
    public float AttackTime => attackTime;

    [SerializeField] private float enableBulletTime;

    public event Action<bool> Evt_EnableBullet;
    private float _currentTime;
    private bool _isEnableBullet;

    public void Init()
    {
        //Debug.Log("Init");
        Evt_EnableBullet?.Invoke(false);
        _currentTime = -1;
    }
    public void Trigger()
    {
        //Debug.Log("Trigger");
        _currentTime = 0;
        _isEnableBullet = false;
    }

    public void TakeTime(float time)
    {
        if (_currentTime >= 0 && _currentTime < attackTime)
        {
            _currentTime += time;
            //Debug.Log("Current: "+_currentTime +"Enable at: " + enableBulletTime + "Enable?: " + _isEnableBullet);
            if (_currentTime >= enableBulletTime && !_isEnableBullet)
            {
                //Debug.Log("Hit");
                Evt_EnableBullet?.Invoke(true);
                _isEnableBullet = true;
            }
            if (_currentTime >= attackTime) Evt_EnableBullet?.Invoke(false);
        }
    }
}
