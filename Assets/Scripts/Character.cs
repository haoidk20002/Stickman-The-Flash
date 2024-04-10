using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;
using Unity.VisualScripting;

public abstract class Character : MonoBehaviour
{
    // Animation
    [SpineAnimation][SerializeField] private string idleAnimationName;
    [Header("Attack")]
    [SerializeField] private UnitAttack basicAttack;
    [SerializeField] protected MeleeBullet basicAttackHitBox;

    [Header("Other Anim")]
    [SpineAnimation][SerializeField] private string runAnimationName;
    [SpineAnimation][SerializeField] private string hurtAnimationName;
    [SpineAnimation][SerializeField] private string dieAnimationName;
    [SpineAnimation][SerializeField] private string jumpAnimationName;
    // access skeleton animation from outside
    [SerializeField] SkeletonAnimation skeletonAnimation;
    public Spine.AnimationState spineAnimationState => skeletonAnimation.AnimationState;
    public Spine.Skeleton skeleton => skeletonAnimation.Skeleton;
    protected HealthLogic playerHealth = new HealthLogic();
    protected Vector2 hitboxOffset;
    protected Coroutine attackCoroutine;
    protected void Rotate(float degree)
    {
        transform.Rotate(0f,degree,0f);
    }
    protected string playing_anim;

    public Action<int> Evt_MeleeAttack;

    [Header("Stats")]
    [SerializeField] protected int health;
    [SerializeField] protected int damage;
    // void Awake()
    // {
    //     playerHealth.Init(10);
    // }
    void Start()
    {
        playerHealth.Init(health);
        basicAttack.Init();
        basicAttack.Evt_EnableBullet += value => basicAttackHitBox.GetComponent<BoxCollider2D>().enabled = value;
        basicAttackHitBox.OnHit += dealDmg;
        start2();
    }
    protected virtual void start2() { }
    protected virtual void update2() {}
    protected void PlayAnimation(string name, float durationInSeconds, bool isLoop)
        {
            if (name == playing_anim) return;
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
            playing_anim = name;
        }
    protected void AddAnimation(string anim_name, bool is_loop, float delay)
    {
        spineAnimationState.AddAnimation(0, anim_name, is_loop, delay);
        playing_anim = anim_name;
    }
    protected abstract Character findTarget();
    protected void Attack()
    {
        PlayAnimation(basicAttack.AttackAnim, basicAttack.AttackTime ,false);
        basicAttack.Trigger();
        //Debug.Break();
    }
    protected void Turn(float direction)
    {
        transform.rotation = Quaternion.Euler(0,direction < 0? -180:0,0);
    }
    protected void Run()
    {
        PlayAnimation(runAnimationName, 0f ,true);
    }
    protected void Run(float sec)
    {
        AddAnimation(runAnimationName, true, sec);
    }
    protected void Idle()
    {
        PlayAnimation(idleAnimationName, 0f , true);
    }
    protected void Idle(float sec)
    {
        AddAnimation(idleAnimationName, true, sec);
    }
    protected void Die()
    {
        PlayAnimation(dieAnimationName, 0f , false);
    }

    protected void EmptyAttack()
    {
        PlayAnimation(basicAttack.AttackAnim, basicAttack.AttackTime ,false);
    }
    protected void dealDmg(Character enemy, int value)
    {
        //Debug.Log("enemy" + enemy.name + ", this: " + this.name);
        if (enemy != this){
            enemy.beingHit(value);
            Debug.Log("Hit");
        }
    }
    private void beingHit(int damage)
    {
        playerHealth.TakeDamage(damage);
        //show damage pop up
        GameManager.Instance.ShowDamagePopUp(transform.position, damage.ToString());
        // different interaction whether character is dead or not
        if (playerHealth.IsDead)
        {
            Die();
            Destroy(gameObject, 1f);
        }
        else
        {
            //Debug.Log("...");
            PlayAnimation(hurtAnimationName, 0f , false);
            AddAnimation(idleAnimationName, true, 0);
        }
    }
    protected void Jump()
    {
        PlayAnimation(jumpAnimationName, 0f , false);
    }

    protected void Jump(float sec)
    {
        AddAnimation(jumpAnimationName, false, sec);
    }
    void Update()
    {
        basicAttack.TakeTime(Time.deltaTime);
        update2();
    }

    public void CallMeleeAttack(int damage){
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
        if(_currentTime >= 0 && _currentTime < attackTime)
        {
            _currentTime += time;
            //Debug.Log("Current: "+_currentTime +"Enable at: " + enableBulletTime + "Enable?: " + _isEnableBullet);
            if(_currentTime >= enableBulletTime && !_isEnableBullet)
            { 
                //Debug.Log("Hit");
                Evt_EnableBullet?.Invoke(true);
                _isEnableBullet = true;
            }
            if(_currentTime >= attackTime) Evt_EnableBullet?.Invoke(false);
        }
    }
}
