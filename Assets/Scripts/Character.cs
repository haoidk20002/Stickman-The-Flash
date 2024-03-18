using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public abstract class Character : MonoBehaviour
{
// Animation
    [SpineAnimation] [SerializeField] private string idleAnimationName;
    [SpineAnimation] [SerializeField] private string attackAnimationName;
    [SpineAnimation] [SerializeField] private string runAnimationName;
    [SpineAnimation] [SerializeField] private string hurtAnimationName;
    [SpineAnimation] [SerializeField] private string dieAnimationName;
    [SerializeField] SkeletonAnimation skeletonAnimation;
    public Spine.AnimationState spineAnimationState=> skeletonAnimation.AnimationState;
	public Spine.Skeleton skeleton => skeletonAnimation.Skeleton;
    HealthLogic playerHealth = new HealthLogic();

    void Awake(){
        playerHealth.Init(10);
    }

    void Start(){
        start2();
    }
    protected virtual void start2(){}
    void PlayAnimation(string anim_name, bool is_loop){
        spineAnimationState.SetAnimation(0, anim_name, is_loop);
    }

    protected abstract List<Character> findTarget();
    protected void Attack(){
        PlayAnimation(attackAnimationName,false);
        var charaters = findTarget();
        foreach (Character c in charaters){
           c.BeingHit(); 
        }
    }
    protected void Run(){
        PlayAnimation(runAnimationName,true);
    }

    protected void Idle(){
        PlayAnimation(idleAnimationName,true);
    }

    protected void Die(){
        PlayAnimation(dieAnimationName,false);
    }

    protected void BeingHit(){
        playerHealth.TakeDamage(5);
        if (playerHealth.IsDead){
            Die();
            Destroy(gameObject, 2f);
        }else PlayAnimation(hurtAnimationName,false);
    }
    void Update(){

    }
}
