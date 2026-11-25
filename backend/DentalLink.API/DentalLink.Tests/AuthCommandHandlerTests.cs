//using DentalLink.Application.Common.Interfaces;
//using DentalLink.Application.Features.Auth;
//using DentalLink.Domain.Entities;
//using DentalLink.Domain.Exceptions;
//using DentalLink.Infrastructure.Data;
//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;

//namespace DentalLink.Tests.Features;

//public class AuthCommandHandlerTests
//{
//    private DentalLinkDbContext CreateDbContext(string dbName)
//    {
//        var options = new DbContextOptionsBuilder<DentalLinkDbContext>()
//            .UseInMemoryDatabase(databaseName: dbName)
//            .Options;
//        return new DentalLinkDbContext(options);
//    }

//    [Fact]
//    public async Task Login_WithValidCredentials_ShouldReturnTokensAndUser()
//    {
//        // Arrange
//        var db = CreateDbContext(Guid.NewGuid().ToString());
//        var role = new UserRole { Id = 1, Code = "admin", Name = "Administrator" };
//        var user = new User
//        {
//            Id = 10,
//            FullName = "Администратор Системы",
//            Email = "admin@dentallink.tj",
//            PasswordHash = "hashed_pw",
//            Phone = "+992900000001",
//            RoleId = 1,
//            Role = role,
//            IsActive = true
//        };

//        db.UserRoles.Add(role);
//        db.Users.Add(user);
//        await db.SaveChangesAsync();

//        var hasherMock = new Mock<IPasswordHasher>();
//        hasherMock.Setup(h => h.VerifyPassword("Password123!", "hashed_pw")).Returns(true);

//        var jwtMock = new Mock<IJwtTokenService>();
//        jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("fake_access_token");
//        jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("raw_refresh_token");
//        jwtMock.Setup(j => j.HashToken("raw_refresh_token")).Returns("hashed_refresh_token");

//        var handler = new LoginCommandHandler(db, hasherMock.Object, jwtMock.Object);

//        // Act
//        var result = await handler.Handle(new LoginCommand("admin@dentallink.tj", "Password123!"), CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.AccessToken.Should().Be("fake_access_token");
//        result.RefreshToken.Should().Be("raw_refresh_token");
//        result.User.Email.Should().Be("admin@dentallink.tj");
//        result.User.RoleCode.Should().Be("admin");

//        var tokenInDb = await db.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == 10);
//        tokenInDb.Should().NotBeNull();
//        tokenInDb!.TokenHash.Should().Be("hashed_refresh_token");
//        tokenInDb.IsActive.Should().BeTrue();
//    }

//    [Fact]
//    public async Task Login_WithInvalidPassword_ShouldThrowUnauthorizedException()
//    {
//        // Arrange
//        var db = CreateDbContext(Guid.NewGuid().ToString());
//        var role = new UserRole { Id = 1, Code = "admin", Name = "Administrator" };
//        var user = new User
//        {
//            Id = 10,
//            FullName = "Администратор",
//            Email = "admin@dentallink.tj",
//            PasswordHash = "hashed_pw",
//            Phone = "+992900000001",
//            RoleId = 1,
//            Role = role,
//            IsActive = true
//        };

//        db.UserRoles.Add(role);
//        db.Users.Add(user);
//        await db.SaveChangesAsync();

//        var hasherMock = new Mock<IPasswordHasher>();
//        hasherMock.Setup(h => h.VerifyPassword("WrongPassword", "hashed_pw")).Returns(false);

//        var jwtMock = new Mock<IJwtTokenService>();
//        var handler = new LoginCommandHandler(db, hasherMock.Object, jwtMock.Object);

//        // Act & Assert
//        var act = async () => await handler.Handle(new LoginCommand("admin@dentallink.tj", "WrongPassword"), CancellationToken.None);
//        await act.Should().ThrowAsync<UnauthorizedException>()
//            .WithMessage("*Invalid email or password*");
//    }
//}
