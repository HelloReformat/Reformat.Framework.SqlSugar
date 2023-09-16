using System.ComponentModel.DataAnnotations;

namespace Reformat.Framework.SqlSugar.Domain;

public class IdsVo
{
    [Required]
    public List<long> Ids { get; set; }
}