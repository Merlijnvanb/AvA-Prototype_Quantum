namespace Quantum.Addons.Animator
{
  using System.Collections.Generic;
  using Photon.Deterministic;
  using System;

  [Serializable]
  public unsafe class AnimatorState
  {
    public int Id;
    public string Name;
    public bool IsAny;
    public bool IsDefault;
    public FP CycleOffset = FP._0;
    public FP Speed = FP._1;
    [NonSerialized] public AnimatorMotion Motion;

    public AnimatorTransition[] Transitions;

    public List<SerializableMotion> SerialisedMotions;

    // AssetRef for custom baking. Insert any asset you want here from the Unity baking
    public AssetRef StateAsset;


    public void FadeTo(Frame f, AnimatorComponent* animator, AnimatorState toState, FP deltaTime,
      bool resetVariables = false)
    {
      FadeTo(f, animator, toState, deltaTime, FP._0_10, FP._0, resetVariables);
    }

    public void FadeTo(Frame f, AnimatorComponent* animator, AnimatorState toState, FP deltaTime, FP duration,
      bool resetVariables = false)
    {
      FadeTo(f, animator, toState, deltaTime, duration, FP._0, resetVariables);
    }

    public void FadeTo(Frame f, AnimatorComponent* animator, AnimatorState toState, FP deltaTime, FP duration,
      FP offset,
      bool resetVariables = false)
    {
      animator->TransitionTime = FP._0;
      animator->TransitionDuration = duration;
      animator->TransitionIndex = 0;

      animator->FromStateId = animator->CurrentStateId;
      animator->FromStateTime = animator->Time;
      animator->FromStateLastTime = animator->LastTime;
      animator->FromStateNormalizedTime = animator->NormalizedTime;
      animator->FromLength = animator->Length;

      animator->ToStateId = toState.Id;
      animator->ToStateTime = offset;
      animator->ToStateLastTime = FPMath.Max(offset - deltaTime, FP._0);

      // If AnimatorState.Update run the code for s, the weights are not initialized and we get a divide by zero exception.
      //var s = graph.GetState(a->to_state_id);
      if (toState.GetLength(f, animator) == 0)
      {
        toState.Motion.CalculateWeights(f, animator, animator->ToStateId);
      }

      animator->ToLength = toState.GetLength(f, animator);
      animator->ToStateNormalizedTime = FPMath.Clamp(animator->ToStateTime / animator->ToLength, FP._0, FP._1);

      if (resetVariables)
      {
        var graph = f.FindAsset<AnimatorGraph>(animator->AnimatorGraph.Id);
        var variables = graph.Variables;
        var variablesList = f.AllocateList<AnimatorRuntimeVariable>(variables.Length);

        // set variable defaults
        for (Int32 v = 0; v < variables.Length; v++)
        {
          AnimatorRuntimeVariable newParameter = new AnimatorRuntimeVariable();
          switch (variables[v].Type)
          {
            case AnimatorVariable.VariableType.FP:
              *newParameter.FPValue = variables[v].DefaultFp;
              break;

            case AnimatorVariable.VariableType.Int:
              *newParameter.IntegerValue = variables[v].DefaultInt;
              break;

            case AnimatorVariable.VariableType.Bool:
              *newParameter.BooleanValue = variables[v].DefaultBool;
              break;

            case AnimatorVariable.VariableType.Trigger:
              *newParameter.BooleanValue = variables[v].DefaultBool;
              break;
          }
        }
      }
    }

    /// <summary>
    /// Progress the state machine state by a frame
    /// </summary>
    public void Update(Frame f, AnimatorComponent* animator, AnimatorGraph graph, AnimatorLayer layer, FP deltaTime)
    {
      if (!IsAny)
      {
        if ((Motion == null || Motion.IsEmpty) && !IsDefault)
        {
          animator->CurrentStateId = 0;
          animator->FromStateId = 0;
          animator->ToStateId = 0;
          return;
        }

        if (Motion != null && !Motion.IsEmpty)
        {
          Motion.CalculateWeights(f, animator, Id);
          if (Motion.CalculateSpeed(f, animator, out var motionSpeed) == false)
          {
            motionSpeed = Speed;
          }

          FP deltaTimeSpeed = deltaTime * motionSpeed;

          //advance time - current state
          if (Id == animator->CurrentStateId && animator->ToStateId == 0)
          {
            var length = Motion.CalculateLength(f, animator, FP._1, this);
            if (length == FP._0)
            {
              return;
            }

            FP sampleTime = animator->NormalizedTime * length;
            FP currentTime = sampleTime + deltaTimeSpeed;
            FP lastTime = sampleTime;

            

            if (!Motion.Looped && length + deltaTimeSpeed < currentTime)
            {
              currentTime = length; //clamp
              if (length < lastTime) lastTime = currentTime - deltaTimeSpeed; //clamp
            }

            if (Motion.Looped && length + deltaTimeSpeed < currentTime)
            {
              currentTime = currentTime % length;
              lastTime = currentTime - deltaTimeSpeed;
            }

            FP normalizedTime = currentTime / length;

            if (Motion.Looped)
            {
              normalizedTime = normalizedTime % FP._1;
            }
            else
            {
              normalizedTime = FPMath.Clamp(normalizedTime, FP._0, FP._1);
            }

            animator->Time = currentTime;
            animator->LastTime = lastTime;
            animator->NormalizedTime = normalizedTime;
            animator->Length = length;
            
            Motion.ProcessEvents(f, animator, Id, currentTime);
          }

          //advance time - transition state
          if (animator->FromStateId == Id)
          {
            var length = Motion.CalculateLength(f, animator, FP._1, this);
            if (length == FP._0) //lengthless motion - ignore
              return;

            FP sampleTime = animator->FromStateNormalizedTime * length;
            FP lastTime = sampleTime;
            FP currentTime = sampleTime + deltaTimeSpeed;

            if (!Motion.Looped && length + deltaTimeSpeed < currentTime)
            {
              currentTime = length; //clamp
              if (length < lastTime) lastTime = currentTime - deltaTimeSpeed; //clamp
            }

            if (Motion.Looped && length + deltaTimeSpeed < currentTime)
            {
              currentTime = currentTime % length;
              lastTime = currentTime - deltaTimeSpeed;
            }

            FP normalisedTime = currentTime / length;

            if (Motion.Looped)
            {
              normalisedTime = normalisedTime % FP._1;
            }
            else
            {
              normalisedTime = FPMath.Clamp(normalisedTime, FP._0, FP._1);
            }

            animator->FromStateTime = currentTime;
            animator->FromStateLastTime = lastTime;
            animator->FromStateNormalizedTime = normalisedTime;
            animator->FromLength = length;
          }

          if (animator->ToStateId == Id)
          {
            var length = Motion.CalculateLength(f, animator, FP._1, this);
            if (length == FP._0) //lengthless motion - ignore
              return;

            FP sampleTime = animator->ToStateNormalizedTime * length;
            FP lastTime = sampleTime;
            FP currentTime = sampleTime + deltaTimeSpeed;

            if (!Motion.Looped && length + deltaTimeSpeed < currentTime)
            {
              currentTime = length; //clamp
              if (length < lastTime) lastTime = currentTime - deltaTimeSpeed; //clamp
            }

            if (Motion.Looped && length + deltaTimeSpeed < currentTime)
            {
              currentTime = currentTime % length;
              lastTime = currentTime - deltaTimeSpeed;
            }

            FP normalisedTime = currentTime / length;

            if (Motion.Looped)
            {
              normalisedTime = normalisedTime % FP._1;
            }
            else
            {
              normalisedTime = FPMath.Clamp(normalisedTime, FP._0, FP._1);
            }

            animator->ToStateTime = currentTime;
            animator->ToStateLastTime = lastTime;
            animator->ToStateNormalizedTime = normalisedTime;
            animator->ToLength = length;
          }
        }
      }

      if (animator->IgnoreTransitions == false)
      {
        for (Int32 i = 0; i < Transitions.Length; i++)
        {
          Transitions[i].Update(f, animator, graph, this, deltaTime);
        }
      }
    }

    public FP GetLength(Frame f, AnimatorComponent* animator)
    {
      if (Motion != null && !Motion.IsEmpty)
        return Motion.CalculateLength(f, animator, FP._1, this);
      return FP._0;
    }

    /// <summary>
    /// Generate the blend list
    /// This is a list of all the animations used in the current state machine frame
    /// The output will be a list of animations and weights that can be used to pose an animation
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="list">The list to build into</param>
    /// <param name="f"></param>
    /// <param name="animator"></param>
    /// <param name="graph"></param>
    public void GenerateBlendList(Frame f, AnimatorComponent* animator, AnimatorGraph graph, AnimatorLayer layer,
      List<AnimatorRuntimeBlendData> list)
    {
      if (!IsAny)
      {
        if (Motion == null || Motion.IsEmpty && !IsDefault)
        {
          return;
        }

        Motion.CalculateWeights(f, animator, Id);

        var length = Motion.CalculateLength(f, animator, FP._1, this);
        if (length == FP._0)
        {
          return;
        }

        Motion.GenerateBlendList(f, animator, layer, this, FP._1, list);
      }
    }

    /// <summary>
    /// Get a motion from within a blend tree by the index
    /// </summary>
    /// <returns></returns>
    public AnimatorMotion GetMotion(int treeIndex, List<AnimatorMotion> processList)
    {
      if (Motion != null)
      {
        processList.Add(Motion);
      }

      while (processList.Count > 0)
      {
        AnimatorMotion current = processList[0];
        processList.RemoveAt(0);

        if (current.TreeIndex == treeIndex)
        {
          return current;
        }

        if (current.IsTree)
        {
          if (current is AnimatorBlendTree tree)
          {
            processList.AddRange(tree.Motions);
          }
        }
      }

      return null;
    }
  }
}