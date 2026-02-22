using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Manager;

public sealed class NpcAnimationManager : AnimationManager {
    public AnimationSequenceMetadata IdleSequence { get; }
    public AnimationSequenceMetadata? WalkSequence { get; }
    public AnimationSequenceMetadata? FlySequence { get; }

    public NpcAnimationManager(IActor actor) : base(actor) {
        RigMetadata = actor switch {
            FieldNpc fieldNpc => actor.Field.NpcMetadata.GetAnimation(fieldNpc.Value.Metadata.Model.Name),
            _ => null,
        };

        if (actor is FieldNpc npc) {
            WalkSequence = npc.Value.Animations.GetValueOrDefault("Walk_A");
            FlySequence = npc.Value.Animations.GetValueOrDefault("Fly_A");

            if (RigMetadata is null) {
                IdleSequenceId = 0;
                IdleSequence = npc.Value.Animations.GetValueOrDefault("Idle_A")
                    ?? new AnimationSequenceMetadata(string.Empty, 0, 1f, []);
                return;
            }

            string idleName = npc.Value.Metadata.Action.Actions.FirstOrDefault()?.Name ?? "Idle_A";
            IdleSequenceId = RigMetadata.Sequences.FirstOrDefault(sequence => sequence.Key.Contains(idleName)).Value.Id;
            IdleSequence = npc.Value.Animations.GetValueOrDefault(idleName)
                ?? npc.Value.Animations.GetValueOrDefault("Idle_A")
                ?? new AnimationSequenceMetadata(string.Empty, IdleSequenceId, 1f, []);
        } else {
            if (RigMetadata is null) {
                IdleSequenceId = 0;
                IdleSequence = new AnimationSequenceMetadata(string.Empty, 0, 1f, []);
                return;
            }

            string idleName = "Idle_A";
            IdleSequenceId = RigMetadata.Sequences.FirstOrDefault(sequence => sequence.Key == idleName).Value.Id;
            IdleSequence = new AnimationSequenceMetadata(string.Empty, IdleSequenceId, 1f, []);
        }
    }

    protected override float GetSpeedModifier(AnimationPriority priority) => priority switch {
        AnimationPriority.Move => MoveSpeed,
        AnimationPriority.Skill => AttackSpeed,
        _ => 1f,
    };

    protected override void OnSequenceChanged(AnimationState? previous, AnimationState? next) {
        if (Actor is not FieldNpc npc || previous?.Sequence == next?.Sequence) return;

        npc.SendControl = true;
        npc.AppendDebugMessage($"Anim: {previous?.Sequence.Name ?? "none"} → {next?.Sequence.Name ?? "none"}");
    }
}
