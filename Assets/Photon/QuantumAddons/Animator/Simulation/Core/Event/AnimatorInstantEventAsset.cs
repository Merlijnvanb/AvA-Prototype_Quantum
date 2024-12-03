namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  /// <inheritdoc cref="AnimatorEventAsset"/>
  [Serializable]
  public abstract class AnimatorInstantEventAsset : AnimatorEventAsset, IAnimatorEventAsset
  {
    /// <inheritdoc cref="AnimatorEventAsset.OnBake"/>
    public new AnimatorEvent OnBake(AnimationClip unityAnimationClip, AnimationEvent unityAnimationEvent)
    {
      var quantumAnimatorEvent = new AnimatorInstantEvent();
      quantumAnimatorEvent.AssetRef = Guid;
      quantumAnimatorEvent.Time = FP.FromFloat_UNSAFE(unityAnimationEvent.time);
      return quantumAnimatorEvent;
    }
  }
}