namespace Quantum.Addons.Animator
{
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;

  public unsafe interface IAnimatorMotion
  {
    string Name { get; set; }
    bool IsEmpty { get; }
    bool IsTree { get; }
    bool Looped { get; }
    int TreeIndex { get; set; }

    void ProcessEvents(Frame f, AnimatorComponent* animator, FP currentTime)
    {
    }

    void GenerateBlendList(Frame f, AnimatorComponent* animator, AnimatorLayer layer, AnimatorState state, FP weight,
      List<AnimatorRuntimeBlendData> list);

    FP CalculateLength(Frame f, AnimatorComponent* animator, FP weight, AnimatorState state);
    void CalculateWeights(Frame f, AnimatorComponent* animator, int stateId);
    bool CalculateSpeed(Frame f, AnimatorComponent* animator, out FP speed);
  }

  public abstract unsafe class AnimatorMotion : IAnimatorMotion
  {
    public virtual string Name { get; set; }

    public virtual bool IsEmpty => false;

    public virtual bool IsTree => false;

    public virtual bool Looped => false;

    public virtual int TreeIndex
    {
      get => -1;
      set { }
    }

    public virtual void ProcessEvents(Frame f, AnimatorComponent* animator, int stateId, FP currentTime)
    {
    }

    public virtual void GenerateBlendList(Frame f, AnimatorComponent* animator, AnimatorLayer layer,
      AnimatorState state, FP weight, List<AnimatorRuntimeBlendData> list)
    {
    }

    public virtual FP CalculateLength(Frame f, AnimatorComponent* animator, FP weight, AnimatorState state)
    {
      return FP._0;
    }

    public virtual void CalculateWeights(Frame f, AnimatorComponent* animator, int stateId)
    {
    }

    public virtual bool CalculateSpeed(Frame f, AnimatorComponent* animator, out FP speed)
    {
      speed = FP._1;
      return false;
    }
  }

  public unsafe class AnimatorClip : AnimatorMotion
  {
    public AnimatorData Data;

    public override bool IsEmpty => Data.FrameCount == 0;

    public override bool IsTree => false;

    public override bool Looped => Data.Looped;

    public string ClipName => Data != null ? Data.ClipName : "";

    public override int TreeIndex { get; set; }

    public override void ProcessEvents(Frame f, AnimatorComponent* animator, int stateId, FP currentTime)
    {
      if (Data.Events == null)
      {
        return;
      }

      foreach (var animatorEvent in Data.Events)
      {
        animatorEvent.Evaluate(f, animator, currentTime);
      }
    }

    public override void GenerateBlendList(Frame f, AnimatorComponent* animator, AnimatorLayer layer,
      AnimatorState state, FP weight, List<AnimatorRuntimeBlendData> list)
    {
      FP time = FP._0;
      FP lastTime = FP._0;
      FP normalizedTime = FP._0;
      FP calculatedLength = FP._0;
      FP clipWeight = weight;
      FP length = Data.Length;

      if (state.Id == animator->CurrentStateId)
      {
        time = animator->Time;
        lastTime = animator->LastTime;
        normalizedTime = animator->NormalizedTime;
        calculatedLength = animator->Length;
      }

      if (state.Id == animator->FromStateId)
      {
        time = animator->FromStateId;
        lastTime = animator->FromStateLastTime;
        normalizedTime = animator->FromStateNormalizedTime;
        calculatedLength = animator->FromLength;
      }

      if (state.Id == animator->ToStateId)
      {
        time = animator->ToStateTime;
        lastTime = animator->ToStateLastTime;
        normalizedTime = animator->ToStateNormalizedTime;
        calculatedLength = animator->ToLength;
      }

      if (animator->ToStateId != 0)
      {
        // transition is happening
        FP transitionWeight = FPMath.Clamp(animator->TransitionTime / animator->TransitionDuration, FP._0, FP._1);
        if (state.Id == animator->FromStateId) transitionWeight = FP._1 - transitionWeight;
        clipWeight = weight * transitionWeight;
      }

      AnimatorRuntimeBlendData blendData = new AnimatorRuntimeBlendData(layer, state, TreeIndex, lastTime, time,
        normalizedTime, clipWeight, length, calculatedLength);
      list.Add(blendData);
    }

    public override FP CalculateLength(Frame f, AnimatorComponent* animator, FP weight, AnimatorState state)
    {
      if (Data == null)
        return 0;
      else
        return Data.Length * weight;
    }

    public override void CalculateWeights(Frame f, AnimatorComponent* animator, int stateId)
    {
      //not implemented
    }

