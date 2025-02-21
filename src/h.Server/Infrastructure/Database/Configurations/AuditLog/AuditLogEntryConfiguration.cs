using h.Server.Entities.AuditLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace h.Server.Infrastructure.Database.Configurations.AuditLog;

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("current_timestamp")
            .IsRequired();

        builder.Property(e => e.Message)
            .IsRequired();
        
        builder.Property(e => e.Format)
            .IsRequired();

        builder.Property(e => e.Arguments)
            .HasConversion(new ArgumentsValueConverter())
            .IsRequired();

        builder.Property(e => e.IPAdressV4)
            .IsRequired();
    }
}

file class ArgumentsValueConverter : ValueConverter<object?[], string>
{
    public ArgumentsValueConverter() 
        : base(
            v => Serialize(v),
            v => Deserialize(v))
    { }

    private static string Serialize(object?[]? value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions { 
            WriteIndented = false,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        });
    }

    private static object?[] Deserialize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return Array.Empty<object?>();

        return JsonSerializer.Deserialize<object?[]>(value, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        }) ?? Array.Empty<object?>();
    }
}