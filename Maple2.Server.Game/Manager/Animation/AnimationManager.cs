using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Manager;

/// <summary>
/// Abstract base class for managing animation sequences for actors.
/// Subclasses implement actor-specific speed modifiers, sequence-change hooks,
/// timing enforcement, and debug output.
/// </summary>
public abstract class AnimationManager {
    protected IActor Actor { get; private set; }
    public AnimationMetadata? RigMetadata { get; protected set; }
    public short IdleSequenceId { get; protected set; }

    public AnimationState? Current { get; private set; }
    public AnimationSequenceMetadata? PlayingSequence => Current?.Sequence;
    public float PlaybackSpeed => Current?.Request.Speed ?? 1f;

    public float MoveSpeed { get; set; } = 1f;
    public float AttackSpeed { get; set; } = 1f;

    public bool DebugPrintAnimations { get; set; } = false;

    private readonly PriorityQueue<AnimationRequest, int> deferredQueue = new();
    private bool isDispatchingKeyframes;
    private float lastSequenceTime;
    private long lastTick;

    protected AnimationManager(IActor actor) {
        Actor = actor;
    }

    // --- Public API ---

    /// <summary>
    /// Attempts to play an animation sequence from an AnimationRequest.
    /// Respects the priority system; may defer if currently dispatching keyframe events.
    /// </summary>
    public bool TryPlay(AnimationRequest request) {
        if (RigMetadata is null || !RigMetadata.Sequences.TryGetValue(request.SequenceName, out AnimationSequenceMetadata? sequence)) {
            DebugPrint($"Attempt to play nonexistent sequence '{request.SequenceName}' at x{request.Speed} speed, previous: '{PlayingSequence?.Name ?? "none"}' x{PlaybackSpeed}");
            ResetSequence();
            return false;
        }

        if (isDispatchingKeyframes) {
            deferredQueue.Enqueue(request, -(int) request.Priority);
            return true;
        }

        if (!CanPlay(request)) {
            return false;
        }

        PlaySequence(sequence, request);
        return true;
    }

    /// <summary>
    /// Unconditionally cancels the currently playing animation. Does NOT fire OnComplete.
    /// </summary>
    public void Cancel() {
        if (Current is not null) {
            DebugPrint($"Canceled playing sequence: '{PlayingSequence?.Name}' x{PlaybackSpeed}");
            OnSequenceChanged(Current, null);
            Current = null;
        }
        deferredQueue.Clear();
        lastSequenceTime = 0;
        lastTick = 0;
    }

    /// <summary>
    /// Sets whether the current sequence should loop.
    /// </summary>
    public void SetLooping(bool shouldLoop, bool loopOnlyOnce) {
        if (Current is null) return;
        Current.IsLooping = shouldLoop;
        Current.LoopOnlyOnce = loopOnlyOnce;
    }

    /// <summary>
    /// Updates the animation state based on the current tick count.
    /// </summary>
    public void Update(long tickCount) {
        if (RigMetadata is null) return;
        if (PlayingSequence?.Keys.Count == 0) {
            ResetSequence();
            return;
        }
        if (Current is null) return;

        float speed = PlaybackSpeed * GetSpeedModifier(Current.Request.Priority) / 1000f;

        // Step 1: advance time
        float sequenceTime = AdvanceTime(tickCount, speed);

        // Step 2: collect keyframe events hit this tick
        List<AnimationKey> keysHit = CollectKeyframes(sequenceTime);

        // Step 3: dispatch keyframe events
        isDispatchingKeyframes = true;
        DispatchKeyframes(keysHit, sequenceTime, speed);
        isDispatchingKeyframes = false;

        // Step 4: handle loop boundary (may re-collect + dispatch post-wrap keys)
        sequenceTime = HandleLoopBoundary(sequenceTime, speed, tickCount);

        // Step 5: handle sequence end (fires OnComplete after reset)
        HandleSequenceEnd(sequenceTime, tickCount);

        // Step 6: flush deferred TryPlay calls that were queued during keyframe dispatch
        FlushDeferredQueue();

        // Step 7: commit timing for next tick
        CommitTiming(tickCount, sequenceTime);
    }

    /// <summary>
    /// Returns normalized time (0–1) for the current position within a segment
    /// defined by two named keyframes. Returns -1 if not currently within the segment.
    /// </summary>
    public float GetNormalizedSegmentTime(string startKeyframe, string endKeyframe) {
        if (PlayingSequence is null) return -1;

        float startTime = -1;
        float endTime = -1;

        foreach (AnimationKey key in PlayingSequence.Keys) {
            if (key.Name == startKeyframe) startTime = key.Time;
            if (key.Name == endKeyframe) {
                endTime = key.Time;
                break;
            }
        }

        if (startTime == -1 || endTime == -1 || startTime > endTime) return -1;
        if (lastSequenceTime < startTime || lastSequenceTime >= endTime) return -1;
        if (startTime == endTime) return lastSequenceTime == startTime ? 1 : -1;

        return (lastSequenceTime - startTime) / (endTime - startTime);
    }

    /// <summary>
    /// Returns true if an animation is currently playing.
    /// </summary>
    public bool IsPlaying => Current != null && PlayingSequence != null;

    /// <summary>
    /// Replaces the actor reference (used for pooling/reuse scenarios).
    /// </summary>
    public void ResetActor(IActor actor) {
        Actor = actor;
    }

    // --- Abstract/Virtual hooks for subclasses ---

    protected abstract float GetSpeedModifier(AnimationPriority priority);

    protected virtual void OnSequenceChanged(AnimationState? previous, AnimationState? next) { }

    /// <summary>Override to add grace ticks for client-authoritative loop timing.</summary>
    protected virtual bool ShouldEnforceLoopBoundary(long tickCount) => true;

