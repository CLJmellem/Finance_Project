using CardsService.Application.Commands.CreateCard;
using CardsService.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;

namespace CardsService.Tests.Application.Validators;

/// <summary>
/// Testes unitários para CreateCardCommandValidator.
/// </summary>
public class CreateCardCommandValidatorTests
{
    private readonly CreateCardCommandValidator _validator = new();

    private static CreateCardCommand ValidCommand() =>
        new("Nubank", CardBrand.Mastercard, "1234", 5000m, 10);

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
        var longName = new string('A', 101);
        var cmd = ValidCommand() with { Name = longName };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Card name must not exceed 100 characters.");
    }

    [Fact]
    public async Task Validate_NameExactly100Chars_NoError()
    {
        var exactName = new string('B', 100);
        var cmd = ValidCommand() with { Name = exactName };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // -------------------------------------------------------------------------
    // Brand
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(CardBrand.Visa)]
    [InlineData(CardBrand.Mastercard)]
    [InlineData(CardBrand.Elo)]
    [InlineData(CardBrand.AmericanExpress)]
    [InlineData(CardBrand.Hipercard)]
    [InlineData(CardBrand.Other)]
    public async Task Validate_ValidBrand_NoError(CardBrand brand)
    {
        var cmd = ValidCommand() with { Brand = brand };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Brand);
    }

    [Fact]
    public async Task Validate_InvalidBrandValue_ReturnsError()
    {
        var cmd = ValidCommand() with { Brand = (CardBrand)999 };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Brand)
            .WithErrorMessage("Invalid card brand.");
    }

    // -------------------------------------------------------------------------
    // LastFourDigits
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("1234")]
    [InlineData("0000")]
    [InlineData("9999")]
    public async Task Validate_ValidLastFourDigits_NoError(string digits)
    {
        var cmd = ValidCommand() with { LastFourDigits = digits };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.LastFourDigits);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_EmptyLastFourDigits_ReturnsRequiredError(string? digits)
    {
        var cmd = ValidCommand() with { LastFourDigits = digits! };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.LastFourDigits);
    }

    [Theory]
    [InlineData("123")]     // 3 dígitos
    [InlineData("12345")]   // 5 dígitos
    [InlineData("12")]
    public async Task Validate_LastFourDigitsWrongLength_ReturnsError(string digits)
    {
        var cmd = ValidCommand() with { LastFourDigits = digits };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.LastFourDigits);
    }

    [Theory]
    [InlineData("123A")]    // letra
    [InlineData("12 4")]    // espaço
    [InlineData("abcd")]
    [InlineData("12.4")]
    public async Task Validate_LastFourDigitsNonNumeric_ReturnsError(string digits)
    {
        var cmd = ValidCommand() with { LastFourDigits = digits };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.LastFourDigits);
    }

    // -------------------------------------------------------------------------
    // CreditLimit
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(10000)]
    [InlineData(999999.99)]
    public async Task Validate_ValidCreditLimit_NoError(decimal limit)
    {
        var cmd = ValidCommand() with { CreditLimit = limit };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.CreditLimit);
    }

    [Fact]
    public async Task Validate_NegativeCreditLimit_ReturnsError()
    {
        var cmd = ValidCommand() with { CreditLimit = -0.01m };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.CreditLimit)
            .WithErrorMessage("Credit limit cannot be negative.");
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
    [InlineData(-1)]
    [InlineData(100)]
    public async Task Validate_InvalidDueDay_ReturnsError(int dueDay)
    {
        var cmd = ValidCommand() with { DueDay = dueDay };
        var result = await _validator.TestValidateAsync(cmd);

        result.ShouldHaveValidationErrorFor(x => x.DueDay)
            .WithErrorMessage("Due day must be between 1 and 31.");
    }
}
