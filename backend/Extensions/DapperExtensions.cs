using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CodeSnippetManager.Api.Extensions
{
    /// <summary>
    /// Dapper 性能优化扩展方法
    /// 提供批量操作、连接池管理和查询优化功能
    /// </summary>
    public static class DapperExtensions
    {
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableName">表名</param>
        /// <param name="entities">实体集合</param>
        /// <param name="batchSize">批次大小</param>
        /// <param name="transaction">事务</param>
        /// <returns>影响的行数</returns>
        public static async Task<int> BulkInsertAsync<T>(
            this IDbConnection connection,
            string tableName,
            IEnumerable<T> entities,
            int batchSize = 1000,
            IDbTransaction? transaction = null)
        {
            if (entities == null || !entities.Any())
                return 0;

            var entityList = entities.ToList();
            var totalAffected = 0;

            // 获取实体属性
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.GetGetMethod() != null)
                .ToList();

            var columnNames = string.Join(", ", properties.Select(p => p.Name));
            var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";

            // 分批处理
            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize);
                var affected = await connection.ExecuteAsync(sql, batch, transaction);
                totalAffected += affected;
            }

            return totalAffected;
        }

        /// <summary>
        /// 批量更新数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableName">表名</param>
        /// <param name="entities">实体集合</param>
        /// <param name="keyColumn">主键列名</param>
        /// <param name="batchSize">批次大小</param>
        /// <param name="transaction">事务</param>
        /// <returns>影响的行数</returns>
        public static async Task<int> BulkUpdateAsync<T>(
            this IDbConnection connection,
            string tableName,
            IEnumerable<T> entities,
            string keyColumn = "Id",
            int batchSize = 1000,
            IDbTransaction? transaction = null)
        {
            if (entities == null || !entities.Any())
                return 0;

            var entityList = entities.ToList();
            var totalAffected = 0;

            // 获取实体属性（排除主键）
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.GetGetMethod() != null && p.Name != keyColumn)
                .ToList();

            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
            var sql = $"UPDATE {tableName} SET {setClause} WHERE {keyColumn} = @{keyColumn}";

            // 分批处理
            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize);
                var affected = await connection.ExecuteAsync(sql, batch, transaction);
                totalAffected += affected;
            }

            return totalAffected;
        }

        /// <summary>
        /// 执行带性能监控的查询
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="param">参数</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="transaction">事务</param>
        /// <returns>查询结果</returns>
        public static async Task<IEnumerable<T>> QueryWithPerformanceAsync<T>(
            this IDbConnection connection,
            string sql,
            object? param = null,
            ILogger? logger = null,
            int? commandTimeout = null,
            IDbTransaction? transaction = null)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var result = await connection.QueryAsync<T>(sql, param, transaction, commandTimeout);
                
                var duration = DateTime.UtcNow - startTime;
                logger?.LogDebug("查询执行完成: {Duration}ms, SQL: {Sql}", 
                    duration.TotalMilliseconds, sql);
                
                // 如果查询时间超过阈值，记录警告
                if (duration.TotalMilliseconds > 1000)
                {
                    logger?.LogWarning("慢查询检测: {Duration}ms, SQL: {Sql}", 
                        duration.TotalMilliseconds, sql);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                logger?.LogError(ex, "查询执行失败: {Duration}ms, SQL: {Sql}", 
                    duration.TotalMilliseconds, sql);
                throw;
            }
        }

        /// <summary>
        /// 执行带性能监控的单个查询
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="param">参数</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="transaction">事务</param>
        /// <returns>查询结果</returns>
        public static async Task<T?> QuerySingleOrDefaultWithPerformanceAsync<T>(
            this IDbConnection connection,
            string sql,
            object? param = null,
            ILogger? logger = null,
            int? commandTimeout = null,
            IDbTransaction? transaction = null)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var result = await connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout);
                
                var duration = DateTime.UtcNow - startTime;
                logger?.LogDebug("单个查询执行完成: {Duration}ms, SQL: {Sql}", 
                    duration.TotalMilliseconds, sql);
                
                if (duration.TotalMilliseconds > 500)
                {
                    logger?.LogWarning("慢查询检测: {Duration}ms, SQL: {Sql}", 
                        duration.TotalMilliseconds, sql);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                logger?.LogError(ex, "单个查询执行失败: {Duration}ms, SQL: {Sql}", 
                    duration.TotalMilliseconds, sql);
                throw;
            }
        }

        /// <summary>
        /// Multi-mapping 查询优化
        /// </summary>
        /// <typeparam name="TFirst">第一个类型</typeparam>
        /// <typeparam name="TSecond">第二个类型</typeparam>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="map">映射函数</param>
        /// <param name="param">参数</param>
        /// <param name="splitOn">分割字段</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="transaction">事务</param>
        /// <returns>查询结果</returns>
        public static async Task<IEnumerable<TReturn>> QueryMultiMappingAsync<TFirst, TSecond, TReturn>(
            this IDbConnection connection,
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            object? param = null,
            string splitOn = "Id",
            ILogger? logger = null,
            int? commandTimeout = null,
            IDbTransaction? transaction = null)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var result = await connection.QueryAsync(sql, map, param, transaction, true, splitOn, commandTimeout);
                
                var duration = DateTime.UtcNow - startTime;
                logger?.LogDebug("Multi-mapping 查询执行完成: {Duration}ms, SQL: {Sql}", 
                    duration.TotalMilliseconds, sql);
                
                if (duration.TotalMilliseconds > 1000)
                {
                    logger?.LogWarning("慢查询检测: {Duration}ms, SQL: {Sql}", 
                        duration.TotalMilliseconds, sql);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                logger?.LogError(ex, "Multi-mapping 查询执行失败: {Duration}ms, SQL: {Sql}", 
                    duration.TotalMilliseconds, sql);
                throw;
            }
        }

        /// <summary>
        /// 分页查询优化
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">基础 SQL 语句（不包含 LIMIT）</param>
        /// <param name="countSql">计数 SQL 语句</param>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="param">参数</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="transaction">事务</param>
        /// <returns>分页结果</returns>
        public static async Task<(IEnumerable<T> Items, int TotalCount)> QueryPagedAsync<T>(
            this IDbConnection connection,
            string sql,
            string countSql,
            int page,
            int pageSize,
            object? param = null,
            ILogger? logger = null,
            IDbTransaction? transaction = null)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var offset = (page - 1) * pageSize;
                var pagedSql = $"{sql} LIMIT {pageSize} OFFSET {offset}";

                // 并行执行数据查询和计数查询
                var dataTask = connection.QueryAsync<T>(pagedSql, param, transaction);
                var countTask = connection.QuerySingleAsync<int>(countSql, param, transaction);

                await Task.WhenAll(dataTask, countTask);

                var duration = DateTime.UtcNow - startTime;
                logger?.LogDebug("分页查询执行完成: {Duration}ms, Page: {Page}, PageSize: {PageSize}", 
                    duration.TotalMilliseconds, page, pageSize);

                if (duration.TotalMilliseconds > 1000)
                {
                    logger?.LogWarning("慢分页查询检测: {Duration}ms, Page: {Page}, PageSize: {PageSize}", 
                        duration.TotalMilliseconds, page, pageSize);
                }

                return (dataTask.Result, countTask.Result);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                logger?.LogError(ex, "分页查询执行失败: {Duration}ms, Page: {Page}, PageSize: {PageSize}", 
                    duration.TotalMilliseconds, page, pageSize);
                throw;
            }
        }

        /// <summary>
        /// 事务批处理
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="operations">操作列表</param>
        /// <param name="logger">日志记录器</param>
        /// <returns>是否成功</returns>
        public static async Task<bool> ExecuteTransactionAsync(
            this IDbConnection connection,
            IEnumerable<Func<IDbConnection, IDbTransaction, Task>> operations,
            ILogger? logger = null)
        {
            var startTime = DateTime.UtcNow;
            
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();
            
            try
            {
                foreach (var operation in operations)
                {
                    await operation(connection, transaction);
                }

                transaction.Commit();
                
                var duration = DateTime.UtcNow - startTime;
                logger?.LogDebug("事务批处理执行完成: {Duration}ms", duration.TotalMilliseconds);
                
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                
                var duration = DateTime.UtcNow - startTime;
                logger?.LogError(ex, "事务批处理执行失败: {Duration}ms", duration.TotalMilliseconds);
                
                return false;
            }
        }
    }

    /// <summary>
    /// SQL 查询构建器
    /// 帮助构建优化的 SQL 查询
    /// </summary>
    public class SqlQueryBuilder
    {
        private readonly List<string> _selectColumns = new();
        private readonly List<string> _fromTables = new();
        private readonly List<string> _joinClauses = new();
        private readonly List<string> _whereClauses = new();
        private readonly List<string> _orderByClauses = new();
        private readonly Dictionary<string, object> _parameters = new();

        public SqlQueryBuilder Select(params string[] columns)
        {
            _selectColumns.AddRange(columns);
            return this;
        }

        public SqlQueryBuilder From(string table)
        {
            _fromTables.Add(table);
            return this;
        }

        public SqlQueryBuilder LeftJoin(string table, string condition)
        {
            _joinClauses.Add($"LEFT JOIN {table} ON {condition}");
            return this;
        }

        public SqlQueryBuilder InnerJoin(string table, string condition)
        {
            _joinClauses.Add($"INNER JOIN {table} ON {condition}");
            return this;
        }

        public SqlQueryBuilder Where(string condition, object? value = null)
        {
            _whereClauses.Add(condition);
            if (value != null)
            {
                var paramName = $"param{_parameters.Count}";
                _parameters[paramName] = value;
            }
            return this;
        }

        public SqlQueryBuilder OrderBy(string column, bool descending = false)
        {
            _orderByClauses.Add($"{column} {(descending ? "DESC" : "ASC")}");
            return this;
        }

        public (string Sql, object Parameters) Build()
        {
            var sql = "SELECT " + string.Join(", ", _selectColumns);
            sql += " FROM " + string.Join(", ", _fromTables);
            
            if (_joinClauses.Any())
                sql += " " + string.Join(" ", _joinClauses);
            
            if (_whereClauses.Any())
                sql += " WHERE " + string.Join(" AND ", _whereClauses);
            
            if (_orderByClauses.Any())
                sql += " ORDER BY " + string.Join(", ", _orderByClauses);

            return (sql, _parameters);
        }
    }
}