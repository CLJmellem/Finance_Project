namespace TransactionsService.Application.Constants;

/// <summary>
/// Centralized validation messages consumed by all validators.
/// </summary>
public static class ValidationMessages
{
    #region Invalid

    public const string InvalidObjectId = "Must be a valid ObjectId (24-character hex string).";
    public const string InvalidTransactionType = "Invalid transaction type.";
    public const string InvalidTotalAmount = "Total amount must be greater than zero.";
    public const string InvalidMonthAmount = "Month amount must be greater than zero.";
    public const string InvalidInstallments = "Installments must be at least 2.";

    #endregion Invalid

    #region Size

    public const string MaxLengthDescription = "Description must not exceed 200 characters.";

    #endregion Size

    #region Required / Empty

    public const string RequiredCardId = "Card ID is required.";
    public const string RequiredTransactionId = "Transaction ID is required.";
    public const string RequiredTotalAmount = "Total amount is required.";
    public const string RequiredType = "Transaction type is required.";
    public const string RequiredInstallmentsWhenMonthAmount = "Installments is required when MonthAmount is provided.";
    public const string RequiredMonthAmountWhenInstallments = "MonthAmount is required when Installments is provided.";
    public const string RequiredUserIdForAdmin = "UserId is required for admin requests.";
    public const string InvalidLastFourDigitsFormat = "LastFourDigits must contain exactly 4 numeric digits.";

    #endregion Required / Empty
}
