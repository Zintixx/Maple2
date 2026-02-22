using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.ActorStateComponent;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Manager;

public sealed class PlayerAnimationManager : AnimationManager {
    private readonly GameSession session;

    public PlayerAnimationManager(GameSession session) : base(session.Player) {
        this.session = session;
        string model = session.Player.Value.Character.Gender == Gender.Male ? "male" : "female";
        RigMetadata = session.NpcMetadata.GetAnimation(model);

        if (RigMetadata is null) {
            throw new Exception("Failed to initialize PlayerAnimationManager, could not find metadata for player model " + model);
        }

        IdleSequenceId = RigMetadata.Sequences.FirstOrDefault(sequence => sequence.Key == "Idle_A").Value.Id;
    }

    /// <summary>
    /// Client-reported loop confirmation — equivalent to SetLooping(true, true).
    /// </summary>
    public void ConfirmClientLoop() {
        if (Current is null) return;
        Current.IsLooping = true;
        Current.LoopOnlyOnce = true;
    }

    protected override float GetSpeedModifier(AnimationPriority priority) => 1f; // client drives speed

    // Player timing is client-authoritative; use grace window
    protected override bool ShouldEnforceLoopBoundary(long tickCount) =>
        tickCount <= (Current?.LoopEndTick ?? 0) + Constant.ClientGraceTimeTick;

    protected override bool ShouldEnforceSequenceEnd(long tickCount) =>
        tickCount <= (Current?.EndTick ?? 0);

    protected override void DebugPrint(string message) {
        if (DebugPrintAnimations) {
            session.Send(NoticePacket.Message(message));
        }
    }
}
