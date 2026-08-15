using Abp;
using Abp.Application.Features;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Abp.MultiTenancy;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.Zero.EntityFrameworkCore;
using Castle.Core.Logging;
using EnterpriseBase.Authorization.Roles;
using EnterpriseBase.Authorization.Users;
using EnterpriseBase.Editions;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseBase.MultiTenancy;

public class TenantManager : AbpTenantManager<Tenant, User>
{
    public IAbpSession AbpSession { get; set; }
    public IEventBus EventBus { get; set; }

    public ILogger Logger { get; set; }

    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly UserManager _userManager;
    private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
    private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly EditionManager _editionManager;
    protected readonly IBackgroundJobManager _backgroundJobManager;




    public TenantManager(
        IRepository<Tenant> tenantRepository,
        IRepository<TenantFeatureSetting, long> tenantFeatureRepository,
        IUnitOfWorkManager unitOfWorkManager,
        UserManager userManager,
        INotificationSubscriptionManager notificationSubscriptionManager,
        IAbpZeroDbMigrator abpZeroDbMigrator,
        IPasswordHasher<User> passwordHasher,
        EditionManager editionManager,
        IBackgroundJobManager backgroundJobManager,
        IAbpZeroFeatureValueStore featureValueStore)
        : base(
            tenantRepository,
            tenantFeatureRepository,
            editionManager,
            featureValueStore)
    {
        AbpSession = NullAbpSession.Instance;
        EventBus = NullEventBus.Instance;
        Logger = NullLogger.Instance;

        _editionManager = editionManager;
        _unitOfWorkManager = unitOfWorkManager;
        _userManager = userManager;
        _notificationSubscriptionManager = notificationSubscriptionManager;
        _abpZeroDbMigrator = abpZeroDbMigrator;
        _passwordHasher = passwordHasher;
        _backgroundJobManager = backgroundJobManager;
    }

    
}
