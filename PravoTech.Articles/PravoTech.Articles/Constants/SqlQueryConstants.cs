namespace PravoTech.Articles.Constants;

/// <summary>
/// Contains SQL query related constants used throughout the application
/// </summary>
public static class SqlQueryConstants
{
    /// <summary>
    /// Default section name when no tags are present
    /// </summary>
    public const string NoTagsSectionName = "No Tags";

    /// <summary>
    /// Maximum length for section name
    /// </summary>
    public const int MaxSectionNameLength = 1024;

    /// <summary>
    /// Separator used for joining tag IDs
    /// </summary>
    public const string TagIdSeparator = ",";

    /// <summary>
    /// Separator used for joining tag names in section name
    /// </summary>
    public const string TagNameSeparator = ", ";
} 