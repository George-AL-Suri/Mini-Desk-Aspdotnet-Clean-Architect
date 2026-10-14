using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MiniDesk.Application.Auth;
using MiniDesk.Application.Auth.Dtos;
using MiniDesk.Application.Common.Constants;
using MiniDesk.Application.Common.Exceptions;
using MiniDesk.Infrastructure.Identity;
using MiniDesk.Infrastructure.Persistence;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MiniDesk.Infrastructure.Auth;

internal class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    IOptions<JwtOptions> jwtOptions,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegisterRequest> registerValidator,
    IValidator<RefreshRequest> refreshValidator) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    // ---------- Register (user + role + token in one transaction) ----------
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);

        if (await userManager.FindByEmailAsync(request.Email) is not null)
            throw new ConflictException("This email is already registered.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded) throw ToValidation(create);

        var role = await userManager.AddToRoleAsync(user, Roles.User);   // never from the request
        if (!role.Succeeded) throw ToValidation(role);

        var response = await IssueAsync(user, ct);
        await tx.CommitAsync(ct);          // any exception above rolls everything back
        return response;
    }

    // ---------- Login ----------
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException();

        return await IssueAsync(user, ct);
    }

    // ---------- Refresh (rotation + reuse detection) ----------
    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        await refreshValidator.ValidateAndThrowAsync(request, ct);

        var hash = Hash(request.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (stored.RevokedAt is not null)
        {
            // a revoked token came back: assume theft, kill every session of this user
            await RevokeAllAsync(stored.UserId, ct);
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expired.");

        var user = await userManager.FindByIdAsync(stored.UserId.ToString())
            ?? throw new UnauthorizedException("Invalid refresh token.");

        stored.RevokedAt = DateTime.UtcNow;          // old one dies...
        return await IssueAsync(user, ct);           // ...new one is added; one SaveChanges saves both
    }

    // ---------- Logout ----------
    public async Task LogoutAsync(RefreshRequest request, CancellationToken ct)
    {
        await refreshValidator.ValidateAndThrowAsync(request, ct);

        var hash = Hash(request.RefreshToken);
        var now = DateTime.UtcNow;

        await db.RefreshTokens
            .Where(t => t.TokenHash == hash && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    // ---------- Helpers ----------
    private async Task<AuthResponse> IssueAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (access, expires) = CreateAccessToken(user, roles);

        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var now = DateTime.UtcNow;

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(raw),                   // only the hash is stored
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwt.RefreshTokenDays)
        });
        await db.SaveChangesAsync(ct);

        return new AuthResponse(access, expires, raw);
    }

    private Task RevokeAllAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    private (string Token, DateTime Expires) CreateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                SecurityAlgorithms.HmacSha256)
        };
        return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));   // 64 hex chars

    // Identity errors (weak password, etc.) -> 400 through your existing middleware
    private static ValidationException ToValidation(IdentityResult result) =>
        new(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
}