    public override bool CalculateSpeed(Frame f, AnimatorComponent* animator, out FP speed)
    {
      speed = FP._1;
      return false;
    }
  }

  public unsafe class AnimatorBlendTree : AnimatorMotion
  {
    public AnimatorMotion[] Motions;
    public FPVector2[] Positions;
    public FP[] TimesScale;
    public int Resolution;
    public FP[,][] WeightTable;
    public FP[] TimeScaleTable;
    public int MotionCount;

    public int BlendParameterIndex = -1;
    public int BlendParameterIndexY = -1;

    public override bool IsEmpty => MotionCount == 0;

    public override bool IsTree => true;

    public override bool Looped => true;

    public override int TreeIndex { get; set; }

    public override void ProcessEvents(Frame f, AnimatorComponent* animator, int stateId, FP currentTime)
    {
      var weights = AnimatorComponent.GetStateWeights(f, animator, stateId);
      for (int i = 0; i < Motions.Length; i++)
      {
        var clipWeight = weights[i];
        if (clipWeight > 0)
        {
          Motions[i].ProcessEvents(f, animator, stateId, currentTime);
        }
      }
    }

    public override void GenerateBlendList(Frame f, AnimatorComponent* animator, AnimatorLayer layer,
      AnimatorState state, FP weight, List<AnimatorRuntimeBlendData> list)
    {
      FP zf = FP._0_05;
      var weights = AnimatorComponent.GetStateWeights(f, animator, state.Id);
      for (int i = 0; i < MotionCount; i++)
        if (weights[i] > zf)
          Motions[i].GenerateBlendList(f, animator, layer, state, weights[i], list);
    }

    public override FP CalculateLength(Frame f, AnimatorComponent* animator, FP weight, AnimatorState state)
    {
      FP output = FP._0;
      FP zf = FP._0_05;
      var weights = AnimatorComponent.GetStateWeights(f, animator, state.Id);
      for (int i = 0; i < MotionCount; i++)
        if (Motions[i] != null && weights[i] > zf)
          output += Motions[i].CalculateLength(f, animator, weights[i], state);
      return output;
    }

    public bool Is2D()
    {
      return BlendParameterIndex != BlendParameterIndexY;
    }

    public override void CalculateWeights(Frame f, AnimatorComponent* animator, int stateId)
    {
      var weights = AnimatorComponent.GetStateWeights(f, animator, stateId);

      //ClearWeights();

      if (MotionCount == 0)
      {
        Log.Warn("No motions to blend");
        return;
      }

      if (MotionCount == 1)
      {
        *weights.GetPointer(0) = FP._1;
        return;
      }

      FP totalWeight = FP._0;
      if (Is2D())
      {
        // 2d blend calculation
        //ASSUMPTION - blend variables are between -1 and 1
        FP blendParameterX = FPMath.Clamp(*AnimatorComponent.Variable(f, animator, BlendParameterIndex)->FPValue,
          -FP._1, FP._1);
        FP blendParameterY = FPMath.Clamp(*AnimatorComponent.Variable(f, animator, BlendParameterIndexY)->FPValue,
          -FP._1, FP._1);

        blendParameterX = blendParameterX / FP._2 + FP._0_50;
        blendParameterY = blendParameterY / FP._2 + FP._0_50;

        FP res = Resolution - 2;
        FP blendIndexXLerp = blendParameterX * res;
        FP blendIndexYLerp = blendParameterY * res;

        //TODO - replace with FP.FloorToInt/CeilToInt when available
        int blendIndexXa = FPMath.Floor(blendIndexXLerp).AsInt;
        int blendIndexXb = blendIndexXa + 1;
        int blendIndexYa = FPMath.Floor(blendIndexYLerp).AsInt;
        int blendIndexYb = blendIndexYa + 1;

        blendIndexXLerp = blendIndexXLerp - blendIndexXa;
        blendIndexYLerp = blendIndexYLerp - blendIndexYa;

        for (int z = 0; z < MotionCount; z++)
        {
          FP weightA = WeightTable[blendIndexXa, blendIndexYa][z];
          FP weightB = WeightTable[blendIndexXb, blendIndexYa][z];
          FP weightC = WeightTable[blendIndexXa, blendIndexYb][z];
          FP weightD = WeightTable[blendIndexXb, blendIndexYb][z];

          FP lerpA = FPMath.Lerp(weightA, weightB, blendIndexXLerp);
          FP lerpB = FPMath.Lerp(weightA, weightC, blendIndexYLerp);
          FP lerpC = FPMath.Lerp(weightB, weightD, blendIndexYLerp);
          FP lerpD = FPMath.Lerp(weightC, weightD, blendIndexXLerp);
          *weights.GetPointer(z) = (lerpA + lerpB + lerpC + lerpD) / 4;
          totalWeight += weights[z];
        }
      }
      else
      {
        var firstPoint = this.Positions[0].X;
        var lastPoint = this.Positions[^1].X;
        var blendVariable = FPMath.Clamp(*AnimatorComponent.Variable(f, animator, BlendParameterIndex)->FPValue,
          firstPoint, lastPoint);
        var blendAmplitude = lastPoint - firstPoint;
        //blendParameter is the Percentage of the blend value
        var blendParameter = 1 - ((lastPoint - blendVariable) / blendAmplitude);
        FP lerp = blendParameter * (Resolution - 2);
        int indexA = FPMath.CeilToInt(lerp);
        int indexB = indexA + 1;
        lerp = lerp - indexA;

        for (int z = 0; z < MotionCount; z++)
        {
          FP weightA = WeightTable[indexA, 0][z];
          FP weightB = WeightTable[indexB, 0][z];
          *weights.GetPointer(z) = FPMath.Lerp(weightA, weightB, lerp);
          totalWeight += weights[z];
        }
      }
    }

