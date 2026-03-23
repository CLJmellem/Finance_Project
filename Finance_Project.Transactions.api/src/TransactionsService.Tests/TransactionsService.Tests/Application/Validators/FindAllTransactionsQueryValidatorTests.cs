using FluentValidation.TestHelper;
using TransactionsService.Application.Queries.FindAllTransactions;

namespace TransactionsService.Tests.Application.Validators;

/// <summary>
/// Unit tests for FindAllTransactionsQueryValidator.
/// </summary>
public class FindAllTransactionsQueryValidatorTests
{
    private readonly FindAllTransactionsQueryValidator _validator = new();

    private const string ValidObjectId = "507f1f77bcf86cd799439011";

    private static FindAllTransactionsQuery EmptyQuery() =>
        new(null, null, null, null, null, 0, 0);

    [Fact]
    public async Task Validate_AllNullFilters_ReturnsNoErrors()
    {
        var result = await _validator.TestValidateAsync(EmptyQuery());
        result.IsValid.Should().BeTrue();
    }

    // ── UserId ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValidUserId_NoError()
    {
        var query = EmptyQuery() with { UserId = ValidObjectId };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_InvalidUserId_ReturnsError()
    {
        var query = EmptyQuery() with { UserId = "not-an-objectid" };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Must be a valid ObjectId (24-character hex string).");
    }

    // ── CardId ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValidCardId_NoError()
    {
        var query = EmptyQuery() with { CardId = ValidObjectId };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldNotHaveValidationErrorFor(x => x.CardId);
    }

    [Fact]
    public async Task Validate_InvalidCardId_ReturnsError()
    {
        var query = EmptyQuery() with { CardId = "abc" };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(x => x.CardId);
    }

    // ── TransactionId ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValidTransactionId_NoError()
    {
        var query = EmptyQuery() with { TransactionId = ValidObjectId };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldNotHaveValidationErrorFor(x => x.TransactionId);
    }

    [Fact]
    public async Task Validate_InvalidTransactionId_ReturnsError()
    {
        var query = EmptyQuery() with { TransactionId = "xyz123" };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(x => x.TransactionId);
    }

    // ── LastFourDigits ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("1234")]
    [InlineData("0000")]
    [InlineData("9999")]
    public async Task Validate_ValidLastFourDigits_NoError(string digits)
    {
        var query = EmptyQuery() with { LastFourDigits = digits };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldNotHaveValidationErrorFor(x => x.LastFourDigits);
    }

    [Theory]
    [InlineData("123")]       // 3 digits
    [InlineData("12345")]     // 5 digits
    [InlineData("12ab")]      // letters
    [InlineData("12 4")]      // space
    [InlineData("abcd")]      // all letters
    public async Task Validate_InvalidLastFourDigits_ReturnsError(string digits)
    {
        var query = EmptyQuery() with { LastFourDigits = digits };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(x => x.LastFourDigits)
            .WithErrorMessage("LastFourDigits must contain exactly 4 numeric digits.");
    }
}
