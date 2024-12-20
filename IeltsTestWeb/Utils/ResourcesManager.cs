﻿using NPOI.XWPF.UserModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text;

namespace IeltsTestWeb.Utils
{
    public class ResourcesManager
    {
        public static string uploadDir { get; } = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        public static string avatarsDir { get; } = Path.Combine(uploadDir, "images", "avatars");
        public static string soundsDir { get; } = Path.Combine(uploadDir, "sounds");
        public static string sectionsDir { get; } = Path.Combine(uploadDir, "images", "sections");
        public static string qlistDir { get; } = Path.Combine(uploadDir, "images", "question_list");
        public static bool IsImageValid(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                return false;

            return true;
        }
        public static bool RemoveFile(string? url)
        {
            if (url == null) return false;

            var dirPath = Path.GetDirectoryName(url);
            if (dirPath == null) return false;

            var files = Directory.GetFiles(Directory.GetCurrentDirectory() + dirPath);
            var fileName = Path.GetFileNameWithoutExtension(url);

            foreach (var file in files)
            {
                if (Path.GetFileNameWithoutExtension(file) == fileName)
                {
                    File.Delete(file);
                    return true;
                }
            }

            return false;
        }
        public static async Task SaveImage(IFormFile file, string filePath, int maxWidth = 500, int maxHeight = 500)
        {
            using (var image = Image.Load(file.OpenReadStream()))
            {
                // Reduce image size if larger than maxWidth x maxHeight
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxWidth, maxHeight)
                    }));
                }

                // Set image compression's quality
                var encoder = new JpegEncoder
                {
                    Quality = 90 //(90/100)
                };

                // Save image
                await image.SaveAsync(filePath, encoder);
            }
        }
        public static string GetRelativePath(string fullPath)
        {
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), fullPath);
            return "/" + relativePath.Replace("\\", "/");
        }
        public static bool IsSoundValid(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = new[] { ".mp3", ".wav", ".flac" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                return false;

            return true;
        }
        public static async Task SaveSound(IFormFile file, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        public static double ConvertScoreToBand(double score)
        {
            if (score <= 1) return 1.0;
            else if (score <= 3) return 2.0;
            else if (score <= 5) return 3.0;
            else if (score <= 7) return 3.5;
            else if (score <= 9) return 4.0;
            else if (score <= 12) return 4.5;
            else if (score <= 15) return 5.0;
            else if (score <= 19) return 5.5;
            else if (score <= 22) return 6.0;
            else if (score <= 26) return 6.5;
            else if (score <= 29) return 7.0;
            else if (score <= 32) return 7.5;
            else if (score <= 34) return 8.0;
            else if (score <= 36) return 8.5;
            else return 9.0;
        }
        public async static Task<string> ReadTextFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return "";

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            try
            {
                if (fileExtension == ".txt")
                {
                    using (var streamReader = new StreamReader(file.OpenReadStream()))
                    {
                        string content = await streamReader.ReadToEndAsync();

                        content = content.Replace(Environment.NewLine, "<br>")
                                         .Replace("\n", "<br>")
                                         .Replace("\r", "<br>");

                        return content;
                    }
                }
                else if (fileExtension == ".docx")
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        try
                        {
                            using (var doc = new XWPFDocument(memoryStream))
                            {
                                StringBuilder content = new StringBuilder();
                                foreach (var paragraph in doc.Paragraphs)
                                {
                                    content.Append(paragraph.Text).Append("<br>");
                                }
                                return content.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            return $"Error reading DOCX file: {ex.Message}";
                        }
                    }
                }
                else
                {
                    return "Unsupported file format. Please upload a .txt or .docx file.";
                }
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }
    }
}
