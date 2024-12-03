namespace Quantum.Addons.Animator
{
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;
  using UnityEditor;
  using UnityEditor.Animations;
  using UnityEngine;
  using UA = UnityEditor.Animations;
  using Quantum;

  public class ExportAnimations : MonoBehaviour
  {
    public static void CreateAsset(AnimatorGraph dataAsset, RuntimeAnimatorController runtimeController)
    {
      if (runtimeController == null)
      {
        dataAsset.IsValid = false;
        throw new Exception(
          string.Format("[Quantum Animator] AnimatorGraph Controller is not valid, fix before importing animations."));
      }
      var controller = (UnityEditor.Animations.AnimatorController)runtimeController;
      
      if (!dataAsset)
      {
        return;
      }

      int weightTableResolution = (int)dataAsset.WeightTableResolution;
      int variableCount = controller.parameters.Length;

      var clips = new List<AnimationClip>();

      dataAsset.Variables = new AnimatorVariable[variableCount];

      // Mecanim Parameters/Variables
      // Make a dictionary of paramets by name for use when extracting conditions for transitions
      Dictionary<string, AnimatorControllerParameter> parameterDic =
        new Dictionary<string, AnimatorControllerParameter>();
      for (int i = 0; i < variableCount; i++)
      {
        AnimatorControllerParameter parameter = controller.parameters[i];
        parameterDic.Add(parameter.name, parameter);
        AnimatorVariable newVariable = new AnimatorVariable();

        newVariable.Name = parameter.name;
        newVariable.Index = i;
        switch (parameter.type)
        {
          case AnimatorControllerParameterType.Bool:
            newVariable.Type = AnimatorVariable.VariableType.Bool;
            newVariable.DefaultBool = parameter.defaultBool;
            break;
          case AnimatorControllerParameterType.Float:
            newVariable.Type = AnimatorVariable.VariableType.FP;
            newVariable.DefaultFp = FP.FromFloat_UNSAFE(parameter.defaultFloat);
            break;
          case AnimatorControllerParameterType.Int:
            newVariable.Type = AnimatorVariable.VariableType.Int;
            newVariable.DefaultInt = parameter.defaultInt;
            break;
          case AnimatorControllerParameterType.Trigger:
            newVariable.Type = AnimatorVariable.VariableType.Trigger;
            break;
        }

        dataAsset.Variables[i] = newVariable;
      }

      // Mecanim State Graph
      int layerCount = controller.layers.Length;
      dataAsset.Layers = new AnimatorLayer[layerCount];
      for (int l = 0; l < layerCount; l++)
      {
        AnimatorLayer newLayer = new AnimatorLayer();
        newLayer.Name = controller.layers[l].name;
        newLayer.Id = l;

        int stateCount = controller.layers[l].stateMachine.states.Length;
        newLayer.States = new AnimatorState[stateCount + 1]; // additional element for the any state
        Dictionary<UA.AnimatorState, AnimatorState> stateDictionary =
          new Dictionary<UA.AnimatorState, AnimatorState>();

        for (int s = 0; s < stateCount; s++)
        {
          UnityEditor.Animations.AnimatorState state = controller.layers[l].stateMachine.states[s].state;
          AnimatorState newState = new AnimatorState();
          newState.Name = state.name;
          newState.Id = state.nameHash;
          newState.IsDefault = controller.layers[l].stateMachine.defaultState == state;
          newState.Speed = FP.FromFloat_UNSAFE(state.speed);
          newState.CycleOffset = FP.FromFloat_UNSAFE(state.cycleOffset);

          if (state.motion != null)
          {
            AnimationClip clip = state.motion as AnimationClip;
            if (clip != null)
            {
              clips.Add(clip);
              AnimatorClip newClip = new AnimatorClip();
              newClip.Name = state.motion.name;
              newClip.Data = Extract(clip);
              newState.Motion = newClip;
            }
            else
            {
              BlendTree tree = state.motion as BlendTree;
              if (tree != null)
              {
                foreach (var child in tree.children)
                {
                  if (child.motion == null)
                  {
                    dataAsset.IsValid = false;
                    throw new Exception(string.Format(
                      "There is a missing motion on State {0}. This is no allowed, fix before importing animations.",
                      state.name));
                  }
                }

                int childCount = tree.children.Length;

                AnimatorBlendTree newBlendTree = new AnimatorBlendTree();
                newBlendTree.Name = state.motion.name;
                newBlendTree.MotionCount = childCount;
                newBlendTree.Motions = new AnimatorMotion[childCount];
                newBlendTree.Positions = new FPVector2[childCount];
                newBlendTree.TimesScale = new FP[childCount];

                string parameterXname = tree.blendParameter;
                string parameterYname = tree.blendParameterY;

                for (int v = 0; v < variableCount; v++)
                {
                  if (controller.parameters[v].name == parameterXname)
                    newBlendTree.BlendParameterIndex = v;
                  if (controller.parameters[v].name == parameterYname)
                    newBlendTree.BlendParameterIndexY = v;
                }

                if (tree.blendType == BlendTreeType.Simple1D)
                {
                  newBlendTree.BlendParameterIndexY = newBlendTree.BlendParameterIndex;
                }

                if (newBlendTree.BlendParameterIndex == -1)
                {
                  Debug.LogError(
                    $"[Quantum Animator] Blend Tree parameter named {parameterXname} was not found on the Animator Controller during the baking process");
                }

                if (tree.blendType == BlendTreeType.Simple1D && newBlendTree.BlendParameterIndexY == -1)
                {
                  Debug.LogError(
                    $"[Quantum Animator] Blend Tree parameter named {parameterYname} was not found on the Animator Controller during the baking process");
                }

                for (int c = 0; c < childCount; c++)
                {
                  ChildMotion cMotion = tree.children[c];
                  AnimationClip cClip = cMotion.motion as AnimationClip;
                  if (tree.blendType == BlendTreeType.Simple1D)
                  {
                    newBlendTree.Positions[c] = new FPVector2(FP.FromFloat_UNSAFE(cMotion.threshold), 0);
                    newBlendTree.TimesScale[c] = FP.FromFloat_UNSAFE(cMotion.timeScale);
                  }
                  else
                  {
                    newBlendTree.Positions[c] = new FPVector2(FP.FromFloat_UNSAFE(cMotion.position.x),
                      FP.FromFloat_UNSAFE(cMotion.position.y));
                    //TODO timesScale
                  }

                  if (cClip != null)
                  {
                    clips.Add(cClip);
                    AnimatorClip newClip = new AnimatorClip();
                    newClip.Data = Extract(cClip);
                    newClip.Name = newClip.ClipName;
                    newBlendTree.Motions[c] = newClip;
                  }
                }

                newBlendTree.CalculateWeightTable(weightTableResolution);

                //Debug WeightTable
                System.Text.StringBuilder debugString = new System.Text.StringBuilder();
                debugString.Append("weightTable content:\n");

                for (int i = 0; i < newBlendTree.WeightTable.GetLength(0); i++)
                {
                  for (int j = 0; j < newBlendTree.WeightTable.GetLength(1); j++)
                  {
                    FP[] arrayElement = newBlendTree.WeightTable[i, j];

                    debugString.Append($"weightTable[{i},{j}] = [");
                    for (int k = 0; k < arrayElement.Length; k++)
                    {
                      debugString.Append(arrayElement[k].ToString());
                      if (k < arrayElement.Length - 1)
                      {
                        debugString.Append(", ");
                      }
                    }

                    debugString.Append("]\n");
                  }
                }
                //Debug.Log(debugString);

                newBlendTree.CalculateTimeScaleTable(weightTableResolution);

                //Debug SpeedTable
                debugString = new System.Text.StringBuilder();
                debugString.Append("speedTable content:\n");

                for (int i = 0; i < newBlendTree.TimeScaleTable.GetLength(0); i++)
                {
                  debugString.Append($"speedTable[{i}] = [");
                  debugString.Append($"{newBlendTree.TimeScaleTable[i]}");
                  debugString.Append("]\n");
                }
                //Debug.Log(debugString);

                newState.Motion = newBlendTree;
              }
            }
          }

          newLayer.States[s] = newState;

          stateDictionary.Add(state, newState);
        }

        // State Transitions
        // once the states have all been created
        // we'll hook up the transitions
        for (int s = 0; s < stateCount; s++)
        {
          UnityEditor.Animations.AnimatorState state = controller.layers[l].stateMachine.states[s].state;
          AnimatorState newState = newLayer.States[s];
          int transitionCount = state.transitions.Length;
          newState.Transitions = new AnimatorTransition[transitionCount];
          for (int t = 0; t < transitionCount; t++)
          {
            AnimatorStateTransition transition = state.transitions[t];
            var destinationState = transition.isExit
              ? controller.layers[l].stateMachine.defaultState
              : transition.destinationState;
            if (!stateDictionary.ContainsKey(destinationState)) continue;

            //if (state.motion == null)
            //{
            //  throw new Exception(string.Format("Baking '{0}' failed: Animation state '{1}' in controller '{2}' (layer '{3}') requires a motion.", dataAsset.name, state.name, controller.name, controller.layers[l].name));
            //}
            //if (destinationState.motion == null)
            //{
            //  throw new Exception(string.Format("Baking '{0}' failed: Animation state '{1}' in controller '{2}' (layer '{3}') requires a motion.", dataAsset.name, destinationState.name, controller.name, controller.layers[l].name));
            //}

            AnimatorTransition newTransition = new AnimatorTransition();
            newTransition.Index = t;
            newTransition.Name = string.Format("{0} to {1}", state.name, destinationState.name);

            FP transitionDuration = transition.duration.ToFP();
            FP transitionOffset = transition.offset.ToFP();
            if (transition.hasFixedDuration == false && state.motion != null && destinationState.motion != null)
            {
              transitionDuration *= state.motion.averageDuration.ToFP();
              transitionOffset *= destinationState.motion.averageDuration.ToFP();
            }

            newTransition.Duration = transitionDuration;
            newTransition.Offset = transitionOffset;
            newTransition.HasExitTime = transition.hasExitTime;

            var exitTime = state.motion != null
              ? transition.exitTime * state.motion.averageDuration
              : transition.exitTime;

            newTransition.ExitTime = FP.FromFloat_UNSAFE(exitTime);
            newTransition.DestinationStateId = stateDictionary[destinationState].Id;
            newTransition.DestinationStateName = stateDictionary[destinationState].Name;
            newTransition.CanTransitionToSelf = transition.canTransitionToSelf;

            int conditionCount = transition.conditions.Length;
            newTransition.Conditions = new AnimatorCondition[conditionCount];
            for (int c = 0; c < conditionCount; c++)
            {
              UnityEditor.Animations.AnimatorCondition condition = state.transitions[t].conditions[c];

              if (!parameterDic.ContainsKey(condition.parameter)) continue;
              AnimatorControllerParameter parameter = parameterDic[condition.parameter];
              AnimatorCondition newCondition = new AnimatorCondition();

              newCondition.VariableName = condition.parameter;
              newCondition.Mode = (AnimatorCondition.Modes)condition.mode;

              switch (parameter.type)
              {
                case AnimatorControllerParameterType.Float:
                  newCondition.ThresholdFp = FP.FromFloat_UNSAFE(condition.threshold);
                  break;

                case AnimatorControllerParameterType.Int:
                  newCondition.ThresholdInt = Mathf.RoundToInt(condition.threshold);
                  break;
              }

              newTransition.Conditions[c] = newCondition;
            }

            newState.Transitions[t] = newTransition;
          }
        }

        //Create Any State
        AnimatorState anyState = new AnimatorState();
        anyState.Name = "Any State";
        anyState.Id = anyState.Name.GetHashCode();
        anyState.IsAny = true; //important for this one
        AnimatorStateTransition[] anyStateTransitions = controller.layers[l].stateMachine.anyStateTransitions;
        int anyStateTransitionCount = anyStateTransitions.Length;
        anyState.Transitions = new AnimatorTransition[anyStateTransitionCount];
        for (int t = 0; t < anyStateTransitionCount; t++)
        {
          AnimatorStateTransition transition = anyStateTransitions[t];
          if (!stateDictionary.ContainsKey(transition.destinationState)) continue;
          AnimatorTransition newTransition = new AnimatorTransition();
          newTransition.Index = t;
          newTransition.Name = string.Format("Any State to {0}", transition.destinationState.name);
          newTransition.Duration = FP.FromFloat_UNSAFE(transition.duration);
          newTransition.HasExitTime = transition.hasExitTime;
          newTransition.ExitTime = FP._1;
          newTransition.Offset =
            FP.FromFloat_UNSAFE(transition.offset * transition.destinationState.motion.averageDuration);
          newTransition.DestinationStateId = stateDictionary[transition.destinationState].Id;
          newTransition.DestinationStateName = stateDictionary[transition.destinationState].Name;
          newTransition.CanTransitionToSelf = transition.canTransitionToSelf;

          int conditionCount = transition.conditions.Length;
          newTransition.Conditions = new AnimatorCondition[conditionCount];
          for (int c = 0; c < conditionCount; c++)
          {
            UnityEditor.Animations.AnimatorCondition condition = anyStateTransitions[t].conditions[c];

            if (!parameterDic.ContainsKey(condition.parameter)) continue;
            AnimatorControllerParameter parameter = parameterDic[condition.parameter];
            AnimatorCondition newCondition = new AnimatorCondition();

            newCondition.VariableName = condition.parameter;
            newCondition.Mode = (AnimatorCondition.Modes)condition.mode;

            switch (parameter.type)
            {
              case AnimatorControllerParameterType.Float:
                newCondition.ThresholdFp = FP.FromFloat_UNSAFE(condition.threshold);
                break;

              case AnimatorControllerParameterType.Int:
                newCondition.ThresholdInt = Mathf.RoundToInt(condition.threshold);
                break;
            }

            newTransition.Conditions[c] = newCondition;
          }

          anyState.Transitions[t] = newTransition;
        }

        newLayer.States[stateCount] = anyState;

        dataAsset.Layers[l] = newLayer;
      }

      AnimatorGraph.Serialize(dataAsset);

      // Actually write the quantum asset onto the scriptable object.
      dataAsset.Clips = clips;
      //dataAsset = quantumGraph;
      dataAsset.Controller = controller;

      dataAsset.IsValid = true;

      EditorUtility.SetDirty(dataAsset);

      Debug.Log($"[Quantum Animator] Imported {dataAsset.name} data.");
    }

    public static AnimatorData Extract(AnimationClip clip)
    {
      AnimatorData animationData = new AnimatorData();
      animationData.ClipName = clip.name;

      EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
      AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);

      float usedTime = settings.stopTime - settings.startTime;

      animationData.FrameRate = Mathf.RoundToInt(clip.frameRate);
      animationData.Length = FP.FromFloat_UNSAFE(usedTime);
      animationData.FrameCount = Mathf.RoundToInt(clip.frameRate * usedTime);
      animationData.Frames = new AnimatorFrame[animationData.FrameCount];
      animationData.Looped = clip.isLooping && settings.loopTime;
      animationData.Mirror = settings.mirror;
      animationData.Events = ProcessEvents(clip);

      //Read the curves of animation
      int frameCount = animationData.FrameCount;
      int curveBindingsLength = curveBindings.Length;
      if (curveBindingsLength == 0) return animationData;

      AnimationCurve curveTx = null,
        curveTy = null,
        curveTz = null,
        curveRx = null,
        curveRy = null,
        curveRz = null,
        curveRw = null;

      for (int c = 0; c < curveBindingsLength; c++)
      {
        string propertyName = curveBindings[c].propertyName;
        if (propertyName == "m_LocalPosition.x" || propertyName == "RootT.x")
          curveTx = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
        if (propertyName == "m_LocalPosition.y" || propertyName == "RootT.y")
          curveTy = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
        if (propertyName == "m_LocalPosition.z" || propertyName == "RootT.z")
          curveTz = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);

        if (propertyName == "m_LocalRotation.x" || propertyName == "RootQ.x")
          curveRx = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
        if (propertyName == "m_LocalRotation.y" || propertyName == "RootQ.y")
          curveRy = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
        if (propertyName == "m_LocalRotation.z" || propertyName == "RootQ.z")
          curveRz = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
        if (propertyName == "m_LocalRotation.w" || propertyName == "RootQ.w")
          curveRw = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
      }

      //        if (curveBindingsLength >= 7)
      //        {
      //            //Position Curves
      //            curveTx = AnimationUtility.GetEditorCurve(clip, curveBindings[0]);
      //            curveTy = AnimationUtility.GetEditorCurve(clip, curveBindings[1]);
      //            curveTz = AnimationUtility.GetEditorCurve(clip, curveBindings[2]);
      //
      //            //Rotation Curves
      //            curveRx = AnimationUtility.GetEditorCurve(clip, curveBindings[3]);
      //            curveRy = AnimationUtility.GetEditorCurve(clip, curveBindings[4]);
      //            curveRz = AnimationUtility.GetEditorCurve(clip, curveBindings[5]);
      //            curveRw = AnimationUtility.GetEditorCurve(clip, curveBindings[6]);
      //        }

      bool hasPosition = curveTx != null && curveTy != null && curveTz != null;
      bool hasRotation = curveRx != null && curveRy != null && curveRz != null && curveRw != null;

      if (!hasPosition) Debug.LogWarning("No movement data was found in the animation: " + clip.name);
      if (!hasRotation) Debug.LogWarning("No rotation data was found in the animation: " + clip.name);

      // The initial pose might not be the first frame and might not face foward
      // calculate the initial direction and create an offset Quaternion to apply to transforms;

      Quaternion startRotUq = Quaternion.identity;
      FPQuaternion startRot = FPQuaternion.Identity;
      if (hasRotation)
      {
        float srotxu = curveRx.Evaluate(settings.startTime);
        float srotyu = curveRy.Evaluate(settings.startTime);
        float srotzu = curveRz.Evaluate(settings.startTime);
        float srotwu = curveRw.Evaluate(settings.startTime);

        FP srotx = FP.FromFloat_UNSAFE(srotxu);
        FP sroty = FP.FromFloat_UNSAFE(srotyu);
        FP srotz = FP.FromFloat_UNSAFE(srotzu);
        FP srotw = FP.FromFloat_UNSAFE(srotwu);

        startRotUq = new Quaternion(srotxu, srotyu, srotzu, srotwu);
        startRot = new FPQuaternion(srotx, sroty, srotz, srotw);
      }

      Quaternion offsetRotUq = Quaternion.Inverse(startRotUq);
      FPQuaternion offsetRot = FPQuaternion.Inverse(startRot);

      for (int i = 0; i < frameCount; i++)
      {
        var frameData = new AnimatorFrame();
        frameData.Id = i;
        float percent = i / (frameCount - 1f);
        float frameTime = usedTime * percent;
        frameData.Time = FP.FromFloat_UNSAFE(frameTime);
        float clipTIme = settings.startTime + percent * (settings.stopTime - settings.startTime);

        if (hasPosition)
        {
          FP posx = FP.FromFloat_UNSAFE(i > 0 ? curveTx.Evaluate(clipTIme) - curveTx.Evaluate(settings.startTime) : 0);
          FP posy = FP.FromFloat_UNSAFE(i > 0 ? curveTy.Evaluate(clipTIme) - curveTy.Evaluate(settings.startTime) : 0);
          FP posz = FP.FromFloat_UNSAFE(i > 0 ? curveTz.Evaluate(clipTIme) - curveTz.Evaluate(settings.startTime) : 0);
          FPVector3 newPosition = offsetRot * new FPVector3(posx, posy, posz);
          if (settings.mirror) newPosition.X = -newPosition.X;
          frameData.Position = newPosition;
        }

        if (hasRotation)
        {
          float curveRxEval = curveRx.Evaluate(clipTIme);
          float curveRyEval = curveRy.Evaluate(clipTIme);
          float curveRzEval = curveRz.Evaluate(clipTIme);
          float curveRwEval = curveRw.Evaluate(clipTIme);
          Quaternion curveRotation = offsetRotUq * new Quaternion(curveRxEval, curveRyEval, curveRzEval, curveRwEval);
          if (settings.mirror) //mirror the Y axis rotation
          {
            Quaternion mirrorRotation =
              new Quaternion(curveRotation.x, -curveRotation.y, -curveRotation.z, curveRotation.w);

            if (Quaternion.Dot(curveRotation, mirrorRotation) < 0)
            {
              mirrorRotation = new Quaternion(-mirrorRotation.x, -mirrorRotation.y, -mirrorRotation.z,
                -mirrorRotation.w);
            }

            curveRotation = mirrorRotation;
          }

          FP rotx = FP.FromFloat_UNSAFE(curveRotation.x);
          FP roty = FP.FromFloat_UNSAFE(curveRotation.y);
          FP rotz = FP.FromFloat_UNSAFE(curveRotation.z);
          FP rotw = FP.FromFloat_UNSAFE(curveRotation.w);
          FPQuaternion newRotation = new FPQuaternion(rotx, roty, rotz, rotw);
          frameData.Rotation = FPQuaternion.Product(offsetRot, newRotation);

          float rotY = curveRotation.eulerAngles.y * Mathf.Deg2Rad;
          while (rotY < -Mathf.PI) rotY += Mathf.PI * 2;
          while (rotY > Mathf.PI) rotY += -Mathf.PI * 2;
          frameData.RotationY = FP.FromFloat_UNSAFE(rotY);
        }

        animationData.Frames[i] = frameData;
      }

      return animationData;
    }

    private static AnimatorEvent[] ProcessEvents(AnimationClip unityClip)
    {
      var clipEvents = new List<AnimatorEvent>();
      for (int i = 0; i < unityClip.events.Length; i++)
      {
        var unityEvent = unityClip.events[i];
        var animationEventData = (IAnimatorEventAsset)unityEvent.objectReferenceParameter;
        if (animationEventData != null)
        {
          var newEvent = animationEventData.OnBake(unityClip, unityEvent);
          if (newEvent != null)
          {
            clipEvents.Add(newEvent);
          }
        }
      }

      return clipEvents.ToArray();
    }
  }
}