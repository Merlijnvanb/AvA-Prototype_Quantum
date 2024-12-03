using System.Diagnostics;

namespace Quantum.Addons.Animator
{
  using System;
  using UnityEngine;
  using Photon.Deterministic;

  /// <inheritdoc cref="AnimatorEventAsset"/>
  [Serializable]
  public abstract unsafe class AnimatorTimeWindowEventAsset : AnimatorEventAsset, IAnimatorEventAsset
  {
    /// <inheritdoc cref="IAnimatorEventAsset.OnBake"/>
    public new virtual AnimatorEvent OnBake(AnimationClip unityAnimationClip,
      AnimationEvent unityAnimationEvent)
    {
      var quantumTimeWindowAnimatorEvent = new AnimatorTimeWindowEvent();
      quantumTimeWindowAnimatorEvent.AssetRef = Guid;
      quantumTimeWindowAnimatorEvent.Time = FP.FromFloat_UNSAFE(unityAnimationEvent.time);

      quantumTimeWindowAnimatorEvent.EndTime = -1;
      bool hasPair = false;
      foreach (var unityEvent in unityAnimationClip.events)
      {
        if (unityEvent.objectReferenceParameter.GetType() != GetType())
        {
          continue;
        }

        if (unityEvent.time == unityAnimationEvent.time)
        {
          continue;
        }

        hasPair = true;

        if (unityEvent.time < unityAnimationEvent.time)
        {
          continue;
        }

        quantumTimeWindowAnimatorEvent.EndTime = FP.FromFloat_UNSAFE(unityEvent.time);
        return quantumTimeWindowAnimatorEvent;
      }

      if (hasPair == false)
      {
        Debug.LogWarning(
          $"[QuantumAnimator] QuantumAnimatorTimeWindowEventAsset not setup correctly on clip: {unityAnimationClip.name}. ");
      }
      return null;
    }

    /// <summary>
    /// Called the first time the event Evaluate is valid.
    /// </summary>
    /// <param name="f">The Quantum Frame.</param>
    /// <param name="animator">The AnimatorComponent being executed.</param>
    public abstract void OnEnter(Frame f, AnimatorComponent* animator);

    /// <summary>
    /// Called the last time the event Evaluate is valid.
    /// </summary>
    /// <param name="f">The Quantum Frame.</param>
    /// <param name="animator">The AnimatorComponent being executed.</param>
    public abstract void OnExit(Frame f, AnimatorComponent* animator);
  }
}