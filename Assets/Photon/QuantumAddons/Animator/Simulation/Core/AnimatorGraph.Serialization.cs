namespace Quantum
{
  using System;
  using Photon.Deterministic;
  using System.Collections.Generic;
  using Addons.Animator;

  public partial class AnimatorGraph
  {
    public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
    {
      Deserialize(this);
    }

    public static void Serialize(AnimatorGraph animatorGraph)
    {
      int layerCount = animatorGraph.Layers.Length;
      for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
      {
        AnimatorLayer layer = animatorGraph.Layers[layerIndex];
        if (layer.States == null) layer.States = new AnimatorState[0];
        int stateCount = layer.States.Length;
        for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
        {
          AnimatorState state = layer.States[stateIndex];
          if (state.SerialisedMotions == null)
          {
            state.SerialisedMotions = new List<SerializableMotion>();
          }
          state.SerialisedMotions.Clear();
          SerializeObject(state);
        }
      }
    }

    public static void Deserialize(AnimatorGraph animatorGraph)
    {
      int layerCount = animatorGraph.Layers.Length;
      for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
      {
        AnimatorLayer layer = animatorGraph.Layers[layerIndex];
        int stateCount = layer.States.Length;
        for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
        {
          AnimatorState state = layer.States[stateIndex];
          if (state.SerialisedMotions.Count > 0)
          {
            state.Motion = ReadNodeFromSerializedNodes(state, 0);
          }
        }
      }
    }

    private static void SerializeObject(AnimatorState state, AnimatorMotion motion = null)
    {
      SerializableMotion serialisedBo = new SerializableMotion();

      if (motion == null) //initial blend object will be patched in on first call
        motion = state.Motion;

      if (motion is AnimatorClip)
      {
        AnimatorClip anim = motion as AnimatorClip;
        serialisedBo.IsAnimation = true;
        serialisedBo.Name = anim.Data.ClipName;
        serialisedBo.AnimatorData = anim.Data;
        serialisedBo.ChildCount = 0;
        serialisedBo.IndexOfFirstChild = state.SerialisedMotions.Count + 1;

        state.SerialisedMotions.Add(serialisedBo);
      }

      if (motion is AnimatorBlendTree)
      {
        AnimatorBlendTree blend = motion as AnimatorBlendTree;
        serialisedBo.IsAnimation = false;
        serialisedBo.Name = blend.Name; //string.Format("Tree of {0}", blend.motionCount);
        serialisedBo.Positions = blend.Positions;
        serialisedBo.BlendParameterIndex = blend.BlendParameterIndex;
        serialisedBo.BlendParameterIndexY = blend.BlendParameterIndexY;
        serialisedBo.TimesScale = blend.TimesScale;
        serialisedBo.WeightTable = SerializeWeightTable(blend.WeightTable);
        serialisedBo.TimeScaleTable = SerializeTimeScaleTable(blend.TimeScaleTable);
        serialisedBo.Resolution = blend.Resolution;

        serialisedBo.ChildCount = blend.MotionCount;
        serialisedBo.IndexOfFirstChild = state.SerialisedMotions.Count + 1;

        state.SerialisedMotions.Add(serialisedBo);
        foreach (var child in blend.Motions)
        {
          SerializeObject(state, child);
        }
      }
    }

    private static AnimatorMotion ReadNodeFromSerializedNodes(AnimatorState state,
      int index)
    {
      SerializableMotion serialisedBo = state.SerialisedMotions[index];
      List<AnimatorMotion> children = new List<AnimatorMotion>();
      for (int i = 0; i < serialisedBo.ChildCount; i++)
      {
        children.Add(ReadNodeFromSerializedNodes(state, serialisedBo.IndexOfFirstChild + i));
      }

      if (serialisedBo.IsAnimation)
      {
        AnimatorClip anim = new AnimatorClip();
        anim.Name = serialisedBo.Name;
        anim.Data = serialisedBo.AnimatorData;
        anim.TreeIndex = index;
        return anim;
      }
      else
      {
        AnimatorBlendTree blend = new AnimatorBlendTree();
        blend.Name = serialisedBo.Name;
        blend.Positions = serialisedBo.Positions;
        blend.BlendParameterIndex = serialisedBo.BlendParameterIndex;
        blend.BlendParameterIndexY = serialisedBo.BlendParameterIndexY;
        blend.TimesScale = serialisedBo.TimesScale;
        blend.Resolution = serialisedBo.Resolution;
        blend.WeightTable = DeserializeWeightTable(serialisedBo.WeightTable);
        blend.TimeScaleTable = DeserializeTimeScaleTable(serialisedBo.TimeScaleTable);
        blend.Motions = children.ToArray();
        blend.MotionCount = children.Count;
        //blend.weights = new FP[blend.motionCount];
        blend.TreeIndex = index;
        return blend;
      }
    }

    private static FP[,][] DeserializeWeightTable(SerializableWeightDimensionX table)
    {
      int xLength = table.Data.Length;
      if (xLength == 0) return new FP[0, 0][];
      int yLength = table.Data[0].Data.Length;
      FP[,][] output = new FP[xLength, yLength][];
      for (int x = 0; x < xLength; x++)
      {
        for (int y = 0; y < yLength; y++)
        {
          output[x, y] = table.Data[x].Data[y].Data;
        }
      }

      return output;
    }

    private static FP[] DeserializeTimeScaleTable(SerializableTimesScaleDimensionX table)
    {
      int xLength = table.Data.Length;
      if (xLength == 0) return Array.Empty<FP>();
      FP[] output = new FP[xLength];
      for (int x = 0; x < xLength; x++)
      {
        output[x] = table.Data[x];
      }

      return output;
    }

    private static SerializableWeightDimensionX SerializeWeightTable(FP[,][] table)
    {
      int xLength = table.GetLength(0);
      int yLength = table.GetLength(1);
      if (xLength == 0) return new SerializableWeightDimensionX();
      SerializableWeightDimensionX output = new SerializableWeightDimensionX();
      output.Data = new SerializableWeightDimensionY[xLength];
      for (int x = 0; x < xLength; x++)
      {
        output.Data[x].Data = new SerializableWeightDimensionZ[yLength];
        for (int y = 0; y < yLength; y++)
        {
          output.Data[x].Data[y].Data = table[x, y];
        }
      }

      return output;
    }

    private static SerializableTimesScaleDimensionX SerializeTimeScaleTable(FP[] table)
    {
      int xLength = table.Length;
      if (xLength == 0) return new SerializableTimesScaleDimensionX();
      SerializableTimesScaleDimensionX output = new SerializableTimesScaleDimensionX();
      output.Data = new FP[xLength];
      for (int x = 0; x < xLength; x++)
      {
        output.Data[x] = table[x];
      }

      return output;
    }
  }
}