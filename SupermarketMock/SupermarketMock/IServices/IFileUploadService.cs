namespace SupermarketMock.Services
{
    public interface IFileUploadService
    {
        /// <summary>
        /// 處理圖片上傳
        /// </summary>
        /// <param name="file">上傳的檔案</param>
        /// <param name="subFolder">要儲存的子資料夾路徑，例如 "images/products"</param>
        /// <returns>若成功回傳儲存後的檔名，失敗則回傳 null</returns>
        Task<string?> UploadImageAsync(IFormFile file, string subFolder);
    }
}
