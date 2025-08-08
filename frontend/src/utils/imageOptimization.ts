/**
 * 图片优化工具
 * 提供图片压缩、格式转换和懒加载功能
 */

interface CompressOptions {
  /** 压缩质量 (0-1) */
  quality?: number
  /** 最大宽度 */
  maxWidth?: number
  /** 最大高度 */
  maxHeight?: number
  /** 输出格式 */
  outputFormat?: 'jpeg' | 'png' | 'webp'
}

interface ImageInfo {
  width: number
  height: number
  size: number
  type: string
}

/**
 * 压缩图片文件
 */
export function compressImage(
  file: File,
  options: CompressOptions = {}
): Promise<{ file: File; info: ImageInfo }> {
  return new Promise((resolve, reject) => {
    const {
      quality = 0.8,
      maxWidth = 1920,
      maxHeight = 1080,
      outputFormat = 'jpeg'
    } = options

    const canvas = document.createElement('canvas')
    const ctx = canvas.getContext('2d')
    const img = new Image()

    img.onload = () => {
      try {
        // 计算新的尺寸
        const { width, height } = calculateNewDimensions(
          img.width,
          img.height,
          maxWidth,
          maxHeight
        )

        canvas.width = width
        canvas.height = height

        // 绘制图片
        if (ctx) {
          ctx.drawImage(img, 0, 0, width, height)
        }

        // 转换为 Blob
        canvas.toBlob(
          (blob) => {
            if (blob) {
              const compressedFile = new File(
                [blob],
                `compressed_${file.name}`,
                { type: blob.type }
              )

              const info: ImageInfo = {
                width,
                height,
                size: blob.size,
                type: blob.type
              }

              resolve({ file: compressedFile, info })
            } else {
              reject(new Error('图片压缩失败'))
            }
          },
          `image/${outputFormat}`,
          quality
        )
      } catch (error) {
        reject(error)
      }
    }

    img.onerror = () => {
      reject(new Error('图片加载失败'))
    }

    img.src = URL.createObjectURL(file)
  })
}

/**
 * 计算新的图片尺寸
 */
function calculateNewDimensions(
  originalWidth: number,
  originalHeight: number,
  maxWidth: number,
  maxHeight: number
): { width: number; height: number } {
  let { width, height } = { width: originalWidth, height: originalHeight }

  // 如果图片尺寸小于最大限制，保持原尺寸
  if (width <= maxWidth && height <= maxHeight) {
    return { width, height }
  }

  // 计算缩放比例
  const widthRatio = maxWidth / width
  const heightRatio = maxHeight / height
  const ratio = Math.min(widthRatio, heightRatio)

  return {
    width: Math.round(width * ratio),
    height: Math.round(height * ratio)
  }
}

/**
 * 获取图片信息
 */
export function getImageInfo(file: File): Promise<ImageInfo> {
  return new Promise((resolve, reject) => {
    const img = new Image()

    img.onload = () => {
      resolve({
        width: img.width,
        height: img.height,
        size: file.size,
        type: file.type
      })
    }

    img.onerror = () => {
      reject(new Error('无法获取图片信息'))
    }

    img.src = URL.createObjectURL(file)
  })
}

/**
 * 检查浏览器是否支持 WebP 格式
 */
export function supportsWebP(): Promise<boolean> {
  return new Promise((resolve) => {
    const webP = new Image()
    webP.onload = webP.onerror = () => {
      resolve(webP.height === 2)
    }
    webP.src = 'data:image/webp;base64,UklGRjoAAABXRUJQVlA4IC4AAACyAgCdASoCAAIALmk0mk0iIiIiIgBoSygABc6WWgAA/veff/0PP8bA//LwYAAA'
  })
}

/**
 * 检查浏览器是否支持 AVIF 格式
 */
export function supportsAVIF(): Promise<boolean> {
  return new Promise((resolve) => {
    const avif = new Image()
    avif.onload = avif.onerror = () => {
      resolve(avif.height === 2)
    }
    avif.src = 'data:image/avif;base64,AAAAIGZ0eXBhdmlmAAAAAGF2aWZtaWYxbWlhZk1BMUIAAADybWV0YQAAAAAAAAAoaGRscgAAAAAAAAAAcGljdAAAAAAAAAAAAAAAAGxpYmF2aWYAAAAADnBpdG0AAAAAAAEAAAAeaWxvYwAAAABEAAABAAEAAAABAAABGgAAAB0AAAAoaWluZgAAAAAAAQAAABppbmZlAgAAAAABAABhdjAxQ29sb3IAAAAAamlwcnAAAABLaXBjbwAAABRpc3BlAAAAAAAAAAIAAAACAAAAEHBpeGkAAAAAAwgICAAAAAxhdjFDgQ0MAAAAABNjb2xybmNseAACAAIAAYAAAAAXaXBtYQAAAAAAAAABAAEEAQKDBAAAACVtZGF0EgAKCBgABogQEAwgMg8f8D///8WfhwB8+ErK42A='
  })
}

