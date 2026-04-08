namespace Auth.Application.Constants;

/// <summary>
/// ValidationMessages
/// </summary>
public static class ValidationMessages
{
    #region Invalid
    
    /// <summary>The invalid format username</summary>
    public const string InvalidFormatUsername = "Username must contain only letters, numbers, and underscores";
    /// <summary>The invalid email</summary>
    public const string InvalidEmail = "Email is invalid";
    /// <summary>The requires uppercase password</summary>
    public const string RequiresUppercasePassword = "Password must contain at least one uppercase letter";
    /// <summary>The requires lowercase password</summary>
    public const string RequiresLowercasePassword = "Password must contain at least one lowercase letter";
    /// <summary>The requires digit password</summary>
    public const string RequiresDigitPassword = "Password must contain at least one number";
    /// <summary>The requires special character password</summary>
    public const string RequiresSpecialCharPassword = "Password must contain at least one special character";
    /// <summary>The password not matche</summary>
    public const string PasswordNotMatche = "Passwords do not matche";
    /// <summary>The invalid user identifier.</summary>
    public const string InvalidUserId = "UserId is not valid";
    

    #endregion Invalid

    #region Size

    /// <summary>The minimum length username</summary>
    public const string MinLengthUsername = "Username must be at least 3 characters long";
    /// <summary>The maximum length username</summary>
    public const string MaxLengthUsername = "Username must be at most 20 characters long";
    /// <summary>The maximum length email</summary>
    public const string MaxLengthEmail = "Email must be at most 100 characters long";
    /// <summary>The minimum length password</summary>
    public const string MinLengthPassword = "Password must be at least 8 characters long";

    #endregion Size

    #region Required / Empty    

    /// <summary>The required username</summary>
    public const string RequiredUsername = "Username is required";
    /// <summary>The required email</summary>
    public const string RequiredEmail = "Email is required";
    /// <summary>The required username or email.</summary>
    public const string RequiredUsernameOrEmail = "Email or username is required";
    /// <summary>The required password</summary>
    public const string RequiredPassword = "Password is required";
    /// <summary>The required confirm password.</summary>
    public const string RequiredConfirmPassword = "Need to confirm password";
    /// <summary>The required user identifier.</summary>
    public const string RequiredUserId = "User id is required";
    /// <summary>The required refresh token.</summary>
    public const string RequiredRefreshToken = "Refresh token is required";

    #endregion Required / Empty
}