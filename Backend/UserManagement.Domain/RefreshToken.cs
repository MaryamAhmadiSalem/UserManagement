namespace UserManagement.Domain
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTimeOffset Expires { get; set; }
        public DateTimeOffset? Revoked { get; set; }
    }
}
