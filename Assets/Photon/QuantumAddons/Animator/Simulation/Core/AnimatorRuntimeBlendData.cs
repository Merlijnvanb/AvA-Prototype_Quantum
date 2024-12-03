namespace Quantum.Addons.Animator
{
  using Photon.Deterministic;

  public struct AnimatorRuntimeBlendData
  {
    public int LayerId;
    public int StateId;
    public int AnimationIndex;
    public FP CurrentTime;
    public FP NormalTime;
    public FP LastTime;
    public FP Weight;
    public FP Length;
    public FP CalculatedLength;

    public AnimatorRuntimeBlendData(AnimatorLayer layer, AnimatorState state, int index, FP lastTime, FP currentTime,
      FP normalisedTime, FP weight, FP length, FP calculatedLength)
    {
      LayerId = layer.Id;
      StateId = state.Id;
      AnimationIndex = index;
      this.CurrentTime = currentTime;
      NormalTime = normalisedTime;
      this.LastTime = lastTime;
      this.Weight = weight;
      this.Length = length;
      this.CalculatedLength = calculatedLength;
    }
  }
}