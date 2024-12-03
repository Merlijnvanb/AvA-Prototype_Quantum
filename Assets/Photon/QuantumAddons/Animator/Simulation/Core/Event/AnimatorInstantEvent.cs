namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;

  /// <summary>
  /// AnimatorInstantEvent will call it's Execute method once Evaluate is valid.
  /// </summary>
  [Serializable]
  public unsafe class AnimatorInstantEvent : AnimatorEvent
  {
    /// <inheritdoc cref="AnimatorEvent.Evaluate"/>
    public override void Evaluate(Frame f, AnimatorComponent* animator, FP currentTime)
    {
      if (currentTime >= Time && animator->LastTime < Time)
      {
        AnimatorEventAsset eventAsset = f.FindAsset(AssetRef);
        eventAsset.Execute(f, animator);
      }
    }
  }
}