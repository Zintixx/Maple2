namespace Maple2.Server.Game.Model.ActorStateComponent;

public sealed class AnimationCallbacks {
    /// Called for every non-reserved keyframe event (excludes "loopstart", "loopend", "end")
    public Action<string>? OnEvent { get; init; }
    /// Called each time a loop cycle completes (when "loopend" keyframe is hit)
    public Action? OnLoopEnd { get; init; }
    /// Called when the sequence terminates naturally (not on Cancel)
    public Action? OnComplete { get; init; }
}
