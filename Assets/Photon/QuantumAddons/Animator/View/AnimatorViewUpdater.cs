namespace Quantum.Addons.Animator
{
  using System.Collections.Generic;
  using UnityEngine;

  public unsafe class AnimatorViewUpdater : QuantumCallbacks
  {
    private QuantumEntityViewUpdater _entityViewUpdater;

    private Dictionary<EntityRef, AnimatorPlayables>
      _animatorPlayables = new Dictionary<EntityRef, AnimatorPlayables>();

    private List<EntityRef> _removedEntities = new List<EntityRef>();

    private void Awake()
    {
      _entityViewUpdater = FindAnyObjectByType<QuantumEntityViewUpdater>();
    }

    public override void OnUpdateView(QuantumGame game)
    {
      var frame = game.Frames.Predicted;
      
      // Remove destroyed entities
      foreach (var kvp in _animatorPlayables)
      {
        if (frame.Exists(kvp.Key) == false)
        {
          _removedEntities.Add(kvp.Key);
        }
      }

      for (int i = 0; i < _removedEntities.Count; i++)
      {
        _animatorPlayables.Remove(_removedEntities[i]);
      }

      // Animate
      var animators = frame.Filter<AnimatorComponent>();
      while (animators.NextUnsafe(out var entity, out var animator) == true)
      {
        var entityView = _entityViewUpdater.GetView(entity);
        if (entityView == null)
        {
          continue;
        }

        if (_animatorPlayables.TryGetValue(entity, out var ap) == false)
        {
          var animatorPlayables = entityView.GetComponent<AnimatorPlayables>();
          if (animatorPlayables != null)
          {
            _animatorPlayables.Add(entity, animatorPlayables);
          }
          else
          {
            Debug.LogWarning(
              $"[Quantum Animator] Trying to update animations of entity {entity} but it's EntityView does not have a Quantum playables component.");
          }
        }

        if (ap != null)
        {
          ap.Animate(frame, animator);
        }
      }
    }
  }
}