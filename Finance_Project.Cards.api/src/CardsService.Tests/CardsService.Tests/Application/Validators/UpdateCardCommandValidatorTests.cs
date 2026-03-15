using CardsService.Application.Commands.UpdateCard;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CardsService.Tests.Application.Validators;

/// <summary>
/// Testes unitários para UpdateCardCommandValidator.
/// </summary>
public class UpdateCardCommandValidatorTests
{
    private readonly UpdateCardCommandValidator _validator = new();

    private static UpdateCardCommand ValidCommand() =>
        new("507f1f77bcf86cd799439011", "Cartão Atualizado", 5000m, 10, true);

    // -------------------------------------------------------------------------
    // Comando válido — sem erros
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_ValidCommand_ReturnsNoErrors()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // CardId
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_EmptyCardId_ReturnsError(string? cardId)
    {
        var cmd = ValidCommand() with { CardId = cardId! };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.CardId)
            .WithErrorMessage("Card ID is required.");
    }

    [Fact]
    public async Task Validate_ValidCardId_NoError()
    {
        var cmd = ValidCommand() with { CardId = "507f1f77bcf86cd799439011" };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.CardId);
    }

    // -------------------------------------------------------------------------
    // Name
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_EmptyName_ReturnsError(string? name)
    {
        var cmd = ValidCommand() with { Name = name! };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Card name is required.");
    }

    [Fact]
    public async Task Validate_NameExceeds100Chars_ReturnsError()
    {
        var longName = new string('X', 101);
        var cmd = ValidCommand() with { Name = longName };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Card name must not exceed 100 characters.");
    }

    [Fact]
    public async Task Validate_NameExactly100Chars_NoError()
    {
        var exactName = new string('Y', 100);
        var cmd = ValidCommand() with { Name = exactName };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_NameOneSingleChar_NoError()
    {
        var cmd = ValidCommand() with { Name = "A" };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // -------------------------------------------------------------------------
    // CreditLimit
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50000)]
    public async Task Validate_ValidCreditLimit_NoError(decimal limit)
    {
        var cmd = ValidCommand() with { CreditLimit = limit };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.CreditLimit);
    }

    [Fact]
    public async Task Validate_NegativeCreditLimit_ReturnsError()
    {
        var cmd = ValidCommand() with { CreditLimit = -1m };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.CreditLimit)
            .WithErrorMessage("Credit limit cannot be negative.");
    }

    [Fact]
    public async Task Validate_ZeroCreditLimit_NoError()
    {
        var cmd = ValidCommand() with { CreditLimit = 0m };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.CreditLimit);
    }

    // -------------------------------------------------------------------------
    // DueDay
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(31)]
    public async Task Validate_ValidDueDay_NoError(int dueDay)
    {
        var cmd = ValidCommand() with { DueDay = dueDay };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.DueDay);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(-5)]
    [InlineData(99)]
    public async Task Validate_InvalidDueDay_ReturnsError(int dueDay)
    {
        var cmd = ValidCommand() with { DueDay = dueDay };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.DueDay)
            .WithErrorMessage("Due day must be between 1 and 31.");
    }

    // -------------------------------------------------------------------------
    // Múltiplos erros ao mesmo tempo
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_MultipleViolations_ReturnsAllErrors()
    {
        var cmd = new UpdateCardCommand(
            CardId: "",
            Name: "",
            CreditLimit: -10m,
            DueDay: 0,
            IsActive: false);

        var result = await _validator.TestValidateAsync(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        result.ShouldHaveValidationErrorFor(x => x.CardId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.CreditLimit);
        result.ShouldHaveValidationErrorFor(x => x.DueDay);
    }
}
