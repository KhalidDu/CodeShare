using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论统计分析服务接口 - 遵循单一职责原则
/// 提供评论相关的统计分析和报告功能
/// </summary>
public interface ICommentAnalyticsService
{
    // 基础统计分析
    #region 基础统计分析

    /// <summary>
    /// 获取评论基础统计信息
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论基础统计信息</returns>
    Task<CommentBasicStatsDto> GetBasicStatsAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论增长趋势
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论增长趋势</returns>
    Task<CommentGrowthTrendDto> GetGrowthTrendAsync(StatisticalPeriod period, Guid? snippetId = null);

    /// <summary>
    /// 获取评论活动热力图
    /// </summary>
    /// <param name="days">统计天数</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论活动热力图</returns>
    Task<CommentActivityHeatmapDto> GetActivityHeatmapAsync(int days = 30, Guid? snippetId = null);

    /// <summary>
    /// 获取评论参与度统计
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论参与度统计</returns>
    Task<CommentEngagementStatsDto> GetEngagementStatsAsync(StatisticalPeriod period, Guid? snippetId = null);

    /// <summary>
    /// 获取评论质量评估
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论质量评估</returns>
    Task<CommentQualityAssessmentDto> GetQualityAssessmentAsync(Guid? snippetId = null);

    #endregion

    // 用户行为分析
    #region 用户行为分析

    /// <summary>
    /// 获取用户评论行为分析
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="period">统计周期</param>
    /// <returns>用户评论行为分析</returns>
    Task<UserCommentBehaviorDto> GetUserCommentBehaviorAsync(Guid userId, StatisticalPeriod period);

    /// <summary>
    /// 获取用户评论模式分析
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户评论模式分析</returns>
    Task<UserCommentPatternDto> GetUserCommentPatternAsync(Guid userId);

    /// <summary>
    /// 获取用户影响力分析
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户影响力分析</returns>
    Task<UserInfluenceAnalysisDto> GetUserInfluenceAnalysisAsync(Guid userId);

    /// <summary>
    /// 获取活跃用户排名
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <param name="count">排名数量</param>
    /// <returns>活跃用户排名</returns>
    Task<IEnumerable<UserCommentRankingDto>> GetActiveUserRankingAsync(StatisticalPeriod period, int count = 10);

    /// <summary>
    /// 获取用户评论生命周期分析
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户评论生命周期分析</returns>
    Task<UserCommentLifecycleDto> GetUserCommentLifecycleAsync(Guid userId);

    #endregion

    // 内容分析
    #region 内容分析

    /// <summary>
    /// 获取评论内容分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论内容分析</returns>
    Task<CommentContentAnalysisDto> GetContentAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论关键词分析
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论关键词分析</returns>
    Task<CommentKeywordAnalysisDto> GetKeywordAnalysisAsync(StatisticalPeriod period, Guid? snippetId = null);

    /// <summary>
    /// 获取评论情感分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论情感分析</returns>
    Task<CommentSentimentAnalysisDto> GetSentimentAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论话题分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论话题分析</returns>
    Task<CommentTopicAnalysisDto> GetTopicAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论语言分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论语言分析</returns>
    Task<CommentLanguageAnalysisDto> GetLanguageAnalysisAsync(Guid? snippetId = null);

    #endregion

    // 时间序列分析
    #region 时间序列分析

    /// <summary>
    /// 获取评论时间分布分析
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论时间分布分析</returns>
    Task<CommentTimeDistributionDto> GetTimeDistributionAsync(StatisticalPeriod period, Guid? snippetId = null);

    /// <summary>
    /// 获取评论季节性分析
    /// </summary>
    /// <param name="years">分析年数</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论季节性分析</returns>
    Task<CommentSeasonalityAnalysisDto> GetSeasonalityAnalysisAsync(int years = 2, Guid? snippetId = null);

    /// <summary>
    /// 获取评论周期性分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论周期性分析</returns>
    Task<CommentPeriodicityAnalysisDto> GetPeriodicityAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论异常检测
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论异常检测</returns>
    Task<CommentAnomalyDetectionDto> GetAnomalyDetectionAsync(StatisticalPeriod period, Guid? snippetId = null);

    /// <summary>
    /// 获取评论预测分析
    /// </summary>
    /// <param name="period">预测周期</param>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论预测分析</returns>
    Task<CommentPredictionAnalysisDto> GetPredictionAnalysisAsync(StatisticalPeriod period, Guid? snippetId = null);

    #endregion

    // 社交网络分析
    #region 社交网络分析

    /// <summary>
    /// 获取评论社交网络分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论社交网络分析</returns>
    Task<CommentSocialNetworkDto> GetSocialNetworkAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论回复链分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论回复链分析</returns>
    Task<CommentReplyChainAnalysisDto> GetReplyChainAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论影响力传播分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论影响力传播分析</returns>
    Task<CommentInfluencePropagationDto> GetInfluencePropagationAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论社区发现分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论社区发现分析</returns>
    Task<CommentCommunityDetectionDto> GetCommunityDetectionAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论中心性分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论中心性分析</returns>
    Task<CommentCentralityAnalysisDto> GetCentralityAnalysisAsync(Guid? snippetId = null);

    #endregion

    // 高级统计分析
    #region 高级统计分析

    /// <summary>
    /// 获取评论相关性分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论相关性分析</returns>
    Task<CommentCorrelationAnalysisDto> GetCorrelationAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论聚类分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论聚类分析</returns>
    Task<CommentClusteringAnalysisDto> GetClusteringAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论分类分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论分类分析</returns>
    Task<CommentClassificationAnalysisDto> GetClassificationAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论回归分析
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论回归分析</returns>
    Task<CommentRegressionAnalysisDto> GetRegressionAnalysisAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论机器学习洞察
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论机器学习洞察</returns>
    Task<CommentMLInsightsDto> GetMLInsightsAsync(Guid? snippetId = null);

    #endregion

    // 报告和导出
    #region 报告和导出

