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
    [SerializeField] protected UnitAttack basicAttack;
    [SerializeField] protected MeleeBullet basicAttackHitBox;
    [Header("Special Attacks")]
    [SerializeField] protected UnitAttack dashAttack;
    [SerializeField] protected MeleeBullet dashAttackHitbox;
    [SerializeField] protected UnitAttack spinningAttack;
    [SerializeField] protected MeleeBullet spinningAttackHitbox;

    [Header("Other Animations")]
    [SpineAnimation][SerializeField] private string fallAnimationName;
    [SpineAnimation][SerializeField] private string landAnimationName;
    [SpineAnimation][SerializeField] private string runAnimationName;
    [SpineAnimation][SerializeField] private string hurtAnimationName;
    [SpineAnimation][SerializeField] private string dieAnimationName;
    [SpineAnimation][SerializeField] private string jumpAnimationName1;
    [SpineAnimation][SerializeField] private string jumpAnimationName2;
    [SpineAnimation][SerializeField] private string prepareAnimationName1;
    [SpineAnimation][SerializeField] private string prepareAnimationName2;
    [SpineAnimation][SerializeField] private string spinningAnimationName;
    [SpineAnimation][SerializeField] private string endSpinningAnimationName;
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
    protected bool isMoving = false, isAttacking = false, isJumping = false, isDamaged = false, isFalling = false, isDashing = false, isLanding = false;
    public int directionSign;
    protected float damagedAnimTime;
    protected float landingAnimTime = 0.5f;
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

    public Action<bool> Evt_PlayerDead;
    protected Rigidbody2D body;

    protected BoxCollider2D characterHurtBox;
    protected Vector2 offset;
    protected Vector3 offset3D;
    protected Vector2 verlocity;

    protected Material material;
    protected Spine.TrackEntry playing;

    protected MeshRenderer meshRenderer;

    protected Color lightRed = new Color(212f / 255f, 79f / 255f, 79f / 255f, 1f); //(212f,79f,79f,1f);
    protected Color transparent = new Color(212f / 255f, 79f / 255f, 79f / 255f, 0f); //(212f,79f,79f,0f);

    protected SpriteRenderer meleeHitBoxSprite;

    private RaycastHit2D hit;

    // other stuff
    private MaterialPropertyBlock mpb;
    public CharacterStats stats;


    public virtual void LoadStats(CharacterStats characterStats)
    {
        stats = characterStats;
    }
    private void Start()
    {
        damagedAnimTime = setdamagedAnimTime;
        // Access spine Unity sprite marterial
        mpb = new MaterialPropertyBlock();
        FillPhase(0f);

        body = GetComponent<Rigidbody2D>(); // get character's Rigidbody
        characterHealth.Init(stats.health); // Initialize HP
        // Basic Attack Initialize
        basicAttack.Init(); // Initialize Basic Attack Hitbox
        basicAttack.Evt_EnableBullet += value => basicAttackHitBox.GetComponent<BoxCollider2D>().enabled = value;
        basicAttackHitBox.OnHit = DealDmg;
        //basicAttack.Evt_PlayAttackSound+= PlayPunchEffect;
        // Dash Attack Initialize
        dashAttack.Init();
        dashAttack.Evt_EnableBullet += value => dashAttackHitbox.GetComponent<BoxCollider2D>().enabled = value;
        dashAttackHitbox.OnHit = DealDmg;
        // Spinning Attack Initialize
        spinningAttack.Init();
        spinningAttack.Evt_EnableBullet += value => spinningAttackHitbox.GetComponent<CircleCollider2D>().enabled = value;
        spinningAttackHitbox.OnHit = DealDmg;
        // Get sprite renderer
        meleeHitBoxSprite = basicAttackHitBox.GetComponent<SpriteRenderer>();

        // Setting Hurtbox's offset
        characterHurtBox = GetComponent<BoxCollider2D>();
        offset = characterHurtBox.offset;
        offset.x = 0f;
        // Get ground's y pos
        getGroundPos = Physics2D.Raycast(transform.position, Vector2.down, 1000f, groundLayerMask);
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
            //Debug.Log(skeletonAnimation.timeScale);
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
    }
    protected abstract Character FindTarget();
    public void PlayPunchEffect(){
        SoundManager.Instance.PlaySoundEffect(3,transform,1f);
    }
    protected void Attack()
    {
        SoundManager.Instance.PlaySoundEffect(3,transform,1f);
        isAttacking = true;
        attackAnimTime = basicAttack.AttackTime;
        PlayAnimation(basicAttack.AttackAnim, basicAttack.AttackTime, false);
        basicAttack.Trigger();
        Evt_MeleeAttack?.Invoke(stats.damage);
        AddAnimation(idleAnimationName, true, 0f);
    }
    protected void DashAttack()
    {
        // player
        // old hitbox size (5.9,10) offset (0,0)
        // new hitbox size (9,13) offset(-2,0)
        if (gameObject.tag == "Player")
        {
            dashAttackHitbox.GetComponent<BoxCollider2D>().size = new Vector2(9f, 13f);
            dashAttackHitbox.GetComponent<BoxCollider2D>().offset = new Vector2(-2f, 0f);
        }
        // boss
        // old hitbox size (5.9,10) offset (0,0)
        // new hitbox size (12,13) offset(-5,0) 
        if (gameObject.tag == "Boss")
        {
            dashAttackHitbox.GetComponent<BoxCollider2D>().size = new Vector2(12f, 13f);
            dashAttackHitbox.GetComponent<BoxCollider2D>().offset = new Vector2(5f, 0f);
        }

        isDashing = true;
        isAttacking = true;
        attackAnimTime = dashAttack.AttackTime;
        Dash(0);
        dashAttack.Trigger();
        if (gameObject.tag == "Player") { Evt_MeleeAttack?.Invoke((int)stats.damage / 2); }
        if (gameObject.tag == "Boss") { Evt_MeleeAttack?.Invoke(stats.damage * 2); }
    }
    protected void SpinningAttack()
    {
        isAttacking = true;
        attackAnimTime = spinningAttack.AttackTime;
        Spin();
        spinningAttack.Trigger();
        Evt_MeleeAttack?.Invoke(stats.damage * 3);
    }
    protected void Turn(float direction)
    {
        directionSign = direction < 0 ? -1 : 1;
        transform.rotation = Quaternion.Euler(0, direction < 0 ? -180 : 0, 0);
    }
    protected void Spin()
    {
        PlayAnimation(spinningAnimationName, 0f, true);
    }
    protected void EndSpin()
    {
        PlayAnimation(endSpinningAnimationName, 0f, false);
    }
    protected void Run()
    {
        if (gameObject.tag == "Player" ||isGrounded == true && (gameObject.tag == "Boss" || gameObject.tag == "Enemy"))
        {
            PlayAnimation(runAnimationName, 0f, true);
        }
    }
    protected void Dash(float duration)
    {
        SoundManager.Instance.PlaySoundEffect(2,transform,0.5f);
        PlayAnimation(dashAttack.AttackAnim, duration, true);
    }

    protected void Prepare()
    {
        PlayAnimation(prepareAnimationName1, 0f, false);
        AddAnimation(prepareAnimationName2, true, 0);
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
        if (gameObject.tag == "Player")
        {
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
        Debug.Log("Attacked");
        if (enemy != this)
        {
            if (enemy.isImmune) return; // if other is immune, no damage => BeingHit not called
            enemy.BeingHit(value);
        }
    }
    protected void BeingHit(int damage)
    {
        Debug.Log("punched");
        BeingHit2();
        isDamaged = true;
        SoundManager.Instance.PlaySoundEffect(0,transform,0.75f);
        damagedAnimTime = setdamagedAnimTime;
        characterHealth.TakeDamage(damage);
        // Show damage pop up
        GameManager.Instance.ShowDamagePopUp(transform.position, damage.ToString());
        // Different anim whether character is dead or not
        if (characterHealth.IsDead)
        {
            Die();
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
        if (spinningAttack.IsPerforming) { return; }
        if (!isDashing && !isDamaged && !isAttacking && Mathf.Abs(transform.position.y - groundYPos) > 14.5f)
        {
            PlayAnimation(fallAnimationName, 0f, true);
            isFalling = true;
        }
    }

    protected void Land()
    {
        if (spinningAttack.IsPerforming)
        {
            SoundManager.Instance.PlaySoundEffect(4,transform,1f);
            spinningAttack.CancelAttack();
            isAttacking = false;
            EndSpin();
            return;
        }
        else if (!isLanding && isFalling && !isAttacking)
        {
            SoundManager.Instance.PlaySoundEffect(1,transform,1f);
            PlayAnimation(landAnimationName, 0f, false);
            AddAnimation(idleAnimationName, true, 0);
            isLanding = true;
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
    protected void IsAttackingControl()
    {
        if (isAttacking == true)
        {
            attackAnimTime -= Time.deltaTime;

            if (attackAnimTime < 0)
            {
                isAttacking = false;
            }
        }
    }
    protected void IsLandingControl()
    {
        if (isLanding == true)
        {
            landingAnimTime -= Time.deltaTime;

            if (landingAnimTime < 0)
            {
                isLanding = false;
                landingAnimTime = 0.5f;
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
            if (!characterHealth.IsDead) // Don't execute if dead
            {
                try
                {
                    playing = spineAnimationState.GetCurrent(0);
                    playingAnim = playing.ToString();
                }
                catch (Exception e) { Debug.Log(e); }
                //
                if (!isLanding && !isAttacking && !isMoving && !isJumping && !isDamaged && !isFalling && !isDashing)
                {
                    Idle();
                }


                basicAttack.TakeTime(Time.deltaTime);
                dashAttack.TakeTime(Time.deltaTime);
                spinningAttack.TakeTime(Time.deltaTime);

                IsAttackingControl();
                IsDamagedControl();
                IsLandingControl();
            }
            update2();
        }
    }
    void FixedUpdate()
    {
        if (!characterHealth.IsDead)
        {
            // The old frame data (old isGounded value) is stored in wasGrounded
            wasGrounded = isGrounded;
            // The new frame data is collected using raycast and put in isGrounded
            hit = Physics2D.Raycast(new Vector2(transform.position.x + offset.x, transform.position.y + offset.y), Vector2.down, characterHurtBox.size.y / 2 + 1f, groundLayerMask);
            Debug.DrawLine(transform.position, new Vector2(transform.position.x + offset.x, transform.position.y + offset.y - characterHurtBox.size.y / 2 - 1f), Color.green);
            //Debug.DrawLine(transform.position, new Vector2(transform.position.x + offset.x, transform.position.y + offset.y - characterHurtBox.size.y / 2), Color.green);
            if (hit.collider != null)
            {
                isGrounded = true;
                isJumping = false;
            }
            else isGrounded = false;
            // Land not called during attack and damaged
            if (body.velocity.y < 0 && !isGrounded)
            {
                Fall();
            }
            if (isGrounded == true && wasGrounded == false)
            {
                Land();
            }
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
    [SerializeField] private float disableBulletTime;
    public event Action<bool> Evt_EnableBullet;
    public event Action Evt_PlayAttackSound;
    //private Character _character;


    public Action OnEnd;
    private float _currentTime;
    private bool _isEnableBullet = false;
    public bool IsPerforming => _currentTime >= 0 && _currentTime < attackTime;

    public void Init()
    {
        //_character = character;
        Evt_EnableBullet?.Invoke(false);
        _currentTime = -1;
    }
    public void Trigger()
    {
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
                //_character.PlayPunchEffect();
                Evt_PlayAttackSound?.Invoke();
                Evt_EnableBullet?.Invoke(true);
                _isEnableBullet = true;
            }
            // if (_currentTime >= attackTime)
            // {
            //     //Debug.Break();
            //     _isEnableBullet = false;
            //     Evt_EnableBullet?.Invoke(false);
            //     OnEnd?.Invoke();
            // }
            if (_currentTime >= disableBulletTime)
            {
                _isEnableBullet = false;
                Evt_EnableBullet?.Invoke(false);
                OnEnd?.Invoke();
            }
        }
    }
    public void CancelAttack()
    {
        //Debug.Break();
        //_currentTime = AttackTime;
        _currentTime = disableBulletTime;
        _isEnableBullet = false;
        Evt_EnableBullet?.Invoke(false);
        OnEnd?.Invoke();
    }
}

[Serializable]
public class CharacterStats
{
    public int health;
    public int damage;
    public float moveSpeed;
    public float detectRange;
}
