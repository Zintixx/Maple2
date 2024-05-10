using System;

namespace Maple2.Model.Enum;

public enum BuffType {
    None = 0,
    Buff = 1,
    Debuff = 2,
    Debuff2 = 3, // Also a debuff?
}

[Flags]
public enum BuffSubType {
    None = 0,
    Buff = 1, // Used for lots of random things
    Status = 2,
    Damage = 4,
    Motion = 8,
    Recovery = 16,
    Consumable = 32, // Healing, Souvenir, ???
    PcBang = 64,
    Fishing = 128,
    Guild = 256,
    Lapenta = 512,
    Prestige = 1024,
}

public enum BuffCategory {
    None = 0,
    Unknown1 = 1,
    Unknown2 = 2,
    Unknown4 = 4,
    EnemyDot = 6,
    Stunned = 7, // ?
    Slow = 8, // ?
    BossResistance = 9,
    Unknown99 = 99,
    MonsterStunned = 1007, // ?
    Unknown2001 = 2001,
}

public enum BuffEventType {
    None = 0,
    AutoFish = 1,
    SafeRiding = 2,
    AmphibiousRide = 3,
    AutoPerform = 4,
}

public enum BuffKeepCondition {
    TimerDuration = 0, // ?
    SkillDuration = 1, // ?
    TimerDurationTrackCooldown = 5, // ?
    UnlimitedDuration = 99,
}

public enum BuffResetCondition {
    None = 0,
    PersistEndTick = 1, // end tick does not reset
    Reset2 = 2, // behaves the same as Reset ??
    DisableOverwrite = 3, // Prevents overwrite and allows to add another buff
}

public enum BuffDotCondition {
    Instant = 0,
    Delayed = 1,
    Unknown = 2,
}

[Flags]
public enum BuffFlag {
    None = 0,
    UpdateBuff = 1,
    UpdateShield = 2,
}

public enum SpecialEffectType {
    EliteBoss = 2, //  Damage reduction
    WorldBoss = 3, // Double drop rate
    Potion = 4, // Decrease cooldown
    Enchant = 5, // Double defiance crystals on enchanting
    RingCraft = 6, // Double ring crafting
    DailyQuest = 7, // Exp increase
    DailyMission = 8, // Additional reward
    Fishing = 9, // Exp increase
    Performance = 10, // Exp increase
    Pvp = 11, // Valor token reward increase
    CityRestExp = 12, // City rest exp
    GoldChest = 13, // Gold chest added drops
}
