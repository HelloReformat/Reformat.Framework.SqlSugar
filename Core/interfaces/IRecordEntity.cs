using Reformat.Framework.Core.Swagger.Annotation;

namespace Reformat.Framework.SqlSugar.Core.interfaces;

public interface IRecordEntity
{
    /// <summary>
    /// 创建人
    /// </summary>
    public long? CreateBy { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    /// <summary>
    /// 更新人
    /// </summary>
    public long? UpdateBy { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}