using System;
using System.Collections.Generic;
using System.Text;

namespace Dashboard.Infrastructure.DTOs
{
    public class RegisterRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
