namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;

  /// <summary>
  /// AnimatorTimeWindowEvent will call its Execute method every time the current evaluated time is greater than the Time and less than EndTime.
  /// OnEnter will be called on the first valid evaluation. OnExit will be called on the last valid evaluation.
  /// </summary>
  [Serializable]
  public unsafe class AnimatorTimeWindowEvent : AnimatorEvent
  {
    public FP EndTime;

    /// <inheritdoc cref="AnimatorEvent.Evaluate"/>
    public override void Evaluate(Frame f, AnimatorComponent* animator, FP currentTime)
    {
      if (currentTime >= Time && currentTime <= EndTime)
      {
        AnimatorTimeWindowEventAsset eventAsset = f.FindAsset(AssetRef) as AnimatorTimeWindowEventAsset;
        eventAsset.Execute(f, animator);

        if (animator->LastTime < Time)
        {
          eventAsset.OnEnter(f, animator);
        }
      }
      else if(currentTime >= EndTime && animator->LastTime < EndTime)
      {
          AnimatorTimeWindowEventAsset eventAsset = f.FindAsset(AssetRef) as AnimatorTimeWindowEventAsset;
          eventAsset.OnExit(f, animator);
      }
    }

    /// <inheritdoc cref="AnimatorEvent.GetInspectorStringFormat"/>
    public override string GetInspectorStringFormat()
    {
      return $"Event: {GetType().Name}; Start-Time: {Time}, End-Time: {EndTime}";
    }
  }
}