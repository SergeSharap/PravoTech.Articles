namespace PravoTech.Articles.Constants;

/// <summary>
/// Contains business-related constants used throughout the application
/// </summary>
public static class BusinessConstants
{
    /// <summary>
    /// Maximum number of tags allowed per article
    /// </summary>
    public const int MaxTagsPerArticle = 256;

    /// <summary>
    /// Error message for exceeding maximum number of tags
    /// </summary>
    public const string TooManyTagsErrorMessage = "Too many tags (max 256)";

    /// <summary>
    /// Error message for article not found
    /// </summary>
    public const string ArticleNotFoundErrorMessage = "Article not found";

    /// <summary>
    /// Error message template for tag not found
    /// </summary>
    public const string TagNotFoundErrorMessageTemplate = "Tag '{0}' not found in allTags.";

    /// <summary>
    /// Default page size for pagination
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Maximum page size for pagination
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Minimum page size for pagination
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// Minimum page number for pagination
    /// </summary>
    public const int MinPageNumber = 1;
} 