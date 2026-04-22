using Dashboard.Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dashboard.Infrastructure.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}
