namespace Maple2.Server.Game.Model.Routine;

public class WaitRoutine : NpcRoutine {
    private TimeSpan duration;

    public WaitRoutine(FieldNpc npc, float duration) : base(npc) {
        this.duration = TimeSpan.FromSeconds(duration);
    }

    public override Result Update(TimeSpan elapsed) {
        duration -= elapsed;
        if (duration.Ticks > 0) {
            return Result.InProgress;
        }

        OnCompleted();
        return Result.Success;
    }
}
