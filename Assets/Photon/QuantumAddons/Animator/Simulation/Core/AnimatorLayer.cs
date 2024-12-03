namespace Quantum.Addons.Animator
{
  using Photon.Deterministic;
  using System;
  using System.Collections.Generic;

  [Serializable]
  public unsafe class AnimatorLayer
  {
    public int Id;
    public string Name;
    public AnimatorState[] States;

    public void Update(Frame f, AnimatorGraph graph, AnimatorComponent* animator, FP deltaTime)
    {
      for (int i = 0; i < States.Length; i++)
      {
        var state = States[i];
        if (IsStateActive(animator, state))
        {
          state.Update(f, animator, graph, this, deltaTime);
        }
        else if (States[i].IsDefault && animator->CurrentStateId == 0)
        {
          animator->CurrentStateId = States[i].Id;
          state.Update(f, animator, graph, this, deltaTime);
        }
      }

      if (animator->ToStateId != 0) //transition occuring
      {
        animator->TransitionTime += deltaTime; //advance transition time
        if (animator->TransitionTime >= animator->TransitionDuration) //on completion 
        {
          animator->CurrentStateId = animator->ToStateId;
          animator->Time = animator->ToStateTime;
          animator->LastTime = animator->ToStateLastTime;
          animator->NormalizedTime = FPMath.Clamp(animator->ToStateTime / animator->ToLength, FP._0, FP._1);
          //reset transition state
          animator->FromStateId = 0;
          animator->FromStateTime = FP._0;
          animator->FromStateLastTime = FP._0;
          animator->FromStateNormalizedTime = FP._0;
          animator->FromLength = FP._0;

          animator->ToStateId = 0;
          animator->ToStateTime = FP._0;
          animator->ToStateLastTime = FP._0;
          animator->ToStateNormalizedTime = FP._0;
          animator->ToLength = FP._0;

          animator->TransitionIndex = 0;
          animator->TransitionTime = FP._0;
          animator->TransitionDuration = FP._0;
        }
      }
    }

    public void GenerateBlendList(Frame f, AnimatorGraph graph, AnimatorComponent* animator,
      List<AnimatorRuntimeBlendData> list)
    {
      for (int i = 0; i < States.Length; i++)
      {
        var state = States[i];

        if (IsStateActive(animator, state))
        {
          state.GenerateBlendList(f, animator, graph, this, list);
        }
        else if (state.IsDefault && animator->CurrentStateId == 0)
        {
          animator->CurrentStateId = state.Id;
          state.GenerateBlendList(f, animator, graph, this, list);
        }
      }
    }

    public bool IsStateActive(AnimatorComponent* animator, AnimatorState state)
    {
      var isCurrentState = animator->CurrentStateId == state.Id;
      var isTransitionState = animator->ToStateId == state.Id || animator->FromStateId == state.Id;
      var isTransitioning = animator->ToStateId != 0;

      if (isCurrentState && !isTransitioning || isTransitionState && isTransitioning || state.IsAny)
      {
        return true;
      }

      return false;
    }
  }
}