    public override bool CalculateSpeed(Frame f, AnimatorComponent* animator, out FP speed)
    {
      speed = 0;
      if (MotionCount == 0)
      {
        Log.Warn("No motions to blend");
        return false;
      }

      if (MotionCount == 1)
      {
        return false;
      }

      if (Is2D())
      {
        return false;
      }
      else
      {
        var firstPoint = this.TimeScaleTable[0];
        var lastPoint = this.TimeScaleTable[^1];
        var blendVariable = FPMath.Clamp(*AnimatorComponent.Variable(f, animator, BlendParameterIndex)->FPValue,
          firstPoint, lastPoint);
        var blendAmplitude = lastPoint - firstPoint;
        //blendParameter is the Percentage of the blend value
        var blendParameter = 1 - ((lastPoint - blendVariable) / blendAmplitude);
        FP lerp = blendParameter * (Resolution - 2);
        int indexA = FPMath.CeilToInt(lerp);
        int indexB = indexA + 1;
        lerp = lerp - indexA;

        for (int i = 0; i < MotionCount; i++)
        {
          FP weightA = WeightTable[indexA, 0][i];
          FP weightB = WeightTable[indexB, 0][i];
          var motionWeight = FPMath.Lerp(weightA, weightB, lerp);

          FP timeScaleA = TimeScaleTable[indexA];
          FP timeScaleB = TimeScaleTable[indexB];
          var motionTimeScale = FPMath.Lerp(timeScaleA, timeScaleB, lerp);
          speed += motionTimeScale * motionWeight;
        }
      }

      return true;
    }

    public void CalculateTimeScaleTable(int res)
    {
      TimeScaleTable = new FP[res];

      if (!Is2D())
      {
        //1D

        for (int x = 0; x < res; x++)
        {
          TimeScaleTable[x] = CalculateTimeScale_1D(x);
        }
      }
      else
      {
        //2d
      }
    }

    public void CalculateWeightTable(int res)
    {
      Resolution = res;
      int xLength = res;
      int yLength = Is2D() ? res : 1;
      int zLength = Positions.Length;

      WeightTable = new FP[xLength, yLength][];

      if (!Is2D())
      {
        //1D

        var firstPointPosition = Positions[0].X;
        var blendSize = Positions[Positions.Length - 1].X - Positions[0].X;
        for (int resolutionIndex = 0; resolutionIndex < xLength; resolutionIndex++)
        {
          // xTarget is a set of values that goes from the smaller point to the grater point value on the blend tree
          FP xTarget = firstPointPosition + ((FP)resolutionIndex / ((FP)xLength - FP._1)) * blendSize;
          FP[] weightValues = CalculateWeight_1D(xTarget, Positions);
          WeightTable[resolutionIndex, 0] = new FP[zLength];
          for (int z = 0; z < zLength; z++)
          {
            WeightTable[resolutionIndex, 0][z] = weightValues[z];
          }
        }
      }
      else
      {
        //2d
        FP xDiv = xLength - 1;
        FP yDiv = yLength - 1;
        for (int x = 0; x < xLength; x++)
        {
          for (int y = 0; y < yLength; y++)
          {
            FP blendParameterX = ((FP)x / xDiv) * FP._2 - FP._1;
            FP blendParameterY = ((FP)y / yDiv) * FP._2 - FP._1;
            FPVector2 fix = new FPVector2(blendParameterX, blendParameterY);
            FP[] weightValues = Calculate_2d(fix, Positions);
            WeightTable[x, y] = new FP[zLength];
            for (int z = 0; z < zLength; z++)
            {
              WeightTable[x, y][z] = weightValues[z];
            }
          }
        }
      }
    }

