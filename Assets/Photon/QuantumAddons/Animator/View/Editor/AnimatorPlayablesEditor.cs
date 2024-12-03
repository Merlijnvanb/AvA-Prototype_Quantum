namespace Quantum.Addons.Animator
{
  using UnityEditor;
  using UnityEngine;
  using Quantum;

  [CustomEditor(typeof(AnimatorPlayables))]
  public unsafe class AnimatorPlayablesEditor : Editor
  {
    public bool EnableDebug = true;

    private AnimatorPlayables _animatorPlayables;

    void OnEnable()
    {
      _animatorPlayables = (AnimatorPlayables)target;
    }

    public override void OnInspectorGUI()
    {
      EnableDebug = GUILayout.Toggle(EnableDebug, "Enable Debug");
      if (!EnableDebug)
      {
        return;
      }

      if (_animatorPlayables.Game == null)
      {
        GUILayout.Label("Debug values will be updated on the Quantum session has started.");
        return;
      }

      Frame frame = _animatorPlayables.PredictedFrame;
      var animator = frame.Get<AnimatorComponent>(_animatorPlayables.EntityRef);
      var animatorGraph = frame.FindAsset<AnimatorGraph>(animator.AnimatorGraph.Id);

      EditorGUILayout.LabelField("Graph Variables:");
      EditorGUILayout.BeginVertical("box");
      var variablesList = frame.ResolveList(animator.AnimatorVariables);

      for (int i = 0; i < animatorGraph.Variables.Length; i++)
      {
        EditorGUILayout.BeginVertical();
        switch (animatorGraph.Variables[i].Type)
        {
          case AnimatorVariable.VariableType.Bool:
            EditorGUILayout.LabelField(
              $"{animatorGraph.Variables[i].Name}", variablesList[i].BooleanValue->ToString());
            break;
          case AnimatorVariable.VariableType.Int:
            EditorGUILayout.LabelField(
              $"{animatorGraph.Variables[i].Name}", variablesList[i].IntegerValue->ToString());
            break;
          case AnimatorVariable.VariableType.FP:
            EditorGUILayout.LabelField(
              $"{animatorGraph.Variables[i].Name}", variablesList[i].FPValue->ToString());
            break;
        }

        EditorGUILayout.EndVertical();
      }

      EditorGUILayout.EndVertical();

      var blendTreeWeights = frame.ResolveDictionary(animator.BlendTreeWeights);
      EditorGUILayout.LabelField("States:");
      for (int layerIndex = 0; layerIndex < animatorGraph.Layers.Length; layerIndex++)
      {
        var layer = animatorGraph.Layers[layerIndex];
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Layer: {layer.Name}");
        for (int stateIndex = 0; stateIndex < layer.States.Length; stateIndex++)
        {
          var state = layer.States[stateIndex];
          if (state.IsAny)
          {
            continue;
          }

          EditorGUILayout.LabelField($"{state.Name}");

          //ProgressBar
          var rect = EditorGUILayout.GetControlRect(false, 2);
          Color originalColor = GUI.color;
          float fillAmount = 0;
          if (layer.IsStateActive(&animator, state))
          {
            fillAmount = (animator.Time / animator.Length).AsFloat;
            GUI.color = Color.green;
          }

          EditorGUI.ProgressBar(rect, fillAmount, "");
          GUI.color = originalColor;
          //

          if (state.Motion is AnimatorBlendTree blendState)
          {
            for (int blendStateIndex = 0; blendStateIndex < blendState.Motions.Length; blendStateIndex++)
            {
              var subBlendState = blendState.Motions[blendStateIndex];
              var weightsStruct = blendTreeWeights[state.Id];
              var weightsValues = frame.ResolveList(weightsStruct.Values);
              float length = Mathf.Clamp(weightsValues[blendStateIndex].AsFloat, 0, 1);
              rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
              EditorGUI.ProgressBar(rect, length, $"{subBlendState.Name}: {weightsValues[blendStateIndex].ToString()}");
            }
          }
        }

        EditorGUILayout.EndVertical();
      }

      base.OnInspectorGUI();
      Repaint();
    }
  }
}