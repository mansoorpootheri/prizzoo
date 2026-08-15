using System.ComponentModel.DataAnnotations;

namespace EnterpriseBase.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}