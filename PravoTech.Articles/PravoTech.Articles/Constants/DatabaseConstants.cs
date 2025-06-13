namespace PravoTech.Articles.Constants;

/// <summary>
/// Contains database-related constants used throughout the application
/// </summary>
public static class DatabaseConstants
{
    /// <summary>
    /// SQL Server function for generating sequential GUIDs
    /// </summary>
    public const string NewSequentialIdFunction = "NEWSEQUENTIALID()";

    /// <summary>
    /// SQL Server function for computing effective date
    /// </summary>
    public const string EffectiveDateComputation = "ISNULL([UpdatedAt], [CreatedAt])";

    /// <summary>
    /// Name of the index for Articles.EffectiveDate
    /// </summary>
    public const string ArticlesEffectiveDateIndex = "IX_Articles_EffectiveDate";

    /// <summary>
    /// Name of the unique index for Tags.NormalizedName
    /// </summary>
    public const string TagsNormalizedNameIndex = "IX_Tags_NormalizedName";
} 