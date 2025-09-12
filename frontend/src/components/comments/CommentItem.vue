<template>
  <div 
    :class="itemClasses"
    :data-comment-id="comment.id"
    :style="itemStyles"
  >
    <!-- 评论内容 -->
    <div class="comment-content">
      <!-- 用户头像 -->
      <div v-if="showAvatar" class="comment-avatar">
        <div 
          v-if="comment.userAvatar"
          class="avatar-image"
          :style="{ backgroundImage: `url(${comment.userAvatar})` }"
        ></div>
        <div v-else class="avatar-placeholder">
          {{ userInitials }}
        </div>
      </div>

      <!-- 评论主体 -->
      <div class="comment-body">
        <!-- 评论头部 -->
        <div class="comment-header">
          <div class="comment-meta">
            <span class="user-name">{{ comment.userName }}</span>
            <span v-if="showTimestamp" class="comment-time">
              {{ formatTime(comment.createdAt) }}
            </span>
            <span v-if="comment.depth > 0" class="reply-indicator">
              <i class="fas fa-reply"></i>
              回复
            </span>
          </div>
          
          <!-- 评论操作 -->
          <div v-if="showActions" class="comment-actions">
            <button
              v-if="canEdit"
              class="action-btn action-btn--edit"
              @click="handleEdit"
              title="编辑评论"
            >
              <i class="fas fa-edit"></i>
            </button>
            <button
              v-if="canDelete"
              class="action-btn action-btn--delete"
              @click="handleDelete"
              title="删除评论"
            >
              <i class="fas fa-trash"></i>
            </button>
            <button
              v-if="showReportButton && canReport"
              class="action-btn action-btn--report"
              @click="handleReport"
              title="举报评论"
            >
              <i class="fas fa-flag"></i>
            </button>
            <button
              v-if="replies.length > 0"
              class="action-btn action-btn--collapse"
              @click="handleToggleCollapse"
              :title="isCollapsed ? '展开回复' : '收起回复'"
            >
              <i :class="isCollapsed ? 'fas fa-chevron-down' : 'fas fa-chevron-up'"></i>
            </button>
          </div>
        </div>

        <!-- 评论文本 -->
        <div class="comment-text">
          <p class="text-content">{{ comment.content }}</p>
        </div>

        <!-- 评论底部操作 -->
        <div class="comment-footer">
          <!-- 点赞按钮 -->
          <div v-if="showLikeButton" class="like-action">
            <button
              :class="likeButtonClasses"
              @click="handleLike"
              :disabled="likeLoading"
            >
              <i :class="likeIconClasses"></i>
              <span class="like-count">{{ formatCount(comment.likeCount) }}</span>
            </button>
          </div>

          <!-- 回复按钮 -->
          <div v-if="showReplyButton && canReply && depth < maxDepth" class="reply-action">
            <button
              class="reply-btn"
              @click="handleReply"
            >
              <i class="fas fa-reply"></i>
              回复
            </button>
          </div>

          <!-- 回复计数 -->
          <div v-if="comment.replyCount > 0" class="reply-count">
            <span class="count-text">{{ comment.replyCount }} 条回复</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 回复列表 -->
    <div v-if="replies.length > 0 && !isCollapsed" class="comment-replies">
      <div class="replies-container">
        <CommentItem
          v-for="replyNode in replies"
          :key="replyNode.comment.id"
          :comment="replyNode.comment"
          :replies="replyNode.children"
          :depth="depth + 1"
          :max-depth="maxDepth"
          :show-avatar="showAvatar"
          :show-actions="showActions"
          :show-timestamp="showTimestamp"
          :show-like-button="showLikeButton"
          :show-reply-button="showReplyButton"
          :show-report-button="showReportButton"
          :current-user-id="currentUserId"
          :permissions="permissions"
          :highlighted-comment-id="highlightedCommentId"
          :is-collapsed="collapsedReplies.has(replyNode.comment.id)"
          @like="$emit('like', $event)"
          @reply="$emit('reply', $event)"
          @edit="$emit('edit', $event)"
          @delete="$emit('delete', $event)"
          @report="$emit('report', $event)"
          @toggle-collapse="$emit('toggle-collapse', $event)"
          @load-more="$emit('load-more', $event)"
        />
      </div>

      <!-- 加载更多回复 -->
      <div v-if="hasMoreReplies" class="load-more-replies">
        <AnimatedButton
          variant="ghost"
          size="sm"
          :loading="loadingMore"
          @click="handleLoadMore"
        >
          <i class="fas fa-chevron-down"></i>
          加载更多回复
        </AnimatedButton>
      </div>
    </div>

    <!-- 删除确认模态框 -->
    <AnimatedModal
      v-if="showDeleteModal"
      title="删除评论"
      :show="showDeleteModal"
      @close="showDeleteModal = false"
    >
      <div class="delete-confirm">
        <p>确定要删除这条评论吗？此操作无法撤销。</p>
        <div class="delete-actions">
          <AnimatedButton
            variant="outline"
            @click="showDeleteModal = false"
          >
            取消
          </AnimatedButton>
          <AnimatedButton
            variant="error"
            :loading="deleteLoading"
            @click="confirmDelete"
          >
            删除
          </AnimatedButton>
        </div>
      </div>
    </AnimatedModal>

    <!-- 编辑模态框 -->
    <AnimatedModal
      v-if="showEditModal"
      title="编辑评论"
      :show="showEditModal"
      @close="showEditModal = false"
    >
      <div class="edit-form">
        <textarea
          v-model="editContent"
          class="edit-textarea"
          rows="4"
          maxlength="1000"
          placeholder="编辑你的评论..."
        ></textarea>
        <div class="edit-actions">
          <AnimatedButton
            variant="outline"
            @click="showEditModal = false"
          >
            取消
          </AnimatedButton>
          <AnimatedButton
            variant="primary"
            :loading="editLoading"
            :disabled="!editContent.trim()"
            @click="confirmEdit"
          >
            保存
          </AnimatedButton>
        </div>
      </div>
    </AnimatedModal>

    <!-- 举报模态框 -->
    <AnimatedModal
      v-if="showReportModal"
      title="举报评论"
      :show="showReportModal"
      @close="showReportModal = false"
    >
      <div class="report-form">
        <div class="report-reasons">
          <label class="reason-item">
            <input
              type="radio"
              v-model="reportReason"
              value="0"
              name="report-reason"
            />
            <span>垃圾信息</span>
          </label>
          <label class="reason-item">
            <input
              type="radio"
              v-model="reportReason"
              value="1"
              name="report-reason"
            />
            <span>不当内容</span>
          </label>
          <label class="reason-item">
            <input
              type="radio"
              v-model="reportReason"
              value="2"
              name="report-reason"
            />
            <span>骚扰</span>
          </label>
          <label class="reason-item">
            <input
              type="radio"
              v-model="reportReason"
              value="3"
              name="report-reason"
            />
            <span>仇恨言论</span>
          </label>
          <label class="reason-item">
            <input
              type="radio"
              v-model="reportReason"
              value="4"
              name="report-reason"
            />
            <span>虚假信息</span>
          </label>
          <label class="reason-item">
            <input
              type="radio"
              v-model="reportReason"
              value="5"
              name="report-reason"
            />
            <span>版权侵犯</span>
          </label>
          <label class="reason-item">
            <input
              type="radio"
              v-model="reportReason"
              value="99"
              name="report-reason"
            />
            <span>其他</span>
          </label>
        </div>
        <textarea
          v-model="reportDescription"
          class="report-textarea"
          rows="3"
          placeholder="详细描述举报原因（可选）"
        ></textarea>
        <div class="report-actions">
          <AnimatedButton
            variant="outline"
            @click="showReportModal = false"
          >
            取消
          </AnimatedButton>
          <AnimatedButton
            variant="warning"
            :loading="reportLoading"
            :disabled="!reportReason"
            @click="confirmReport"
          >
            提交举报
          </AnimatedButton>
        </div>
      </div>
    </AnimatedModal>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Comment, CommentPermissions } from '@/types/comment'
