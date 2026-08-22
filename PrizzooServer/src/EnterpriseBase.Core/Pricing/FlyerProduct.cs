using Abp.Domain.Entities.Auditing;
using EnterpriseBase.MasterData;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseBase.Pricing
{
    /// <summary>
    /// Links a Flyer to a Product it features - master/detail, one Flyer has
    /// many FlyerProduct rows. Deliberately carries no price of its own: a
    /// flyer's product is expected to already have a real Price at that
    /// store, and the price shown for it is always looked up live from
    /// there (see FlyerAppService.InsertItemsAsync/BuildFlyerDetailDtosAsync)
    /// rather than frozen onto this link. Not IMustHaveTenant/IMayHaveTenant,
    /// same shared-public-catalog reasoning as Store/Product/Price/Flyer.
    /// </summary>
    [Table("FlyerProducts")]
    public class FlyerProduct : FullAuditedEntity<Guid>
    {
        public Guid FlyerId { get; set; }
        [ForeignKey("FlyerId")]
        public virtual Flyer Flyer { get; set; }

        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
