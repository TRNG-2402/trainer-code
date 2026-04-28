using ProductCatalog.DTOs;

namespace ProductCatalog.Services;

public interface IAuthService
{
    Task<TokenResponseDTO> LoginAsync(LoginDTO loginDto);
    
}