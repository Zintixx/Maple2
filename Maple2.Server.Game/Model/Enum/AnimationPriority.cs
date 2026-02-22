namespace Maple2.Server.Game.Model.Enum;

public enum AnimationPriority {
    Idle   = 0, // Idle_A, Regen_A, Emote — no speed modifier
    Move   = 1, // Walk/Run — MoveSpeed modifier
    Skill  = 2, // Skill casts — AttackSpeed modifier
    React  = 3, // Hit stun, knockback (reserved for future)
    Forced = 4, // Cutscene-type, uninterruptible (reserved for future)
}
