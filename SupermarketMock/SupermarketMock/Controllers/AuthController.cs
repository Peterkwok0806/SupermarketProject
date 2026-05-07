using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Services;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody] UserRegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(result);

        }




    }
}
