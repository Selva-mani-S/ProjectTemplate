namespace Solution.Domain.Enums;

public enum DrinkerPreference
{
    NonDrinker = 0,
    Drinker = 1
}

public enum MealPreference
{
    Vegetarian = 0,
    NonVegetarian = 1
}

public enum TShirtSize
{
    M,
    L,
    XL,
    XXL
}

public enum RegistrationStatus
{
    Pending,
    Submitted,
    Confirmed,
    Cancelled
}

public enum PaymentStatus
{
    Unpaid,
    PaymentDetailsSubmitted,
    PendingVerification,
    Verified,
    Rejected,
    PartiallyPaid
}

public enum PaymentMode
{
    UPI,
    BankTransfer,
    Cash,
    Cheque,
    Other
}

public static class ArrivalLocations
{
    public const string Airport = "Airport";
    public const string Egmore = "Egmore";
    public const string Central = "Central";
    public const string Other = "Other";

    public static readonly string[] All = [Airport, Egmore, Central, Other];
}
