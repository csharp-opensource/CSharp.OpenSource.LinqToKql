﻿using CSharp.OpenSource.LinqToKql.Extensions;
using System.Linq.Expressions;

namespace CSharp.OpenSource.LinqToKql.Translator.Builders
{
    public class TaskLinqToKQLTranslator : LinqToKQLTranslatorBase
    {
        public TaskLinqToKQLTranslator(LinqToKQLQueryTranslatorConfig config) : base(config, new() { nameof(Enumerable.Take), })
        {
        }

        public override string Handle(MethodCallExpression methodCall, Expression? parent)
        {
            var count = ((ConstantExpression)methodCall.Arguments[1]).Value.GetKQLValue();
            return $"take {count}";
        }
    }
}