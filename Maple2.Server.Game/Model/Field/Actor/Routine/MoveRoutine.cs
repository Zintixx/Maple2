using System.Numerics;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Model.Enum;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Model.Routine;

public class MoveRoutine : NpcRoutine {
    private const float MIN_DISTANCE = 25f;

    public static NpcRoutine Walk(FieldNpc npc, short sequenceId) {
        string name = npc.Animation.RigMetadata?.Sequences
            .FirstOrDefault(kvp => kvp.Value.Id == sequenceId).Key
            ?? npc.Animation.WalkSequence?.Name ?? string.Empty;
        try {
            return new MoveRoutine(npc, name, npc.Value.Metadata.Action.WalkSpeed);
        } catch (ArgumentException) {
            return new WaitRoutine(npc, npc.Animation.IdleSequence.Time);
        }
    }

    public static NpcRoutine Run(FieldNpc npc, short sequenceId) {
        string name = npc.Animation.RigMetadata?.Sequences
            .FirstOrDefault(kvp => kvp.Value.Id == sequenceId).Key
            ?? npc.Animation.WalkSequence?.Name ?? string.Empty;
        try {
            return new MoveRoutine(npc, name, npc.Value.Metadata.Action.RunSpeed);
        } catch (ArgumentException) {
            return new WaitRoutine(npc, npc.Animation.IdleSequence.Time);
        }
    }

    private readonly float speed;
    private TimeSpan segmentTime;

    private MoveRoutine(FieldNpc npc, string sequenceName, float speed) : base(npc) {
        if (speed <= 0 || string.IsNullOrEmpty(sequenceName)) {
            throw new ArgumentException($"Npc {npc.Value.Id} is not eligible for movement (speed={speed}, sequence='{sequenceName}')");
        }
        if (npc.Navigation is not null && !npc.Navigation.HasPath) {
            throw new ArgumentException($"Npc {npc.Value.Id} has no path for movement");
        }

        this.speed = speed;
        npc.Velocity = speed * Vector3.UnitY;

        AnimationCallbacks? callbacks = null;
        callbacks = new AnimationCallbacks {
            OnComplete = () => {
                PlayAnimation(new AnimationRequest {
                    SequenceName = sequenceName,
                    Priority = AnimationPriority.Move,
                    CanInterruptSelf = true,
                    Callbacks = callbacks,
                });
            },
        };
        PlayAnimation(new AnimationRequest {
            SequenceName = sequenceName,
            Priority = AnimationPriority.Move,
            CanInterruptSelf = true,
            Callbacks = callbacks,
        });
        segmentTime = TimeSpan.Zero;
    }

    public override Result Update(TimeSpan elapsed) {
        if (Npc.Navigation is null) {
            return Result.Failure;
        }

        if (segmentTime.Ticks > 0) {
            Npc.Position = Npc.Position.Offset((float) elapsed.TotalSeconds * speed, Npc.Rotation);

            segmentTime -= elapsed;
            if (segmentTime.Ticks > 0) {
                return Result.InProgress;
            }
        }

        // Compute the next segment for movement (time = 1s)
        (Vector3 start, Vector3 end) = Npc.Navigation.Advance(TimeSpan.FromSeconds(1), speed, out bool followSegment);

        // If target is close enough, no movement is necessary.
        float distance = Vector2.Distance(new Vector2(start.X, start.Y), new Vector2(end.X, end.Y));
        if (distance > MIN_DISTANCE) {
            segmentTime = TimeSpan.FromSeconds(distance / speed);
            Npc.Rotation = start.Angle2D(end) * Vector3.UnitZ;

            return Result.InProgress;
        }

        OnCompleted();
        return Result.Success;
    }

    public override void OnCompleted() {
        if (Completed) {
            return;
        }

        base.OnCompleted();
        segmentTime = TimeSpan.Zero;
        Npc.Velocity = default;
        Npc.Navigation?.UpdatePosition();
        Npc.ClearPatrolData();
    }
}
