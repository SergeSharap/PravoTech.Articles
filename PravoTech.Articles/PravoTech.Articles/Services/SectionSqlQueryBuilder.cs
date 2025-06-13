using Microsoft.EntityFrameworkCore;
using PravoTech.Articles.Constants;

namespace PravoTech.Articles.Services
{
    public static class SectionSqlQueryBuilder
    {
        public static FormattableString BuildGetSectionsSqlQuery()
        {
            return $@"
                   WITH SectionTagKeys AS (
                       SELECT 
                           s.Id AS SectionId,
                           s.Name,
                           STRING_AGG(t.NormalizedName, {SqlQueryConstants.TagIdSeparator}) WITHIN GROUP (ORDER BY t.NormalizedName) AS RawTagKey
                       FROM Sections s
                       LEFT JOIN SectionTags st ON st.SectionId = s.Id
                       LEFT JOIN Tags t ON t.Id = st.TagId
                       GROUP BY s.Id, s.Name
                   ),
                   
                   SectionTagLists AS (
                       SELECT 
                           s.Id AS SectionId,
                           STRING_AGG(t.Name, {SqlQueryConstants.TagIdSeparator}) WITHIN GROUP (ORDER BY t.Name) AS RawTagList
                       FROM Sections s
                       LEFT JOIN SectionTags st ON st.SectionId = s.Id
                       LEFT JOIN Tags t ON t.Id = st.TagId
                       GROUP BY s.Id
                   ),
                   
                   ArticleTagGroups AS (
                       SELECT 
                           a.Id AS ArticleId,
                           STRING_AGG(t.NormalizedName, {SqlQueryConstants.TagIdSeparator}) WITHIN GROUP (ORDER BY t.NormalizedName) AS TagKey
                       FROM Articles a
                       LEFT JOIN ArticleTags at ON at.ArticleId = a.Id
                       LEFT JOIN Tags t ON t.Id = at.TagId
                       GROUP BY a.Id
                   )
                   
                   SELECT 
                       k.SectionId AS Id,
                       k.Name,
                       ISNULL(l.RawTagList, '') AS TagList,
                       COUNT(atg.ArticleId) AS ArticlesCount
                   FROM SectionTagKeys k
                   LEFT JOIN SectionTagLists l ON l.SectionId = k.SectionId
                   LEFT JOIN ArticleTagGroups atg ON 
                       (k.RawTagKey IS NULL AND atg.TagKey IS NULL) 
                       OR (k.RawTagKey = atg.TagKey)
                   GROUP BY k.SectionId, k.Name, l.RawTagList
                   ORDER BY ArticlesCount DESC;";
        }

        public static FormattableString BuildGetArticlesBySectionSqlQuery(string tagKey)
        {
            if (string.IsNullOrWhiteSpace(tagKey))
            {
                return $@"
                    WITH ArticleTagInfo AS (
                        SELECT 
                            a.Id AS ArticleId,
                            COUNT(at.TagId) AS TagCount
                        FROM Articles a
                        LEFT JOIN ArticleTags at ON at.ArticleId = a.Id
                        GROUP BY a.Id
                    )
                    
                    SELECT 
                        a.Id,
                        a.Title,
                        a.CreatedAt,
                        a.UpdatedAt,
                        NULL AS TagName,
                        NULL AS TagOrder
                    FROM Articles a
                    JOIN ArticleTagInfo ati ON ati.ArticleId = a.Id
                    WHERE ati.TagCount = 0
                    ORDER BY a.EffectiveDate DESC;";
            }
            else
            {
                return $@"
                    WITH ArticleTagKeys AS (
                        SELECT 
                            a.Id AS ArticleId,
                            STRING_AGG(CAST(at.TagId AS VARCHAR), {SqlQueryConstants.TagIdSeparator}) WITHIN GROUP (ORDER BY at.TagId) AS TagKey
                        FROM Articles a
                        LEFT JOIN ArticleTags at ON at.ArticleId = a.Id
                        GROUP BY a.Id
                    )
                    
                    SELECT 
                        a.Id,
                        a.Title,
                        a.CreatedAt,
                        a.UpdatedAt,
                        t.Name AS TagName,
                        at.[Order] AS TagOrder
                    FROM Articles a
                    LEFT JOIN ArticleTagKeys atk ON atk.ArticleId = a.Id
                    INNER JOIN ArticleTags at ON at.ArticleId = a.Id
                    INNER JOIN Tags t ON t.Id = at.TagId
                    WHERE atk.TagKey = {tagKey}
                    ORDER BY a.EffectiveDate DESC;";
            }
        }

        public static FormattableString BuildGetSectionIdByTagsSqlQuery(string? tagKey)
        {
            if (string.IsNullOrEmpty(tagKey))
            {
                return $@"
                    WITH SectionTagKeys AS (
                        SELECT 
                            s.Id AS SectionId,
                            COUNT(st.TagId) AS TagCount
                        FROM Sections s
                        LEFT JOIN SectionTags st ON st.SectionId = s.Id
                        GROUP BY s.Id
                    )
                    SELECT SectionId
                    FROM SectionTagKeys
                    WHERE TagCount = 0";
            }
            else
            {
                return $@"
                    WITH SectionTagKeys AS (
                        SELECT 
                            s.Id AS SectionId,
                            STRING_AGG(CAST(st.TagId AS VARCHAR), {SqlQueryConstants.TagIdSeparator}) WITHIN GROUP (ORDER BY st.TagId) AS TagKey
                        FROM Sections s
                        LEFT JOIN SectionTags st ON st.SectionId = s.Id
                        GROUP BY s.Id
                    )
                    SELECT SectionId
                    FROM SectionTagKeys
                    WHERE TagKey = {tagKey}";
            }
        }

        public static FormattableString BuildGetArticlesExistenceByTagsSqlQuery(string? tagKey)
        {
            return $@"
                WITH OrderedTags AS (
                    SELECT at.ArticleId, CAST(at.TagId AS VARCHAR) AS TagId
                    FROM ArticleTags at
                )
                SELECT CASE 
                    WHEN EXISTS (
                        SELECT 1
                        FROM (
                            SELECT a.Id AS ArticleId,
                                   STRING_AGG(CAST(ot.TagId AS VARCHAR), {SqlQueryConstants.TagIdSeparator}) WITHIN GROUP (ORDER BY ot.TagId) AS TagKey
                            FROM Articles a
                            LEFT JOIN OrderedTags ot ON ot.ArticleId = a.Id
                            GROUP BY a.Id
                        ) AS TagGroups
                        WHERE ISNULL(TagGroups.TagKey, '') = ISNULL({tagKey}, '')
                    )
                    THEN 1 ELSE 0
                END";
        }
    }
}
