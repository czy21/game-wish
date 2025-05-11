namespace WishServer.Model
{
    public class CommonResult<T>
    {
        public int Code { get; set; } = 0;               // 0 表示成功，非 0 表示错误
        public string Message { get; set; } = "Success"; // 可根据错误码生成
        public T Data { get; set; }

        public static CommonResult<T> Ok(T data, string message = "Success") => new() { Data = data, Message = message };

        public static CommonResult<T> Fail(int code, string message) => new() { Code = code, Message = message };
    }
}
