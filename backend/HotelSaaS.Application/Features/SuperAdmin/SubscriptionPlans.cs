using FluentValidation;
using HotelSaaS.Application.Common.Interfaces;
using HotelSaaS.Domain.Entities;
using HotelSaaS.Domain.Exceptions;
using HotelSaaS.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelSaaS.Application.Features.SuperAdmin;

public record GetSubscriptionPlansQuery : IRequest<IReadOnlyList<SubscriptionPlanDto>>;

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionPlansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _context.SubscriptionPlans
            .OrderBy(p => p.Price)
            .ToListAsync(cancellationToken);

        return plans.Select(p => new SubscriptionPlanDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            BillingCycle = p.BillingCycle,
            MaxBookingsPerMonth = p.MaxBookingsPerMonth,
            FeaturesJson = p.FeaturesJson,
            IsActive = p.IsActive
        }).ToList();
    }
}

public record CreateSubscriptionPlanCommand : IRequest<SubscriptionPlanDto>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string BillingCycle { get; init; } = "Monthly";
    public int MaxBookingsPerMonth { get; init; }
    public string FeaturesJson { get; init; } = "[]";
}

public class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BillingCycle).NotEmpty();
    }
}

public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, SubscriptionPlanDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateSubscriptionPlanCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionPlanDto> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            throw new BusinessRuleViolationException("Access denied. SuperAdmin role required.", "SUPERADMIN_REQUIRED");
        }

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            BillingCycle = request.BillingCycle,
            MaxBookingsPerMonth = request.MaxBookingsPerMonth,
            FeaturesJson = request.FeaturesJson,
            IsActive = true
        };

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            BillingCycle = plan.BillingCycle,
            MaxBookingsPerMonth = plan.MaxBookingsPerMonth,
            FeaturesJson = plan.FeaturesJson,
            IsActive = plan.IsActive
        };
    }
}

public record UpdateSubscriptionPlanCommand : IRequest<SubscriptionPlanDto>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string BillingCycle { get; init; } = "Monthly";
    public int MaxBookingsPerMonth { get; init; }
    public string FeaturesJson { get; init; } = "[]";
    public bool IsActive { get; init; } = true;
}

public class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, SubscriptionPlanDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSubscriptionPlanCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionPlanDto> Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            throw new BusinessRuleViolationException("Access denied. SuperAdmin role required.", "SUPERADMIN_REQUIRED");
        }

        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan == null)
        {
            throw new EntityNotFoundException("SubscriptionPlan", request.Id);
        }

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.Price = request.Price;
        plan.BillingCycle = request.BillingCycle;
        plan.MaxBookingsPerMonth = request.MaxBookingsPerMonth;
        plan.FeaturesJson = request.FeaturesJson;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            BillingCycle = plan.BillingCycle,
            MaxBookingsPerMonth = plan.MaxBookingsPerMonth,
            FeaturesJson = plan.FeaturesJson,
            IsActive = plan.IsActive
        };
    }
}