import AnimatedButton from '@/components/common/AnimatedButton.vue'
import AnimatedModal from '@/components/common/AnimatedModal.vue'

// 定义Props
interface Props {
  comment: Comment
  replies: any[]
  depth: number
  maxDepth: number
  showAvatar?: boolean
  showActions?: boolean
  showTimestamp?: boolean
  showLikeButton?: boolean
  showReplyButton?: boolean
  showReportButton?: boolean
  currentUserId?: string | null
  permissions?: CommentPermissions | null
  highlightedCommentId?: string | null
  isCollapsed?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  replies: () => [],
  depth: 0,
  maxDepth: 5,
  showAvatar: true,
  showActions: true,
  showTimestamp: true,
  showLikeButton: true,
  showReplyButton: true,
  showReportButton: true,
  currentUserId: null,
  permissions: null,
  highlightedCommentId: null,
  isCollapsed: false
})

// 定义Emits
interface Emits {
  (e: 'like', commentId: string): void
  (e: 'reply', commentId: string): void
  (e: 'edit', commentId: string): void
  (e: 'delete', commentId: string): void
  (e: 'report', commentId: string): void
  (e: 'toggle-collapse', commentId: string): void
  (e: 'load-more', commentId: string): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const likeLoading = ref(false)
const deleteLoading = ref(false)
const editLoading = ref(false)
const reportLoading = ref(false)
const loadingMore = ref(false)
const showDeleteModal = ref(false)
const showEditModal = ref(false)
const showReportModal = ref(false)
const editContent = ref(props.comment.content)
const reportReason = ref('')
const reportDescription = ref('')
const collapsedReplies = ref<Set<string>>(new Set())

// 计算属性
const itemClasses = computed(() => {
  return [
    'comment-item',
    `comment-item--depth-${props.depth}`,
    {
      'comment-item--highlighted': props.comment.id === props.highlightedCommentId,
      'comment-item--collapsed': props.isCollapsed,
      'comment-item--has-replies': props.replies.length > 0,
      'comment-item--is-reply': props.depth > 0
    }
  ]
})

const itemStyles = computed(() => {
  return {
    marginLeft: `${props.depth * 2}rem`
  }
})

const userInitials = computed(() => {
  if (!props.comment.userName) return 'U'
  return props.comment.userName.charAt(0).toUpperCase()
})

const canEdit = computed(() => {
  return props.comment.canEdit && props.currentUserId === props.comment.userId
})

const canDelete = computed(() => {
  return props.comment.canDelete && props.currentUserId === props.comment.userId
})

const canReport = computed(() => {
  return props.comment.canReport && props.currentUserId !== props.comment.userId
})

const canReply = computed(() => {
  return props.permissions?.canCreate && props.depth < props.maxDepth
})

const isLiked = computed(() => {
  return props.comment.isLikedByCurrentUser
})

const likeButtonClasses = computed(() => {
  return [
    'like-btn',
    {
      'like-btn--liked': isLiked.value,
      'like-btn--loading': likeLoading.value
    }
  ]
})

const likeIconClasses = computed(() => {
  return [
    'like-icon',
    {
      'fas': isLiked.value,
      'far': !isLiked.value,
      'fa-heart': true
    }
  ]
})

const hasMoreReplies = computed(() => {
  // 这里可以根据实际需求判断是否有更多回复
  return props.replies.length < props.comment.replyCount
})

// 方法
function handleLike() {
  if (likeLoading.value) return
  
  likeLoading.value = true
  emit('like', props.comment.id)
  
  // 模拟加载状态，实际应该等待API响应
  setTimeout(() => {
    likeLoading.value = false
  }, 1000)
}

function handleReply() {
  emit('reply', props.comment.id)
}

function handleEdit() {
  editContent.value = props.comment.content
  showEditModal.value = true
}

function handleDelete() {
  showDeleteModal.value = true
}

function handleReport() {
  showReportModal.value = true
}

function handleToggleCollapse() {
  emit('toggle-collapse', props.comment.id)
}

function handleLoadMore() {
  if (loadingMore.value) return
  
  loadingMore.value = true
  emit('load-more', props.comment.id)
  
  // 模拟加载状态
  setTimeout(() => {
    loadingMore.value = false
  }, 1000)
}

async function confirmDelete() {
  if (deleteLoading.value) return
  
  try {
    deleteLoading.value = true
    emit('delete', props.comment.id)
    showDeleteModal.value = false
  } finally {
    deleteLoading.value = false
  }
}

async function confirmEdit() {
  if (editLoading.value || !editContent.value.trim()) return
  
  try {
    editLoading.value = true
    // 这里应该调用编辑API
    // await commentService.updateComment(props.comment.id, { content: editContent.value })
    showEditModal.value = false
  } finally {
    editLoading.value = false
  }
}

async function confirmReport() {
  if (reportLoading.value || !reportReason.value) return
  
  try {
    reportLoading.value = true
    // 这里应该调用举报API
    // await commentService.reportComment(props.comment.id, {
    //   reason: parseInt(reportReason.value),
    //   description: reportDescription.value
    // })
    showReportModal.value = false
    reportReason.value = ''
    reportDescription.value = ''
  } finally {
    reportLoading.value = false
  }
}

function formatTime(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)
  
  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (hours < 24) return `${hours}小时前`
  if (days < 7) return `${days}天前`
  
