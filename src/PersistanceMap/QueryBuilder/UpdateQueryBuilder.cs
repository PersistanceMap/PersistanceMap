﻿using PersistanceMap.Internals;
using PersistanceMap.QueryBuilder.Commands;
using PersistanceMap.QueryBuilder.QueryPartsBuilders;
using PersistanceMap.QueryParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceMap.QueryBuilder
{
    public class UpdateQueryBuilder : IUpdateQueryProvider, IQueryProvider
    {
        public UpdateQueryBuilder(IDatabaseContext context)
        {
            _context = context;
        }

        public UpdateQueryBuilder(IDatabaseContext context, IQueryPartsMap container)
        {
            _context = context;
            _queryPartsMap = container;
        }

     
        #region IQueryProvider Implementation

        readonly IDatabaseContext _context;
        public IDatabaseContext Context
        {
            get
            {
                return _context;
            }
        }

        IQueryPartsMap _queryPartsMap;
        public IQueryPartsMap QueryPartsMap
        {
            get
            {
                if (_queryPartsMap == null)
                    _queryPartsMap = new QueryPartsMap();
                return _queryPartsMap;
            }
        }

        #endregion

        public IUpdateQueryProvider AddToStore()
        {
            Context.AddQuery(new UpdateQueryCommand(QueryPartsMap));

            return this;
        }

        public IUpdateQueryProvider Update<T>(Expression<Func<T>> dataPredicate, Expression<Func<T, object>> where = null)
        {
            // update all except the key elements used in the reference expression

            // create expression containing key and value for the where statement
            var whereexpr = ExpressionFactory.CreateKeyExpression(dataPredicate, where);
            if (whereexpr == null)
            {
                // find the property called ID or {objectname}ID to define the where expression
                whereexpr = ExpressionFactory.CreateKeyExpression(dataPredicate);
            }

            QueryPartsBuilder.Instance.AppendEntityQueryPart<T>(QueryPartsMap, OperationType.Update);
            var simple = QueryPartsBuilder.Instance.AppendSimpleQueryPart(QueryPartsMap, OperationType.Set);

            var keyName = FieldHelper.TryExtractPropertyName(whereexpr);

            var tableFields = TypeDefinitionFactory.GetFieldDefinitions<T>();
            var dataObject = dataPredicate.Compile().Invoke();
            foreach(var field in tableFields)
            {
                if (field.MemberName != keyName)
                    simple.Add(new KeyValueAssignExpression<T>(OperationType.None, dataObject, field));
            }

            QueryPartsBuilder.Instance.AppendExpressionQueryPart(QueryPartsMap, whereexpr, OperationType.Where);

            return new UpdateQueryBuilder(Context, QueryPartsMap);
        }

        public IUpdateQueryProvider Update<T>(Expression<Func<object>> anonym, Expression<Func<T, bool>> where = null)
        {
            // create expression containing key and value for the where statement
            var whereexpr = where;
            if (whereexpr == null)
            {
                // find the property called ID or {objectname}ID to define the where expression
                whereexpr = ExpressionFactory.CreateKeyExpression<T>(anonym);
            }

            QueryPartsBuilder.Instance.AppendEntityQueryPart<T>(QueryPartsMap, OperationType.Update);
            var simple = QueryPartsBuilder.Instance.AppendSimpleQueryPart(QueryPartsMap, OperationType.Set);

            var keyName = FieldHelper.TryExtractPropertyName(whereexpr);

            var dataObject = anonym.Compile().Invoke();
            var tableFields = TypeDefinitionFactory.GetFieldDefinitions<T>(dataObject.GetType());
            foreach (var field in tableFields)
            {
                if (field.MemberName != keyName)
                    simple.Add(new KeyValueAssignExpression<T>(OperationType.None, dataObject, field));
            }

            QueryPartsBuilder.Instance.AppendExpressionQueryPart(QueryPartsMap, whereexpr, OperationType.Where);

            return new UpdateQueryBuilder(Context, QueryPartsMap);
        }
    }
}