    /// <summary>Override to add grace ticks for client-authoritative sequence end timing.</summary>
    protected virtual bool ShouldEnforceSequenceEnd(long tickCount) => true;

    protected virtual void DebugPrint(string message) { }

    // --- Private implementation ---

    private bool CanPlay(AnimationRequest request) {
        if (Current is null) return true;
        if (request.Priority > Current.Request.Priority) return true;
        if (request.Priority == Current.Request.Priority && request.CanInterruptSelf) return true;
        return false;
    }

    private void PlaySequence(AnimationSequenceMetadata sequenceMetadata, AnimationRequest request) {
        AnimationState? previous = Current;
        Current = new AnimationState(sequenceMetadata, request);

        DebugPrint($"Playing sequence '{sequenceMetadata.Name}' at x{request.Speed} speed, previous: '{previous?.Sequence.Name ?? "none"}' x{previous?.Request.Speed ?? 1f}");

        OnSequenceChanged(previous, Current);

        lastSequenceTime = 0;
        lastTick = Actor.Field.FieldTick;
    }

    private void ResetSequence() {
        AnimationState? previous = Current;
        Current = null;
        lastSequenceTime = 0;
        lastTick = 0;
        if (previous is not null) {
            OnSequenceChanged(previous, null);
        }
    }

    private float AdvanceTime(long tickCount, float speed) {
        long lastServerTick = lastTick == 0 ? tickCount : lastTick;
        float delta = (float) (tickCount - lastServerTick) * speed;
        return lastSequenceTime + delta;
    }

    private List<AnimationKey> CollectKeyframes(float sequenceTime) {
        if (PlayingSequence is null || Current is null) return [];

        var result = new List<AnimationKey>();
        foreach (AnimationKey key in PlayingSequence.Keys) {
            if (HasHitKeyframe(sequenceTime, key)) {
                result.Add(key);
            }
        }
        return result;
    }

    private bool HasHitKeyframe(float sequenceTime, AnimationKey key) {
        bool keyBeforeLoop = !Current!.IsLooping || Current.Loop.end == 0 || key.Time <= Current.Loop.end + 0.001f;
        bool hitKeySinceLastTick = key.Time > lastSequenceTime && key.Time <= sequenceTime;
        return keyBeforeLoop && hitKeySinceLastTick;
    }

    private void DispatchKeyframes(IEnumerable<AnimationKey> keys, float sequenceTime, float speed) {
        foreach (AnimationKey key in keys) {
            if (Current is null) break;

            DebugPrint($"Sequence '{PlayingSequence?.Name}' keyframe event '{key.Name}'");

            switch (key.Name) {
                case "loopstart":
                    Current.Loop = (key.Time, 0);
                    break;
                case "loopend":
                    Current.Loop = (Current.Loop.start, key.Time);
                    Current.LoopEndTick = Actor.Field.FieldTick + (long) ((key.Time - sequenceTime) / speed);
                    Current.Request.Callbacks?.OnLoopEnd?.Invoke();
                    break;
                case "end":
                    Current.EndTime = key.Time + Constant.ClientGraceTimeTick / 1000f;
                    Current.EndTick = Actor.Field.FieldTick + (long) ((key.Time - sequenceTime) / speed) + Constant.ClientGraceTimeTick;
                    break;
                default:
                    DebugPrint($"  → animation event '{key.Name}'");
                    Current.Request.Callbacks?.OnEvent?.Invoke(key.Name);
                    break;
            }
        }
    }

    private float HandleLoopBoundary(float sequenceTime, float speed, long tickCount) {
        if (Current is null || !Current.IsLooping || Current.Loop.end == 0) return sequenceTime;
        if (sequenceTime <= Current.Loop.end) return sequenceTime;
        if (!ShouldEnforceLoopBoundary(tickCount)) return sequenceTime;

        if (Current.LoopOnlyOnce) {
            Current.IsLooping = false;
            Current.LoopOnlyOnce = false;
        }

        float loopDelta = sequenceTime - Current.Loop.end;
        sequenceTime -= Current.Loop.end - Current.Loop.start;

        // Update lastSequenceTime temporarily so CollectKeyframes picks up post-wrap keys
        lastSequenceTime = sequenceTime - Math.Max(loopDelta, sequenceTime - Current.Loop.end + 0.001f);

        List<AnimationKey> postWrapKeys = CollectKeyframes(sequenceTime);
        isDispatchingKeyframes = true;
        DispatchKeyframes(postWrapKeys, sequenceTime, speed);
        isDispatchingKeyframes = false;

        return sequenceTime;
    }

    private void HandleSequenceEnd(float sequenceTime, long tickCount) {
        if (Current is null || Current.EndTime == 0) return;
        if (sequenceTime <= Current.EndTime) return;
        if (!ShouldEnforceSequenceEnd(tickCount)) return;

        // Save callback before reset so it can call TryPlay successfully
        Action? onComplete = Current.Request.Callbacks?.OnComplete;
        ResetSequence();
        onComplete?.Invoke();
    }

    private void FlushDeferredQueue() {
        while (deferredQueue.Count > 0) {
            AnimationRequest request = deferredQueue.Dequeue();
            if (!CanPlay(request)) continue;
            if (!RigMetadata!.Sequences.TryGetValue(request.SequenceName, out AnimationSequenceMetadata? sequence)) continue;
            PlaySequence(sequence, request);
            break; // play highest-priority deferred request, discard rest
        }
        deferredQueue.Clear();
    }

    private void CommitTiming(long tickCount, float sequenceTime) {
        lastTick = tickCount;
        lastSequenceTime = sequenceTime;
    }
}
