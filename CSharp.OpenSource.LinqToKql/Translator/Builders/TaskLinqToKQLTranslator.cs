using CSharp.OpenSource.LinqToKql.Extensions;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders
{
    public class TaskLinqToKQLTranslator : LinqToKQLTranslatorBase
    {
        public TaskLinqToKQLTranslator(LinqToKQLQueryTranslatorConfig config) : base(config, new() { nameof(Enumerable.Take), nameof(Enumerable.Skip) })
        {
        }

        public override string Handle(MethodCallExpression methodCall, Expression? parent)
        {
            var count = ((ConstantExpression)methodCall.Arguments[1]).Value.GetKQLValue();
            return methodCall.Method.Name switch
            {
                nameof(Enumerable.Take) => $"take {count}",
                nameof(Enumerable.Skip) => $"skip {count}",
                _ => throw new NotSupportedException($"{GetType().Name} - Method {methodCall.Method.Name} is not supported.")
            };
        }
    }
}