/**
 * 获取最佳图片格式
 */
export async function getBestImageFormat(): Promise<'avif' | 'webp' | 'jpeg'> {
  if (await supportsAVIF()) {
    return 'avif'
  }
  if (await supportsWebP()) {
    return 'webp'
  }
  return 'jpeg'
}

/**
 * 生成响应式图片 srcset
 */
export function generateSrcSet(
  baseUrl: string,
  sizes: number[] = [320, 640, 960, 1280, 1920]
): string {
  return sizes
    .map(size => `${baseUrl}?w=${size} ${size}w`)
    .join(', ')
}

/**
 * 生成响应式图片 sizes 属性
 */
export function generateSizes(
  breakpoints: Array<{ maxWidth: string; size: string }> = [
    { maxWidth: '768px', size: '100vw' },
    { maxWidth: '1024px', size: '50vw' },
    { maxWidth: '1440px', size: '33vw' }
  ]
): string {
  const mediaQueries = breakpoints
    .map(bp => `(max-width: ${bp.maxWidth}) ${bp.size}`)
    .join(', ')

  return `${mediaQueries}, 25vw`
}

/**
 * 图片预加载
 */
export function preloadImage(src: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.onload = () => resolve()
    img.onerror = reject
    img.src = src
  })
}

/**
 * 批量预加载图片
 */
export async function preloadImages(
  urls: string[],
  options: { concurrency?: number; timeout?: number } = {}
): Promise<void> {
  const { concurrency = 3, timeout = 10000 } = options

  const chunks = []
  for (let i = 0; i < urls.length; i += concurrency) {
    chunks.push(urls.slice(i, i + concurrency))
  }

  for (const chunk of chunks) {
    const promises = chunk.map(url =>
      Promise.race([
        preloadImage(url),
        new Promise((_, reject) =>
          setTimeout(() => reject(new Error(`预加载超时: ${url}`)), timeout)
        )
      ])
    )

    try {
      await Promise.allSettled(promises)
    } catch (error) {
      console.warn('部分图片预加载失败:', error)
    }
  }
}

/**
 * 图片缓存管理
 */
class ImageCache {
  private cache = new Map<string, string>()
  private maxSize = 50 // 最大缓存数量

  /**
   * 获取缓存的图片 URL
   */
  get(key: string): string | undefined {
    return this.cache.get(key)
  }

  /**
   * 设置缓存
   */
  set(key: string, url: string): void {
    // 如果缓存已满，删除最旧的条目
    if (this.cache.size >= this.maxSize) {
      const firstKey = this.cache.keys().next().value
      if (firstKey) {
        this.cache.delete(firstKey)
      }
    }

    this.cache.set(key, url)
  }

  /**
   * 清除缓存
   */
  clear(): void {
    // 释放 blob URLs
    for (const url of this.cache.values()) {
      if (url.startsWith('blob:')) {
        URL.revokeObjectURL(url)
      }
    }
    this.cache.clear()
  }

  /**
   * 获取缓存大小
   */
  size(): number {
    return this.cache.size
  }
}

export const imageCache = new ImageCache()

/**
 * 图片优化中间件
 */
export function createImageOptimizer(options: CompressOptions = {}) {
  return {
    /**
     * 处理单个图片文件
     */
    async processFile(file: File): Promise<File> {
      const cacheKey = `${file.name}-${file.size}-${file.lastModified}`
      const cachedUrl = imageCache.get(cacheKey)

      if (cachedUrl) {
        // 从缓存中获取
        const response = await fetch(cachedUrl)
        const blob = await response.blob()
        return new File([blob], file.name, { type: blob.type })
      }

      // 压缩图片
      const { file: compressedFile } = await compressImage(file, options)

      // 缓存结果
      const url = URL.createObjectURL(compressedFile)
      imageCache.set(cacheKey, url)

      return compressedFile
    },

    /**
     * 批量处理图片文件
     */
    async processFiles(files: File[]): Promise<File[]> {
      const results = await Promise.allSettled(
        files.map(file => this.processFile(file))
      )

      return results
        .filter((result): result is PromiseFulfilledResult<File> =>
          result.status === 'fulfilled'
        )
        .map(result => result.value)
    }
  }
}
