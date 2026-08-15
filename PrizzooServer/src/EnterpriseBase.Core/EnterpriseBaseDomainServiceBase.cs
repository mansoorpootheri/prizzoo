using Abp.Domain.Services;

namespace EnterpriseBase
{
    public abstract class EnterpriseBaseDomainServiceBase : DomainService
    {
        /* Add your common members for all your domain services. */

        protected EnterpriseBaseDomainServiceBase()
        {
            LocalizationSourceName = EnterpriseBaseConsts.LocalizationSourceName;
        }
    }
}
