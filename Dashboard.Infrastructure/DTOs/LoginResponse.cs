namespace Dashboard.Infrastructure.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
