namespace HotelSaaS.Domain.Exceptions;

public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message, string? code = "BUSINESS_RULE_VIOLATION")
        : base(message, code)
    {
        RuleName = ruleName;
    }
}
