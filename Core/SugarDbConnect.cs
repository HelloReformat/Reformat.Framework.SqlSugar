using Reformat.Framework.Core.IOC.Attributes;
using SqlSugar;

namespace Reformat.Framework.SqlSugar.Core;

[ScopedService]
public class SugarDbConnect : IDisposable
{
    public SqlSugarClient Db { get; }
    private HttpContext httpContext;
    private IConfiguration Icfg;

    public SugarDbConnect(IConfiguration cfg, IHttpContextAccessor Accessor)
    {
        Icfg = cfg;
        httpContext = Accessor == null ? null : Accessor.HttpContext;
        string dbstring = cfg.GetSection("DbConn").Value;
        var _dbtype = DbType.SqlServer;
        var _dbtype_string = cfg.GetSection("dbtype").Value;

        // SqlSuger 内置雪花初始化
        var _snowId = cfg.GetSection("snowId").Value?? "1";
        SnowFlakeSingle.WorkId = Convert.ToInt32(_snowId);

        if (!string.IsNullOrEmpty(_dbtype_string))
        {
            switch (_dbtype_string)
            {
                case "mysql":
                    _dbtype = DbType.MySql;
                    break;
                case "mssql":
                    _dbtype = DbType.SqlServer;
                    break;
                case "dm":
                    _dbtype = DbType.Dm;
                    break;
            }
        }

        Console.WriteLine(dbstring);
        Db = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = cfg.GetSection("DbConn").Value,
            DbType = _dbtype,
            InitKeyType = InitKeyType.Attribute, //从特性读取主键和自增列信息
            IsAutoCloseConnection = true,
        });

        Db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Console.WriteLine(sql + "\r\n" +
                              Db.Utilities.SerializeObject(
                                  pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            Console.WriteLine();
        };
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}