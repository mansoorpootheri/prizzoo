using Abp.Net.Mail;
using Abp.UI;
using EnterpriseBase.Configuration.Host.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Configuration
{
    public abstract class SettingsAppServiceBase : EnterpriseBaseAppServiceBase
    {
        private readonly IEmailSender _emailSender;
        private readonly IAppConfigurationAccessor _configurationAccessor;

        protected SettingsAppServiceBase(
            IEmailSender emailSender,
            IAppConfigurationAccessor configurationAccessor)
        {
            _emailSender = emailSender;
            _configurationAccessor = configurationAccessor;
        }

        #region Send Test Email

        public async Task SendTestEmail(SendTestEmailInput input)
        {
            try
            {
                await _emailSender.SendAsync(
                    input.EmailAddress,
                    L("TestEmail_Subject"),
                    L("TestEmail_Body")
                );
            }
            catch (Exception e)
            {
                throw new UserFriendlyException("An error was encountered while sending an email. " + e.Message, e);
            }
        }

        #endregion
    }
}
