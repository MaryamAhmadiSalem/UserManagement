namespace UserManagement.Application.ResultClass
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public List<string>? Error { get; set; }
    }
}