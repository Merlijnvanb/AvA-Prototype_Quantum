namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;

  [Serializable]
  public unsafe class AnimatorTransition
  {
    public int Index;
    public string Name;

    /// <summary>
    /// The duration of the transition.
    /// </summary>
    public FP Duration;

    /// <summary>
    /// The time at which the destination state will start.
    /// </summary>
    public FP Offset;

    /// <summary>
    /// If AnimatorStateTransition.has_exit_time is true, exit_time represents the exact time at which the transition can take effect.
    /// This is represented in normalized time, so for example an exit time of 0.75 means that on the first frame where 75% of the animation has played, the Exit Time condition will be true. On the next frame, the condition will be false.
    /// For looped animations, transitions with exit times smaller than 1 will be evaluated every loop, so you can use this to time your transition with the proper timing in the animation, every loop.
    /// Transitions with exit times greater than one will be evaluated only once, so they can be used to exit at a specific time, after a fixed number of loops. For example, a transition with an exit time of 3.5 will be evaluated once, after three and a half loops.</summary>
    public FP ExitTime;

    /// <summary>
    /// When active the transition will have an exit time condition.
    /// </summary>
    public bool HasExitTime;

    /// <summary>
    /// Allow the transition to occur if the current state is the same as the next state
    /// </summary>
    public bool CanTransitionToSelf;

    /// <summary>
    /// AnimatorCondition conditions that need to be met for a transition to happen.
    /// </summary>
    public AnimatorCondition[] Conditions;

    public int DestinationStateId;

    public string DestinationStateName;


    public void Update(Frame f, AnimatorComponent* animator, AnimatorGraph graph, AnimatorState state, FP deltaTime)
    {
      if (Duration == FP._0 && !state.IsAny)
      {
        //Log.Warn(string.Format("Transistion {0} has a duration of 0", name));
      }

      bool noCurrentTransition = animator->ToStateId == 0;
      bool selfConditional = animator->CurrentStateId != DestinationStateId || CanTransitionToSelf;

      if (HasExitTime == false && Conditions.Length == 0)
        return;

      if (noCurrentTransition && selfConditional)
      {
        if (!HasExitTime || animator->Time > ExitTime)
        {
          bool conditionsMet = true;
          for (int c = 0; c < Conditions.Length; c++)
          {
            if (!Conditions[c].Check(f, animator, graph))
            {
              conditionsMet = false;
              break;
            }
          }

          if (conditionsMet)
          {
            //fill in a transition state

            animator->TransitionTime = FP._0;
            animator->TransitionDuration = Duration;
            animator->TransitionIndex = Index;

            animator->FromStateId = animator->CurrentStateId;
            animator->FromStateTime = animator->Time;
            animator->FromStateLastTime = animator->LastTime;
            animator->FromStateNormalizedTime = animator->NormalizedTime;
            animator->FromLength = animator->Length;

            animator->ToStateId = DestinationStateId;
            animator->ToStateTime = Offset;
            animator->ToStateLastTime = FPMath.Max(Offset - deltaTime, FP._0);

            // If AnimatorState.Update run the code for s, the weights are not initialized and we get a divide by zero exception.
            var nextState = graph.GetState(animator->ToStateId);
            if (nextState.Motion != null && nextState.GetLength(f, animator) == 0)
            {
              nextState.Motion.CalculateWeights(f, animator, animator->ToStateId);
            }

            animator->ToLength = nextState.GetLength(f, animator);
            if (animator->ToLength != FP._0)
            {
              animator->ToStateNormalizedTime = FPMath.Clamp(animator->ToStateTime / animator->ToLength, FP._0, FP._1);
            }
            else
            {
              animator->ToStateNormalizedTime =  FP._0;
            }
          }
        }
      }
    }
  }
}