namespace SupermarketMock.DTOs
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; set; }
    }
}
