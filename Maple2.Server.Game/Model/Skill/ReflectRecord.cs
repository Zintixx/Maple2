using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class ReflectRecord {
    public readonly int SourceBuffObjectId;
    public readonly AdditionalEffectMetadataReflect Metadata;

    public int Counter = 0;

    public ReflectRecord(int id, AdditionalEffectMetadataReflect metadata) {
        SourceBuffObjectId = id;
        Metadata = metadata;
    }
}
