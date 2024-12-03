namespace Quantum.Addons.Animator
{
  using System;
  using UnityEngine;

  /// <summary>
  /// Interface implemented for AnimatorEventAssets.
  /// </summary>
  public interface IAnimatorEventAsset
  {
    /// <summary>
    /// Generates the AnimatorEvent data using Unity's AnimationEvent.
    /// </summary>
    /// <param name="unityAnimationClip">Unity's AnimatorClip that contains the animation event.</param>
    /// <param name="unityAnimationEvent">The event that will be transformed on AnimatorEvent.</param>
    public AnimatorEvent OnBake(AnimationClip unityAnimationClip, AnimationEvent unityAnimationEvent);
  }

  /// <summary>
  /// Asset that stores information about the AnimatorEvent, Bake process, and Execution.
  /// </summary>
  [Serializable]
  public abstract unsafe class AnimatorEventAsset : AssetObject, IAnimatorEventAsset
  {
    /// <inheritdoc cref="IAnimatorEventAsset.OnBake"/>
    public AnimatorEvent OnBake(AnimationClip unityAnimationClip, AnimationEvent unityAnimationEvent)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Called every time the event Evaluate is valid.
    /// </summary>
    /// <param name="f">The Quantum Frame.</param>
    /// <param name="animator">The AnimatorComponent being executed.</param>
    public abstract void Execute(Frame f, AnimatorComponent* animator);
  }
}