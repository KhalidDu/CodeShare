using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Data.Sqlite;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 代码片段仓储实现 - 使用 Dapper ORM
/// </summary>
public class CodeSnippetRepository : ICodeSnippetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly bool _isSqlite;

    public CodeSnippetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _isSqlite = connectionFactory.GetType().Name.Contains("Sqlite");
    }

    /// <summary>
    /// 处理数据库类型转换 - SQLite字符串转Guid
    /// </summary>
    private object HandleIdConversion(Guid id)
    {
        return _isSqlite ? id.ToString() : id;
    }

    /// <summary>
    /// 处理查询结果类型转换 - SQLite字符串转Guid
    /// </summary>
    private void HandleIdConversion(CodeSnippet snippet)
    {
        if (_isSqlite)
        {
            // SQLite中ID已经是Guid类型，无需转换
            return;
        }
    }

    /// <summary>
    /// 处理SQLite日期时间转换
    /// </summary>
    private DateTime ParseDateTime(object dateTimeValue)
    {
        if (_isSqlite)
        {
            // SQLite返回的是字符串格式的日期时间
            if (dateTimeValue is string dateString)
            {
                return DateTime.Parse(dateString);
            }
        }
        
        // MySQL直接返回DateTime类型
        return Convert.ToDateTime(dateTimeValue);
    }

    private bool ParseBool(object boolValue)
    {
        if (_isSqlite)
        {
            // SQLite中布尔值可能以整数形式存储 (0 或 1)
            if (boolValue is long longValue)
            {
                return longValue != 0;
            }
            if (boolValue is int intValue)
            {
                return intValue != 0;
            }
        }
        
        // MySQL直接返回bool类型
        return Convert.ToBoolean(boolValue);
    }

    private int ParseInt(object intValue)
    {
        if (_isSqlite)
        {
            // SQLite中整数可能以long形式返回
            if (intValue is long longValue)
            {
                return (int)longValue;
            }
        }
        
        // MySQL直接返回int类型
        return Convert.ToInt32(intValue);
    }

    public async Task<CodeSnippet?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT cs.*, u.Username as CreatorName 
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                WHERE cs.Id = @Id";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id.ToString() });
            
            if (result == null)
                return null;
                
            return new CodeSnippet
            {
                Id = Guid.Parse(result.Id),
                Title = result.Title,
                Description = result.Description,
                Code = result.Code,
                Language = result.Language,
                CreatedBy = Guid.Parse(result.CreatedBy),
                CreatedAt = ParseDateTime(result.CreatedAt),
                UpdatedAt = ParseDateTime(result.UpdatedAt),
                IsPublic = ParseBool(result.IsPublic),
                ViewCount = ParseInt(result.ViewCount),
                CopyCount = ParseInt(result.CopyCount),
                CreatorName = result.CreatorName,
                Tags = new List<Tag>()
            };
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT cs.*, u.Username as CreatorName 
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                WHERE cs.Id = @Id";
            
            return await connection.QuerySingleOrDefaultAsync<CodeSnippet>(sql, new { Id = id });
        }
    }

    /// <summary>
    /// 获取代码片段详情，包含标签信息 - 使用 Dapper Multi-mapping
    /// </summary>
    public async Task<CodeSnippet?> GetByIdWithTagsAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理字符串ID
            const string sql = @"
                SELECT cs.Id, cs.Title, cs.Description, cs.Code, cs.Language, cs.CreatedBy, cs.CreatedAt, cs.UpdatedAt, cs.IsPublic, cs.ViewCount, cs.CopyCount,
                       u.Username as CreatorName,
                       t.Id as TagId, t.Name as TagName, t.Color as TagColor, t.CreatedBy as TagCreatedBy, t.CreatedAt as TagCreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                WHERE cs.Id = @Id";

            var result = await connection.QueryAsync<dynamic>(sql, new { Id = id.ToString() });
            
            if (!result.Any())
                return null;

            var firstRow = result.First();
            var snippet = new CodeSnippet
            {
                Id = Guid.Parse(firstRow.Id),
                Title = firstRow.Title,
                Description = firstRow.Description,
                Code = firstRow.Code,
                Language = firstRow.Language,
                CreatedBy = Guid.Parse(firstRow.CreatedBy),
                CreatedAt = ParseDateTime(firstRow.CreatedAt),
                UpdatedAt = ParseDateTime(firstRow.UpdatedAt),
                IsPublic = ParseBool(firstRow.IsPublic),
                ViewCount = ParseInt(firstRow.ViewCount),
                CopyCount = ParseInt(firstRow.CopyCount),
                CreatorName = firstRow.CreatorName,
                Tags = new List<Tag>()
            };

            // 处理标签
            foreach (var row in result)
            {
                if (row.TagId != null)
                {
                    snippet.Tags.Add(new Tag
                    {
                        Id = Guid.Parse(row.TagId),
                        Name = row.TagName,
                        Color = row.TagColor,
                        CreatedBy = Guid.Parse(row.TagCreatedBy),
                        CreatedAt = ParseDateTime(row.TagCreatedAt)
                    });
                }
            }

            return snippet;
        }
        else
        {
            // MySQL查询 - 使用Dapper Multi-mapping
            const string sql = @"
                SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                WHERE cs.Id = @Id";

            var snippetDict = new Dictionary<Guid, CodeSnippet>();
            
            var queryResult = await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
                sql,
                (snippet, tag) =>
                {
                    if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                    {
                        existingSnippet = snippet;
                        existingSnippet.Tags = new List<Tag>();
                        snippetDict.Add(snippet.Id, existingSnippet);
                    }

                    if (tag != null)
                    {
                        existingSnippet.Tags.Add(tag);
                    }

                    return existingSnippet;
                },
                new { Id = id },
                splitOn: "Id"
            );

            return queryResult.FirstOrDefault();
        }
    }

    public async Task<PaginatedResult<CodeSnippet>> GetPagedAsync(SnippetFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(filter.Search))
        {
            whereClause += " AND (cs.Title LIKE @Search OR cs.Description LIKE @Search OR cs.Code LIKE @Search)";
            parameters.Add("Search", $"%{filter.Search}%");
        }
        
        if (!string.IsNullOrEmpty(filter.Language))
        {
            whereClause += " AND cs.Language = @Language";
            parameters.Add("Language", filter.Language);
        }
        
        if (!string.IsNullOrEmpty(filter.Tag))
        {
            whereClause += " AND EXISTS (SELECT 1 FROM SnippetTags st INNER JOIN Tags t ON st.TagId = t.Id WHERE st.SnippetId = cs.Id AND t.Name = @Tag)";
            parameters.Add("Tag", filter.Tag);
        }
        
        if (filter.CreatedBy.HasValue)
        {
            whereClause += " AND cs.CreatedBy = @CreatedBy";
            parameters.Add("CreatedBy", filter.CreatedBy.Value);
        }

        // 获取总数
        var countSql = $"SELECT COUNT(DISTINCT cs.Id) FROM CodeSnippets cs {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // 获取分页数据
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        if (_isSqlite)
        {
            // SQLite专用查询 - 处理字符串ID
            var dataSql = $@"
                SELECT cs.Id, cs.Title, cs.Description, cs.Code, cs.Language, cs.CreatedBy, cs.CreatedAt, cs.UpdatedAt, cs.IsPublic, cs.ViewCount, cs.CopyCount,
                       u.Username as CreatorName,
                       t.Id as TagId, t.Name as TagName, t.Color as TagColor, t.CreatedBy as TagCreatedBy, t.CreatedAt as TagCreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                {whereClause}
                ORDER BY cs.CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            var result = await connection.QueryAsync<dynamic>(dataSql, parameters);
            
            var snippetDict = new Dictionary<Guid, CodeSnippet>();
            
            foreach (var row in result)
            {
                var snippetId = Guid.Parse(row.Id);
                
                if (!snippetDict.TryGetValue(snippetId, out CodeSnippet? existingSnippet))
                {
                    existingSnippet = new CodeSnippet
                    {
                        Id = snippetId,
                        Title = row.Title,
                        Description = row.Description,
                        Code = row.Code,
                        Language = row.Language,
                        CreatedBy = Guid.Parse(row.CreatedBy),
                        CreatedAt = ParseDateTime(row.CreatedAt),
                        UpdatedAt = ParseDateTime(row.UpdatedAt),
                        IsPublic = Convert.ToBoolean(row.IsPublic),
                        ViewCount = Convert.ToInt32(row.ViewCount),
                        CopyCount = Convert.ToInt32(row.CopyCount),
                        CreatorName = row.CreatorName,
                        Tags = new List<Tag>()
                    };
                    snippetDict.Add(snippetId, existingSnippet);
                }

                // 处理标签
                if (row.TagId != null)
                {
                    var tagId = Guid.Parse(row.TagId);
                    if (!existingSnippet.Tags.Any(t => t.Id == tagId))
                    {
                        existingSnippet.Tags.Add(new Tag
                        {
                            Id = tagId,
                            Name = row.TagName,
                            Color = row.TagColor,
                            CreatedBy = Guid.Parse(row.TagCreatedBy),
                            CreatedAt = ParseDateTime(row.TagCreatedAt)
                        });
                    }
                }
            }

            return new PaginatedResult<CodeSnippet>
            {
                Items = snippetDict.Values,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        else
        {
            // MySQL查询 - 使用Dapper Multi-mapping
            var dataSql = $@"
                SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                {whereClause}
                ORDER BY cs.CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            var snippetDict = new Dictionary<Guid, CodeSnippet>();
            
            await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
                dataSql,
                (snippet, tag) =>
                {
                    if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                    {
                        existingSnippet = snippet;
                        existingSnippet.Tags = new List<Tag>();
                        snippetDict.Add(snippet.Id, existingSnippet);
                    }

                    if (tag != null && !existingSnippet.Tags.Any(t => t.Id == tag.Id))
                    {
                        existingSnippet.Tags.Add(tag);
                    }

                    return existingSnippet;
                },
                parameters,
                splitOn: "Id"
            );

            return new PaginatedResult<CodeSnippet>
            {
                Items = snippetDict.Values,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }

    public async Task<CodeSnippet> CreateAsync(CodeSnippet snippet)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO CodeSnippets (Id, Title, Description, Code, Language, CreatedBy, CreatedAt, UpdatedAt, IsPublic, ViewCount, CopyCount)
            VALUES (@Id, @Title, @Description, @Code, @Language, @CreatedBy, @CreatedAt, @UpdatedAt, @IsPublic, @ViewCount, @CopyCount)";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)snippet.Id.ToString() : snippet.Id,
            snippet.Title,
            snippet.Description,
            snippet.Code,
            snippet.Language,
            CreatedBy = _isSqlite ? (object)snippet.CreatedBy.ToString() : snippet.CreatedBy,
            snippet.CreatedAt,
            snippet.UpdatedAt,
            snippet.IsPublic,
            snippet.ViewCount,
            snippet.CopyCount
        };
        
        await connection.ExecuteAsync(sql, parameters);
        return snippet;
    }

    public async Task<CodeSnippet> UpdateAsync(CodeSnippet snippet)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CodeSnippets 
            SET Title = @Title, Description = @Description, Code = @Code, 
                Language = @Language, UpdatedAt = @UpdatedAt, IsPublic = @IsPublic
            WHERE Id = @Id";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)snippet.Id.ToString() : snippet.Id,
            snippet.Title,
            snippet.Description,
            snippet.Code,
            snippet.Language,
            snippet.UpdatedAt,
            snippet.IsPublic
        };
        
        await connection.ExecuteAsync(sql, parameters);
        return snippet;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM CodeSnippets WHERE Id = @Id";
        
        var idParam = _isSqlite ? (object)id.ToString() : id;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = idParam });
        return rowsAffected > 0;
    }

    /// <summary>
    /// 根据用户ID获取代码片段，包含标签信息 - 使用 Dapper Multi-mapping
    /// </summary>
    public async Task<IEnumerable<CodeSnippet>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理字符串ID
            const string sql = @"
                SELECT cs.Id, cs.Title, cs.Description, cs.Code, cs.Language, cs.CreatedBy, cs.CreatedAt, cs.UpdatedAt, cs.IsPublic, cs.ViewCount, cs.CopyCount,
                       u.Username as CreatorName,
                       t.Id as TagId, t.Name as TagName, t.Color as TagColor, t.CreatedBy as TagCreatedBy, t.CreatedAt as TagCreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                WHERE cs.CreatedBy = @UserId
                ORDER BY cs.CreatedAt DESC";

            var result = await connection.QueryAsync<dynamic>(sql, new { UserId = userId.ToString() });
            
            var snippetDict = new Dictionary<Guid, CodeSnippet>();
            
            foreach (var row in result)
            {
                var snippetId = Guid.Parse(row.Id);
                
                if (!snippetDict.TryGetValue(snippetId, out CodeSnippet? existingSnippet))
                {
                    existingSnippet = new CodeSnippet
                    {
                        Id = snippetId,
                        Title = row.Title,
                        Description = row.Description,
                        Code = row.Code,
                        Language = row.Language,
                        CreatedBy = Guid.Parse(row.CreatedBy),
                        CreatedAt = ParseDateTime(row.CreatedAt),
                        UpdatedAt = ParseDateTime(row.UpdatedAt),
                        IsPublic = Convert.ToBoolean(row.IsPublic),
                        ViewCount = Convert.ToInt32(row.ViewCount),
                        CopyCount = Convert.ToInt32(row.CopyCount),
                        CreatorName = row.CreatorName,
                        Tags = new List<Tag>()
                    };
                    snippetDict.Add(snippetId, existingSnippet);
                }

                // 处理标签
                if (row.TagId != null)
                {
                    var tagId = Guid.Parse(row.TagId);
                    if (!existingSnippet.Tags.Any(t => t.Id == tagId))
                    {
                        existingSnippet.Tags.Add(new Tag
                        {
                            Id = tagId,
                            Name = row.TagName,
                            Color = row.TagColor,
                            CreatedBy = Guid.Parse(row.TagCreatedBy),
                            CreatedAt = ParseDateTime(row.TagCreatedAt)
                        });
                    }
                }
            }
            
            return snippetDict.Values;
        }
        else
        {
            // MySQL查询 - 使用Dapper Multi-mapping
            const string sql = @"
                SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                WHERE cs.CreatedBy = @UserId
                ORDER BY cs.CreatedAt DESC";

            var snippetDict = new Dictionary<Guid, CodeSnippet>();
            
            await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
                sql,
                (snippet, tag) =>
                {
                    if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                    {
                        existingSnippet = snippet;
                        existingSnippet.Tags = new List<Tag>();
                        snippetDict.Add(snippet.Id, existingSnippet);
                    }

                    if (tag != null && !existingSnippet.Tags.Any(t => t.Id == tag.Id))
                    {
                        existingSnippet.Tags.Add(tag);
                    }

                    return existingSnippet;
                },
                new { UserId = userId },
                splitOn: "Id"
            );
            
            return snippetDict.Values;
        }
    }

    /// <summary>
    /// 根据标签获取代码片段，包含标签信息 - 使用 Dapper Multi-mapping
    /// </summary>
    public async Task<IEnumerable<CodeSnippet>> GetByTagAsync(string tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理字符串ID
            const string sql = @"
                SELECT cs.Id, cs.Title, cs.Description, cs.Code, cs.Language, cs.CreatedBy, cs.CreatedAt, cs.UpdatedAt, cs.IsPublic, cs.ViewCount, cs.CopyCount,
                       u.Username as CreatorName,
                       t.Id as TagId, t.Name as TagName, t.Color as TagColor, t.CreatedBy as TagCreatedBy, t.CreatedAt as TagCreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                WHERE EXISTS (
                    SELECT 1 FROM SnippetTags st2 
                    INNER JOIN Tags t2 ON st2.TagId = t2.Id 
                    WHERE st2.SnippetId = cs.Id AND t2.Name = @Tag
                )
                ORDER BY cs.CreatedAt DESC";

            var result = await connection.QueryAsync<dynamic>(sql, new { Tag = tag });
            
            var snippetDict = new Dictionary<Guid, CodeSnippet>();
            
            foreach (var row in result)
            {
                var snippetId = Guid.Parse(row.Id);
                
                if (!snippetDict.TryGetValue(snippetId, out CodeSnippet? existingSnippet))
                {
                    existingSnippet = new CodeSnippet
                    {
                        Id = snippetId,
                        Title = row.Title,
                        Description = row.Description,
                        Code = row.Code,
                        Language = row.Language,
                        CreatedBy = Guid.Parse(row.CreatedBy),
                        CreatedAt = ParseDateTime(row.CreatedAt),
                        UpdatedAt = ParseDateTime(row.UpdatedAt),
                        IsPublic = Convert.ToBoolean(row.IsPublic),
                        ViewCount = Convert.ToInt32(row.ViewCount),
                        CopyCount = Convert.ToInt32(row.CopyCount),
                        CreatorName = row.CreatorName,
                        Tags = new List<Tag>()
                    };
                    snippetDict.Add(snippetId, existingSnippet);
                }

                // 处理标签
                if (row.TagId != null)
                {
                    var tagId = Guid.Parse(row.TagId);
                    if (!existingSnippet.Tags.Any(t => t.Id == tagId))
                    {
                        existingSnippet.Tags.Add(new Tag
                        {
                            Id = tagId,
                            Name = row.TagName,
                            Color = row.TagColor,
                            CreatedBy = Guid.Parse(row.TagCreatedBy),
                            CreatedAt = ParseDateTime(row.TagCreatedAt)
                        });
                    }
                }
            }
            
            return snippetDict.Values;
        }
        else
        {
            // MySQL查询 - 使用Dapper Multi-mapping
            const string sql = @"
                SELECT cs.*, u.Username as CreatorName, t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
                FROM CodeSnippets cs 
                LEFT JOIN Users u ON cs.CreatedBy = u.Id 
                LEFT JOIN SnippetTags st ON cs.Id = st.SnippetId
                LEFT JOIN Tags t ON st.TagId = t.Id
                WHERE EXISTS (
                    SELECT 1 FROM SnippetTags st2 
                    INNER JOIN Tags t2 ON st2.TagId = t2.Id 
                    WHERE st2.SnippetId = cs.Id AND t2.Name = @Tag
                )
                ORDER BY cs.CreatedAt DESC";

            var snippetDict = new Dictionary<Guid, CodeSnippet>();
            
            await connection.QueryAsync<CodeSnippet, Tag, CodeSnippet>(
                sql,
                (snippet, tag) =>
                {
                    if (!snippetDict.TryGetValue(snippet.Id, out var existingSnippet))
                    {
                        existingSnippet = snippet;
                        existingSnippet.Tags = new List<Tag>();
                        snippetDict.Add(snippet.Id, existingSnippet);
                    }

                    if (tag != null && !existingSnippet.Tags.Any(t => t.Id == tag.Id))
                    {
                        existingSnippet.Tags.Add(tag);
                    }

                    return existingSnippet;
                },
                new { Tag = tag },
                splitOn: "Id"
            );
            
            return snippetDict.Values;
        }
    }

    /// <summary>
    /// 为代码片段添加标签
    /// </summary>
    public async Task<bool> AddTagToSnippetAsync(Guid snippetId, Guid tagId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT IGNORE INTO SnippetTags (SnippetId, TagId)
            VALUES (@SnippetId, @TagId)";
        
        var parameters = new
        {
            SnippetId = _isSqlite ? (object)snippetId.ToString() : snippetId,
            TagId = _isSqlite ? (object)tagId.ToString() : tagId
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    /// <summary>
    /// 从代码片段移除标签
    /// </summary>
    public async Task<bool> RemoveTagFromSnippetAsync(Guid snippetId, Guid tagId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SnippetTags WHERE SnippetId = @SnippetId AND TagId = @TagId";
        
        var parameters = new
        {
            SnippetId = _isSqlite ? (object)snippetId.ToString() : snippetId,
            TagId = _isSqlite ? (object)tagId.ToString() : tagId
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    /// <summary>
    /// 更新代码片段的所有标签 - 使用事务确保数据一致性
    /// </summary>
    public async Task<bool> UpdateSnippetTagsAsync(Guid snippetId, IEnumerable<Guid> tagIds)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // 删除现有标签关联
            const string deleteSql = "DELETE FROM SnippetTags WHERE SnippetId = @SnippetId";
            var snippetIdParam = _isSqlite ? (object)snippetId.ToString() : snippetId;
            await connection.ExecuteAsync(deleteSql, new { SnippetId = snippetIdParam }, transaction);

            // 添加新的标签关联
            if (tagIds.Any())
            {
                const string insertSql = "INSERT INTO SnippetTags (SnippetId, TagId) VALUES (@SnippetId, @TagId)";
                var parameters = tagIds.Select(tagId => new 
                { 
                    SnippetId = _isSqlite ? (object)snippetId.ToString() : snippetId, 
                    TagId = _isSqlite ? (object)tagId.ToString() : tagId 
                });
                await connection.ExecuteAsync(insertSql, parameters, transaction);
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    /// <summary>
    /// 增加代码片段查看次数
    /// </summary>
    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE CodeSnippets SET ViewCount = ViewCount + 1 WHERE Id = @Id";
        
        var idParam = _isSqlite ? (object)id.ToString() : id;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = idParam });
        return rowsAffected > 0;
    }

    /// <summary>
    /// 增加代码片段复制次数
    /// </summary>
    public async Task<bool> IncrementCopyCountAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE CodeSnippets SET CopyCount = CopyCount + 1 WHERE Id = @Id";
        
        var idParam = _isSqlite ? (object)id.ToString() : id;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = idParam });
        return rowsAffected > 0;
    }
}