using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 消息草稿附件仓储接口 - 定义消息草稿附件数据访问操作
/// </summary>
public interface IMessageDraftAttachmentRepository
{
    /// <summary>
    /// 创建草稿附件
    /// </summary>
    /// <param name="draftAttachment">草稿附件实体</param>
    /// <returns>创建后的草稿附件实体</returns>
    Task<MessageDraftAttachment> CreateAsync(MessageDraftAttachment draftAttachment);

    /// <summary>
    /// 批量创建草稿附件
    /// </summary>
    /// <param name="draftAttachments">草稿附件实体列表</param>
    /// <returns>创建后的草稿附件实体列表</returns>
    Task<IEnumerable<MessageDraftAttachment>> CreateBatchAsync(IEnumerable<MessageDraftAttachment> draftAttachments);

    /// <summary>
    /// 根据ID获取草稿附件
    /// </summary>
    /// <param name="id">草稿附件ID</param>
    /// <returns>草稿附件实体或null</returns>
    Task<MessageDraftAttachment?> GetByIdAsync(Guid id);

    /// <summary>
    /// 根据草稿ID获取附件列表
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>草稿附件列表</returns>
    Task<IEnumerable<MessageDraftAttachment>> GetByDraftIdAsync(Guid draftId);

    /// <summary>
    /// 根据草稿ID和文件名获取附件
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="fileName">文件名</param>
    /// <returns>草稿附件实体或null</returns>
    Task<MessageDraftAttachment?> GetByDraftIdAndFileNameAsync(Guid draftId, string fileName);

    /// <summary>
    /// 更新草稿附件
    /// </summary>
    /// <param name="draftAttachment">草稿附件实体</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateAsync(MessageDraftAttachment draftAttachment);

    /// <summary>
    /// 删除草稿附件
    /// </summary>
    /// <param name="id">草稿附件ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// 根据草稿ID删除所有附件
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteByDraftIdAsync(Guid draftId);

    /// <summary>
    /// 批量删除草稿附件
    /// </summary>
    /// <param name="ids">草稿附件ID列表</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteBatchAsync(IEnumerable<Guid> ids);

    /// <summary>
    /// 获取草稿附件的总大小
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>总大小（字节）</returns>
    Task<long> GetTotalSizeByDraftIdAsync(Guid draftId);

    /// <summary>
    /// 检查草稿附件是否存在
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="fileName">文件名</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(Guid draftId, string fileName);

    /// <summary>
    /// 获取草稿附件数量
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>附件数量</returns>
    Task<int> GetCountByDraftIdAsync(Guid draftId);
}