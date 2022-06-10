﻿using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Extensions;

internal static class PropertyBuilderExtensions {
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions();

    static PropertyBuilderExtensions() {
        Options.Converters.Add(new Vector3Converter());
    }

    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder) {
        return builder.HasConversion(
            property => JsonSerializer.Serialize(property, Options),
            value => JsonSerializer.Deserialize<TProperty>(value, Options)!
        ).HasColumnType("json");
    }

    public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> OneToOne<TEntity, TRelatedEntity>(
            this EntityTypeBuilder<TEntity> builder) where TEntity : class where TRelatedEntity : class {
        return builder.HasOne<TRelatedEntity>().WithOne();
    }
}
