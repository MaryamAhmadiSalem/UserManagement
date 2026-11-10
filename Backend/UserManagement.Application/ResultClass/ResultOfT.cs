namespace UserManagement.Application.ResultClass
{
    public class ResultOfT<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public List<string>? Error { get; set; }
    }
}