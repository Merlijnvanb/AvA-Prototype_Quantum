using UnityEngine.Serialization;

namespace Quantum
{
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;
  using UnityEngine;
  using Addons.Animator;

  public unsafe partial class AnimatorGraph : AssetObject
  {
    public enum Resolutions
    {
      _8 = 8,
      _16 = 16,
      _32 = 32,
      _64 = 64
    }

    [HideInInspector] public bool IsValid = false;

    public Resolutions WeightTableResolution = Resolutions._32;

    public RuntimeAnimatorController Controller;

    public List<AnimationClip> Clips = new List<AnimationClip>();

    public AnimatorLayer[] Layers;
    public AnimatorVariable[] Variables;
    public bool AllowFadeToTransitions = true;

    public bool RootMotion = false;
    public bool RootMotionAppliesPhysics = false;

    public bool DebugMode = true;

    public void Initialise(Frame f, AnimatorComponent* animator)
    {
      animator->AnimatorGraph = this.Guid;
      animator->Speed = FP._1;
      animator->CurrentStateId = 0;
      animator->ToStateId = 0;
      animator->TransitionTime = FP._0;
      animator->TransitionDuration = FP._0;

      var blendTreeWeights = f.AllocateDictionary<int, BlendTreeWeights>();
      for (int layerIndex = 0; layerIndex < Layers.Length; layerIndex++)
      {
        var layer = Layers[layerIndex];
        for (int stateIndex = 0; stateIndex < layer.States.Length; stateIndex++)
        {
          var state = layer.States[stateIndex];
          var weightsList = f.AllocateList<FP>();
          if (state.Motion is AnimatorBlendTree tree)
          {
            for (int motionIndex = 0; motionIndex < tree.MotionCount; motionIndex++)
            {
              weightsList.Add(0);
            }
          }

          blendTreeWeights.Add(state.Id, new BlendTreeWeights { Values = weightsList });
        }
      }

      animator->BlendTreeWeights = blendTreeWeights;

      if (animator->AnimatorVariables.Ptr != default)
      {
        f.FreeList(animator->AnimatorVariables);
      }

      if (Variables.Length > 0)
      {
        var variablesList = f.AllocateList<AnimatorRuntimeVariable>(Variables.Length);

        // set variable defaults
        for (Int32 variableIndex = 0; variableIndex < Variables.Length; variableIndex++)
        {
          AnimatorRuntimeVariable newParameter = new AnimatorRuntimeVariable();
          switch (Variables[variableIndex].Type)
          {
            case AnimatorVariable.VariableType.FP:
              *newParameter.FPValue = Variables[variableIndex].DefaultFp;
              break;

            case AnimatorVariable.VariableType.Int:
              *newParameter.IntegerValue = Variables[variableIndex].DefaultInt;
              break;

            case AnimatorVariable.VariableType.Bool:
              *newParameter.BooleanValue = Variables[variableIndex].DefaultBool;
              break;

            case AnimatorVariable.VariableType.Trigger:
              *newParameter.BooleanValue = Variables[variableIndex].DefaultBool;
              break;
          }

          variablesList.Add(newParameter);
        }

        animator->AnimatorVariables = variablesList;
      }
    }

    /// <summary>
    /// Updates the state machine graph
    /// </summary>
    public void UpdateGraphState(Frame f, AnimatorComponent* animator, FP deltaTime)
    {
      for (Int32 i = 0; i < Layers.Length; i++)
      {
        Layers[i].Update(f, this, animator, deltaTime);
      }
    }

    /// <summary>
    /// Cross fade from the current state to a specific state in the state machine
    /// This will override the current transitions until this cross fade is complete
    /// Once the crossfade completes, the state machine will continue normal operation from the destination state of the crossfade
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateNameHash"></param>
    /// <param name="transitionDuration"></param>
    public void CrossFade(AnimatorComponent* animator, int stateNameHash, FP transitionDuration)
    {
      if (animator->ToStateId == 0)
      {
        animator->LastTime = FP._0;
        animator->Time = FP._0;
        animator->NormalizedTime = FP._0;

        animator->ToStateId = stateNameHash;
        animator->TransitionTime = FP._0;
        animator->TransitionDuration = transitionDuration;
      }
    }

    /// <summary>
    /// Generate a list of weighted animations to use in posing an animation
    /// </summary>
    /// <param name="f"></param>
    /// <param name="animator"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public void GenerateBlendList(Frame f, AnimatorComponent* animator, List<AnimatorRuntimeBlendData> output)
    {
      for (Int32 i = 0; i < Layers.Length; i++)
      {
        Layers[i].GenerateBlendList(f, this, animator, output);
      }

      // normalise
      var blendCount = output.Count;
      var totalWeight = FP._0;

      for (int b = 0; b < blendCount; b++)
      {
        totalWeight += output[b].Weight;
      }

      if (totalWeight == FP._0) totalWeight = FP._1;

      for (int b = 0; b < blendCount; b++)
      {
        AnimatorRuntimeBlendData blend = output[b];
        blend.Weight /= totalWeight; //normalise
        output[b] = blend;
      }
    }

    public AnimatorFrame CalculateRootMotion(Frame f, AnimatorComponent* animator,
      List<AnimatorRuntimeBlendData> blendList, List<AnimatorMotion> motionList)
    {
      GenerateBlendList(f, animator, blendList);

      int blendSize = blendList.Count;
      AnimatorFrame output = new AnimatorFrame();

      for (Int32 i = 0; i < blendSize; i++)
      {
        var blendData = blendList[i];
        if (blendData.StateId == 0)
        {
          continue;
        }

        var state = GetState(blendData.StateId);
        if (state == null)
        {
          continue;
        }

        var motion = state.GetMotion(blendData.AnimationIndex, motionList);
        if (motion != null)
        {
          if (motion is AnimatorClip clip)
          {
            output += clip.Data.CalculateDelta(blendData.LastTime, blendData.CurrentTime) * blendData.Weight;
          }
        }
      }

      return output;
    }

    public AnimatorState GetState(int stateId)
    {
      for (Int32 l = 0; l < Layers.Length; l++)
      {
        for (Int32 s = 0; s < Layers[l].States.Length; s++)
        {
          if (Layers[l].States[s].Id == stateId)
          {
            return Layers[l].States[s];
          }
        }
      }

      return null;
    }

    public void FadeTo(Frame frame, AnimatorComponent* animatorComponent, string StateName, FP deltaTime)
    {
      if (AllowFadeToTransitions == false)
      {
        if (DebugMode)
        {
          Debug.LogWarning(
            $"[Quantum Animator] It is not possible to transition to state {StateName}. Enable AllowFadeToTransitions on {name}.");
        }

        return;
      }

      var state = GetStateByName(StateName);
      state.FadeTo(frame, animatorComponent, state, deltaTime);
    }

    private AnimatorState GetStateByName(string stateName)
    {
      for (Int32 l = 0; l < Layers.Length; l++)
      {
        for (Int32 s = 0; s < Layers[l].States.Length; s++)
        {
          if (Layers[l].States[s].Name == stateName)
          {
            return Layers[l].States[s];
          }
        }
      }

      return null;
    }

    public Int32 VariableIndex(string name)
    {
      for (Int32 v = 0; v < Variables.Length; v++)
      {
        if (Variables[v].Name == name)
        {
          return v;
        }
      }

      return -1;
    }
  }
}