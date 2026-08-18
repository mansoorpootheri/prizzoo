using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Application.Ratings.Dto
{
    public class RateProductDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }
    }

    public class MyProductRatingDto
    {
        public Guid ProductId { get; set; }
        public int? Stars { get; set; }
    }
}
