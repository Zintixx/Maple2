﻿using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Tools.VectorMath;

namespace Maple2.Model.Metadata.FieldEntity;

public enum FieldEntityType : byte {
    Unknown,
    Vibrate,
    SpawnTile, // for mob spawns
    BoxCollider, // for cube tiles (IsWhiteBox = false) & arbitrarily sized white boxes
    MeshCollider,
    Fluid,
    SellableTile,
    Cell, // for cells culled from the grid & demoted to AABB tree
}

public record FieldEntityId(
    ulong High,
    ulong Low) {
    public bool IsNull { get => High == 0 && Low == 0; }

    public static FieldEntityId FromString(string id) {
        ulong High = Convert.ToUInt64(id.Substring(0, 16), 16);
        ulong Low = Convert.ToUInt64(id.Substring(16, 16), 16);

        return new FieldEntityId(High, Low);
    }
}

public record FieldEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds);

public record FieldVibrateEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    int VibrateIndex) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldSpawnTile(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldBoxColliderEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    Vector3 Size,
    bool IsWhiteBox,
    bool IsFluid) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldMeshColliderEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    uint MeshLlid) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldFluidEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    LiquidType LiquidType,
    BoundingBox3 Bounds,
    uint MeshLlid,
    bool IsShallow,
    bool IsSurface) : FieldMeshColliderEntity(Id, Position, Rotation, Scale, Bounds, MeshLlid);

public record FieldCellEntities(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    List<FieldEntity> Entities) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldSellableTile(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    int SellableGroup
) : FieldEntity(Id, Position, Rotation, Scale, Bounds);
