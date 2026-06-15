namespace SupermarketMock.Services
{
    public class FileUploadService : IFileUploadService
    {
        public async Task<string?> UploadImageAsync(IFormFile file, string subFolder)
        {
            // 1. 檢查檔案是否存在
            if (file == null || file.Length == 0)
                return null;

            // 2. 檢查檔案類型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                return null;

            // 3. 建立儲存資料夾路徑
            var targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            // 4. 產生唯一檔名與完整路徑
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(targetFolder, fileName);

            // 5. 儲存檔案
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;

        }
    }
}
