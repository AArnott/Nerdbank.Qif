// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// A class, that may be used to help describe a transaction.
/// </summary>
/// <param name="Name">The name of the class.</param>
public record Class(string Name)
{
    /// <summary>
    /// Gets the description of this class.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Deserializes a <see cref="Class"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    public static Class Load(QifReader reader)
    {
        string? name = null;
        string? description = null;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(FieldNames.Name, fieldName))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Description, fieldName))
            {
                description = reader.ReadFieldAsString();
            }
        }

        return new(ValueOrThrow(name, FieldNames.Name))
        {
            Description = description,
        };
    }

    /// <summary>
    /// Saves this record in QIF format.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    public void Save(QifWriter writer)
    {
        writer.WriteField(FieldNames.Name, this.Name);
        writer.WriteFieldIfNotEmpty(FieldNames.Description, this.Description);
        writer.WriteEndOfRecord();
    }

    private static class FieldNames
    {
        internal const string Name = "N";
        internal const string Description = "D";
    }
}
