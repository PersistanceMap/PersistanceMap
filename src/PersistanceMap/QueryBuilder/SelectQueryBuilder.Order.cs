﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceMap.QueryBuilder
{
    public partial class SelectQueryBuilder<T> : IOrderQueryProvider<T>, ISelectQueryProviderBase<T>, IQueryProvider
    {
        #region OrderBy Expressions

        public IOrderQueryProvider<T> ThenBy(Expression<Func<T, object>> predicate)
        {
            return CreateExpressionQueryPart<T>(Context, QueryPartsMap, OperationType.ThenBy, predicate);
        }

        public IOrderQueryProvider<T> ThenBy<T2>(Expression<Func<T2, object>> predicate)
        {
            return CreateExpressionQueryPart<T>(Context, QueryPartsMap, OperationType.ThenBy, predicate);
        }

        //public IOrderQueryProvider<T> ThenByDesc<TOrder>(Expression<Func<T, TOrder>> predicate)
        public IOrderQueryProvider<T> ThenByDesc(Expression<Func<T, object>> predicate)
        {
            return CreateExpressionQueryPart<T>(Context, QueryPartsMap, OperationType.ThenByDesc, predicate);
        }

        //public IOrderQueryProvider<T> ThenByDesc<T2, TOrder>(Expression<Func<T2, TOrder>> predicate)
        public IOrderQueryProvider<T> ThenByDesc<T2>(Expression<Func<T2, object>> predicate)
        {
            return CreateExpressionQueryPart<T>(Context, QueryPartsMap, OperationType.ThenByDesc, predicate);
        }

        #endregion
    }
}

