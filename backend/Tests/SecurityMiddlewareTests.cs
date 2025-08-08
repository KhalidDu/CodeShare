using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;
using CodeSnippetManager.Api.Middleware;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 安全中间件单元测试 - 测试安全功能的面向接口设计
/// </summary>
[TestClass]
public class SecurityMiddlewareTests
{
    #region XSS Protection Middleware Tests

    [TestClass]
    public class XssProtectionMiddlewareTests
    {
        private readonly Mock<ILogger<XssProtectionMiddleware>> _mockLogger;
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly XssProtectionMiddleware _middleware;

        public XssProtectionMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<XssProtectionMiddleware>>();
            _mockNext = new Mock<RequestDelegate>();
            _middleware = new XssProtectionMiddleware(_mockNext.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task InvokeAsync_GetRequest_CallsNext()
        {
            // Arrange
            var context = CreateHttpContext("GET", "/api/test");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(next => next(context), Times.Once);
        }

        [TestMethod]
        public async Task InvokeAsync_PostWithXssContent_BlocksRequest()
        {
            // Arrange
            var context = CreateHttpContext("POST", "/api/test");
            context.Request.ContentType = "application/json";
            var xssContent = "{\"title\":\"<script>alert('xss')</script>\"}";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(xssContent));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.AreEqual(400, context.Response.StatusCode);
            _mockNext.Verify(next => next(context), Times.Never);
        }

        [TestMethod]
        public async Task InvokeAsync_PostWithSafeContent_CallsNext()
        {
            // Arrange
            var context = CreateHttpContext("POST", "/api/test");
            context.Request.ContentType = "application/json";
            var safeContent = "{\"title\":\"Safe Content\"}";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(safeContent));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(next => next(context), Times.Once);
        }

        [TestMethod]
        public async Task InvokeAsync_XssInQueryParameter_BlocksRequest()
        {
            // Arrange
            var context = CreateHttpContext("POST", "/api/test");
            context.Request.QueryString = new QueryString("?search=<script>alert('xss')</script>");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.AreEqual(400, context.Response.StatusCode);
            _mockNext.Verify(next => next(context), Times.Never);
        }

        private static DefaultHttpContext CreateHttpContext(string method, string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            return context;
        }
    }

    #endregion

    #region Rate Limiting Middleware Tests

    [TestClass]
    public class RateLimitingMiddlewareTests
    {
        private readonly Mock<ILogger<RateLimitingMiddleware>> _mockLogger;
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly RateLimitingMiddleware _middleware;

        public RateLimitingMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();
            _mockNext = new Mock<RequestDelegate>();
            var options = new RateLimitOptions
            {
                DefaultLimit = 2,
                TimeWindow = TimeSpan.FromMinutes(1)
            };
            _middleware = new RateLimitingMiddleware(_mockNext.Object, _mockLogger.Object, options);
        }

        [TestMethod]
        public async Task InvokeAsync_FirstRequest_CallsNext()
        {
            // Arrange
            var context = CreateHttpContext("GET", "/api/test");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(next => next(context), Times.Once);
        }

        [TestMethod]
        public async Task InvokeAsync_ExceedsRateLimit_Returns429()
        {
            // Arrange
            var context = CreateHttpContext("GET", "/api/test");

            // Act - Make requests exceeding the limit
            await _middleware.InvokeAsync(context);
            await _middleware.InvokeAsync(context);
            await _middleware.InvokeAsync(context); // This should be blocked

            // Assert
            Assert.AreEqual(429, context.Response.StatusCode);
        }

        private static DefaultHttpContext CreateHttpContext(string method, string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            return context;
        }
    }

    #endregion

    #region CSRF Protection Middleware Tests

    [TestClass]
    public class CsrfProtectionMiddlewareTests
    {
        private readonly Mock<ILogger<CsrfProtectionMiddleware>> _mockLogger;
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly CsrfProtectionMiddleware _middleware;

        public CsrfProtectionMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<CsrfProtectionMiddleware>>();
            _mockNext = new Mock<RequestDelegate>();
            _middleware = new CsrfProtectionMiddleware(_mockNext.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task InvokeAsync_GetRequest_GeneratesToken()
        {
            // Arrange
            var context = CreateHttpContext("GET", "/api/test");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.IsTrue(context.Response.Headers.ContainsKey("X-CSRF-Token"));
            _mockNext.Verify(next => next(context), Times.Once);
        }

        [TestMethod]
        public async Task InvokeAsync_PostWithoutToken_Returns403()
        {
            // Arrange
            var context = CreateHttpContext("POST", "/api/test");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.AreEqual(403, context.Response.StatusCode);
            _mockNext.Verify(next => next(context), Times.Never);
        }

        [TestMethod]
        public async Task InvokeAsync_LoginEndpoint_SkipsCsrfCheck()
        {
            // Arrange
            var context = CreateHttpContext("POST", "/api/auth/login");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(next => next(context), Times.Once);
        }

        private static DefaultHttpContext CreateHttpContext(string method, string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            return context;
        }
    }

    #endregion

    #region Security Headers Middleware Tests

    [TestClass]
    public class SecurityHeadersMiddlewareTests
    {
        private readonly Mock<ILogger<SecurityHeadersMiddleware>> _mockLogger;
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly SecurityHeadersMiddleware _middleware;

        public SecurityHeadersMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();
            _mockNext = new Mock<RequestDelegate>();
            _middleware = new SecurityHeadersMiddleware(_mockNext.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task InvokeAsync_AddsSecurityHeaders()
        {
            // Arrange
            var context = CreateHttpContext("GET", "/api/test");

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.IsTrue(context.Response.Headers.ContainsKey("X-Content-Type-Options"));
            Assert.IsTrue(context.Response.Headers.ContainsKey("X-Frame-Options"));
            Assert.IsTrue(context.Response.Headers.ContainsKey("X-XSS-Protection"));
            Assert.IsTrue(context.Response.Headers.ContainsKey("Content-Security-Policy"));
            _mockNext.Verify(next => next(context), Times.Once);
        }

        [TestMethod]
        public async Task InvokeAsync_HttpsRequest_AddsHstsHeader()
        {
            // Arrange
            var context = CreateHttpContext("GET", "/api/test");
            context.Request.IsHttps = true;

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.IsTrue(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
            _mockNext.Verify(next => next(context), Times.Once);
        }

        private static DefaultHttpContext CreateHttpContext(string method, string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            return context;
        }
    }

    #endregion
}