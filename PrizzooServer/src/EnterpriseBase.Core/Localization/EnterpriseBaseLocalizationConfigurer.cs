using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace EnterpriseBase.Localization;

public static class EnterpriseBaseLocalizationConfigurer
{
    public static void Configure(ILocalizationConfiguration localizationConfiguration)
    {
        localizationConfiguration.Sources.Add(
            new DictionaryBasedLocalizationSource(EnterpriseBaseConsts.LocalizationSourceName,
                new XmlEmbeddedFileLocalizationDictionaryProvider(
                    typeof(EnterpriseBaseLocalizationConfigurer).GetAssembly(),
                    "EnterpriseBase.Localization.SourceFiles"
                )
            )
        );
    }
}
