namespace Quantum.Addons.Animator
{
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class AnimatorTriggersSystem : SystemMainThread
  {
    public override void Update(Frame f)
    {
      var filter = f.Filter<AnimatorComponent>();
      while (filter.NextUnsafe(out _, out var animator))
      {
        if (animator->AnimatorGraph.Id.Equals(default))
        {
          continue;
        }

        AnimatorGraph animatorGraph = f.FindAsset<AnimatorGraph>(animator->AnimatorGraph.Id);
        foreach (var variable in animatorGraph.Variables)
        {
          if (variable.Type != AnimatorVariable.VariableType.Trigger)
          {
            continue;
          }

          if (AnimatorComponent.IsTriggerActive(f, animator, variable.Index))
          {
            AnimatorComponent.ResetTrigger(f, animator, variable.Index);
          }
        }
      }
    }
  }
}