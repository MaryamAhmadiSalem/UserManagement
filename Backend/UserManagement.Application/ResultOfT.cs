namespace UserManagement.Application
{
    public class ResultOfT<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
    }
}