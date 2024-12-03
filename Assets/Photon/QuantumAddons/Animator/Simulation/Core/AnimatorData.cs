namespace Quantum.Addons.Animator
{
  using Photon.Deterministic;
  using System;
  using UnityEngine;

  /// <summary>
  /// Contains data extracted during the AnimationGraph construction, it is stored in the AnimatorClip 
  /// </summary>
  [Serializable]
  public class AnimatorData
  {
    public string ClipName;
    public FP Length;
    public int Index;
    public int FrameRate;
    public int FrameCount;
    public AnimatorFrame[] Frames;
    [SerializeReference] 
    public AnimatorEvent[] Events;

    public bool Looped;
    public bool Mirror;

    public AnimatorFrame CalculateDelta(FP lastTime, FP currentTime)
    {
      return GetFrameAtTime(lastTime) - GetFrameAtTime(currentTime);
    }

    public AnimatorFrame GetFrameAtTime(FP time)
    {
      AnimatorFrame output = new AnimatorFrame();
      if (Length == FP._0)
        return Frames[0];

      while (time > Length)
      {
        time -= Length;
        output += Frames[FrameCount - 1];
      }

      int timeIndex = FrameCount - 1;
      for (int f = 1; f < FrameCount; f++)
      {
        if (Frames[f].Time > time)
        {
          timeIndex = f;
          break;
        }
      }

      AnimatorFrame frameA = Frames[timeIndex - 1];
      AnimatorFrame frameB = Frames[timeIndex];
      FP currentTime = time - frameA.Time;
      FP frameTime = frameB.Time - frameA.Time;
      FP lerp = currentTime / frameTime;
      return output + AnimatorFrame.Lerp(frameA, frameB, lerp);
    }
  }
}