    private FP CalculateTimeScale_1D(int pointIndex)
    {
      FP output = 0;
      for (int i = 0; i < WeightTable[pointIndex, 0].Length; i++)
      {
        output += TimesScale[i] * WeightTable[pointIndex, 0][i];
      }

      return output;
    }

    private FP[] CalculateWeight_1D(FP targetPoint, FPVector2[] points)
    {
      FP[] output = new FP[points.Length];
      for (int pointsIndex = 0; pointsIndex < points.Length; pointsIndex++)
      {
        FP point = points[pointsIndex].X;
        bool isTargetPointPreviousToPoint = targetPoint < point;
        FP pointAmplitude = CalculatePointAmplitude(pointsIndex, points, isTargetPointPreviousToPoint);
        FP targetPointDistance = FPMath.Abs(targetPoint - point);

        output[pointsIndex] = 1 - (targetPointDistance / (pointAmplitude));

        if (targetPointDistance > pointAmplitude)
        {
          output[pointsIndex] = 0;
        }
      }

      return output;
    }

    //Calculate amplitude for points. Points can have a different amplitude for each side, this is why the method needs to know if the target point is on the left or right 
    private FP CalculatePointAmplitude(int index, FPVector2[] points, bool isTargetPointPreviousToPoint)
    {
      int indexA;
      int indexB;
      if (index == 0)
      {
        indexA = 1;
        indexB = index;
      }
      else
      {
        if (index != points.Length - 1)
        {
          //Check the left or right amplitude
          if (isTargetPointPreviousToPoint)
          {
            indexA = index;
            indexB = index - 1;
          }
          else
          {
            indexA = index + 1;
            indexB = index;
          }
        }
        else
        {
          //The last point comes always after
          indexA = index;
          indexB = index - 1;
        }
      }

      return FPMath.Abs(points[indexA].X - points[indexB].X);
    }

    private FP[] Calculate_2d(FPVector2 targetPoint, FPVector2[] points)
    {
      int pointCount = points.Length;
      FP[] output = new FP[pointCount];
      FP totalWeight = FP._0;

      for (int i = 0; i < pointCount; ++i)
      {
        FPVector2 firstPoint = points[i];
        FPVector2 comparePoint = targetPoint - firstPoint;

        FP weight = FP._1;

        for (int j = 0; j < pointCount; ++j)
        {
          if (j == i)
            continue;

          FPVector2 secondPoint = points[j] - firstPoint;
          FP sqrLen = FPVector2.Dot(secondPoint, secondPoint);
          if (sqrLen > FP.Epsilon)
          {
            weight = FPMath.Min(weight,
              FPMath.Clamp((FP._1 - FPVector2.Dot(comparePoint, secondPoint) / sqrLen), FP._0, FP._1));
          }
        }

        output[i] = weight;
        totalWeight += weight;
      }

      if (totalWeight > FP.Epsilon)
      {
        for (int i = 0; i < pointCount; ++i)
        {
          output[i] /= totalWeight;
        }
      }

      return output;
    }
  }

  //class that we will use for serialization
  [Serializable]
  public struct SerializableMotion
  {
    public bool IsAnimation;
    public string Name;
    public AnimatorData AnimatorData;

    public int BlendParameterIndex;
    public int BlendParameterIndexY;
    public FPVector2[] Positions;
    public FP[] TimesScale;

    public int Resolution;
    public SerializableWeightDimensionX WeightTable;
    public SerializableTimesScaleDimensionX TimeScaleTable;

    public int ChildCount;
    public int IndexOfFirstChild;
  }

  [Serializable]
  public struct SerializableWeightDimensionX
  {
    public SerializableWeightDimensionY[] Data;
  }

  [Serializable]
  public struct SerializableWeightDimensionY
  {
    public SerializableWeightDimensionZ[] Data;
  }

  [Serializable]
  public struct SerializableWeightDimensionZ
  {
    public FP[] Data;
  }

  [Serializable]
  public struct SerializableTimesScaleDimensionX
  {
    public FP[] Data;
  }
}