    /// <summary>
    /// 生成评论分析报告
    /// </summary>
    /// <param name="request">报告请求</param>
    /// <returns>评论分析报告</returns>
    Task<CommentAnalysisReportDto> GenerateAnalysisReportAsync(CommentAnalysisRequestDto request);

    /// <summary>
    /// 导出评论分析数据
    /// </summary>
    /// <param name="request">导出请求</param>
    /// <returns>导出文件内容</returns>
    Task<byte[]> ExportAnalysisDataAsync(CommentAnalysisExportRequestDto request);

    /// <summary>
    /// 获取评论仪表板数据
    /// </summary>
    /// <param name="request">仪表板请求</param>
    /// <returns>评论仪表板数据</returns>
    Task<CommentDashboardDataDto> GetDashboardDataAsync(CommentDashboardRequestDto request);

    /// <summary>
    /// 获取评论实时统计
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>评论实时统计</returns>
    Task<CommentRealtimeStatsDto> GetRealtimeStatsAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取评论性能指标
    /// </summary>
    /// <param name="period">统计周期</param>
    /// <returns>评论性能指标</returns>
    Task<CommentPerformanceMetricsDto> GetPerformanceMetricsAsync(StatisticalPeriod period);

    #endregion

    // 缓存和优化
    #region 缓存和优化

    /// <summary>
    /// 预计算统计数据
    /// </summary>
    /// <param name="request">预计算请求</param>
    /// <returns>是否预计算成功</returns>
    Task<bool> PrecalculateStatsAsync(CommentStatsPrecalculationRequestDto request);

    /// <summary>
    /// 清除分析缓存
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>是否清除成功</returns>
    Task<bool> ClearAnalysisCacheAsync(Guid? snippetId = null);

    /// <summary>
    /// 预热分析缓存
    /// </summary>
    /// <param name="snippetId">代码片段ID（可选）</param>
    /// <returns>是否预热成功</returns>
    Task<bool> WarmupAnalysisCacheAsync(Guid? snippetId = null);

    /// <summary>
    /// 获取缓存状态
    /// </summary>
    /// <returns>缓存状态</returns>
    Task<CommentAnalysisCacheStatusDto> GetCacheStatusAsync();

    /// <summary>
    /// 优化分析性能
    /// </summary>
    /// <param name="request">优化请求</param>
    /// <returns>优化结果</returns>
    Task<CommentAnalysisOptimizationResultDto> OptimizeAnalysisAsync(CommentAnalysisOptimizationRequestDto request);

    #endregion
}

/// <summary>
/// 统计周期枚举
/// </summary>
public enum StatisticalPeriod
{
    Last24Hours = 0,
    Last7Days = 1,
    Last30Days = 2,
    Last90Days = 3,
    Last6Months = 4,
    LastYear = 5,
    AllTime = 99
}

