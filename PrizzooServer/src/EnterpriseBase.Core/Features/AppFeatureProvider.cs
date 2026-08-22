using Abp.Application.Features;
using Abp.Localization;
using Abp.Runtime.Validation;
using Abp.UI.Inputs;

namespace EnterpriseBase.Features
{
    public class AppFeatureProvider : FeatureProvider
    {
        public override void SetFeatures(IFeatureDefinitionContext context)
        {
            // IMPORTANT: Default values use "-1" / "__unset__" sentinels so that ABP's
            // SetFeatureValueAsync never considers a real value (like "0" or "false")
            // equal to the default — which would cause ABP to DELETE the DB row,
            // and the seed would then re-insert the original value on next startup.

            context.Create(
                AppFeatures.MaxUserCount,
                defaultValue: "-1",
                displayName: L("MaximumUserCount"),
                description: L("MaximumUserCount_Description"),
                inputType: new SingleLineStringInputType(new NumericValueValidator(0, int.MaxValue))
            )[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            {
                ValueTextNormalizer = value => value == "0" ? L("Unlimited") : new FixedLocalizableString(value),
                IsVisibleOnPricingTable = true
            };

            context.Create(
                AppFeatures.MaxTransactionEntries,
                defaultValue: "-1",
                displayName: L("MaximumTransactionEntries"),
                description: L("MaximumTransactionEntries_Description"),
                inputType: new SingleLineStringInputType(new NumericValueValidator(0, int.MaxValue))
            )[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            {
                ValueTextNormalizer = value => value == "0" ? L("Unlimited") : new FixedLocalizableString(value),
                IsVisibleOnPricingTable = true
            };

            context.Create(
                AppFeatures.HasAdvancedReports,
                defaultValue: "__unset__",
                displayName: L("AdvancedReports"),
                inputType: new CheckboxInputType()
            )[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            {
                IsVisibleOnPricingTable = true
            };

            context.Create(
                AppFeatures.HasApiAccess,
                defaultValue: "__unset__",
                displayName: L("ApiAccess"),
                inputType: new CheckboxInputType()
            )[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            {
                IsVisibleOnPricingTable = true
            };

            context.Create(
                AppFeatures.HasMultiCurrency,
                defaultValue: "__unset__",
                displayName: L("MultiCurrency"),
                inputType: new CheckboxInputType()
            )[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            {
                IsVisibleOnPricingTable = true
            };          
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, EnterpriseBaseConsts.LocalizationSourceName);
        }
    }
}
