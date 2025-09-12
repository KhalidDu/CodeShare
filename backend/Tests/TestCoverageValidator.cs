using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// 测试覆盖率验证工具
/// </summary>
public class TestCoverageValidator
{
    public static void Main()
    {
        Console.WriteLine("=== ShareController 测试覆盖率验证 ===\n");
        
        // 获取ShareController的所有公共方法
        var controllerType = typeof(CodeSnippetManager.Api.Controllers.ShareController);
        var controllerMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType.Name.StartsWith("Task") && m.Name != "GetType")
            .OrderBy(m => m.Name)
            .ToList();
        
        Console.WriteLine($"ShareController 公共方法数量: {controllerMethods.Count}");
        Console.WriteLine("方法列表:");
        foreach (var method in controllerMethods)
        {
            Console.WriteLine($"  - {method.Name}");
        }
        
        // 获取测试类中的所有测试方法
        var testType = typeof(CodeSnippetManager.Api.Tests.ShareControllerTests);
        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes<TestMethodAttribute>().Any())
            .OrderBy(m => m.Name)
            .ToList();
        
        Console.WriteLine($"\nShareControllerTests 测试方法数量: {testMethods.Count}");
        
        // 分析测试覆盖的API方法
        var coverage = new Dictionary<string, int>();
        var methodGroups = new Dictionary<string, List<string>>();
        
        foreach (var testMethod in testMethods)
        {
            var testName = testMethod.Name;
            string apiMethod = ExtractApiMethodName(testName);
            
            if (!string.IsNullOrEmpty(apiMethod))
            {
                if (!coverage.ContainsKey(apiMethod))
                {
                    coverage[apiMethod] = 0;
                    methodGroups[apiMethod] = new List<string>();
                }
                coverage[apiMethod]++;
                methodGroups[apiMethod].Add(testName);
            }
        }
        
        Console.WriteLine("\n=== API 方法测试覆盖情况 ===");
        foreach (var kvp in coverage.OrderBy(k => k.Key))
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value} 个测试");
            foreach (var test in methodGroups[kvp.Key])
            {
                Console.WriteLine($"  - {test}");
            }
            Console.WriteLine();
        }
        
        // 检查未覆盖的方法
        var uncoveredMethods = controllerMethods
            .Select(m => m.Name)
            .Except(coverage.Keys)
            .ToList();
        
        if (uncoveredMethods.Count > 0)
        {
            Console.WriteLine("=== 未覆盖的 API 方法 ===");
            foreach (var method in uncoveredMethods)
            {
                Console.WriteLine($"- {method}");
            }
        }
        else
        {
            Console.WriteLine("✅ 所有 API 方法都有测试覆盖！");
        }
        
        // 计算覆盖率
        double coveragePercentage = (coverage.Count * 100.0) / controllerMethods.Count;
        Console.WriteLine($"\n=== 覆盖率统计 ===");
        Console.WriteLine($"API 方法总数: {controllerMethods.Count}");
        Console.WriteLine($"已测试方法数: {coverage.Count}");
        Console.WriteLine($"测试覆盖率: {coveragePercentage:F1}%");
        Console.WriteLine($"总测试用例数: {testMethods.Count}");
        
        Console.WriteLine("\n=== 测试类型分析 ===");
        var testTypes = new Dictionary<string, int>
        {
            { "成功场景", 0 },
            { "失败场景", 0 },
            { "权限验证", 0 },
            { "输入验证", 0 },
            { "异常处理", 0 },
            { "边界条件", 0 }
        };
        
        foreach (var testMethod in testMethods)
        {
            var testName = testMethod.Name;
            if (testName.Contains("Valid") || testName.EndsWith("Success"))
                testTypes["成功场景"]++;
            else if (testName.Contains("Invalid") || testName.Contains("Empty") || testName.Contains("NotFound"))
                testTypes["失败场景"]++;
            else if (testName.Contains("Unauthorized") || testName.Contains("Forbid") || testName.Contains("AnonymousUser"))
                testTypes["权限验证"]++;
            else if (testName.Contains("Model") || testName.Contains("Null"))
                testTypes["输入验证"]++;
            else if (testName.Contains("Exception") || testName.Contains("Error"))
                testTypes["异常处理"]++;
            else if (testName.Contains("Boundary") || testName.Contains("Limit"))
                testTypes["边界条件"]++;
        }
        
        foreach (var kvp in testTypes)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value} 个测试");
        }
        
        Console.WriteLine("\n✅ 测试覆盖率验证完成！");
    }
    
    private static string ExtractApiMethodName(string testName)
    {
        // 从测试方法名中提取API方法名
        // 例如: CreateShare_ValidRequest_ShouldReturnCreatedShareToken -> CreateShare
        var parts = testName.Split('_');
        if (parts.Length > 0)
        {
            return parts[0];
        }
        return string.Empty;
    }
}