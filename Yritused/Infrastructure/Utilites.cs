﻿using System.Linq.Expressions;

namespace Yritused.Infrastructure
{
    public static class Utilites
    {
        public enum Order
        {
            Asc,
            Desc
        }
        public static int GetPageSize(int s)
        {
            return 41;
        }
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string orderByMember, Order direction)
        {
            var queryElementTypeParam = Expression.Parameter(typeof(T));
            var memberAccess = Expression.PropertyOrField(queryElementTypeParam, orderByMember);
            var keySelector = Expression.Lambda(memberAccess, queryElementTypeParam);

            var orderBy = Expression.Call(
                typeof(Queryable),
                direction == Order.Asc ? "OrderBy" : "OrderByDescending",
                [typeof(T), memberAccess.Type],
                query.Expression,
                Expression.Quote(keySelector));

            return query.Provider.CreateQuery<T>(orderBy);
        }
    }
}
