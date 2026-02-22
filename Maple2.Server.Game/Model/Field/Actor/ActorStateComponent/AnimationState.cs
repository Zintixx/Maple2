using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.ActorStateComponent;

/// <summary>
/// Mutable runtime state owned exclusively by AnimationManager, representing a playing animation clip.
/// </summary>
public sealed class AnimationState {
    public AnimationSequenceMetadata Sequence { get; }
    public AnimationRequest Request { get; }

    public float EndTime { get; internal set; }
    public (float start, float end) Loop { get; internal set; }
    public long EndTick { get; internal set; }
    public long LoopEndTick { get; internal set; }
    public bool IsLooping { get; internal set; }
    public bool LoopOnlyOnce { get; internal set; }

    public AnimationState(AnimationSequenceMetadata sequence, AnimationRequest request) {
        Sequence = sequence;
        Request = request;
    }
}
