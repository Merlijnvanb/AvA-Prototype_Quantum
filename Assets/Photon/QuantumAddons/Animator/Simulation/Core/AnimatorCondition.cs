namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;

  /// <summary>
  /// Contains everything related to the conditions used on the transitions between states.
  /// You'll find here the checks depending on the type of check and on the data type.
  /// </summary>
  [Serializable]
  public unsafe class AnimatorCondition
  {
    /// <summary>
    /// Name of the variable, it is populated during the graph construction.
    /// </summary>
    public string VariableName = "";
    /// <summary>
    /// Determines how the variable will be checked in comparision to the threshold.
    /// </summary>
    public Modes Mode;
    /// <summary>
    /// The values of the threshold after converting to FP.
    /// </summary>
    public FP ThresholdFp;
    /// <summary>
    /// The values of the threshold rounded to Int32 using Mathf.RoundToInt(condition.threshold).
    /// </summary>
    public Int32 ThresholdInt;

    /// <summary>
    /// Mapped 1 to 1 with AnimatorConditionMode.
    /// </summary>
    public enum Modes
    {
      If = 1,
      IfNot = 2,
      Greater = 3,
      Less = 4,
      Equals = 6,
      NotEqual = 7,
    }

    /// <summary>
    /// AnimatorCondition constructor
    /// </summary>
    public AnimatorCondition()
    {
      ThresholdFp = FP._0;
      ThresholdInt = 0;
    }

    /// <summary>
    /// Checks if the condition variable meets the threshold 
    /// </summary>
    public bool Check(Frame f, AnimatorComponent* animator, AnimatorGraph graph)
    {
      int variableIndex = graph.VariableIndex(VariableName);
      if (variableIndex == -1)
      {
        Log.Error($"Variable not found in graph: {VariableName}");
      }

      AnimatorVariable variable = graph.Variables[variableIndex];

      switch (variable.Type)
      {
        case AnimatorVariable.VariableType.FP:
          switch (Mode)
          {
            case Modes.Equals:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->FPValue == ThresholdFp;
            case Modes.Greater:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->FPValue > ThresholdFp;
            case Modes.Less:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->FPValue < ThresholdFp;
            case Modes.NotEqual:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->FPValue != ThresholdFp;
          }

          break;

        case AnimatorVariable.VariableType.Int:
          switch (Mode)
          {
            case Modes.Equals:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->IntegerValue == ThresholdInt;
            case Modes.Greater:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->IntegerValue > ThresholdInt;
            case Modes.Less:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->IntegerValue < ThresholdInt;
            case Modes.NotEqual:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->IntegerValue != ThresholdInt;
          }

          break;

        case AnimatorVariable.VariableType.Bool:
          switch (Mode)
          {
            case Modes.If:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->BooleanValue;
            case Modes.IfNot:
              return !*AnimatorComponent.Variable(f, animator, variableIndex)->BooleanValue;
          }

          break;

        case AnimatorVariable.VariableType.Trigger:
          switch (Mode)
          {
            case Modes.If:
              return *AnimatorComponent.Variable(f, animator, variableIndex)->BooleanValue;
          }

          break;
      }

      return false;
    }
  }
}