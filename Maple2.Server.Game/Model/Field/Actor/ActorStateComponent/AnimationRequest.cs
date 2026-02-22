using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Model.ActorStateComponent;

/// <summary>
/// Immutable intent — created by callers, passed to TryPlay.
/// </summary>
public sealed class AnimationRequest {
    public required string SequenceName { get; init; }
    public float Speed { get; init; } = 1f;
    public AnimationPriority Priority { get; init; } = AnimationPriority.Idle;
    /// If true, this request can interrupt a currently playing animation at the same priority.
    public bool CanInterruptSelf { get; init; } = false;
    public SkillMetadata? Skill { get; init; }
    public AnimationCallbacks? Callbacks { get; init; }
}
