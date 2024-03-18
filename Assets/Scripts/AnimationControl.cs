using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace Spine.Unity.Examples {
	public class SpineBeginnerTwo : MonoBehaviour {

		#region Inspector
		// [SpineAnimation] attribute allows an Inspector dropdown of Spine animation names coming form SkeletonAnimation.
		[SpineAnimation]
		public string runAnimationName;

		[SpineAnimation]
		public string idleAnimationName;

		[SpineAnimation]
		public string walkAnimationName;

		[SpineAnimation]
		public string attackAnimationName;

		[Header("Transitions")]
		[SpineAnimation]
		public string idleTurnAnimationName;

		[SpineAnimation]
		public string runToIdleAnimationName;

		public float runWalkDuration = 1.5f;
		#endregion

		SkeletonAnimation skeletonAnimation;

		// Spine.AnimationState and Spine.Skeleton are not Unity-serialized objects. You will not see them as fields in the inspector.
		public Spine.AnimationState spineAnimationState;
		public Spine.Skeleton skeleton;

		void Start () {
			// Make sure you get these AnimationState and Skeleton references in Start or Later.
			// Getting and using them in Awake is not guaranteed by default execution order.
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			spineAnimationState = skeletonAnimation.AnimationState;
			skeleton = skeletonAnimation.Skeleton;

			StartCoroutine(DoDemoRoutine());
		}

		/// This is an infinitely repeating Unity Coroutine. Read the Unity documentation on Coroutines to learn more.
		IEnumerator DoDemoRoutine () {
			while (true) {
				// SetAnimation is the basic way to set an animation.
				// SetAnimation sets the animation and starts playing it from the beginning.
				// Common Mistake: If you keep calling it in Update, it will keep showing the first pose of the animation, do don't do that.

				spineAnimationState.SetAnimation(0, walkAnimationName, true);
				yield return new WaitForSeconds(runWalkDuration);

				spineAnimationState.SetAnimation(0, runAnimationName, true);
				yield return new WaitForSeconds(runWalkDuration);

				// AddAnimation queues up an animation to play after the previous one ends.
				spineAnimationState.SetAnimation(0, runToIdleAnimationName, false);
				spineAnimationState.AddAnimation(0, idleAnimationName, true, 0);
				yield return new WaitForSeconds(1f);

				skeleton.ScaleX = -1;		// skeleton allows you to flip the skeleton.
				spineAnimationState.SetAnimation(0, idleTurnAnimationName, false);
				spineAnimationState.AddAnimation(0, idleAnimationName, true, 0);
				yield return new WaitForSeconds(0.5f);
				skeleton.ScaleX = 1;
				spineAnimationState.SetAnimation(0, idleTurnAnimationName, false);
				spineAnimationState.AddAnimation(0, idleAnimationName, true, 0);
				yield return new WaitForSeconds(0.5f);


			}
		}

	}

}
