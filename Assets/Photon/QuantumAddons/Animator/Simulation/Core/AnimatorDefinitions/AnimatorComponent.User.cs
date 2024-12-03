namespace Quantum
{
  using Photon.Deterministic;
  using Collections;

  /// <summary>
  /// Extension struct for the AnimatorComponent component
  /// Used mainly to get/set variable values and to initialize an entity's AnimatorComponent
  /// </summary>
  public unsafe partial struct AnimatorComponent
  {
    internal static AnimatorRuntimeVariable* Variable(Frame f, AnimatorComponent* animator, int index)
    {
      var variablesList = f.ResolveList(animator->AnimatorVariables);
      Assert.Check(index >= 0 && index < variablesList.Count);
      return variablesList.GetPointer(index);
    }

    internal static AnimatorRuntimeVariable* VariableByName(Frame f, AnimatorComponent* animator, string name,
      out int variableId)
    {
      variableId = -1;
      if (animator->AnimatorGraph.Equals(default) == false)
      {
        AnimatorGraph graph = f.FindAsset<AnimatorGraph>(animator->AnimatorGraph.Id);
        variableId = graph.VariableIndex(name);
        if (variableId >= 0)
        {
          return Variable(f, animator, variableId);
        }
      }

      return null;
    }

    /// <summary>
    /// Initializes the passed AnimatorComponent component based on the AnimatorGraph passed
    /// This is how timers are initialized and variables are set to default
    /// </summary>
    public static void SetAnimatorGraph(Frame f, AnimatorComponent* animator, AnimatorGraph graph)
    {
      Assert.Check(graph != null, $"[Custom Animator] Tried to initialize Custom Animator component with null graph.");
      graph.Initialise(f, animator);
    }

    private static void SetRuntimeVariable(Frame f, AnimatorComponent* animator, AnimatorRuntimeVariable* variable,
      int variableId)
    {
      Assert.Check(variable != null);
      Assert.Check(variableId >= 0);

      var paramsList = f.ResolveList(animator->AnimatorVariables);
      *paramsList.GetPointer(variableId) = *variable;
    }

    public static QList<FP> GetStateWeights(Frame f, AnimatorComponent* animator, int stateId)
    {
      var weightsDictionary = f.ResolveDictionary(animator->BlendTreeWeights);
      var weights = f.ResolveList(weightsDictionary[stateId].Values);
      return weights;
    }

    #region FixedPoint

    private static void SetFixedPointValue(Frame f, AnimatorComponent* animator, AnimatorRuntimeVariable* variable,
      int variableId, FP value)
    {
      if (variable == null)
      {
        return;
      }

      *variable->FPValue = value;
      SetRuntimeVariable(f, animator, variable, variableId);
    }

    public static void SetFixedPoint(Frame f, AnimatorComponent* animator, string name, FP value)
    {
      var variable = VariableByName(f, animator, name, out var variableId);
      SetFixedPointValue(f, animator, variable, variableId, value);
    }

    public static void SetFixedPoint(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name,
      FP value)
    {
      Assert.Check(animator->AnimatorGraph == graph);

      var variableId = graph.VariableIndex(name);
      SetFixedPoint(f, animator, variableId, value);
    }

    public static void SetFixedPoint(Frame f, AnimatorComponent* animator, int variableId, FP value)
    {
      if (variableId < 0)
      {
        return;
      }

      var variable = Variable(f, animator, variableId);
      SetFixedPointValue(f, animator, variable, variableId, value);
    }

    public static FP GetFixedPoint(Frame f, AnimatorComponent* animator, string name)
    {
      var variable = VariableByName(f, animator, name, out _);
      if (variable != null)
      {
        return *variable->FPValue;
      }

      return FP.PiOver4;
    }

    public static FP GetFixedPoint(Frame f, AnimatorComponent* animator, AnimatorGraph g, string name)
    {
      Assert.Check(animator->AnimatorGraph == g);

      var variableId = g.VariableIndex(name);
      return GetFixedPoint(f, animator, variableId);
    }

    public static FP GetFixedPoint(Frame f, AnimatorComponent* animator, int variableId)
    {
      if (variableId < 0)
      {
        return FP.PiOver4;
      }

      var variable = Variable(f, animator, variableId);
      if (variable != null)
      {
        return *variable->FPValue;
      }

      return FP.PiOver4;
    }

    #endregion

    #region Integer

    static void SetIntegerValue(Frame f, AnimatorComponent* a, AnimatorRuntimeVariable* variable, int variableId,
      int value)
    {
      if (variable == null)
      {
        return;
      }

      *variable->IntegerValue = value;
      SetRuntimeVariable(f, a, variable, variableId);
    }

    public static void SetInteger(Frame f, AnimatorComponent* animator, string name, int value)
    {
      var variable = VariableByName(f, animator, name, out var variableId);
      SetIntegerValue(f, animator, variable, variableId, value);
    }

    public static void SetInteger(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name,
      int value)
    {
      Assert.Check(animator->AnimatorGraph == graph);

      var variableId = graph.VariableIndex(name);
      SetInteger(f, animator, variableId, value);
    }

    public static void SetInteger(Frame f, AnimatorComponent* animator, int variableId, int value)
    {
      if (variableId < 0)
      {
        return;
      }

      var variable = Variable(f, animator, variableId);
      SetIntegerValue(f, animator, variable, variableId, value);
    }

    public static int GetInteger(Frame f, AnimatorComponent* animator, string name)
    {
      var variable = VariableByName(f, animator, name, out _);
      if (variable != null)
      {
        return *variable->IntegerValue;
      }

      return 0;
    }

    public static int GetInteger(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name)
    {
      Assert.Check(animator->AnimatorGraph == graph);

      var variableId = graph.VariableIndex(name);
      return GetInteger(f, animator, variableId);
    }

    public static int GetInteger(Frame f, AnimatorComponent* animator, int variableId)
    {
      if (variableId < 0)
      {
        return 0;
      }

      var variable = Variable(f, animator, variableId);
      if (variable != null)
      {
        return *variable->IntegerValue;
      }

      return 0;
    }

    #endregion

    #region Boolean

    static void SetBooleanValue(Frame f, AnimatorComponent* animator, AnimatorRuntimeVariable* variable, int variableId,
      bool value)
    {
      if (variable == null)
      {
        return;
      }

      *variable->BooleanValue = value;
      SetRuntimeVariable(f, animator, variable, variableId);
    }

    public static void SetBoolean(Frame f, AnimatorComponent* animator, string name, bool value)
    {
      var variable = VariableByName(f, animator, name, out var variableId);
      SetBooleanValue(f, animator, variable, variableId, value);
    }

    public static void SetBoolean(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name,
      bool value)
    {
      Assert.Check(animator->AnimatorGraph == graph);

      var variableId = graph.VariableIndex(name);
      SetBoolean(f, animator, variableId, value);
    }

    public static void SetBoolean(Frame f, AnimatorComponent* animator, int variableId, bool value)
    {
      if (variableId < 0)
      {
        return;
      }

      var variable = Variable(f, animator, variableId);
      SetBooleanValue(f, animator, variable, variableId, value);
    }

    public static bool GetBoolean(Frame f, AnimatorComponent* animator, string name)
    {
      var variable = VariableByName(f, animator, name, out _);
      if (variable != null)
      {
        return *variable->BooleanValue;
      }

      return false;
    }

    public static bool GetBoolean(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name)
    {
      Assert.Check(animator->AnimatorGraph == graph);

      var variableId = graph.VariableIndex(name);
      return GetBoolean(f, animator, variableId);
    }

    public static bool GetBoolean(Frame f, AnimatorComponent* animator, int variableId)
    {
      if (variableId < 0)
      {
        return false;
      }

      var variable = Variable(f, animator, variableId);
      if (variable != null)
      {
        return *variable->BooleanValue;
      }

      return false;
    }

    #endregion

    #region Trigger

    public static void SetTrigger(Frame f, AnimatorComponent* animator, string name)
    {
      SetBoolean(f, animator, name, true);
    }

    public static void SetTrigger(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name)
    {
      SetBoolean(f, animator, graph, name, true);
    }

    public static void SetTrigger(Frame f, AnimatorComponent* animator, int variableId)
    {
      SetBoolean(f, animator, variableId, true);
    }

    public static void ResetTrigger(Frame f, AnimatorComponent* animator, string name)
    {
      SetBoolean(f, animator, name, false);
    }

    public static void ResetTrigger(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name)
    {
      SetBoolean(f, animator, graph, name, false);
    }

    public static void ResetTrigger(Frame f, AnimatorComponent* animator, int variableId)
    {
      SetBoolean(f, animator, variableId, false);
    }

    public static bool IsTriggerActive(Frame f, AnimatorComponent* animator, string name)
    {
      return GetBoolean(f, animator, name);
    }

    public static bool IsTriggerActive(Frame f, AnimatorComponent* animator, AnimatorGraph graph, string name)
    {
      return GetBoolean(f, animator, graph, name);
    }

    public static bool IsTriggerActive(Frame f, AnimatorComponent* animator, int variableId)
    {
      return GetBoolean(f, animator, variableId);
    }

    #endregion
  }
}