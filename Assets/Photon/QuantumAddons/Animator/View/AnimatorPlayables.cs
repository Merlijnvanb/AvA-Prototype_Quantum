namespace Quantum.Addons.Animator
{
  using Quantum;
  using System;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Animations;
  using UnityEngine.Playables;
  using UE = UnityEngine;

  public unsafe class AnimatorPlayables : QuantumEntityViewComponent
  {
    UE.Animator _animator;

    Dictionary<String, KeyValuePair<int, AnimationClipPlayable>> _clips =
      new Dictionary<String, KeyValuePair<int, AnimationClipPlayable>>();

    PlayableGraph _graph;
    AnimationMixerPlayable _mixerPlayable;

    // used during SetAnimationData
    List<int> _activeInputs = new List<int>(64);
    static List<AnimatorRuntimeBlendData> _blendData = new List<AnimatorRuntimeBlendData>(64);
    static List<AnimatorMotion> _motionData = new List<AnimatorMotion>(32);

    void Awake()
    {
      _animator = GetComponentInChildren<UE.Animator>();
    }

    void OnEnable()
    {
      if (_animator)
      {
        _graph = PlayableGraph.Create();
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _mixerPlayable = AnimationMixerPlayable.Create(_graph);
        var output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);
        output.SetSourcePlayable(_mixerPlayable);

        _graph.Play();
      }
    }

    void OnDisable()
    {
      _activeInputs.Clear();
      _clips.Clear();
      _graph.Destroy();
    }

    public void Animate(Frame frame, AnimatorComponent* animator)
    {
      if (!_animator.enabled)
      {
        return;
      }

      var asset = QuantumUnityDB.GetGlobalAsset<AnimatorGraph>(animator->AnimatorGraph.Id);
      if (asset)
      {
        // load clips
        LoadClips(asset.Clips);

        // calculate blend data
        asset.GenerateBlendList(frame, animator, _blendData);

        // update animation state
        SetAnimationData(asset, _blendData);

        // clear old blend data
        _blendData.Clear();
      }
    }

    void LoadClips(List<AnimationClip> clipList)
    {
      if (_clips.Count == 0)
      {
        for (int c = 0; c < clipList.Count; c++)
        {
          if (_clips.ContainsKey(clipList[c].name) == false)
          {
            var clip = AnimationClipPlayable.Create(_graph, clipList[c]);
            _clips.Add(
              clipList[c].name,
              new KeyValuePair<int, AnimationClipPlayable>(_mixerPlayable.AddInput(clip, 0), clip)
            );
#if UNITY_2018_1_OR_NEWER
            clip.Pause();
#else
          clip.SetPlayState(PlayState.Paused);
#endif
          }
        }
      }
    }


    void SetAnimationData(AnimatorGraph graph, List<AnimatorRuntimeBlendData> blend_data)
    {
      foreach (var input in _activeInputs)
      {
        _mixerPlayable.SetInputWeight(input, 0);
      }

      _activeInputs.Clear();

      foreach (var b in blend_data)
      {
        var state = graph.GetState(b.StateId);
        var motion = state.GetMotion(b.AnimationIndex, _motionData) as AnimatorClip;

        if (motion != null && !String.IsNullOrEmpty(motion.ClipName))
        {
          if (_clips.TryGetValue(motion.ClipName, out KeyValuePair<int, AnimationClipPlayable> clip))
          {
            _activeInputs.Add(clip.Key);

            _mixerPlayable.SetInputWeight(clip.Key, b.Weight.AsFloat);
            var normalTime = b.NormalTime.AsDouble;
            var clipLength = clip.Value.GetAnimationClip().length;
            var expectedTime = normalTime * clipLength;
            clip.Value.SetTime(expectedTime);
          }
          else
          {
            Log.Error(
              "SetAnimationData failed to find clip: " + motion.ClipName + " in graph: " + graph.Guid.ToString());
          }
        }

        _motionData.Clear();
      }
    }
  }
}