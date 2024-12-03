namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;

  [Serializable]
  public struct AnimatorFrame
  {
    public int Id;
    public FP Time;
    public FPVector3 Position;
    public FPQuaternion Rotation;
    /// <summary>
    /// Y Rotation in radians
    /// </summary>
    public FP RotationY;

    public static AnimatorFrame operator +(AnimatorFrame animatorFrameA, AnimatorFrame animatorFrameB)
    {
      return new AnimatorFrame()
      {
        Id = animatorFrameA.Id + animatorFrameB.Id,
        Time = animatorFrameA.Time + animatorFrameB.Time,
        Position = animatorFrameA.Position + animatorFrameB.Position,
        Rotation = animatorFrameA.Rotation + animatorFrameB.Rotation,
        RotationY = animatorFrameA.RotationY + animatorFrameB.RotationY
      };
    }

    public static AnimatorFrame operator -(AnimatorFrame animatorFrameA, AnimatorFrame animatorFrameB)
    {
      return new AnimatorFrame()
      {
        Id = animatorFrameB.Id - animatorFrameA.Id,
        Time = animatorFrameB.Time - animatorFrameA.Time,
        Position = animatorFrameB.Position - animatorFrameA.Position,
        Rotation = animatorFrameB.Rotation - animatorFrameA.Rotation,
        RotationY = animatorFrameB.RotationY - animatorFrameA.RotationY
      };
    }

    public static AnimatorFrame operator *(AnimatorFrame animatorFrameA, AnimatorFrame animatorFrameB)
    {
      return new AnimatorFrame()
      {
        Id = animatorFrameA.Id * animatorFrameB.Id,
        Time = animatorFrameA.Time * animatorFrameB.Time,
        Position = FPVector3.Scale(animatorFrameA.Position, animatorFrameB.Position),
        Rotation = animatorFrameA.Rotation * animatorFrameB.Rotation,
        RotationY = animatorFrameA.RotationY * animatorFrameB.RotationY
      };
    }

    public static AnimatorFrame operator *(AnimatorFrame animatorFrameA, FP value)
    {
      return new AnimatorFrame()
      {
        Id = (animatorFrameA.Id * value).AsInt,
        Time = animatorFrameA.Time * value,
        Position = animatorFrameA.Position * value,
        Rotation = animatorFrameA.Rotation * value,
        RotationY = animatorFrameA.RotationY * value
      };
    }

    public static AnimatorFrame Lerp(AnimatorFrame animatorFrameA, AnimatorFrame animatorFrameB, FP value)
    {
      AnimatorFrame output = new AnimatorFrame();

      output.Id = animatorFrameA.Id;
      output.Time = FPMath.Lerp(animatorFrameA.Time, animatorFrameB.Time, value);
      output.Position = FPVector3.Lerp(animatorFrameA.Position, animatorFrameB.Position, value);
      output.RotationY = FPMath.Lerp(animatorFrameA.RotationY, animatorFrameB.RotationY, value);

      try
      {
        output.Rotation = FPQuaternion.Slerp(animatorFrameA.Rotation, animatorFrameB.Rotation, value);
      }
      catch (Exception e)
      {
        Log.Info("quaternion slerp divByZero : " + value + " " + ToString(animatorFrameA.Rotation) + " " +
                 ToString(animatorFrameB.Rotation) +
                 " \n" + e);
        output.Rotation = animatorFrameA.Rotation;
      }

      return output;
    }

    public override string ToString()
    {
      return string.Format("Animator Frame id: " + Id + " time: " + Time.AsFloat + " position " + Position.ToString() +
                           " rotation " + Rotation.AsEuler.ToString() + " rotationY " + RotationY);
    }

    public static string ToString(FPQuaternion q)
    {
      return $"{q.AsEuler.Z.AsFloat}, {q.AsEuler.Y.AsFloat}, {q.AsEuler.Z.AsFloat}";
    }
  }
}