  return date.toLocaleDateString()
}

function formatCount(count: number): string {
  if (count < 1000) return count.toString()
  if (count < 1000000) return `${(count / 1000).toFixed(1)}K`
  return `${(count / 1000000).toFixed(1)}M`
}
</script>

<style scoped>
.comment-item {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: 0.75rem;
  background: var(--gray-50);
  border: 1px solid var(--gray-200);
  transition: all 0.2s ease;
}

.comment-item:hover {
  background: var(--gray-100);
  border-color: var(--gray-300);
}

.comment-item--highlighted {
  background: linear-gradient(90deg, rgba(59, 130, 246, 0.1) 0%, transparent 100%);
  border-left: 3px solid var(--primary-500);
}

.comment-item--collapsed {
  opacity: 0.7;
}

.comment-item--has-replies {
  border-bottom-left-radius: 0;
  border-bottom-right-radius: 0;
}

.comment-item--is-reply {
  background: var(--gray-25);
  border: 1px solid var(--gray-150);
}

.comment-content {
  display: flex;
  gap: 0.75rem;
  align-items: flex-start;
}

/* 头像 */
.comment-avatar {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
  background: var(--gray-200);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  color: var(--gray-600);
  font-size: 0.875rem;
}

.avatar-image {
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  color: var(--gray-600);
  font-size: 0.875rem;
}

