using Reformat.Framework.SqlSugar.Core;

namespace Reformat.Framework.SqlSugar.Aspects.interfaces;

/// <summary>
/// 事务处理接口
/// </summary>
public interface ITransaction
{
    public SugarDbConnect _dbContext { get; set; }

    public bool _isTraned { get; set; }
}