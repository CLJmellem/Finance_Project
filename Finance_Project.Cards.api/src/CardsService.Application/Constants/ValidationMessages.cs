namespace CardsService.Application.Constants;

/// <summary>
/// ValidationMessages
/// </summary>
public static class ValidationMessages
{
    #region Invalid

    /// <summary>Marca do cartão inválida.</summary>
    public const string InvalidCardBrand = "Invalid card brand.";

    /// <summary>Últimos quatro dígitos contêm caracteres não numéricos.</summary>
    public const string InvalidLastFourDigitsFormat = "Must contain only numeric characters.";

    /// <summary>Limite de crédito negativo.</summary>
    public const string InvalidCreditLimit = "Credit limit cannot be negative.";

    /// <summary>Dia de vencimento fora do intervalo permitido.</summary>
    public const string InvalidDueDay = "Due day must be between 1 and 31.";

    #endregion Invalid

    #region Size

    /// <summary>Nome do cartão excede o limite de caracteres.</summary>
    public const string MaxLengthCardName = "Card name must not exceed 100 characters.";

    /// <summary>Últimos quatro dígitos com tamanho incorreto.</summary>
    public const string ExactLengthLastFourDigits = "Must be exactly 4 digits.";

    #endregion Size

    #region Required / Empty

    /// <summary>ID do cartão obrigatório.</summary>
    public const string RequiredCardId = "Card ID is required.";

    /// <summary>Nome do cartão obrigatório.</summary>
    public const string RequiredCardName = "Card name is required.";

    /// <summary>Últimos quatro dígitos obrigatórios.</summary>
    public const string RequiredLastFourDigits = "Last four digits are required.";

    #endregion Required / Empty
}