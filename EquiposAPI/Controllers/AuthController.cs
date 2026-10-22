using AutoMapper;
using EquiposAPI.Data;
using EquiposAPI.DTOs.Auth;
using EquiposAPI.Models;
using EquiposAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquiposAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthController(
        ApplicationDbContext context,
        ITokenService tokenService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _mapper = mapper;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            return Unauthorized(new { mensaje = "Credenciales incorrectas. Verifique su usuario y contraseña." });
        }

        var token = _tokenService.GenerateToken(usuario);
        var expireMinutes = double.TryParse(_configuration["Jwt:ExpireMinutes"], out var min) ? min : 120;

        return Ok(new LoginResponseDto
        {
            Token = token,
            Username = usuario.Username,
            Email = usuario.Email,
            Rol = usuario.Rol,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes)
        });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var exists = await _context.Usuarios
            .AnyAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());

        if (exists)
        {
            return Conflict(new { mensaje = $"El nombre de usuario '{request.Username}' ya está registrado en el sistema." });
        }

        var emailExists = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());

        if (emailExists)
        {
            return Conflict(new { mensaje = $"El correo '{request.Email}' ya está registrado en el sistema." });
        }

        var usuario = _mapper.Map<Usuario>(request);
        usuario.Username = request.Username.Trim();
        usuario.Email = request.Email.Trim();
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var responseDto = _mapper.Map<UsuarioDto>(usuario);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, responseDto);
    }

    [HttpGet("usuarios")]
    [Authorize(Roles = "Admin,Docente")]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
    {
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<UsuarioDto>>(usuarios));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        return Ok(_mapper.Map<UsuarioDto>(usuario));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (usuario == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<UsuarioDto>(usuario));
    }
}
