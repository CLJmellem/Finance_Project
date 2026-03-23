using FluentValidation.TestHelper;
using TransactionsService.Application.Commands.AddTransaction;

namespace TransactionsService.Tests.Application.Validators;

/// <summary>
/// Unit tests for AddTransactionCommandValidator.
/// </summary>
public class AddTransactionCommandValidatorTests
{
    private readonly AddTransactionCommandValidator _validator = new();

    private const string ValidCardId = "507f1f77bcf86cd799439022";

    private static AddTransactionCommand ValidCommand() =>
        new(ValidCardId, 200, null, null, TransactionType.Groceries, null, false, null);

    [Fact]
    public async Task Validate_ValidCommand_ReturnsNoErrors()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
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
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
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
        var cmd = ValidCommand() with { Installments = 4, MonthAmount = null };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.MonthAmount);
    }

    [Fact]
    public async Task Validate_InstallmentsLessThanTwo_ReturnsError()
    {
        var cmd = ValidCommand() with { Installments = 1, MonthAmount = 100 };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Installments);
    }

    [Fact]
    public async Task Validate_DescriptionExceeds200Chars_ReturnsError()
    {
        var cmd = ValidCommand() with { Description = new string('A', 201) };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 200 characters.");
    }

    [Fact]
    public async Task Validate_InvalidTransactionType_ReturnsError()
    {
        var cmd = ValidCommand() with { Type = (TransactionType)999 };
        var result = await _validator.TestValidateAsync(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public async Task Validate_ValidInstallmentCommand_ReturnsNoErrors()
    {
        var cmd = ValidCommand() with { TotalAmount = 400, MonthAmount = 100, Installments = 4 };
        var result = await _validator.TestValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
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
