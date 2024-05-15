using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;
using Unity.VisualScripting;

public abstract class Character : MonoBehaviour
{
    //public GameObject ground;
    // Animation
    [SpineAnimation][SerializeField] protected string idleAnimationName;
    [Header("Basic Attack")]
    [SerializeField] protected UnitAttack basicAttack;
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
    public HealthLogic characterHealth = new HealthLogic();
    protected Vector2 hitboxOffset;
    protected Coroutine characterWaitForAttack, attackWarning;
    [SerializeField] protected string playingAnim;

    protected int enemyLayerMask => 1 << LayerMask.NameToLayer("Enemy");
    protected int groundLayerMask => 1 << LayerMask.NameToLayer("Ground");
    protected int playerLayerMask => 1 << LayerMask.NameToLayer("Player");

    // Check all state using boolean
    protected bool isMoving = false, isAttacking = false, isJumping = false, isDamaged = false, isFalling = false;
    public int directionSign;
    protected float damagedAnimTime;
    [SerializeField] protected float setdamagedAnimTime = 0.5f;

    protected float ratio;
    protected HealthBar healthBar;

    protected float attackAnimTime;
    protected Vector3 groundPos, backgroundPos, groundLocalPos;
    protected float groundYPos;

    protected RaycastHit2D getGroundPos;
    protected bool wasGrounded, isGrounded;
    protected bool isImmune = false;
    public bool CheckImmunity => isImmune;

    public Action<int> Evt_MeleeAttack;
    public Action<Character, int> Evt_ShootingAttack;

    public  Action <bool> Evt_PlayerDead;
    protected Rigidbody2D body;

    protected BoxCollider2D characterHurtBox;
    protected Vector2 offset;
    protected Vector3 offset3D;

    protected Material material;
    protected Spine.TrackEntry playing;

    protected MeshRenderer meshRenderer;

    protected Color lightRed = new Color(212f/255f,79f/255f,79f/255f,1f); //(212f,79f,79f,1f);
    protected Color transparent = new Color(212f/255f,79f/255f,79f/255f,0f); //(212f,79f,79f,0f);

    protected SpriteRenderer meleeHitBoxSprite; 



    [Header("Stats")]
    [SerializeField] protected int health;
    [SerializeField] protected int damage;
    // [SerializeField] protected int setDamage;
    // [SerializeField] protected int setHealth;
    [SerializeField] protected int currentDamage;
    [SerializeField] protected int currentHealth;
    //protected int health, damage;

    //
    private MaterialPropertyBlock mpb;

    // private void Awake()
    // {
    //     //Debug.Log("Do sthing");
    //     // Assign stats
    //     health = setHealth;
    //     damage = setDamage;
    // }

    private void Start()
    {
        damagedAnimTime = setdamagedAnimTime;
        // Access spine Unity sprite marterial
        mpb = new MaterialPropertyBlock();
        FillPhase(0f);

        body = GetComponent<Rigidbody2D>(); // get character's Rigidbody
        characterHealth.Init(health); // Initialize HP
        basicAttack.Init(); // Initialize Basic Attack Hitbox
        basicAttack.Evt_EnableBullet += value => basicAttackHitBox.GetComponent<BoxCollider2D>().enabled = value;
        basicAttackHitBox.OnHit += DealDmg;
        // Get sprite renderer
        meleeHitBoxSprite = basicAttackHitBox.GetComponent<SpriteRenderer>();
        // Setting Hurtbox's offset
        characterHurtBox = GetComponent<BoxCollider2D>();
        offset = characterHurtBox.offset; offset3D = offset; offset3D.x = 0;
        // Get ground's y pos
        getGroundPos = Physics2D.Raycast(transform.position, Vector2.down,  1000f, groundLayerMask); 
        groundYPos = getGroundPos.transform.position.y;
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

        //playingAnim = name;
    }
    protected void AddAnimation(string anim_name, bool is_loop, float delay)
    {
        spineAnimationState.AddAnimation(0, anim_name, is_loop, delay);
        // only set value playingAnim after the added anim started playing

        //playingAnim = anim_name;
    }
    protected abstract Character FindTarget();
    protected void Attack()
    {
        isAttacking = true;
        attackAnimTime = basicAttack.AttackTime;
        PlayAnimation(basicAttack.AttackAnim, basicAttack.AttackTime, false);
        basicAttack.Trigger();
        AddAnimation(idleAnimationName, true, 0f);
    }
    protected void Turn(float direction)
    {
        directionSign = direction < 0 ? -1 : 1;
        transform.rotation = Quaternion.Euler(0, direction < 0 ? -180 : 0, 0);
    }
    protected void Run()
    {
        if (gameObject.tag == "Player"){
            PlayAnimation(runAnimationName, 0f, true);
        }
        else if (isGrounded == true && gameObject.tag == "Boss" || gameObject.tag == "Enemy")
        {
            PlayAnimation(runAnimationName, 0f, true);
        }
    }
    protected void Idle()
    {
        PlayAnimation(idleAnimationName, 0f, true);
    }
    protected void Die()
    {
        if (gameObject.tag == "Boss")
        {
            GameManager.Instance.AddScore(200);
        }
        if (gameObject.tag == "Enemy")
        {
            GameManager.Instance.AddScore(100);
        }
        if (gameObject.tag == "Player"){
            //Evt_PlayerDead(true);
        }
        PlayAnimation(dieAnimationName, 0f, false);
        Destroy(gameObject, 1f);
    }

