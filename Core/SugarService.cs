using Reformat.Framework.Core.Core;
using Reformat.Framework.Core.Exceptions;
using Reformat.Framework.Core.IOC.Attributes;
using Reformat.Framework.Core.IOC.Services;
using Reformat.Framework.Core.JWT.interfaces;
using Reformat.Framework.SqlSugar.Aspects.interfaces;
using Reformat.Framework.SqlSugar.Core.interfaces;
using Reformat.Framework.SqlSugar.Domain;
using SqlSugar;

namespace Reformat.Framework.SqlSugar.Core;

public abstract class SugarService<T> : BaseScopedService,ITransaction where T : SugarEntity, new()
{
    // Warning：仅可用于系统内部使用，不可进行业务调用和修改
    [Autowired] public SugarDbConnect _dbContext { get; set; }

    [Autowired] protected IUserService UserService;
    
    protected SugarService(IocScoped iocScoped) : base(iocScoped) { }

    private bool isBigSave = false;

    public bool _isTraned { get; set; } = false;

    protected SqlSugarClient SqlSugar => _dbContext.Db;

    public ISugarQueryable<T> GetQueryable(bool isLogic)
    {
        ISugarQueryable<T> sugarQueryable = SqlSugar.Queryable<T>();
        if (isLogic) sugarQueryable.Where(i => i.IsDeleted == 0);
        return sugarQueryable;
    }

    public IUpdateable<T> GetUpdateable()
    {
        return SqlSugar.Updateable<T>();
    }

    public IDeleteable<T> GetDeleteable()
    {
        IDeleteable<T> deleteable = SqlSugar.Deleteable<T>();
        // if (isLogic) deleteable.Where(i => i.IsDeleted == 0);
        return SqlSugar.Deleteable<T>();
    }

    protected Expressionable<T> GetExpressionable()
    {
        return Expressionable.Create<T>();
    }

    public virtual T GetById(long id, bool isLogic) => GetQueryable(isLogic).InSingle(id);

    public virtual List<T> GetByIds(long[] ids, bool isLogic = true) =>
        GetQueryable(isLogic).Where(item => ids.Contains(item.Id)).ToList();

    public virtual List<T> GetList(bool isLogic = true) => GetQueryable(isLogic).Select<T>().ToList();

    //public Task<PageList<T>> GetPage(SQLQuery<T> query) => _dbContext.Select<T, T>(query, SqlSugar.Queryable<T>(true));

    public async Task<List<IConditionalModel>> BuildWhereCondition<T1>(SqlQuery<T1> query)
    {
        return await QueryHelper.GetWhere(query);
    }

    public async Task<ISugarQueryable<T2>> BuildWhereQueryable<T1, T2>(SqlQuery<T1> query,
        ISugarQueryable<T2> sugarQueryable)
    {
        List<IConditionalModel> condition = await BuildWhereCondition(query);
        if (condition != null && condition.Count > 0)
        {
            sugarQueryable = sugarQueryable.Where(" true ").Where(condition);
        }

        return sugarQueryable;
    }

    public ISugarQueryable<T2> BuildOrderQueryable<T1, T2>(SqlQuery<T1> query, ISugarQueryable<T2> sugarQueryable)
    {
        string orderString = QueryHelper.GetOrderString<T>(query.OrderCondition);
        if (!string.IsNullOrEmpty(orderString))
        {
            sugarQueryable = sugarQueryable.OrderBy(orderString);
        }

        return sugarQueryable;
    }

    public async Task<PageList<T1>> BuildPage<T1>(SqlQuery<T1> query, ISugarQueryable<T1> sugarQueryable,
        bool isWhere = true, bool isOrder = true)
    {
        if (isWhere) sugarQueryable = await BuildWhereQueryable(query, sugarQueryable);
        ;
        if (isOrder) sugarQueryable = BuildOrderQueryable(query, sugarQueryable);
        ;

        RefAsync<int> totalCount = 0;

        List<T1> list = null;
        if (query.PageSize > 0)
        {
            list = await sugarQueryable.ToPageListAsync(query.PageIndex, query.PageSize, totalCount);
        }
        else
        {
            list = await sugarQueryable.ToListAsync();
            totalCount = list.Count;
        }

        return new PageList<T1>()
        {
            Total = totalCount,
            List = list
        };
    }

    public virtual async Task<PageList<T>> GetPage(SqlQuery<T> query) => await GetPage(query, GetQueryable(true));

