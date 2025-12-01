using CustomerAPI.Data;
using CustomerAPI.DTO.BvnNinVerification;
using CustomerAPI.Entities;
using CustomerAPI.External;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Services;
using FaceAiSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CustomerProfile.Test.UnitTests;

public class NinBvnServiceTests : IDisposable
{
    private readonly UserProfileDbContext _dbContext;
    private readonly QuickVerifyBvnNinService _quickVerifyBvnNinService;
    private readonly FaceRecognitionService _faceRecognitionService;
    private readonly JwtTokenProviderService _jwtTokenProvider;

    private readonly NinBvnService _sut;

    private readonly BvnSearchRequest _validBvnSearchRequest = new(Bvn: "12345678901");
    private readonly BvnSearchRequest _invalidBvnSearchRequest = new(Bvn: "12345");
    private readonly string _validPhoneNumber = "09012345678";
    private readonly string _validEmail = "user@email.com";
    private readonly string _validUsername = "username1";
    private readonly Guid id = Guid.NewGuid();

    public NinBvnServiceTests()
    {
        var dbContextBuilder = new DbContextOptionsBuilder<UserProfileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        _dbContext = new UserProfileDbContext(dbContextBuilder);

        IOptions<JwtOptions> jt = Options.Create(new JwtOptions
        {
            SecretKey = "superSafeHash256%4Test=sceretucvhsaoaiq477(*^%$)(8=hfdhncmi_0987e7sjhkkshdhdkj",
            Audience = "Test",
            Issuer = "Test"
        });

        _quickVerifyBvnNinService = Substitute.For<QuickVerifyBvnNinService>(new HttpClient());
        _faceRecognitionService = Substitute.For<FaceRecognitionService>(null, null, null);
        _jwtTokenProvider = Substitute.For<JwtTokenProviderService>(jt);

        _sut = new NinBvnService(_dbContext, _quickVerifyBvnNinService, _faceRecognitionService, _jwtTokenProvider);
    }

    [Fact]
    public async Task Search_Invalid_BVN_Returns_Error()
    {
        BvnSearchRequest searchRequest = new(Bvn: "12345");
        var result = await _sut.SearchBvnAsync(Guid.NewGuid(), searchRequest);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }
    [Fact]
    public async Task Search_Non_Existent_User_Returns_Error()
    {
        BvnSearchRequest searchRequest = new(Bvn: "12345678901");
        var result = await _sut.SearchBvnAsync(Guid.NewGuid(), searchRequest);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.IsType<string>(result.ErrorMessage);
    }

    //Pass phnone and valid email as theory

    [Fact]
    public async Task Search_But_QuickverifyService_Is_Down_Returns_Error()
    {
        var response = await _quickVerifyBvnNinService.BvnSearchRequest(_validBvnSearchRequest.Bvn);
        Assert.Null(response);

        var result = await _sut.SearchBvnAsync(GetValidUserId(), _validBvnSearchRequest);

        Assert.False(result.IsSuccess);
        Assert.IsType<string>(result.ErrorMessage);
    }

    [Fact]
    public async Task Successful_Search_Returns_Success()
    {
        // Arrange
        var validUserId = GetValidUserId();
        var bvnApiResponse = new BvnApiResponse
        {
            Status = true,
            Detail = "BVN details found",
            Data = new() { Base64Image = "test_image_string" }
        };

        _quickVerifyBvnNinService.BvnSearchRequest(_validBvnSearchRequest.Bvn)
            .Returns(Task.FromResult<BvnApiResponse?>(bvnApiResponse));

        // Act
        var result = await _sut.SearchBvnAsync(GetValidUserId(), _validBvnSearchRequest);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
    private Guid GetValidUserId()
    {
        var newUser = UserProfile.CreateNewUser(_validPhoneNumber, email: _validEmail, username: _validUsername);
        _dbContext.Add(newUser);
        _dbContext.SaveChangesAsync();
        return newUser.Id;
    }
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}