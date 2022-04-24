﻿using System;
using Maple2.Database.Extensions;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Character {
    public DateTime LastModified { get; set; }

    public long Id { get; set; }
    public long AccountId { get; set; }
    public string Name { get; set; }

    public DateTime CreationTime { get; set; }
    public Gender Gender { get; set; }
    public Job Job { get; set; }
    public short Level { get; set; }
    public SkinColor SkinColor { get; set; }
    public long Experience { get; set; }
    public long RestExp { get; set; }
    public int MapId { get; set; }
    public int Title { get; set; }
    public short Insignia { get; set; }

    public static implicit operator Character(Maple2.Model.Game.Character other) {
        return other == null ? null : new Character {
            LastModified = other.LastModified,
            Id = other.Id,
            AccountId = other.AccountId,
            Name = other.Name,
            CreationTime = other.CreationTime,
            Gender = other.Gender,
            Job = other.Job,
            Level = other.Level,
            SkinColor = other.SkinColor,
            Experience = other.Experience,
            RestExp = other.RestExp,
            MapId = other.MapId,
            Title = other.Title,
            Insignia = other.Insignia,
        };
    }

    public static implicit operator Maple2.Model.Game.Character(Character other) {
        return other == null ? null : new Maple2.Model.Game.Character {
            LastModified = other.LastModified,
            Id = other.Id,
            AccountId = other.AccountId,
            Name = other.Name,
            CreationTime = other.CreationTime,
            Gender = other.Gender,
            Job = other.Job,
            Level = other.Level,
            SkinColor = other.SkinColor,
            Experience = other.Experience,
            RestExp = other.RestExp,
            MapId = other.MapId,
            Title = other.Title,
            Insignia = other.Insignia,
        };
    }

    public static void Configure(EntityTypeBuilder<Character> builder) {
        builder.Property(character => character.LastModified).IsRowVersion();
        builder.HasKey(character => character.Id);
        builder.HasOne<Account>()
            .WithMany(account => account.Characters)
            .HasForeignKey(character => character.AccountId);
        builder.HasIndex(character => character.Name).IsUnique();
        builder.Property(character => character.CreationTime)
            .ValueGeneratedOnAdd();
        builder.Property(character => character.SkinColor).HasJsonConversion();
    }
}