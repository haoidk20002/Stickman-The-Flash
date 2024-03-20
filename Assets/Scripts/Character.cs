using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public abstract class Character : MonoBehaviour
{
    // Animation
    [SpineAnimation][SerializeField] private string idleAnimationName;
    [SpineAnimation][SerializeField] private string attackAnimationName;
    [SpineAnimation][SerializeField] private string runAnimationName;
    [SpineAnimation][SerializeField] private string hurtAnimationName;
    [SpineAnimation][SerializeField] private string dieAnimationName;
    [SpineAnimation][SerializeField] private string jumpAnimationName;
    // access skeleton animation from outside
    [SerializeField] SkeletonAnimation skeletonAnimation;
    public Spine.AnimationState spineAnimationState => skeletonAnimation.AnimationState;
    public Spine.Skeleton skeleton => skeletonAnimation.Skeleton;
    protected HealthLogic playerHealth = new HealthLogic();

    // // Get the SkeletonData from the SkeletonDataAsset
    // var skeletonData = skeletonDataAsset.GetSkeletonData(true);

    // // Find the animation by name
    // var animation = skeletonData.FindAnimation(animationName);

    //protected float anim_duration;
    protected string playing_anim;
    void Awake()
    {
        playerHealth.Init(10);
    }

    void Start()
    {
        start2();
    }
    protected virtual void start2() { }
    protected void PlayAnimation(string anim_name, bool is_loop)
    {

        if (anim_name != playing_anim)
        {
            spineAnimationState.SetAnimation(0, anim_name, is_loop);
            playing_anim = anim_name;
        }

        //spineAnimationState.SetAnimation(0, anim_name, is_loop);
    }
    protected void AddAnimation(string anim_name, bool is_loop, float delay)
    {
        spineAnimationState.AddAnimation(0, anim_name, is_loop, delay);
        playing_anim = anim_name;
    }

    protected abstract List<Character> findTarget();
    protected void Attack()
    {
        PlayAnimation(attackAnimationName, false);
        var charaters = findTarget();
        foreach (Character c in charaters)
        {
            c.BeingHit();
        }
    }
    protected void Run()
    {
        PlayAnimation(runAnimationName, true);
    }
    protected void Run(float sec)
    {
        AddAnimation(runAnimationName, true, sec);
    }

    protected void Idle()
    {
        PlayAnimation(idleAnimationName, true);
    }



    protected void Idle(float sec)
    {
        AddAnimation(idleAnimationName, true, sec);
    }

    protected void Die()
    {
        PlayAnimation(dieAnimationName, false);
    }

    protected void EmptyAttack()
    {
        PlayAnimation(attackAnimationName, false);
    }

    protected void BeingHit()
    {
        playerHealth.TakeDamage(5);
        if (playerHealth.IsDead)
        {
            Die();
            Destroy(gameObject, 1f);
        }
        else
        {
            //Debug.Log("...");
            PlayAnimation(hurtAnimationName, false);
            AddAnimation(idleAnimationName, true, 0);
        }
    }
    protected void Jump()
    {
        PlayAnimation(jumpAnimationName, false);
    }

    protected void Jump(float sec)
    {
        AddAnimation(jumpAnimationName, false, sec);
    }
    void Update()
    {

    }
}
