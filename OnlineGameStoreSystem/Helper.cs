using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OnlineGameStoreSystem.Helpers;

public static class FileHelper
{
    /// <summary>
    /// 安全保存图片文件到服务器
    /// </summary>
    /// <param name="file">上传的文件</param>
    /// <param name="folder">相对于 wwwroot 的保存目录，如 "images/posts"</param>
    /// <param name="maxFileSizeMB">允许的最大文件大小，单位 MB</param>
    /// <returns>返回可访问 URL，如 "/images/posts/xxxx.jpg"</returns>
    public static async Task<string> SaveImageAsync(IFormFile file, string folder = "images/posts", int maxFileSizeMB = 5)
    {
        if (file == null || file.Length == 0)
            return "";

        // 检查文件大小
        if (file.Length > maxFileSizeMB * 1024 * 1024)
            throw new Exception($"文件大小不能超过 {maxFileSizeMB} MB");

        // 检查文件类型（简单检查扩展名）
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (Array.IndexOf(allowedExtensions, ext) < 0)
            throw new Exception("不允许的文件类型");

        // 生成唯一文件名
        var fileName = Guid.NewGuid().ToString() + ext;

        // 绝对路径
        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var saveFolder = Path.Combine(wwwrootPath, folder);

        // 创建目录
        Directory.CreateDirectory(saveFolder);

        var savePath = Path.Combine(saveFolder, fileName);

        // 保存文件
        using (var stream = new FileStream(savePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 返回 URL（相对于 wwwroot）
        return "/" + folder.Replace("\\", "/") + "/" + fileName;
    }
}

public static class ModelStateHelper
{
    /// <summary>
    /// 获取 ModelState 所有错误信息
    /// </summary>
    /// <param name="modelState">Controller 的 ModelState</param>
    /// <returns>格式化字符串，包含字段名和错误信息</returns>
    public static string GetErrors(ModelStateDictionary modelState)
    {
        if (modelState == null) return string.Empty;

        var sb = new StringBuilder();
        foreach (var entry in modelState)
        {
            var key = entry.Key;
            var errors = entry.Value.Errors;
            foreach (var error in errors)
            {
                sb.AppendLine($"{key}: {error.ErrorMessage}");
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 控制台输出 ModelState 错误，调试用
    /// </summary>
    /// <param name="modelState"></param>
    public static void LogErrors(ModelStateDictionary modelState)
    {
        var errorText = GetErrors(modelState);
        if (!string.IsNullOrEmpty(errorText))
        {
            Console.WriteLine("ModelState Errors:");
            Console.WriteLine(errorText);
        }
    }
}