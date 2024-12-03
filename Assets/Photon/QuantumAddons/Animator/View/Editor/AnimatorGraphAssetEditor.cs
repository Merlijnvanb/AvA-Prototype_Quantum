namespace Quantum.Addons.Animator
{
  using Photon.Deterministic;
  using UnityEditor;
  using UnityEngine;
  using Quantum;
  using System;

  [CustomEditor(typeof(AnimatorGraph))]
  public class AnimatorGraphAssetEditor : Editor
  {
    private AnimatorGraph _asset = null;
    private int _selectedLayer = 0;
    private int _selectedState = 0;
    private int _selectedBlend = 0;
    private bool _showMovementData;

    private void OnEnable()
    {
      _asset = (AnimatorGraph)target;
    }

    public override void OnInspectorGUI()
    {
      var graph = _asset;

      if (GUILayout.Button("Import Mecanim Controller"))
      {
        try
        {
          ExportAnimations.CreateAsset(_asset, _asset.Controller);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
          throw;
        }

        EditorUtility.SetDirty(_asset);
        AssetDatabase.Refresh();
      }

      if (graph != null && graph.IsValid && graph.Layers != null)
      {
        AnimatorGraph.Deserialize(graph);

        int layerCount = graph.Layers.Length;
        if (layerCount > 0)
        {
          string[] layerNames = new string[layerCount];
          for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
            layerNames[layerIndex] = graph.Layers[layerIndex].Name;
          _selectedLayer = GUILayout.Toolbar(_selectedLayer, layerNames);
          AnimatorLayer layer = graph.Layers[_selectedLayer];

          if (layer != null)
          {
            int stateCount = layer.States.Length;

            if (stateCount > 0)
            {
              string[] stateNames = new string[stateCount];
              for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
              {
                stateNames[stateIndex] = layer.States[stateIndex].Name;
              }

              _selectedState = GUILayout.SelectionGrid(
                _selectedState,
                stateNames,
                3,
                GUILayout.MinWidth(100)
              );

              AnimatorState state = layer.States[_selectedState];

              if (state != null)
              {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("State");
                EditorGUILayout.LabelField(state.Name);
                EditorGUILayout.LabelField(string.Format("Is Default: {0}", state.IsDefault));
                if (state.Motion != null)
                {
                  EditorGUILayout.LabelField(string.Format("Is Blend Tree: {0}", state.Motion.IsTree));

                  if (state.Motion.IsTree == false)
                  {
                    AnimatorClip motion = state.Motion as AnimatorClip;
                    AnimationClipGui(motion);
                  }
                  else
                  {
                    AnimatorBlendTree motion = state.Motion as AnimatorBlendTree;
                    string[] blends = new string[motion.MotionCount];
                    for (int b = 0; b < motion.MotionCount; b++)
                      blends[b] = motion.Motions[b].Name;
                    _selectedBlend = SelectionField(blends, _selectedBlend, 2);

                    AnimatorClip clip = motion.Motions[_selectedBlend] as AnimatorClip;
                    AnimationClipGui(clip);
                  }
                }
                else
                {
                  EditorGUILayout.LabelField("No Motion Set");
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Transitions");
                int transitionCount = state.Transitions.Length;
                for (int transitionIndex = 0; transitionIndex < transitionCount; transitionIndex++)
                {
                  EditorGUILayout.BeginVertical("Box");

                  AnimatorTransition transition = state.Transitions[transitionIndex];

                  EditorGUILayout.LabelField(string.Format("{0}. {1}", transition.Index, transition.Name));
                  EditorGUILayout.LabelField(string.Format("Duration: {0} sec", transition.Duration.AsFloat));
                  EditorGUILayout.LabelField(string.Format("Has Exit Time: {0}", transition.HasExitTime));
                  EditorGUILayout.LabelField(string.Format("Exit Time: {0} sec", transition.ExitTime.AsFloat));
                  EditorGUILayout.LabelField(string.Format("Destination State (Hash): {0} ({1})",
                    transition.DestinationStateName, transition.DestinationStateId));
                  EditorGUILayout.LabelField(string.Format("Offset: {0} sec", transition.Offset.AsFloat));

                  int conditionCount = transition.Conditions.Length;
                  for (int conditionIndex = 0; conditionIndex < conditionCount; conditionIndex++)
                  {
                    EditorGUILayout.BeginVertical("Box");
                    AnimatorCondition condition = transition.Conditions[conditionIndex];
                    AnimatorVariable variable = graph.Variables[graph.VariableIndex(condition.VariableName)];

                    string conditionMode = "";
                    switch (condition.Mode)
                    {
                      case AnimatorCondition.Modes.Equals:
                        conditionMode = "==";
                        break;
                      case AnimatorCondition.Modes.Greater:
                        conditionMode = ">";
                        break;
                      case AnimatorCondition.Modes.If:
                        conditionMode = "is true";
                        break;
                      case AnimatorCondition.Modes.IfNot:
                        conditionMode = "is false";
                        break;
                      case AnimatorCondition.Modes.Less:
                        conditionMode = "<";
                        break;
                      case AnimatorCondition.Modes.NotEqual:
                        conditionMode = "!=";
                        break;
                    }

                    string threshold = "";
                    switch (variable.Type)
                    {
                      case AnimatorVariable.VariableType.FP:
                        threshold = condition.ThresholdFp.AsFloat.ToString();
                        break;
                      case AnimatorVariable.VariableType.Int:
                        threshold = condition.ThresholdInt.ToString();
                        break;
                    }

                    EditorGUILayout.LabelField(string.Format("\"{0}\" - {1} - {2}", condition.VariableName,
                      conditionMode,
                      threshold));

                    EditorGUILayout.EndVertical();
                  }

                  EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
              }
            }
          }
        }
      }
      else
      {
        GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
        errorStyle.normal.textColor = Color.red;
        errorStyle.fontStyle = FontStyle.Bold;

        GUILayout.BeginVertical("Box");
        GUILayout.Label("This is not a valid AnimatorGraph,  fix and import again!", errorStyle, GUILayout.Height(50));
        GUILayout.EndVertical();
      }


      base.OnInspectorGUI();
    }

    private void AnimationClipGui(AnimatorClip clip)
    {
      EditorGUILayout.BeginVertical("box", GUILayout.Width(EditorGUIUtility.currentViewWidth - 20));
      EditorGUILayout.LabelField("Animator Clip");
      EditorGUILayout.LabelField(string.Format("Name: {0}", clip.ClipName));
      EditorGUILayout.LabelField(string.Format("Length: {0}", clip.Data.Length.AsFloat));
      EditorGUILayout.LabelField(string.Format("Frame Rate: {0}", clip.Data.FrameRate));
      EditorGUILayout.LabelField(string.Format("Frame Count: {0}", clip.Data.FrameCount));
      EditorGUILayout.LabelField(string.Format("Looped: {0}", clip.Data.Looped));
      EditorGUILayout.LabelField(string.Format("Mirrored: {0}", clip.Data.Mirror));
      if (clip.Data.Events != null)
      {
        for (int i = 0; i < clip.Data.Events.Length; i++)
        {
          EditorGUILayout.LabelField(clip.Data.Events[i].GetInspectorStringFormat());
        }
      }

      int frameCount = clip.Data.FrameCount;
      Vector3[] positions = new Vector3[frameCount];
      Quaternion[] rotationsQ = new Quaternion[frameCount];
      Vector3[] rotations = new Vector3[frameCount];
      float[] times = new float[frameCount];
      for (int f = 0; f < frameCount; f++)
      {
        AnimatorFrame frame = clip.Data.Frames[f];
        float frameTime = frame.Time.AsFloat;
        FPVector3 position = frame.Position;
        FPQuaternion rotation = frame.Rotation;

        Vector3 pV3 = new Vector3(position.X.AsFloat, position.Y.AsFloat, position.Z.AsFloat);
        Quaternion rQ = new Quaternion(rotation.X.AsFloat, rotation.Y.AsFloat, rotation.Z.AsFloat, rotation.W.AsFloat);
        Vector3 rV3 = rQ.eulerAngles;

        positions[f] = pV3;
        rotationsQ[f] = rQ;
        rotations[f] = rV3;
        times[f] = frameTime;
      }

      EditorGUILayout.BeginVertical("box");

      EditorGUILayout.LabelField(string.Format("Delta Movement: {0}",
        (positions[frameCount - 1] - positions[0]).ToString("F3")));
      Quaternion deltaQ =
        Quaternion.FromToRotation(rotationsQ[0] * Vector3.forward, rotationsQ[frameCount - 1] * Vector3.forward);
      Vector3 deltaQV = deltaQ.eulerAngles;
      if (deltaQV.x > 180) deltaQV.x += -360;
      if (deltaQV.y > 180) deltaQV.y += -360;
      if (deltaQV.z > 180) deltaQV.z += -360;
      EditorGUILayout.LabelField(string.Format("Delta Rotation: {0}", deltaQV.ToString("F3")));

      EditorGUILayout.EndVertical();


      EditorGUILayout.BeginVertical("box");
      if (_showMovementData = EditorGUILayout.Foldout(_showMovementData, "Movement Data"))
      {
        EditorGUILayout.BeginHorizontal();


        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Times", GUILayout.Width(75));
        for (int f = 0; f < frameCount; f++)
          EditorGUILayout.LabelField(times[f].ToString("F3"), GUILayout.Width(75));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Positions", GUILayout.Width(160));
        for (int f = 0; f < frameCount; f++)
          EditorGUILayout.LabelField(positions[f].ToString("F2"), GUILayout.Width(160));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Rotations", GUILayout.Width(160));
        for (int f = 0; f < frameCount; f++)
          EditorGUILayout.LabelField(rotations[f].ToString("F2"), GUILayout.Width(160));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
      }

      EditorGUILayout.EndVertical();

      EditorGUILayout.EndVertical();
    }

    private int SelectionField(string[] options, int selection, int xCount)
    {
      int libraryCount = options.Length;
      int yCount = Mathf.CeilToInt(libraryCount / (float)xCount);

      float calSize = (EditorGUIUtility.currentViewWidth) / xCount;

      EditorGUILayout.BeginVertical();
      for (int y = 0; y < yCount; y++)
      {
        EditorGUILayout.BeginHorizontal();
        for (int x = 0; x < xCount; x++)
        {
          int index = x + y * xCount;
          if (index < libraryCount)
          {
            if (index != selection)
            {
              if (GUILayout.Button(options[index], GUILayout.Width(calSize)))
                return index;
            }
            else
            {
              EditorGUILayout.BeginHorizontal("box", GUILayout.Width(calSize));
              EditorGUILayout.LabelField(options[index], GUILayout.Width(calSize));
              EditorGUILayout.EndHorizontal();
            }
          }
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(2);
      }

      EditorGUILayout.EndVertical();

      return selection;
    }
  }
}