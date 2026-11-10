namespace UserManagement.Application.Interfaces
{
    public interface ITokenBlackListService
    {
        public Task<bool> IsTokenBlackList(string jti);
        public Task<bool> AddTokenToDatabase(string jti, DateTimeOffset exp);

    }
}