using Maple2.Model.Enum;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Game.Model.State;

namespace Maple2.Server.Game.Model.Routine;

public abstract class NpcRoutine {
    public enum Result { Unknown, InProgress, Success, Failure }

    public bool Completed { get; private set; }
    protected FieldNpc Npc;

    // Used for chaining predetermined routines.
    public Func<NpcRoutine>? NextRoutine { get; init; }

    protected NpcRoutine(FieldNpc npc) {
        Npc = npc;
    }

    /// <summary>
    /// Attempts to play an animation via AnimationManager and claims it in MovementState
    /// so the watchdog does not immediately override it.
    /// </summary>
    protected bool PlayAnimation(AnimationRequest request) {
        bool played = Npc.Animation.TryPlay(request);
        if (played) {
            Npc.MovementState.ClaimAnimation();
        }
        return played;
    }

    /// <summary>
    /// Synchronizes an NpcRoutine.
    /// </summary>
    /// <returns>bool representing whether this routine is completed</returns>
    public abstract Result Update(TimeSpan elapsed);

    public virtual void OnCompleted() {
        if (Completed) {
            return;
        }

        Completed = true;
        PlayAnimation(new AnimationRequest {
            SequenceName = Npc.Animation.IdleSequence.Name,
            Priority = AnimationPriority.Idle,
            CanInterruptSelf = true,
        });
        if (Npc.State.State != ActorState.Idle) {
            Npc.State = new NpcState();
        }
    }
}