/* 评论主体 */
.comment-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

/* 评论头部 */
.comment-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.comment-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.user-name {
  font-weight: 600;
  color: var(--gray-900);
  font-size: 0.875rem;
}

.comment-time {
  color: var(--gray-500);
  font-size: 0.75rem;
}

.reply-indicator {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--gray-500);
  font-size: 0.75rem;
}

/* 评论操作 */
.comment-actions {
  display: flex;
  gap: 0.25rem;
}

.action-btn {
  padding: 0.25rem 0.5rem;
  border: none;
  border-radius: 0.375rem;
  background: transparent;
  color: var(--gray-500);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 24px;
  height: 24px;
}

.action-btn:hover {
  background: var(--gray-200);
  color: var(--gray-700);
}

.action-btn--edit:hover {
  background: var(--blue-100);
  color: var(--blue-600);
}

.action-btn--delete:hover {
  background: var(--red-100);
  color: var(--red-600);
}

.action-btn--report:hover {
  background: var(--yellow-100);
  color: var(--yellow-600);
}

.action-btn--collapse:hover {
  background: var(--gray-200);
  color: var(--gray-700);
}

/* 评论文本 */
.comment-text {
  flex: 1;
}

.text-content {
  margin: 0;
  color: var(--gray-800);
  font-size: 0.875rem;
  line-height: 1.5;
  white-space: pre-wrap;
  word-wrap: break-word;
}

/* 评论底部 */
.comment-footer {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
}

/* 点赞 */
.like-action {
  display: flex;
  align-items: center;
}

.like-btn {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-600);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.75rem;
  font-weight: 500;
}

.like-btn:hover {
  border-color: var(--red-400);
  color: var(--red-600);
  background: var(--red-50);
}

.like-btn--liked {
  border-color: var(--red-500);
  color: var(--red-600);
  background: var(--red-50);
}

.like-btn--loading {
  opacity: 0.7;
  cursor: not-allowed;
}

.like-icon {
  font-size: 0.875rem;
}

.like-count {
  font-size: 0.75rem;
  font-weight: 500;
}

/* 回复 */
.reply-action {
  display: flex;
  align-items: center;
}

.reply-btn {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-600);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.75rem;
  font-weight: 500;
}

