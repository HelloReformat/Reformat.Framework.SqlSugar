using Newtonsoft.Json;
using Reformat.Framework.Core.Swagger.Annotation;
using Reformat.Framework.SqlSugar.Annotation;
using SqlSugar;

namespace Reformat.Framework.SqlSugar.Core;

/// <summary>
/// Base实体类
/// </summary>
public abstract class SugarEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [SugarColumn(IsPrimaryKey = true,IsTreeKey = true)]
    [JsonConverter(typeof(ValueToStringConverter))]
    public long Id { get; set; }

    /// <summary>
    /// 逻辑删除
    /// </summary>
    [SwaggerIgnore]
    [Update(step = "delete", defvalue = 1)] // 删除便更新为1
    [Where(isSoftDelete = true, type = ConditionalType.Equal, softDeleteValue = "0")] // 查询时，只查询IsDeleted=0的数据
    public int IsDeleted { get; set; }
}