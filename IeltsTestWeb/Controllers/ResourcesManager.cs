using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace IeltsTestWeb.Controllers
{
    public class ResourcesManager
    {
        private static int maxWidth = 500;
        private static int maxHeight = 500;
        public static string uploadDir { get; } = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        public static string avatarsDir { get; } = Path.Combine(uploadDir, "images", "avatars");
        public static bool IsImageValid(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                return false;

            return true;
        }
        public static bool RemoveImage(string? url)
        {
            if (url == null) return false;

            var filePath = Path.Combine(uploadDir, url.TrimStart('/'));

            if (!System.IO.File.Exists(filePath)) return false;

            System.IO.File.Delete(filePath);
            return true;
        }
        public static async Task SaveImage(IFormFile file, string filePath)
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
    }
}
