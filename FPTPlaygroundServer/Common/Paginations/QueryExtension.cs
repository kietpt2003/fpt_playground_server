using System.Linq.Expressions;

namespace FPTPlaygroundServer.Common.Paginations;

public static class QueryableExtension
{
    public static IOrderedQueryable<T> OrderByColumn<T>(
            this IQueryable<T> query,
            Expression<Func<T, object>> sortExpression,
            SortDir? sortOrder) where T : class
    {
        return sortOrder?.ToString().ToLower() == "asc"
            ? query.OrderBy(sortExpression)
            : query.OrderByDescending(sortExpression);
    }
}
