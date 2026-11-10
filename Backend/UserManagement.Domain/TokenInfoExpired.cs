namespace UserManagement.Domain
{
    public class TokenInfoExpired
    {
        public int Id { get; set; }
        public string Jti {  get; set; }
        public DateTimeOffset Expires { get; set; }
    }
}
