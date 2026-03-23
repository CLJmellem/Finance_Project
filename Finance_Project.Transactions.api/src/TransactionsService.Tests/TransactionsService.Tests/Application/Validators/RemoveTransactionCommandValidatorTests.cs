using FluentValidation.TestHelper;
using TransactionsService.Application.Commands.RemoveTransaction;

namespace TransactionsService.Tests.Application.Validators;

/// <summary>
/// Unit tests for RemoveTransactionCommandValidator.
/// </summary>
public class RemoveTransactionCommandValidatorTests
{
    private readonly RemoveTransactionCommandValidator _validator = new();

    private const string ValidCardId = "507f1f77bcf86cd799439022";
    private const string ValidTxId = "507f1f77bcf86cd799439033";

    private static RemoveTransactionCommand ValidCommand() =>
        new(ValidCardId, ValidTxId, null);

    [Fact]
    public async Task Validate_ValidCommand_ReturnsNoErrors()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyCardId_ReturnsError(string? cardId)
    {
        var cmd = ValidCommand() with { CardId = cardId! };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CardId);
    }

    [Fact]
    public async Task Validate_InvalidObjectIdCardId_ReturnsError()
    {
        var cmd = ValidCommand() with { CardId = "not-an-objectid" };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CardId)
            .WithErrorMessage("Must be a valid ObjectId (24-character hex string).");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyTransactionId_ReturnsError(string? txId)
    {
        var cmd = ValidCommand() with { TransactionId = txId! };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TransactionId);
    }

    [Fact]
    public async Task Validate_InvalidObjectIdTransactionId_ReturnsError()
    {
        var cmd = ValidCommand() with { TransactionId = "not-an-objectid" };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TransactionId)
            .WithErrorMessage("Must be a valid ObjectId (24-character hex string).");
    }

    [Fact]
    public async Task Validate_InvalidUserIdFormat_ReturnsError()
    {
        var cmd = ValidCommand() with { UserId = "not-an-objectid" };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Must be a valid ObjectId (24-character hex string).");
    }

    [Fact]
    public async Task Validate_ValidUserId_NoError()
    {
        var cmd = ValidCommand() with { UserId = "507f1f77bcf86cd799439011" };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_NullUserId_NoError()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}