/// <summary>
/// 评论基础统计信息DTO
/// </summary>
public class CommentBasicStatsDto
{
    public int TotalComments { get; set; }
    public int RootComments { get; set; }
    public int ReplyComments { get; set; }
    public int TotalLikes { get; set; }
    public int TotalReports { get; set; }
    public int ActiveUsers { get; set; }
    public double AverageCommentsPerUser { get; set; }
    public double AverageLikesPerComment { get; set; }
    public double AverageRepliesPerComment { get; set; }
    public double ReportRate { get; set; }
    public DateTime? FirstCommentAt { get; set; }
    public DateTime? LatestCommentAt { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
}

/// <summary>
/// 评论增长趋势DTO
/// </summary>
public class CommentGrowthTrendDto
{
    public List<CommentGrowthDataPointDto> DataPoints { get; set; } = new();
    public double GrowthRate { get; set; }
    public double CompoundMonthlyGrowthRate { get; set; }
    public ProjectedGrowthDto ProjectedGrowth { get; set; }
    public TrendAnalysisDto TrendAnalysis { get; set; }
    public StatisticalPeriod Period { get; set; }
}

/// <summary>
/// 评论增长数据点DTO
/// </summary>
public class CommentGrowthDataPointDto
{
    public DateTime Date { get; set; }
    public int CommentCount { get; set; }
    public int UserCount { get; set; }
    public int LikeCount { get; set; }
    public double MovingAverage { get; set; }
    public double GrowthRate { get; set; }
}

/// <summary>
/// 预测增长DTO
/// </summary>
public class ProjectedGrowthDto
{
    public int ProjectedCommentsNextMonth { get; set; }
    public int ProjectedCommentsNextQuarter { get; set; }
    public int ProjectedCommentsNextYear { get; set; }
    public double ConfidenceLevel { get; set; }
}

/// <summary>
/// 趋势分析DTO
/// </summary>
public class TrendAnalysisDto
{
    public TrendDirection Direction { get; set; }
    public double Strength { get; set; }
    public double Volatility { get; set; }
    public List<string> Insights { get; set; } = new();
}

/// <summary>
/// 趋势方向枚举
/// </summary>
public enum TrendDirection
{
    Increasing = 0,
    Decreasing = 1,
    Stable = 2,
    Volatile = 3
}

/// <summary>
/// 评论活动热力图DTO
/// </summary>
public class CommentActivityHeatmapDto
{
    public List<CommentActivityHeatmapPointDto> DataPoints { get; set; } = new();
    public int MaxActivity { get; set; }
    public int MinActivity { get; set; }
    public double AverageActivity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> PeakHours { get; set; } = new();
    public List<string> PeakDays { get; set; } = new();
}

/// <summary>
/// 评论活动热力图点DTO
/// </summary>
public class CommentActivityHeatmapPointDto
{
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public int ActivityCount { get; set; }
    public double Intensity { get; set; }
}

/// <summary>
/// 评论参与度统计DTO
/// </summary>
public class CommentEngagementStatsDto
{
    public double EngagementRate { get; set; }
    public double ParticipationRate { get; set; }
    public double RetentionRate { get; set; }
    public double ViralityRate { get; set; }
    public int ActiveEngagers { get; set; }
    public double AverageEngagementDepth { get; set; }
    public List<EngagementMetricDto> Metrics { get; set; } = new();
}

/// <summary>
/// 参与度指标DTO
/// </summary>
public class EngagementMetricDto
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double Change { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论质量评估DTO
/// </summary>
public class CommentQualityAssessmentDto
{
    public double OverallScore { get; set; }
    public QualityDimensionDto ContentQuality { get; set; }
    public QualityDimensionDto Relevance { get; set; }
    public QualityDimensionDto Clarity { get; set; }
    public QualityDimensionDto Helpfulness { get; set; }
    public QualityDimensionDto Professionalism { get; set; }
    public List<QualityIssueDto> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// 质量维度DTO
/// </summary>
public class QualityDimensionDto
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Weight { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 质量问题DTO
/// </summary>
public class QualityIssueDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Severity { get; set; }
    public int Frequency { get; set; }
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// 用户评论行为DTO
/// </summary>
public class UserCommentBehaviorDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalComments { get; set; }
    public int TotalLikes { get; set; }
    public int TotalReplies { get; set; }
    public double AverageCommentLength { get; set; }
    public double AverageResponseTime { get; set; }
    public double ActivityScore { get; set; }
    public double InfluenceScore { get; set; }
    public double QualityScore { get; set; }
    public List<UserBehaviorPatternDto> Patterns { get; set; } = new();
}

/// <summary>
/// 用户行为模式DTO
/// </summary>
public class UserBehaviorPatternDto
{
    public string Pattern { get; set; } = string.Empty;
    public double Frequency { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Indicators { get; set; } = new();
}

/// <summary>
/// 用户评论模式DTO
/// </summary>
public class UserCommentPatternDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<CommentingTimeSlotDto> PreferredTimes { get; set; } = new();
    public List<CommentingDayDto> PreferredDays { get; set; } = new();
    public List<string> CommonTopics { get; set; } = new();
    public List<string> WritingStyle { get; set; } = new();
    public double ConsistencyScore { get; set; }
    public double PredictabilityScore { get; set; }
}

/// <summary>
/// 评论时间槽DTO
/// </summary>
public class CommentingTimeSlotDto
{
    public int Hour { get; set; }
    public double ActivityLevel { get; set; }
    public int CommentCount { get; set; }
}

/// <summary>
/// 评论日期DTO
/// </summary>
public class CommentingDayDto
{
    public DayOfWeek Day { get; set; }
    public double ActivityLevel { get; set; }
    public int CommentCount { get; set; }
}

/// <summary>
/// 用户影响力分析DTO
/// </summary>
public class UserInfluenceAnalysisDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double OverallInfluenceScore { get; set; }
    public InfluenceMetricDto Reach { get; set; }
    public InfluenceMetricDto Engagement { get; set; }
    public InfluenceMetricDto Authority { get; set; }
    public InfluenceMetricDto Activity { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// 影响力指标DTO
/// </summary>
public class InfluenceMetricDto
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Weight { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 用户评论排名DTO
/// </summary>
public class UserCommentRankingDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public int CommentCount { get; set; }
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
    public double Score { get; set; }
    public double Change { get; set; }
}

/// <summary>
/// 用户评论生命周期DTO
/// </summary>
public class UserCommentLifecycleDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime FirstCommentAt { get; set; }
    public DateTime? LastCommentAt { get; set; }
    public TimeSpan TotalActivePeriod { get; set; }
    public List<LifecyclePhaseDto> Phases { get; set; } = new();
    public double RetentionRate { get; set; }
    public double ChurnRate { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
}

/// <summary>
/// 生命周期阶段DTO
/// </summary>
public class LifecyclePhaseDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CommentCount { get; set; }
    public double ActivityLevel { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论内容分析DTO
/// </summary>
public class CommentContentAnalysisDto
{
    public int TotalComments { get; set; }
    public double AverageLength { get; set; }
    public double MedianLength { get; set; }
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public List<WordFrequencyDto> TopWords { get; set; } = new();
    public List<PhraseFrequencyDto> TopPhrases { get; set; } = new();
    public ContentComplexityDto Complexity { get; set; }
    public ContentTypeDistributionDto TypeDistribution { get; set; }
}

/// <summary>
/// 词频DTO
/// </summary>
public class WordFrequencyDto
{
    public string Word { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double RelativeFrequency { get; set; }
}

/// <summary>
/// 短语频率DTO
/// </summary>
public class PhraseFrequencyDto
{
    public string Phrase { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double RelativeFrequency { get; set; }
}

/// <summary>
/// 内容复杂度DTO
/// </summary>
public class ContentComplexityDto
{
    public double ReadabilityScore { get; set; }
    public double VocabularyRichness { get; set; }
    public double SentenceComplexity { get; set; }
    public double Technicality { get; set; }
    public string ComplexityLevel { get; set; } = string.Empty;
}

/// <summary>
/// 内容类型分布DTO
/// </summary>
public class ContentTypeDistributionDto
{
    public Dictionary<string, int> TypeCounts { get; set; } = new();
    public Dictionary<string, double> TypePercentages { get; set; } = new();
    public string MostCommonType { get; set; } = string.Empty;
}

/// <summary>
/// 评论关键词分析DTO
/// </summary>
public class CommentKeywordAnalysisDto
{
    public List<KeywordDto> Keywords { get; set; } = new();
    public List<KeywordTrendDto> Trends { get; set; } = new();
    public List<KeywordClusterDto> Clusters { get; set; } = new();
    public Dictionary<string, double> KeywordSentiment { get; set; } = new();
}

/// <summary>
/// 关键词DTO
/// </summary>
public class KeywordDto
{
    public string Word { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double Importance { get; set; }
    public double Trend { get; set; }
    public List<string> RelatedWords { get; set; } = new();
}

/// <summary>
/// 关键词趋势DTO
/// </summary>
public class KeywordTrendDto
{
    public string Keyword { get; set; } = string.Empty;
    public List<TrendDataPointDto> Trend { get; set; } = new();
    public double GrowthRate { get; set; }
    public double Volatility { get; set; }
}

/// <summary>
/// 趋势数据点DTO
/// </summary>
public class TrendDataPointDto
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public double MovingAverage { get; set; }
}

/// <summary>
/// 关键词聚类DTO
/// </summary>
public class KeywordClusterDto
{
    public string ClusterName { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public double CohesionScore { get; set; }
    public int Frequency { get; set; }
}

/// <summary>
/// 评论情感分析DTO
/// </summary>
public class CommentSentimentAnalysisDto
{
    public SentimentDistributionDto OverallSentiment { get; set; }
    public List<SentimentTrendDto> SentimentTrends { get; set; } = new();
    public Dictionary<string, SentimentDistributionDto> TopicSentiment { get; set; } = new();
    public List<SentimentDriverDto> SentimentDrivers { get; set; } = new();
    public List<SentimentAnomalyDto> Anomalies { get; set; } = new();
}

/// <summary>
/// 情感分布DTO
/// </summary>
public class SentimentDistributionDto
{
    public double Positive { get; set; }
    public double Neutral { get; set; }
    public double Negative { get; set; }
    public double Compound { get; set; }
    public string DominantSentiment { get; set; } = string.Empty;
}

/// <summary>
/// 情感趋势DTO
/// </summary>
public class SentimentTrendDto
{
    public DateTime Date { get; set; }
    public SentimentDistributionDto Sentiment { get; set; }
    public int CommentCount { get; set; }
}

/// <summary>
/// 情感驱动因素DTO
/// </summary>
public class SentimentDriverDto
{
    public string Factor { get; set; } = string.Empty;
    public double Impact { get; set; }
    public string Direction { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
}

/// <summary>
/// 情感异常DTO
/// </summary>
public class SentimentAnomalyDto
{
    public DateTime Date { get; set; }
    public double SentimentScore { get; set; }
    public double ExpectedScore { get; set; }
    public double Deviation { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论话题分析DTO
/// </summary>
public class CommentTopicAnalysisDto
{
    public List<TopicDto> Topics { get; set; } = new();
    public List<TopicTrendDto> TopicTrends { get; set; } = new();
    public TopicHierarchyDto TopicHierarchy { get; set; }
    public Dictionary<string, double> TopicSentiment { get; set; } = new();
    public List<TopicRelationDto> TopicRelations { get; set; } = new();
}

/// <summary>
/// 话题DTO
/// </summary>
public class TopicDto
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public int Frequency { get; set; }
    public List<string> Keywords { get; set; } = new();
    public double SentimentScore { get; set; }
    public List<string> Subtopics { get; set; } = new();
}

/// <summary>
/// 话题趋势DTO
/// </summary>
public class TopicTrendDto
{
    public string Topic { get; set; } = string.Empty;
    public List<TrendDataPointDto> Trend { get; set; } = new();
    public double GrowthRate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 话题层次结构DTO
/// </summary>
public class TopicHierarchyDto
{
    public List<TopicNodeDto> RootTopics { get; set; } = new();
    public int TotalTopics { get; set; }
    public int MaxDepth { get; set; }
}

/// <summary>
/// 话题节点DTO
/// </summary>
public class TopicNodeDto
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public List<TopicNodeDto> Children { get; set; } = new();
}

/// <summary>
/// 话题关系DTO
/// </summary>
public class TopicRelationDto
{
    public string Topic1 { get; set; } = string.Empty;
    public string Topic2 { get; set; } = string.Empty;
    public double Strength { get; set; }
    public string RelationType { get; set; } = string.Empty;
}

/// <summary>
/// 评论语言分析DTO
/// </summary>
public class CommentLanguageAnalysisDto
{
    public Dictionary<string, double> LanguageDistribution { get; set; } = new();
    public string PrimaryLanguage { get; set; } = string.Empty;
    public double LanguageDiversity { get; set; }
    public List<LanguageComplexityDto> LanguageComplexity { get; set; } = new();
    public List<LanguagePatternDto> LanguagePatterns { get; set; } = new();
}

/// <summary>
/// 语言复杂度DTO
/// </summary>
public class LanguageComplexityDto
{
    public string Language { get; set; } = string.Empty;
    public double ReadabilityScore { get; set; }
    public double VocabularyRichness { get; set; }
    public double GrammarComplexity { get; set; }
}

/// <summary>
/// 语言模式DTO
/// </summary>
public class LanguagePatternDto
{
    public string Pattern { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public double Frequency { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论时间分布分析DTO
/// </summary>
public class CommentTimeDistributionDto
{
    public List<HourlyDistributionDto> HourlyDistribution { get; set; } = new();
    public List<DailyDistributionDto> DailyDistribution { get; set; } = new();
    public List<MonthlyDistributionDto> MonthlyDistribution { get; set; } = new();
    public TimePatternAnalysisDto PatternAnalysis { get; set; }
}

/// <summary>
/// 小时分布DTO
/// </summary>
public class HourlyDistributionDto
{
    public int Hour { get; set; }
    public int CommentCount { get; set; }
    public double Percentage { get; set; }
    public double ZScore { get; set; }
}

/// <summary>
/// 日期分布DTO
/// </summary>
public class DailyDistributionDto
{
    public DayOfWeek Day { get; set; }
    public int CommentCount { get; set; }
    public double Percentage { get; set; }
    public double ZScore { get; set; }
}

/// <summary>
/// 月度分布DTO
/// </summary>
public class MonthlyDistributionDto
{
    public int Month { get; set; }
    public int CommentCount { get; set; }
    public double Percentage { get; set; }
    public double ZScore { get; set; }
}

/// <summary>
/// 时间模式分析DTO
/// </summary>
public class TimePatternAnalysisDto
{
    public List<string> PeakHours { get; set; } = new();
    public List<string> PeakDays { get; set; } = new();
    public List<string> PeakMonths { get; set; } = new();
    public double SeasonalityScore { get; set; }
    public double RegularityScore { get; set; }
}

/// <summary>
/// 评论季节性分析DTO
/// </summary>
public class CommentSeasonalityAnalysisDto
{
    public List<SeasonalPatternDto> SeasonalPatterns { get; set; } = new();
    public SeasonalDecompositionDto Decomposition { get; set; }
    public List<SeasonalEventDto> SeasonalEvents { get; set; } = new();
    public double SeasonalityStrength { get; set; }
}

/// <summary>
/// 季节性模式DTO
/// </summary>
public class SeasonalPatternDto
{
    public string Season { get; set; } = string.Empty;
    public int AverageComments { get; set; }
    public double StandardDeviation { get; set; }
    public double Trend { get; set; }
}

/// <summary>
/// 季节性分解DTO
/// </summary>
public class SeasonalDecompositionDto
{
    public List<double> Trend { get; set; } = new();
    public List<double> Seasonal { get; set; } = new();
    public List<double> Residual { get; set; } = new();
}

/// <summary>
/// 季节性事件DTO
/// </summary>
public class SeasonalEventDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Impact { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论周期性分析DTO
/// </summary>
public class CommentPeriodicityAnalysisDto
{
    public List<PeriodicityPatternDto> Patterns { get; set; } = new();
    public double DominantPeriod { get; set; }
    public double PeriodicityStrength { get; set; }
    public List<PeriodicityAnomalyDto> Anomalies { get; set; } = new();
}

/// <summary>
/// 周期性模式DTO
/// </summary>
public class PeriodicityPatternDto
{
    public double Period { get; set; }
    public double Strength { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 周期性异常DTO
/// </summary>
public class PeriodicityAnomalyDto
{
    public DateTime Date { get; set; }
    public double ExpectedValue { get; set; }
    public double ActualValue { get; set; }
    public double Deviation { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论异常检测DTO
/// </summary>
public class CommentAnomalyDetectionDto
{
    public List<CommentAnomalyDto> Anomalies { get; set; } = new();
    public AnomalyStatsDto Statistics { get; set; }
    public List<AnomalyPatternDto> Patterns { get; set; } = new();
}

/// <summary>
/// 评论异常DTO
/// </summary>
public class CommentAnomalyDto
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public double ExpectedValue { get; set; }
    public double ZScore { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Severity { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 异常统计DTO
/// </summary>
public class AnomalyStatsDto
{
    public int TotalAnomalies { get; set; }
    public double AnomalyRate { get; set; }
    public double AverageSeverity { get; set; }
    public int HighSeverityAnomalies { get; set; }
    public int MediumSeverityAnomalies { get; set; }
    public int LowSeverityAnomalies { get; set; }
}

/// <summary>
/// 异常模式DTO
/// </summary>
public class AnomalyPatternDto
{
    public string Pattern { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double AverageSeverity { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论预测分析DTO
/// </summary>
public class CommentPredictionAnalysisDto
{
    public List<PredictionDto> Predictions { get; set; } = new();
    public PredictionAccuracyDto Accuracy { get; set; }
    public List<PredictionFeatureDto> ImportantFeatures { get; set; } = new();
    public List<PredictionScenarioDto> Scenarios { get; set; } = new();
}

/// <summary>
/// 预测DTO
/// </summary>
public class PredictionDto
{
    public DateTime Date { get; set; }
    public double PredictedValue { get; set; }
    public double ConfidenceIntervalLower { get; set; }
    public double ConfidenceIntervalUpper { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// 预测准确性DTO
/// </summary>
public class PredictionAccuracyDto
{
    public double MAE { get; set; }  // Mean Absolute Error
    public double RMSE { get; set; } // Root Mean Square Error
    public double MAPE { get; set; } // Mean Absolute Percentage Error
    public double R2 { get; set; }   // R-squared
}

/// <summary>
/// 预测特征DTO
/// </summary>
public class PredictionFeatureDto
{
    public string Name { get; set; } = string.Empty;
    public double Importance { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 预测场景DTO
/// </summary>
public class PredictionScenarioDto
{
    public string Name { get; set; } = string.Empty;
    public List<PredictionDto> Predictions { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public double Probability { get; set; }
}

/// <summary>
/// 评论社交网络分析DTO
/// </summary>
public class CommentSocialNetworkDto
{
    public int Nodes { get; set; }
    public int Edges { get; set; }
    public double Density { get; set; }
    public double AveragePathLength { get; set; }
    public double ClusteringCoefficient { get; set; }
    public List<CentralityMeasureDto> CentralityMeasures { get; set; } = new();
    public List<CommunityDto> Communities { get; set; } = new();
}

/// <summary>
/// 中心性度量DTO
/// </summary>
public class CentralityMeasureDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double DegreeCentrality { get; set; }
    public double BetweennessCentrality { get; set; }
    public double ClosenessCentrality { get; set; }
    public double EigenvectorCentrality { get; set; }
}

/// <summary>
/// 社区DTO
/// </summary>
public class CommunityDto
{
    public string Id { get; set; } = string.Empty;
    public List<Guid> MemberIds { get; set; } = new();
    public double CohesionScore { get; set; }
    public int Size { get; set; }
    public List<string> Characteristics { get; set; } = new();
}

/// <summary>
/// 评论回复链分析DTO
/// </summary>
public class CommentReplyChainAnalysisDto
{
    public List<ReplyChainDto> Chains { get; set; } = new();
    public ReplyChainStatsDto Statistics { get; set; }
    public List<ReplyPatternDto> Patterns { get; set; } = new();
}

/// <summary>
/// 回复链DTO
/// </summary>
public class ReplyChainDto
{
    public Guid RootCommentId { get; set; }
    public int Depth { get; set; }
    public int Size { get; set; }
    public double ResponseTime { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

/// <summary>
/// 回复链统计DTO
/// </summary>
public class ReplyChainStatsDto
{
    public double AverageDepth { get; set; }
    public double AverageSize { get; set; }
    public double AverageResponseTime { get; set; }
    public int TotalChains { get; set; }
    public int MaxDepth { get; set; }
    public int MaxSize { get; set; }
}

/// <summary>
/// 回复模式DTO
/// </summary>
public class ReplyPatternDto
{
    public string Pattern { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double AverageResponseTime { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论影响力传播DTO
/// </summary>
public class CommentInfluencePropagationDto
{
    public List<InfluencePathDto> InfluencePaths { get; set; } = new();
    public InfluenceStatsDto Statistics { get; set; }
    public List<InfluenceNodeDto> KeyInfluencers { get; set; } = new();
}

/// <summary>
/// 影响力路径DTO
/// </summary>
public class InfluencePathDto
{
    public Guid SourceUserId { get; set; }
    public Guid TargetUserId { get; set; }
    public List<Guid> Path { get; set; } = new();
    public double InfluenceStrength { get; set; }
    public int PathLength { get; set; }
}

/// <summary>
/// 影响力统计DTO
/// </summary>
public class InfluenceStatsDto
{
    public double AverageInfluenceStrength { get; set; }
    public double MaxInfluenceStrength { get; set; }
    public int TotalInfluencePaths { get; set; }
    public double InfectionRate { get; set; }
}

/// <summary>
/// 影响力节点DTO
/// </summary>
public class InfluenceNodeDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double InfluenceScore { get; set; }
    public int OutDegree { get; set; }
    public int InDegree { get; set; }
    public double PageRank { get; set; }
}

/// <summary>
/// 评论社区发现DTO
/// </summary>
public class CommentCommunityDetectionDto
{
    public List<DetectedCommunityDto> Communities { get; set; } = new();
    public CommunityStatsDto Statistics { get; set; }
    public List<CommunityInteractionDto> Interactions { get; set; } = new();
}

/// <summary>
/// 检测到的社区DTO
/// </summary>
public class DetectedCommunityDto
{
    public string Id { get; set; } = string.Empty;
    public List<Guid> MemberIds { get; set; } = new();
    public double Modularity { get; set; }
    public double Conductance { get; set; }
    public List<string> Topics { get; set; } = new();
}

/// <summary>
/// 社区统计DTO
/// </summary>
public class CommunityStatsDto
{
    public int TotalCommunities { get; set; }
    public double AverageCommunitySize { get; set; }
    public double ModularityScore { get; set; }
    public double Coverage { get; set; }
}

/// <summary>
/// 社区交互DTO
/// </summary>
public class CommunityInteractionDto
{
    public string Community1 { get; set; } = string.Empty;
    public string Community2 { get; set; } = string.Empty;
    public double InteractionStrength { get; set; }
    public int InteractionCount { get; set; }
}

/// <summary>
/// 评论中心性分析DTO
/// </summary>
public class CommentCentralityAnalysisDto
{
    public List<CentralityRankingDto> DegreeRanking { get; set; } = new();
    public List<CentralityRankingDto> BetweennessRanking { get; set; } = new();
    public List<CentralityRankingDto> ClosenessRanking { get; set; } = new();
    public List<CentralityRankingDto> EigenvectorRanking { get; set; } = new();
    public CentralityCorrelationDto Correlations { get; set; }
}

/// <summary>
/// 中心性排名DTO
/// </summary>
public class CentralityRankingDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double Score { get; set; }
}

/// <summary>
/// 中心性相关性DTO
/// </summary>
public class CentralityCorrelationDto
{
    public double DegreeBetweenness { get; set; }
    public double DegreeCloseness { get; set; }
    public double DegreeEigenvector { get; set; }
    public double BetweennessCloseness { get; set; }
    public double BetweennessEigenvector { get; set; }
    public double ClosenessEigenvector { get; set; }
}

/// <summary>
/// 评论相关性分析DTO
/// </summary>
public class CommentCorrelationAnalysisDto
{
    public List<CorrelationMatrixDto> CorrelationMatrices { get; set; } = new();
    public List<SignificantCorrelationDto> SignificantCorrelations { get; set; } = new();
    public CorrelationStatsDto Statistics { get; set; }
}

/// <summary>
/// 相关性矩阵DTO
/// </summary>
public class CorrelationMatrixDto
{
    public string Variable1 { get; set; } = string.Empty;
    public string Variable2 { get; set; } = string.Empty;
    public double Correlation { get; set; }
    public double PValue { get; set; }
}

/// <summary>
/// 显著相关性DTO
/// </summary>
public class SignificantCorrelationDto
{
    public string Variable1 { get; set; } = string.Empty;
    public string Variable2 { get; set; } = string.Empty;
    public double Correlation { get; set; }
    public double PValue { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

/// <summary>
/// 相关性统计DTO
/// </summary>
public class CorrelationStatsDto
{
    public int TotalCorrelations { get; set; }
    public int SignificantCorrelations { get; set; }
    public double AverageCorrelation { get; set; }
    public double MaxCorrelation { get; set; }
    public double MinCorrelation { get; set; }
}

/// <summary>
/// 评论聚类分析DTO
/// </summary>
public class CommentClusteringAnalysisDto
{
    public List<ClusterDto> Clusters { get; set; } = new();
    public ClusteringStatsDto Statistics { get; set; }
    public List<ClusterCharacteristicsDto> ClusterCharacteristics { get; set; } = new();
}

/// <summary>
/// 聚类DTO
/// </summary>
public class ClusterDto
{
    public string Id { get; set; } = string.Empty;
    public List<Guid> CommentIds { get; set; } = new();
    public double Cohesion { get; set; }
    public double Separation { get; set; }
    public int Size { get; set; }
    public ClusterCenterDto Center { get; set; }
}

/// <summary>
/// 聚类中心DTO
/// </summary>
public class ClusterCenterDto
{
    public Dictionary<string, double> Features { get; set; } = new();
}

/// <summary>
/// 聚类统计DTO
/// </summary>
public class ClusteringStatsDto
{
    public int TotalClusters { get; set; }
    public double AverageClusterSize { get; set; }
    public double SilhouetteScore { get; set; }
    public double DaviesBouldinIndex { get; set; }
}

/// <summary>
/// 聚类特征DTO
/// </summary>
public class ClusterCharacteristicsDto
{
    public string ClusterId { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> Topics { get; set; } = new();
    public double AverageSentiment { get; set; }
    public double AverageLength { get; set; }
}

/// <summary>
/// 评论分类分析DTO
/// </summary>
public class CommentClassificationAnalysisDto
{
    public List<ClassificationResultDto> Results { get; set; } = new();
    public ClassificationStatsDto Statistics { get; set; }
    public List<ClassificationFeatureDto> ImportantFeatures { get; set; } = new();
}

/// <summary>
/// 分类结果DTO
/// </summary>
public class ClassificationResultDto
{
    public Guid CommentId { get; set; }
    public string PredictedClass { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double> Probabilities { get; set; } = new();
}

/// <summary>
/// 分类统计DTO
/// </summary>
public class ClassificationStatsDto
{
    public int TotalClassified { get; set; }
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
}

/// <summary>
/// 分类特征DTO
/// </summary>
public class ClassificationFeatureDto
{
    public string Name { get; set; } = string.Empty;
    public double Importance { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 评论回归分析DTO
/// </summary>
public class CommentRegressionAnalysisDto
{
    public List<RegressionModelDto> Models { get; set; } = new();
    public List<RegressionPredictionDto> Predictions { get; set; } = new();
    public RegressionStatsDto Statistics { get; set; }
}

/// <summary>
/// 回归模型DTO
/// </summary>
public class RegressionModelDto
{
    public string Name { get; set; } = string.Empty;
    public string TargetVariable { get; set; } = string.Empty;
    public List<RegressionCoefficientDto> Coefficients { get; set; } = new();
    public double RSquared { get; set; }
    public double AdjustedRSquared { get; set; }
    public double RMSE { get; set; }
}

/// <summary>
/// 回归系数DTO
/// </summary>
public class RegressionCoefficientDto
{
    public string Variable { get; set; } = string.Empty;
    public double Coefficient { get; set; }
    public double PValue { get; set; }
    public double StandardError { get; set; }
}

/// <summary>
/// 回归预测DTO
/// </summary>
public class RegressionPredictionDto
{
    public Guid CommentId { get; set; }
    public double PredictedValue { get; set; }
    public double ActualValue { get; set; }
    public double Residual { get; set; }
    public double ConfidenceIntervalLower { get; set; }
    public double ConfidenceIntervalUpper { get; set; }
}

/// <summary>
/// 回归统计DTO
/// </summary>
public class RegressionStatsDto
{
    public double OverallRSquared { get; set; }
    public double OverallRMSE { get; set; }
    public int TotalModels { get; set; }
    public int BestModelIndex { get; set; }
}

/// <summary>
/// 评论机器学习洞察DTO
/// </summary>
public class CommentMLInsightsDto
{
    public List<MLInsightDto> Insights { get; set; } = new();
    public List<MLPatternDto> Patterns { get; set; } = new();
    public List<MLAnomalyDto> Anomalies { get; set; } = new();
    public MLModelPerformanceDto ModelPerformance { get; set; }
}

/// <summary>
/// 机器学习洞察DTO
/// </summary>
public class MLInsightDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Evidence { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// 机器学习模式DTO
/// </summary>
public class MLPatternDto
{
    public string Pattern { get; set; } = string.Empty;
    public double Support { get; set; }
    public double Confidence { get; set; }
    public List<string> Features { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 机器学习异常DTO
/// </summary>
public class MLAnomalyDto
{
    public Guid CommentId { get; set; }
    public string AnomalyType { get; set; } = string.Empty;
    public double AnomalyScore { get; set; }
    public double Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> ContributingFeatures { get; set; } = new();
}

/// <summary>
/// 机器学习模型性能DTO
/// </summary>
public class MLModelPerformanceDto
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double AUC { get; set; }
    public double LogLoss { get; set; }
    public int TrainingSamples { get; set; }
    public int TestSamples { get; set; }
}

/// <summary>
/// 评论分析请求DTO
/// </summary>
public class CommentAnalysisRequestDto
{
    [Required(ErrorMessage = "报告类型不能为空")]
    public CommentAnalysisType Type { get; set; }

    [Required(ErrorMessage = "开始日期不能为空")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "结束日期不能为空")]
    public DateTime EndDate { get; set; }

    public Guid? SnippetId { get; set; }
    public Guid? UserId { get; set; }
    public List<CommentAnalysisSection> Sections { get; set; } = new();
    public Dictionary<string, string>? Parameters { get; set; }
}

/// <summary>
/// 评论分析类型枚举
/// </summary>
public enum CommentAnalysisType
{
    Overview = 0,
    UserBehavior = 1,
    ContentAnalysis = 2,
    TimeSeries = 3,
    SocialNetwork = 4,
    AdvancedAnalytics = 5,
    Custom = 99
}

/// <summary>
/// 评论分析章节枚举
/// </summary>
public enum CommentAnalysisSection
{
    Summary = 0,
    GrowthTrends = 1,
    UserBehavior = 2,
    ContentAnalysis = 3,
    SentimentAnalysis = 4,
    TopicAnalysis = 5,
    TimeDistribution = 6,
    SocialNetwork = 7,
    Recommendations = 8
}

/// <summary>
/// 评论分析报告DTO
/// </summary>
public class CommentAnalysisReportDto
{
    public string ReportId { get; set; } = string.Empty;
    public CommentAnalysisType Type { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? SnippetId { get; set; }
    public Guid? UserId { get; set; }
    public ReportMetadataDto Metadata { get; set; }
    public Dictionary<string, object> Sections { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// 报告元数据DTO
/// </summary>
public class ReportMetadataDto
{
    public int TotalComments { get; set; }
    public int TotalUsers { get; set; }
    public int TotalLikes { get; set; }
    public double GenerationTimeSeconds { get; set; }
    public string ReportFormat { get; set; } = string.Empty;
    public int ReportSizeBytes { get; set; }
}

/// <summary>
/// 评论分析导出请求DTO
/// </summary>
public class CommentAnalysisExportRequestDto
{
    [Required(ErrorMessage = "导出类型不能为空")]
    public CommentAnalysisExportType ExportType { get; set; }

    [Required(ErrorMessage = "开始日期不能为空")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "结束日期不能为空")]
    public DateTime EndDate { get; set; }

    public ExportFormat Format { get; set; } = ExportFormat.Csv;
    public Guid? SnippetId { get; set; }
    public Guid? UserId { get; set; }
    public List<string> IncludeFields { get; set; } = new();
    public Dictionary<string, string>? Filters { get; set; }
}

/// <summary>
/// 评论分析导出类型枚举
/// </summary>
public enum CommentAnalysisExportType
{
    RawData = 0,
    AggregatedStats = 1,
    TimeSeries = 2,
    UserBehavior = 3,
    ContentAnalysis = 4,
    FullReport = 5
}

/// <summary>
/// 评论仪表板请求DTO
/// </summary>
public class CommentDashboardRequestDto
{
    [Required(ErrorMessage = "仪表板类型不能为空")]
    public CommentDashboardType DashboardType { get; set; }

    public StatisticalPeriod Period { get; set; } = StatisticalPeriod.Last30Days;
    public Guid? SnippetId { get; set; }
    public Guid? UserId { get; set; }
    public List<string> Widgets { get; set; } = new();
    public Dictionary<string, string>? Filters { get; set; }
}

/// <summary>
/// 评论仪表板类型枚举
/// </summary>
public enum CommentDashboardType
{
    Overview = 0,
    UserBehavior = 1,
    ContentAnalysis = 2,
    Moderation = 3,
    Analytics = 4,
    Custom = 99
}

/// <summary>
/// 评论仪表板数据DTO
/// </summary>
public class CommentDashboardDataDto
{
    public string DashboardId { get; set; } = string.Empty;
    public CommentDashboardType Type { get; set; }
    public DateTime GeneratedAt { get; set; }
    public StatisticalPeriod Period { get; set; }
    public Guid? SnippetId { get; set; }
    public Guid? UserId { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
}

/// <summary>
/// 仪表板组件DTO
/// </summary>
public class DashboardWidgetDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public object Data { get; set; }
    public WidgetConfigDto Config { get; set; }
}

/// <summary>
/// 组件配置DTO
/// </summary>
public class WidgetConfigDto
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Position { get; set; } = string.Empty;
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// 评论实时统计DTO
/// </summary>
public class CommentRealtimeStatsDto
{
    public int TotalComments { get; set; }
    public int ActiveUsers { get; set; }
    public int CommentsLastHour { get; set; }
    public int CommentsLast24Hours { get; set; }
    public double AverageResponseTime { get; set; }
    public double ActiveUsersRate { get; set; }
    public List<RealtimeActivityDto> RecentActivities { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 实时活动DTO
/// </summary>
public class RealtimeActivityDto
{
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? CommentId { get; set; }
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// 评论性能指标DTO
/// </summary>
public class CommentPerformanceMetricsDto
{
    public double ResponseTime { get; set; }
    public double Throughput { get; set; }
    public double Availability { get; set; }
    public double ErrorRate { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double CacheHitRate { get; set; }
    public double QueryPerformance { get; set; }
    public List<PerformanceAlertDto> Alerts { get; set; } = new();
}

/// <summary>
/// 性能警报DTO
/// </summary>
public class PerformanceAlertDto
{
    public string Metric { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double Threshold { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 评论统计预计算请求DTO
/// </summary>
public class CommentStatsPrecalculationRequestDto
{
    [Required(ErrorMessage = "预计算类型不能为空")]
    public CommentStatsPrecalculationType PrecalculationType { get; set; }

    public StatisticalPeriod Period { get; set; }
    public Guid? SnippetId { get; set; }
    public Guid? UserId { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
}

/// <summary>
/// 评论统计预计算类型枚举
/// </summary>
public enum CommentStatsPrecalculationType
{
    BasicStats = 0,
    GrowthTrends = 1,
    UserBehavior = 2,
    ContentAnalysis = 3,
    TimeSeries = 4,
    All = 99
}

/// <summary>
/// 评论分析缓存状态DTO
/// </summary>
public class CommentAnalysisCacheStatusDto
{
    public int CachedItems { get; set; }
    public long CacheSizeBytes { get; set; }
    public double CacheHitRate { get; set; }
    public DateTime LastCleanup { get; set; }
    public List<CacheItemStatusDto> ItemStatuses { get; set; } = new();
}

/// <summary>
/// 缓存项目状态DTO
/// </summary>
public class CacheItemStatusDto
{
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long SizeBytes { get; set; }
    public int HitCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 评论分析优化请求DTO
/// </summary>
public class CommentAnalysisOptimizationRequestDto
{
    [Required(ErrorMessage = "优化类型不能为空")]
    public CommentAnalysisOptimizationType OptimizationType { get; set; }

    public Dictionary<string, string>? Parameters { get; set; }
}

/// <summary>
/// 评论分析优化类型枚举
/// </summary>
public enum CommentAnalysisOptimizationType
{
    QueryOptimization = 0,
    IndexOptimization = 1,
    CacheOptimization = 2,
    MemoryOptimization = 3,
    AlgorithmOptimization = 4
}

/// <summary>
/// 评论分析优化结果DTO
/// </summary>
public class CommentAnalysisOptimizationResultDto
{
    public bool Success { get; set; }
    public string OptimizationType { get; set; } = string.Empty;
    public double PerformanceImprovement { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public DateTime OptimizedAt { get; set; }
}