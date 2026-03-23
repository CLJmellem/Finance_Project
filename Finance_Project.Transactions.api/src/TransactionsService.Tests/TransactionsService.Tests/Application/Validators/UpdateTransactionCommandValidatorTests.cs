using FluentValidation.TestHelper;
using TransactionsService.Application.Commands.UpdateTransaction;

namespace TransactionsService.Tests.Application.Validators;

/// <summary>
/// Unit tests for UpdateTransactionCommandValidator.
/// </summary>
public class UpdateTransactionCommandValidatorTests
{
    private readonly UpdateTransactionCommandValidator _validator = new();

    private const string ValidCardId = "507f1f77bcf86cd799439022";
    private const string ValidTxId = "507f1f77bcf86cd799439033";

    private static UpdateTransactionCommand ValidCommand() =>
        new(ValidCardId, ValidTxId, null, null, null, null, null, null);

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
        var cmd = ValidCommand() with { CardId = "invalid" };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CardId);
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

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task Validate_TotalAmountZeroOrNegative_ReturnsError(double amount)
    {
        var cmd = ValidCommand() with { TotalAmount = amount };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount);
    }

    [Fact]
    public async Task Validate_MonthAmountWithoutInstallments_ReturnsError()
    {
        var cmd = ValidCommand() with { MonthAmount = 100, Installments = null };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Installments);
    }

    [Fact]
    public async Task Validate_InstallmentsWithoutMonthAmount_ReturnsError()
    {
        var cmd = ValidCommand() with { Installments = 3, MonthAmount = null };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.MonthAmount);
    }

    [Fact]
    public async Task Validate_DescriptionOver200Chars_ReturnsError()
    {
        var cmd = ValidCommand() with { Description = new string('X', 201) };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_InvalidType_ReturnsError()
    {
        var cmd = ValidCommand() with { Type = (TransactionType)999 };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Type);
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
