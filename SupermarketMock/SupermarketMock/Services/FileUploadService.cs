namespace SupermarketMock.Services
{
    public class FileUploadService : IFileUploadService
    {
        /// <summary>最大檔案大小：5 MB</summary>
        private const long MaxFileSize = 5L * 1024 * 1024;

        /// <summary>允許的副檔名</summary>
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        /// <summary>允許的 Content-Type（防止副檔名偽造）</summary>
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

        public async Task<string?> UploadImageAsync(IFormFile file, string subFolder)
        {
            // 1. 檢查檔案是否存在
            if (file == null || file.Length == 0)
                return null;

            // 2. 檔案大小驗證（Service 層防禦）
            if (file.Length > MaxFileSize)
                return null;

            // 3. 副檔名驗證（Defense in depth）
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(fileExtension))
                return null;

            // 4. Content-Type 驗證（防止副檔名偽造：例如 .exe 改名為 .jpg）
            if (!string.IsNullOrEmpty(file.ContentType) && !AllowedContentTypes.Contains(file.ContentType))
                return null;

            // 5. 建立儲存資料夾路徑
            var targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            // 6. 產生唯一檔名（使用 GUID 防止路徑穿越攻擊與檔名衝突）
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(targetFolder, fileName);

            // 7. 儲存檔案
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
