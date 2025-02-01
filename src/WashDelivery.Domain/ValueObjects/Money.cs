using Microsoft.EntityFrameworkCore;
using WashDelivery.Domain.Common;

namespace WashDelivery.Domain.ValueObjects;

[Owned]
public class Money : ValueObject
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "PLN";

    private Money() { } // For EF Core

    public Money(decimal amount, string currency = "PLN")
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public static Money operator *(Money money, int multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }
} 