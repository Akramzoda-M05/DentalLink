using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Entities.Lookup;
using DentalLink.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.Application.Features.Auth;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    string RoleCode,
    string RoleName
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserDto User
);

// -----------------------------------------------------------------------------
// 1. Login
// -----------------------------------------------------------------------------
public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Valid email format is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher _hasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = _hasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenService.HashToken(rawRefreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashedRefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.Role.Code,
            user.Role.Name
        );

        return new AuthResponseDto(
            accessToken,
            rawRefreshToken,
            refreshTokenEntity.ExpiresAt,
            userDto
        );
    }
}

// -----------------------------------------------------------------------------
// 2. Refresh Token
// -----------------------------------------------------------------------------
public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);

        var existingToken = await _context.RefreshTokens
            .Include(t => t.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existingToken == null || !existingToken.IsActive || !existingToken.User.IsActive)
        {
            throw new UnauthorizedException("Invalid, expired, or revoked refresh token.");
        }

        // Token rotation
        var rawNewRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var hashedNewRefreshToken = _jwtTokenService.HashToken(rawNewRefreshToken);

        existingToken.RevokedAt = DateTimeOffset.UtcNow;
        existingToken.ReplacedByTokenHash = hashedNewRefreshToken;

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = existingToken.UserId,
            TokenHash = hashedNewRefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        var newAccessToken = _jwtTokenService.GenerateAccessToken(existingToken.User);

        var userDto = new UserDto(
            existingToken.User.Id,
            existingToken.User.FullName,
            existingToken.User.Email,
            existingToken.User.Phone,
            existingToken.User.Role.Code,
            existingToken.User.Role.Name
        );

        return new AuthResponseDto(
            newAccessToken,
            rawNewRefreshToken,
            newRefreshTokenEntity.ExpiresAt,
            userDto
        );
    }
}

// -----------------------------------------------------------------------------
// 3. Get Current User (Me)
// -----------------------------------------------------------------------------
public record GetMeQuery : IRequest<UserDto>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMeQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new NotFoundException(nameof(User), _currentUserService.UserId.Value);
        }

        return new UserDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.Role.Code,
            user.Role.Name
        );
    }
}

// -----------------------------------------------------------------------------
// 4. Register Staff (Admin only)
// -----------------------------------------------------------------------------
public record RegisterStaffCommand(
    string FullName,
    string Email,
    string Password,
    string Phone,
    string RoleCode,
    Guid? SpecializationId,
    string? Qualification,
    int? ExperienceYears,
    string? Bio,
    string? AvatarUrl
) : IRequest<UserDto>;

public class RegisterStaffCommandValidator : AbstractValidator<RegisterStaffCommand>
{
    public RegisterStaffCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(120);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.RoleCode).NotEmpty().Must(code =>
            code == LookupCodes.UserRole.Admin ||
            code == LookupCodes.UserRole.Receptionist ||
            code == LookupCodes.UserRole.Doctor)
            .WithMessage("Invalid role code. Must be 'admin', 'receptionist', or 'doctor'.");

        When(x => x.RoleCode == LookupCodes.UserRole.Doctor, () =>
        {
            RuleFor(x => x.SpecializationId).NotNull().WithMessage("Doctor must have a specialization.");
        });
    }
}

public class RegisterStaffCommandHandler : IRequestHandler<RegisterStaffCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterStaffCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(RegisterStaffCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        if (emailExists)
        {
            throw new ConflictException($"User with email '{request.Email}' already exists.");
        }

        var phoneExists = await _context.Users
            .AnyAsync(u => u.Phone == request.Phone.Trim(), cancellationToken);
        if (phoneExists)
        {
            throw new ConflictException($"User with phone '{request.Phone}' already exists.");
        }

        var role = await _context.UserRoles
            .FirstOrDefaultAsync(r => r.Code == request.RoleCode, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(nameof(UserRole), request.RoleCode);
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Phone = request.Phone.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            RoleId = role.Id,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        Doctor? doctor = null;
        if (request.RoleCode == LookupCodes.UserRole.Doctor && request.SpecializationId.HasValue)
        {
            doctor = new Doctor
            {
                UserId = user.Id,
                SpecializationId = request.SpecializationId.Value,
                Qualification = request.Qualification,
                ExperienceYears = request.ExperienceYears ?? 0,
                Bio = request.Bio,
                AvatarUrl = request.AvatarUrl,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new UserDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            role.Code,
            role.Name
        );
    }
}