.reply-btn:hover {
  border-color: var(--blue-400);
  color: var(--blue-600);
  background: var(--blue-50);
}

/* 回复计数 */
.reply-count {
  display: flex;
  align-items: center;
}

.count-text {
  color: var(--gray-500);
  font-size: 0.75rem;
}

/* 回复列表 */
.comment-replies {
  margin-top: 0.75rem;
  margin-left: 2rem;
  border-left: 2px solid var(--gray-200);
  padding-left: 1rem;
}

.replies-container {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.load-more-replies {
  padding: 0.5rem;
  display: flex;
  justify-content: center;
}

/* 模态框内容 */
.delete-confirm {
  text-align: center;
}

.delete-confirm p {
  margin: 0 0 1rem 0;
  color: var(--gray-700);
}

.delete-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: center;
}

.edit-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.edit-textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.5rem;
  font-size: 0.875rem;
  line-height: 1.5;
  resize: vertical;
  font-family: inherit;
}

.edit-textarea:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.edit-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

.report-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.report-reasons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 0.5rem;
}

.reason-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.reason-item:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
}

.reason-item input[type="radio"] {
  margin: 0;
}

.reason-item span {
  font-size: 0.875rem;
  color: var(--gray-700);
}

.report-textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.5rem;
  font-size: 0.875rem;
  line-height: 1.5;
  resize: vertical;
  font-family: inherit;
}

.report-textarea:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.report-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .comment-item {
    padding: 0.75rem;
  }
  
  .comment-content {
    gap: 0.5rem;
  }
  
  .comment-avatar {
    width: 32px;
    height: 32px;
  }
  
  .comment-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .comment-actions {
    align-self: flex-end;
  }
  
  .comment-footer {
    gap: 0.75rem;
  }
  
  .comment-replies {
    margin-left: 1rem;
    padding-left: 0.75rem;
  }
  
  .report-reasons {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 480px) {
  .comment-item {
    padding: 0.5rem;
  }
  
  .comment-content {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .comment-avatar {
    width: 24px;
    height: 24px;
    font-size: 0.75rem;
  }
  
  .text-content {
    font-size: 0.813rem;
  }
  
  .comment-footer {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
}

/* 深色模式 */
:deep(.dark) .comment-item {
  background: var(--gray-800);
  border-color: var(--gray-700);
}

:deep(.dark) .comment-item:hover {
  background: var(--gray-750);
  border-color: var(--gray-600);
}

:deep(.dark) .comment-item--is-reply {
  background: var(--gray-850);
  border-color: var(--gray-700);
}

:deep(.dark) .user-name {
  color: var(--gray-100);
}

:deep(.dark) .comment-time,
:deep(.dark) .reply-indicator {
  color: var(--gray-400);
}

:deep(.dark) .action-btn {
  color: var(--gray-400);
}

:deep(.dark) .action-btn:hover {
  background: var(--gray-700);
  color: var(--gray-200);
}

:deep(.dark) .like-btn {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-400);
}

:deep(.dark) .like-btn:hover {
  border-color: var(--red-400);
  color: var(--red-400);
  background: var(--red-900/20);
}

:deep(.dark) .like-btn--liked {
  border-color: var(--red-500);
  color: var(--red-400);
  background: var(--red-900/30);
}

:deep(.dark) .reply-btn {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-400);
}

:deep(.dark) .reply-btn:hover {
  border-color: var(--blue-400);
  color: var(--blue-400);
  background: var(--blue-900/20);
}

:deep(.dark) .count-text {
  color: var(--gray-400);
}

:deep(.dark) .text-content {
  color: var(--gray-200);
}

:deep(.dark) .comment-replies {
  border-color: var(--gray-700);
}

:deep(.dark) .edit-textarea,
:deep(.dark) .report-textarea {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-200);
}

:deep(.dark) .reason-item {
  border-color: var(--gray-600);
}

:deep(.dark) .reason-item:hover {
  background: var(--gray-700);
}

:deep(.dark) .reason-item span {
  color: var(--gray-200);
}

/* 无障碍性 */
@media (prefers-reduced-motion: reduce) {
  .comment-item,
  .action-btn,
  .like-btn,
  .reply-btn {
    transition: none;
  }
}
</style>