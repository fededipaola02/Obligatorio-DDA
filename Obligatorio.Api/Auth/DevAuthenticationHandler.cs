using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Obligatorio.Api.Auth;

/// <summary>
/// Esquema de autenticación para desarrollo:
/// Permite simular roles y usuarios a través de encabezados HTTP en español ('X-Rol-Usuario' y 'X-Id-Usuario').
/// Por defecto asume 'Administrador'.
/// </summary>
public class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevAuth";

    public DevAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var rol = "Administrador";
        if (Request.Headers.TryGetValue("X-Rol-Usuario", out var headerRol))
        {
            rol = headerRol.ToString();
        }
        else if (Request.Headers.TryGetValue("X-User-Role", out var headerRoleOld))
        {
            rol = headerRoleOld.ToString();
        }

        var usuarioId = "admin@ciudad.gob";
        if (Request.Headers.TryGetValue("X-Id-Usuario", out var headerUsuario))
        {
            usuarioId = headerUsuario.ToString();
        }
        else if (Request.Headers.TryGetValue("X-User-Id", out var headerUserOld))
        {
            usuarioId = headerUserOld.ToString();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId),
            new Claim(ClaimTypes.Name, usuarioId),
            new Claim(ClaimTypes.Email, usuarioId),
            new Claim(ClaimTypes.Role, rol)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
