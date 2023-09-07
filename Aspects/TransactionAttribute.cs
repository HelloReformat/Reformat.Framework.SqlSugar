using AspectInjector.Broker;
using Reformat.Framework.SqlSugar.Aspects.interfaces;

namespace Reformat.Framework.SqlSugar.Aspects;

/// <summary>
/// AOP:事务处理
/// 20230529：支持嵌套事务，支持异常回滚
/// Beta：待测试
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Aspect(Scope.PerInstance)]
[Injection(typeof(TransactionAttribute))]
public class TransactionAttribute : Attribute
{
    
    [Advice(Kind.Before, Targets = Target.Method)]
    public void Before([Argument(Source.Instance)] object Instance)
    {
        ITransaction obj = Instance as ITransaction;
        if (obj._dbContext.Db.Ado.IsNoTran())
        {
            obj._dbContext.Db.Ado.BeginTran();
            obj._isTraned = true;
        }
        Console.WriteLine("Transaction Begin: {0}", obj._dbContext.Db.Ado.GetHashCode());
    }

    [Advice(Kind.After, Targets = Target.Method)]
    public void After([Argument(Source.Instance)] object Instance)
    {
        ITransaction obj = Instance as ITransaction;
        if (obj._isTraned)
        {
            obj._dbContext.Db.Ado.CommitTran();
            Console.WriteLine("Transaction Commit: {0}", obj._dbContext.Db.Ado.GetHashCode());
        }
    }
    
    [Advice(Kind.Around, Targets = Target.Method)]
    public object RollBackHandle([Argument(Source.Instance)] object Instance,[Argument(Source.Target)] Func<object[], object> target, [Argument(Source.Arguments)] object[] args)
    {
        try
        {
            return target(args);
        }   
        catch (Exception ex)
        {
            ITransaction obj = Instance as ITransaction;
            if (obj._isTraned)
            {
                obj._dbContext.Db.Ado.RollbackTran();
                Console.WriteLine("Transaction RollBack: {0}", obj._dbContext.Db.Ado.GetHashCode());
            }
            throw;
        }
    }
}