// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
//
// If you need to extend the classes generated to add
// fields or methods to them, please create partial
// declarations in another file.
// </auto-generated>
#pragma warning disable 0109
#pragma warning disable 1591


namespace Quantum.Prototypes {
  using Photon.Deterministic;
  using Quantum;
  using Quantum.Core;
  using Quantum.Collections;
  using Quantum.Inspector;
  using Quantum.Physics2D;
  using Quantum.Physics3D;
  using Byte = System.Byte;
  using SByte = System.SByte;
  using Int16 = System.Int16;
  using UInt16 = System.UInt16;
  using Int32 = System.Int32;
  using UInt32 = System.UInt32;
  using Int64 = System.Int64;
  using UInt64 = System.UInt64;
  using Boolean = System.Boolean;
  using String = System.String;
  using Object = System.Object;
  using FlagsAttribute = System.FlagsAttribute;
  using SerializableAttribute = System.SerializableAttribute;
  using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
  using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;
  using FieldOffsetAttribute = System.Runtime.InteropServices.FieldOffsetAttribute;
  using StructLayoutAttribute = System.Runtime.InteropServices.StructLayoutAttribute;
  using LayoutKind = System.Runtime.InteropServices.LayoutKind;
  #if QUANTUM_UNITY //;
  using TooltipAttribute = UnityEngine.TooltipAttribute;
  using HeaderAttribute = UnityEngine.HeaderAttribute;
  using SpaceAttribute = UnityEngine.SpaceAttribute;
  using RangeAttribute = UnityEngine.RangeAttribute;
  using HideInInspectorAttribute = UnityEngine.HideInInspector;
  using PreserveAttribute = UnityEngine.Scripting.PreserveAttribute;
  using FormerlySerializedAsAttribute = UnityEngine.Serialization.FormerlySerializedAsAttribute;
  using MovedFromAttribute = UnityEngine.Scripting.APIUpdating.MovedFromAttribute;
  using CreateAssetMenu = UnityEngine.CreateAssetMenuAttribute;
  using RuntimeInitializeOnLoadMethodAttribute = UnityEngine.RuntimeInitializeOnLoadMethodAttribute;
  #endif //;
  
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(System.Collections.Generic.KeyValuePair<Int32, BlendTreeWeights>))]
  public unsafe class DictionaryEntry_Int32_BlendTreeWeights : Quantum.Prototypes.DictionaryEntry {
    public Int32 Key;
    public Quantum.Prototypes.BlendTreeWeightsPrototype Value;
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.AnimatorComponent))]
  public unsafe class AnimatorComponentPrototype : ComponentPrototype<Quantum.AnimatorComponent> {
    public AssetRef<AnimatorGraph> AnimatorGraph;
    public MapEntityId Self;
    [HideInInspector()]
    public FP Time;
    [HideInInspector()]
    public FP NormalizedTime;
    [HideInInspector()]
    public FP LastTime;
    [HideInInspector()]
    public FP Length;
    [HideInInspector()]
    public Int32 CurrentStateId;
    [HideInInspector()]
    public QBoolean Freeze;
    [HideInInspector()]
    public FP Speed;
    [HideInInspector()]
    public Int32 FromStateId;
    [HideInInspector()]
    public FP FromStateTime;
    [HideInInspector()]
    public FP FromStateLastTime;
    [HideInInspector()]
    public FP FromStateNormalizedTime;
    [HideInInspector()]
    public FP FromLength;
    [HideInInspector()]
    public Int32 ToStateId;
    [HideInInspector()]
    public FP ToStateTime;
    [HideInInspector()]
    public FP ToStateLastTime;
    [HideInInspector()]
    public FP ToStateNormalizedTime;
    [HideInInspector()]
    public FP ToLength;
    [HideInInspector()]
    public Int32 TransitionIndex;
    [HideInInspector()]
    public FP TransitionTime;
    [HideInInspector()]
    public FP TransitionDuration;
    [HideInInspector()]
    public Int32 AnimatorBlendCount;
    public QBoolean IgnoreTransitions;
    [HideInInspector()]
    [DynamicCollectionAttribute()]
    public Quantum.Prototypes.AnimatorRuntimeVariablePrototype[] AnimatorVariables = {};
    [HideInInspector()]
    [DictionaryAttribute()]
    [DynamicCollectionAttribute()]
    public DictionaryEntry_Int32_BlendTreeWeights[] BlendTreeWeights = {};
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.AnimatorComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.AnimatorComponent result, in PrototypeMaterializationContext context = default) {
        result.AnimatorGraph = this.AnimatorGraph;
        PrototypeValidator.FindMapEntity(this.Self, in context, out result.Self);
        result.Time = this.Time;
        result.NormalizedTime = this.NormalizedTime;
        result.LastTime = this.LastTime;
        result.Length = this.Length;
        result.CurrentStateId = this.CurrentStateId;
        result.Freeze = this.Freeze;
        result.Speed = this.Speed;
        result.FromStateId = this.FromStateId;
        result.FromStateTime = this.FromStateTime;
        result.FromStateLastTime = this.FromStateLastTime;
        result.FromStateNormalizedTime = this.FromStateNormalizedTime;
        result.FromLength = this.FromLength;
        result.ToStateId = this.ToStateId;
        result.ToStateTime = this.ToStateTime;
        result.ToStateLastTime = this.ToStateLastTime;
        result.ToStateNormalizedTime = this.ToStateNormalizedTime;
        result.ToLength = this.ToLength;
        result.TransitionIndex = this.TransitionIndex;
        result.TransitionTime = this.TransitionTime;
        result.TransitionDuration = this.TransitionDuration;
        result.AnimatorBlendCount = this.AnimatorBlendCount;
        result.IgnoreTransitions = this.IgnoreTransitions;
        if (this.AnimatorVariables.Length == 0) {
          result.AnimatorVariables = default;
        } else {
          var list = frame.AllocateList(out result.AnimatorVariables, this.AnimatorVariables.Length);
          for (int i = 0; i < this.AnimatorVariables.Length; ++i) {
            Quantum.AnimatorRuntimeVariable tmp = default;
            this.AnimatorVariables[i].Materialize(frame, ref tmp, in context);
            list.Add(tmp);
          }
        }
        if (this.BlendTreeWeights.Length == 0) {
          result.BlendTreeWeights = default;
        } else {
          var dict = frame.AllocateDictionary(out result.BlendTreeWeights, this.BlendTreeWeights.Length);
          for (int i = 0; i < this.BlendTreeWeights.Length; ++i) {
            Int32 tmpKey = default;
            Quantum.BlendTreeWeights tmpValue = default;
            tmpKey = this.BlendTreeWeights[i].Key;
            this.BlendTreeWeights[i].Value.Materialize(frame, ref tmpValue, in context);
            PrototypeValidator.AddToDictionary(dict, tmpKey, tmpValue, in context);
          }
        }
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.AnimatorRuntimeVariable))]
  public unsafe partial class AnimatorRuntimeVariablePrototype : UnionPrototype {
    public string _field_used_;
    public FP FPValue;
    public Int32 IntegerValue;
    public QBoolean BooleanValue;
    partial void MaterializeUser(Frame frame, ref Quantum.AnimatorRuntimeVariable result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.AnimatorRuntimeVariable result, in PrototypeMaterializationContext context = default) {
        switch (_field_used_) {
          case "FPVALUE": *result.FPValue = this.FPValue; break;
          case "INTEGERVALUE": *result.IntegerValue = this.IntegerValue; break;
          case "BOOLEANVALUE": *result.BooleanValue = this.BooleanValue; break;
          case "": case null: break;
          default: PrototypeValidator.UnknownUnionField(_field_used_, in context); break;
        }
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BlendTreeWeights))]
  public unsafe partial class BlendTreeWeightsPrototype : StructPrototype {
    [DynamicCollectionAttribute()]
    public FP[] Values = {};
    partial void MaterializeUser(Frame frame, ref Quantum.BlendTreeWeights result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.BlendTreeWeights result, in PrototypeMaterializationContext context = default) {
        if (this.Values.Length == 0) {
          result.Values = default;
        } else {
          var list = frame.AllocateList(out result.Values, this.Values.Length);
          for (int i = 0; i < this.Values.Length; ++i) {
            FP tmp = default;
            tmp = this.Values[i];
            list.Add(tmp);
          }
        }
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BoxBase))]
  public unsafe partial class BoxBasePrototype : StructPrototype {
    public FPVector2 RectPos;
    public FPVector2 RectWH;
    public FP XMin;
    public FP XMax;
    public FP YMin;
    public FP YMax;
    partial void MaterializeUser(Frame frame, ref Quantum.BoxBase result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.BoxBase result, in PrototypeMaterializationContext context = default) {
        result.RectPos = this.RectPos;
        result.RectWH = this.RectWH;
        result.XMin = this.XMin;
        result.XMax = this.XMax;
        result.YMin = this.YMin;
        result.YMax = this.YMax;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.FighterData))]
  public unsafe partial class FighterDataPrototype : ComponentPrototype<Quantum.FighterData> {
    public AssetRef<FighterConstants> Constants;
    public FPVector2 Position;
    public FPVector2 Velocity;
    public FPVector2 Pushback;
    public QBoolean IsFacingRight;
    public QBoolean IsGrounded;
    public Int32 requestedSideSwitch;
    public Int32 Health;
    public Int32 HitStun;
    public Int32 BlockStun;
    public Quantum.QEnum32<StateID> CurrentState;
    public Int32 StateFrame;
    [DynamicCollectionAttribute()]
    public Quantum.Prototypes.HitBoxPrototype[] HitBoxList = {};
    [DynamicCollectionAttribute()]
    public Quantum.Prototypes.HurtBoxPrototype[] HurtBoxList = {};
    public Quantum.Prototypes.PushBoxPrototype PushBox;
    partial void MaterializeUser(Frame frame, ref Quantum.FighterData result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.FighterData component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.FighterData result, in PrototypeMaterializationContext context = default) {
        result.Constants = this.Constants;
        result.Position = this.Position;
        result.Velocity = this.Velocity;
        result.Pushback = this.Pushback;
        result.IsFacingRight = this.IsFacingRight;
        result.IsGrounded = this.IsGrounded;
        result.requestedSideSwitch = this.requestedSideSwitch;
        result.Health = this.Health;
        result.HitStun = this.HitStun;
        result.BlockStun = this.BlockStun;
        result.CurrentState = this.CurrentState;
        result.StateFrame = this.StateFrame;
        if (this.HitBoxList.Length == 0) {
          result.HitBoxList = default;
        } else {
          var list = frame.AllocateList(out result.HitBoxList, this.HitBoxList.Length);
          for (int i = 0; i < this.HitBoxList.Length; ++i) {
            Quantum.HitBox tmp = default;
            this.HitBoxList[i].Materialize(frame, ref tmp, in context);
            list.Add(tmp);
          }
        }
        if (this.HurtBoxList.Length == 0) {
          result.HurtBoxList = default;
        } else {
          var list = frame.AllocateList(out result.HurtBoxList, this.HurtBoxList.Length);
          for (int i = 0; i < this.HurtBoxList.Length; ++i) {
            Quantum.HurtBox tmp = default;
            this.HurtBoxList[i].Materialize(frame, ref tmp, in context);
            list.Add(tmp);
          }
        }
        this.PushBox.Materialize(frame, ref result.PushBox, in context);
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.HitBox))]
  public unsafe partial class HitBoxPrototype : StructPrototype {
    public QBoolean Proximity;
    public FPVector2 RectPos;
    public Int32 HitNum;
    public FPVector2 RectWH;
    public AssetRef HitData;
    public FP XMin;
    public FP XMax;
    public FP YMin;
    public FP YMax;
    partial void MaterializeUser(Frame frame, ref Quantum.HitBox result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.HitBox result, in PrototypeMaterializationContext context = default) {
        result.Proximity = this.Proximity;
        result.RectPos = this.RectPos;
        result.HitNum = this.HitNum;
        result.RectWH = this.RectWH;
        result.HitData = this.HitData;
        result.XMin = this.XMin;
        result.XMax = this.XMax;
        result.YMin = this.YMin;
        result.YMax = this.YMax;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.HurtBox))]
  public unsafe partial class HurtBoxPrototype : StructPrototype {
    public AssetRef HurtData;
    public FPVector2 RectPos;
    public FPVector2 RectWH;
    public FP XMin;
    public FP XMax;
    public FP YMin;
    public FP YMax;
    partial void MaterializeUser(Frame frame, ref Quantum.HurtBox result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.HurtBox result, in PrototypeMaterializationContext context = default) {
        result.HurtData = this.HurtData;
        result.RectPos = this.RectPos;
        result.RectWH = this.RectWH;
        result.XMin = this.XMin;
        result.XMax = this.XMax;
        result.YMin = this.YMin;
        result.YMax = this.YMax;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.Input))]
  public unsafe partial class InputPrototype : StructPrototype {
    public Button Left;
    public Button Right;
    public Button Up;
    public Button Down;
    public Button Light;
    public Button Medium;
    public Button Heavy;
    public Button Special;
    partial void MaterializeUser(Frame frame, ref Quantum.Input result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.Input result, in PrototypeMaterializationContext context = default) {
        result.Left = this.Left;
        result.Right = this.Right;
        result.Up = this.Up;
        result.Down = this.Down;
        result.Light = this.Light;
        result.Medium = this.Medium;
        result.Heavy = this.Heavy;
        result.Special = this.Special;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.PlayerLink))]
  public unsafe partial class PlayerLinkPrototype : ComponentPrototype<Quantum.PlayerLink> {
    public PlayerRef PlayerRef;
    partial void MaterializeUser(Frame frame, ref Quantum.PlayerLink result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.PlayerLink component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.PlayerLink result, in PrototypeMaterializationContext context = default) {
        result.PlayerRef = this.PlayerRef;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.PushBox))]
  public unsafe partial class PushBoxPrototype : StructPrototype {
    public FPVector2 RectPos;
    public FPVector2 RectWH;
    public FP XMin;
    public FP XMax;
    public FP YMin;
    public FP YMax;
    partial void MaterializeUser(Frame frame, ref Quantum.PushBox result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.PushBox result, in PrototypeMaterializationContext context = default) {
        result.RectPos = this.RectPos;
        result.RectWH = this.RectWH;
        result.XMin = this.XMin;
        result.XMax = this.XMax;
        result.YMin = this.YMin;
        result.YMax = this.YMax;
        MaterializeUser(frame, ref result, in context);
    }
  }
}
#pragma warning restore 0109
#pragma warning restore 1591