    protected void Hurt()
    {
        StartCoroutine(Flickering());
        if (this == gameObject.CompareTag("Boss")) { return; }
        PlayAnimation(hurtAnimationName, 0f, false);
        AddAnimation(idleAnimationName, true, 0);
    }

    protected virtual IEnumerator Flickering()
    {
        FillPhase(0.4f);
        yield return new WaitForSeconds(0.2f);
        FillPhase(0f);
    }
    protected void DealDmg(Character enemy, int value)
    {
        if (enemy != this)
        {
            if (enemy.isImmune) return;
            enemy.BeingHit(value);
        }
    }
    protected void BeingHit(int damage)
    {
        BeingHit2();
        isDamaged = true;
        damagedAnimTime = setdamagedAnimTime;
        characterHealth.TakeDamage(damage);
        //show damage pop up
        GameManager.Instance.ShowDamagePopUp(transform.position, damage.ToString());
        // different anim whether character is dead or not
        if (characterHealth.IsDead)
        {
            Die();
            //Debug.Break();
        }
        else
        {
            Hurt();
        }
    }
    protected virtual void BeingHit2() { }

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
        //if (){ return;}
        if (!isAttacking && Mathf.Abs(transform.position.y - groundYPos) > 14.5f)
        {
            PlayAnimation(fallAnimationName, 0f, false);
            isFalling = true;
        }
    }

    protected void Land()
    {   
        if (!isFalling) {
            PlayAnimation(idleAnimationName, 0f, true);
            return;
        }
        if (isAttacking == false && isDamaged == false)
        {
            Debug.Log("Land");
            PlayAnimation(landAnimationName, 0f, false);
            AddAnimation(idleAnimationName, true, 0);
            isFalling = false;
        }
    }
    protected virtual void IsDamagedControl()
    {
        if (isDamaged == true)
        {
            damagedAnimTime -= Time.deltaTime;
            if (damagedAnimTime < 0)
            {
                damagedAnimTime = setdamagedAnimTime;
                isDamaged = false;
            }
        }
    }
    protected void FillPhase(float value)
    {
        mpb.SetFloat("_FillPhase", value);
        gameObject.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(mpb);
    }

    private void Update()
    {
        if (!PauseAndContinue.gameIsPaused)
        {
            // view current stats
            currentDamage = damage;
            currentHealth = characterHealth.Current;

            if (!characterHealth.IsDead) // Don't execute if dead
            {
                try
                {
                    playing = spineAnimationState.GetCurrent(0);
                    playingAnim = playing.ToString();
                }
                catch (Exception e) { }
                //playingAnim = playing.ToString();
                // calculating ground y's pos


                basicAttack.TakeTime(Time.deltaTime);
                // controlling isAttacking
                if (isAttacking == true)
                {
                    attackAnimTime -= Time.deltaTime;
                    
                    if (attackAnimTime < 0)
                    {
                        isAttacking = false;
                        //
                        if (isGrounded){
                            Idle();
                        }
                    }
                }
                // Controlling isDamaged // Dashing on ground even when teleport
                // Bug: Player freeze before fall anim
                IsDamagedControl(); // 
            }
                update2();
        }

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
            //Debug.Log("Land");
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
    private bool _isEnableBullet = false;

    public bool isDisabled = false;

    public void Init()
    {
        //Debug.Log("Init");
        Evt_EnableBullet?.Invoke(false);
        _currentTime = -1;
    }
    public void Trigger()
    {
        //Debug.Log("Trigger");
        isDisabled = false;
        _currentTime = 0;
        //_isEnableBullet = false;
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
            if (_currentTime >= attackTime || isDisabled) {
                _isEnableBullet = false;
                Evt_EnableBullet?.Invoke(false);
            }
        }
    }
}
