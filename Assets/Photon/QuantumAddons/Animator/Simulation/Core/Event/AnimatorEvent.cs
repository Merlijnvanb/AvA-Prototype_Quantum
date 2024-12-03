namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;

  /// <summary>
  /// The AnimatorEvent abstract class declaration.
  /// </summary>
  [Serializable]
  public unsafe abstract class AnimatorEvent
  {
    /// <summary>
    /// Reference to the AnimatorEventAsset containing the event bake and Execution routine.
    /// </summary>
    public AssetRef<AnimatorEventAsset> AssetRef;
    /// <summary>
    /// Time of the clip that will Execute the event, this value is baked during AnimatorGraph import process.
    /// </summary>
    public FP Time;

    /// <summary>
    /// Called every time that an AnimationMotion with this Event is updated.
    /// </summary>
    /// <param name="f">The Game Frame.</param>
    /// <param name="animator">The AnimatorComponent being evaluated.</param>
    /// <param name="currentTime">Time of the AnimatorMotion.</param>
    public abstract void Evaluate(Frame f, AnimatorComponent* animator,  FP currentTime);

    /// <summary>
    /// Returns the custom data string for this event type.
    /// </summary>
    public virtual string GetInspectorStringFormat()
    {
      return $"Event: {GetType().Name}; Time: {Time}";
    }
  }
}