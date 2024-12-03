namespace Quantum.Addons.Animator
{
  using System;
  using Photon.Deterministic;

  [Serializable]
  public class AnimatorVariable
  {
    public enum VariableType
    {
      Int,
      FP,
      Bool,
      Trigger
    }

    public string Name = "Variable";
    public int Index = 0;
    public VariableType Type;

    public FP DefaultFp;
    public Int32 DefaultInt;
    public Boolean DefaultBool;

    public object GetObject(int objectIndex)
    {
      if (objectIndex == Index)
      {
        return this;
      }

      return null;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}