    public virtual async Task<PageList<T>> GetPage(SqlQuery<T> query, ISugarQueryable<T> sugarQueryable) =>
        await BuildPage(query, sugarQueryable);

    /// <summary>
    /// 单个保存
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public long Save(T entity)
    {
        SaveVerify(ref entity);
        updateCreateMes(ref entity);
        return SqlSugar.Insertable<T>(entity).ExecuteReturnSnowflakeId();
    }

    /// <summary>
    /// 批量插入
    /// </summary>
    /// <param name="entities"></param>
    /// <returns>成功条数</returns>
    public int SaveBatch(List<T> entities)
    {
        entities.ForEach(item => SaveVerify(ref item));
        if (entities.Count > 1000 && isBigSave)
        {
            return SaveBigBatch(entities);
        }

        entities.ForEach(item => updateCreateMes(ref item));
        return SqlSugar.Insertable(entities).ExecuteReturnSnowflakeIdList().Count;
    }

    /// <summary>
    /// 大数据 插入（1000条以上性能无敌手）
    /// 【使用要求】
    /// 1.BulkCopy MySql连接字符串需要添加 AllowLoadLocalInfile=true;
    /// 2.Mysql数据库执行 SET GLOBAL local_infile=1
    /// 3.查看是否开启成功 show global variables like 'local_infile';
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    protected int SaveBigBatch(List<T> entities)
    {
        entities.ForEach(item => SaveVerify(ref item));
        foreach (dynamic entity in entities)
        {
            entity.Id = SnowFlakeSingle.instance.getID();
        }
        entities.ForEach(item => updateCreateMes(ref item));
        return SqlSugar.Fastest<T>().PageSize(100000).BulkCopy(entities);
    }
    
    public virtual void SaveVerify(ref T entity)
    {
        entity.Id = 0;
    }

    public bool UpdateById(T entity, bool isIgnoreNull = true)
    {
        UpdateVerify(ref entity);
        updateUpdateMes(ref entity);
        return SqlSugar.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: isIgnoreNull).ExecuteCommand() > 0;
    }

    public bool UpdateBatch(List<T> entitys, bool isIgnoreNull = true)
    {
        entitys.ForEach(item => UpdateVerify(ref item));
        entitys.ForEach(item => updateCreateMes(ref item));
        int executeCommand = SqlSugar.Updateable<T>(entitys).IgnoreColumns(ignoreAllNullColumns: isIgnoreNull)
            .ExecuteCommand();
        return executeCommand == entitys.Count;
    }

    public virtual void UpdateVerify(ref T entity)
    {
        if (entity.Id == null || entity.Id == 0) throw new ValidateException("修改时 ID 不能为空");
    }
    
    protected bool DeleteById(long id, bool isLogic = true)
    {
        if (isLogic)
        {
            return SqlSugar.Deleteable<T>().In(id).IsLogic().ExecuteCommand() > 0;
        }

        return SqlSugar.Deleteable<T>().In(id).ExecuteCommand() > 0;
    }

    public bool DeleteByIds(long[] ids, bool isLogic = true)
    {
        if (isLogic)
        {
            return SqlSugar.Deleteable<T>().In(ids).IsLogic().ExecuteCommand() == ids.Count();
        }

        return SqlSugar.Deleteable<T>().In(ids).ExecuteCommand() == ids.Count();
    }

    public bool ClearTable()
    {
        return SqlSugar.DbMaintenance.TruncateTable<T>();
    }

    
    
    

    /// <summary>
    /// 获取当前用户
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PermissionException"></exception>
    public IUser GetCurrentUser()
    {
        IUser currentUser = UserService.GetCurrentUser();
        if (currentUser == null)
        {
            throw new PermissionException("当前用户状态异常");
        }

        return currentUser;
    }

    private void updateCreateMes(ref T entity)
    {
        IRecordEntity recordInfo = entity as IRecordEntity;
        if (recordInfo != null)
        {
            IUser currentUser = GetCurrentUser();
            recordInfo.CreateBy = currentUser.Id;
            recordInfo.CreateTime = DateTime.Now;
            recordInfo.UpdateBy = currentUser.Id;
            recordInfo.UpdateTime = DateTime.Now;
        }
    }

    private void updateUpdateMes(ref T entity)
    {
        IRecordEntity recordInfo = entity as IRecordEntity;
        if (recordInfo != null)
        {
            IUser currentUser = GetCurrentUser();
            recordInfo.UpdateBy = currentUser.Id;
            recordInfo.UpdateTime = DateTime.Now;
        }
    }
}