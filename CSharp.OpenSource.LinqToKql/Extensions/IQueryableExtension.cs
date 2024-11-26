using CSharp.OpenSource.LinqToKql.Provider;

namespace CSharp.OpenSource.LinqToKql.Extensions;

public static class IQueryableExtension
{
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> q)
    {
        if (q is not IAsyncEnumerable<T> asyncEnumerable)
        {
            throw new InvalidOperationException($"{q.GetType().Name} is not implement {nameof(IAsyncEnumerable<T>)}");
        }
        return asyncEnumerable;
    }

    public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> q, CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var element in q.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            list.Add(element);
        }
        return list;
    }

    public static ILinqToKqlProvider<T> AsKQL<T>(this IQueryable<T> q)
    {
        if (q is not ILinqToKqlProvider<T> kqlQ)
        {
            throw new InvalidOperationException($"{q.GetType().Name} is not implement {nameof(ILinqToKqlProvider<T>)}");
        }
        return kqlQ;
    }

    public static ILinqToKqlProvider<T> WithDbName<T>(this IQueryable<T> q, string dbName)
    {
        var kql = q.AsKQL();
        kql.DefaultDbName = dbName;
        return kql;
    }

    public static ILinqToKqlProvider<T> FromKQL<T>(this IQueryable<T> q, string tableOrKQL, bool appendKQL = true)
        => q.FromKQL<T, T>(tableOrKQL, appendKQL);

    public static ILinqToKqlProvider<S> FromKQL<T, S>(this IQueryable<T> q, string tableOrKQL, bool appendKQL = true)
    {
        var kql = q.AsKQL();
        if (appendKQL)
        {
            var currentKQL = kql.TranslateToKQL().Trim(' ').TrimEnd('|').Trim(' ');
            tableOrKQL = tableOrKQL.Trim(' ').TrimStart('|').Trim(' ');
            tableOrKQL = string.Join(kql.Translator.PipeWithIndentation, currentKQL, tableOrKQL);
        }
        kql.TableOrKQL = tableOrKQL;
        return kql.Clone<S>(null);
    }
}
