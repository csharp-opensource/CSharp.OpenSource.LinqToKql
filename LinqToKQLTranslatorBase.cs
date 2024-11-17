using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Builders;

public abstract class LinqToKQLTranslatorBase
{
    public HashSet<string> LinqMethods { get; private set; }

    protected LinqToKQLTranslatorBase(HashSet<string> linqMethods)
    {
        LinqMethods = linqMethods;
    }

    public abstract string Handle(MethodCallExpression methodCall);
}
