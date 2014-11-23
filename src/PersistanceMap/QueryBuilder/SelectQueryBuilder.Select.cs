﻿using PersistanceMap.Factories;
using PersistanceMap.QueryBuilder.QueryPartsBuilders;
using PersistanceMap.QueryParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PersistanceMap.QueryBuilder
{
    public partial class SelectQueryBuilder<T> : IJoinQueryExpression<T>
    {
        #region Private implementation

        private SelectQueryBuilder<T> Ignore(Expression<Func<T, object>> predicate)
        {
            foreach (var part in QueryPartsMap.Parts.Where(p => p.OperationType == OperationType.Select))
            {
                var map = part as IQueryPartDecorator;
                if (map == null)
                    continue;

                var fieldName = FieldHelper.TryExtractPropertyName(predicate);

                // remove all previous mappings of the ignored field
                var subparts = map.Parts.OfType<IFieldQueryPart>().Where(f => f.Field == fieldName || f.FieldAlias == fieldName);
                foreach (var subpart in subparts.ToList())
                {
                    map.Remove(subpart);
                }

                // add a field marked as ignored
                map.Add(new IgnoreFieldQueryPart(fieldName, ""));
            }

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        #endregion

        #region ISelectQueryProvider<T> Implementation

        #region Join Expressions

        /// <summary>
        /// Joines a new entity type to the last entity
        /// </summary>
        /// <typeparam name="TJoin">The type to join</typeparam>
        /// <param name="predicate">The expression that defines the connection</param>
        /// <param name="alias">The alias of the joining entity</param>
        /// <param name="source">The alias of the source entity</param>
        /// <returns>A IJoinQueryProvider{TJoin}</returns>
        public IJoinQueryExpression<TJoin> Join<TJoin>(Expression<Func<TJoin, T, bool>> predicate, string alias = null, string source = null)
        {
            return CreateEntityQueryPart(predicate, OperationType.Join, alias, source);
        }

        /// <summary>
        /// Joines a new entity type to a previous entity
        /// </summary>
        /// <typeparam name="TJoin">The type to join</typeparam>
        /// <typeparam name="TOrig">The type of the previous entity to join to</typeparam>
        /// <param name="predicate">The expression that defines the connection</param>
        /// <param name="alias">The alias of the joining entity</param>
        /// <param name="source">The alias of the source entity</param>
        /// <returns>A IJoinQueryProvider{TJoin}</returns>
        public IJoinQueryExpression<TJoin> Join<TJoin, TOrig>(Expression<Func<TJoin, TOrig, bool>> predicate, string alias = null, string source = null)
        {
            return CreateEntityQueryPart(predicate, OperationType.Join, alias, source);
        }

        #endregion

        #region Map Expressions

        protected ISelectQueryExpression<T> Map<TProp>(string source, string alias, string entity, string entityalias, Expression<Func<TProp, object>> valueConverter = null)
        {
            //TODO: is this the corect place to do this? shouldn't the QueryPart map its own children with the right alias?
            // if there is a alias on the last item it has to be used with the map
            var last = QueryPartsMap.Parts.OfType<IEntityQueryPart>().LastOrDefault(l => l.OperationType == OperationType.From || l.OperationType == OperationType.Join) as IEntityQueryPart;
            if (last != null && !string.IsNullOrEmpty(last.EntityAlias) && entity == last.Entity)
                entity = last.EntityAlias;

            // make sure the select part is not sealed so the custom map can be added
            bool isSealed = false;
            var parent = QueryPartsMap.Parts.OfType<IQueryPartDecorator>().LastOrDefault(p => p.OperationType == OperationType.Select);
            if (parent != null)
            {
                isSealed = parent.IsSealded;
                parent.IsSealded = false;
            }

            SelectQueryPartsBuilder.Instance.AddFieldQueryMap(QueryPartsMap, source, alias, entity, entityalias, valueConverter);

            if (parent != null)
            {
                parent.IsSealded = isSealed;
            }

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }
        /// <summary>
        /// Map a Property that is included in the result that belongs to a joined type with an alias defined (Table.Field as Alias)
        /// </summary>
        /// <param name="source">The expression that returns the Property</param>
        /// <param name="alias">The alias name the field will get (... as Alias)</param>
        /// <param name="valueConverter">The converter that converts the database value to the desired value in the dataobject</param>
        /// <returns>ISelectQueryProvider containing the maps</returns>
        public ISelectQueryExpression<T> Map<TProp>(Expression<Func<T, TProp>> source, string alias = null, Expression<Func<TProp, object>> valueConverter = null)
        {
            var sourceField = FieldHelper.TryExtractPropertyName(source);
            var entity = typeof(T).Name;

            return Map(sourceField, alias, entity, null, valueConverter);
        }

        /// <summary>
        /// Map a Property that is included in the result that belongs to a joined type with an alias from the select type
        /// </summary>
        /// <typeparam name="TAlias">The select type containig the alias property</typeparam>
        /// <param name="source">The source expression returning the source property</param>
        /// <param name="alias">The select expression returning the alias property</param>
        /// <returns>ISelectQueryProvider containing the maps</returns>
        public ISelectQueryExpression<T> Map<TAlias>(Expression<Func<T, object>> source, Expression<Func<TAlias, object>> alias)
        {
            return Map<T, TAlias>(source, alias);
        }

        /// <summary>
        /// Map a Property that is included in the result that belongs to a joined type with an alias from the select type
        /// </summary>
        /// <typeparam name="TSource">The select type containig the source alias property</typeparam>
        /// <typeparam name="TAlias">The select type containig the alias property</typeparam>
        /// <param name="source">The source expression returning the source property</param>
        /// <param name="alias">The select expression returning the alias property</param>
        /// <returns>ISelectQueryProvider containing the maps</returns>
        public ISelectQueryExpression<T> Map<TSource, TAlias>(Expression<Func<TSource, object>> source, Expression<Func<TAlias, object>> alias)
        {
            var aliasField = FieldHelper.TryExtractPropertyName(alias);
            var sourceField = FieldHelper.TryExtractPropertyName(source);
            var entity = typeof(TSource).Name;

            return Map<object>(sourceField, aliasField, entity, null);
        }

        /// <summary>
        /// Marks a field to be ignored in the query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ISelectQueryExpression<T> ISelectQueryExpression<T>.Ignore(Expression<Func<T, object>> predicate)
        {
            return Ignore(predicate);
        }

        /// <summary>
        /// Marks a field to return the max value of
        /// </summary>
        /// <param name="predicate">The expression that returns the Proerty to retrieve the value from</param>
        /// <returns>ISelectQueryProvider containing the maps</returns>
        public ISelectQueryExpression<T> Max(Expression<Func<T, object>> predicate, string alias = null)
        {
            var parent = QueryPartsMap.Parts.OfType<IQueryPartDecorator>().LastOrDefault(p => p.OperationType == OperationType.Select);
            if (parent != null)
            {
                var field = FieldHelper.TryExtractPropertyName(predicate);
                alias = alias ?? field;
                var id = Guid.NewGuid().ToString();
                var part = new DelegateQueryPart(OperationType.Max, () => string.Format("MAX({0}) AS {1}{2} ", field, alias, parent.Parts.Last().ID != id ? "," : ""), id);

                parent.Add(part);
                parent.IsSealded = true;
            }

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Marks a field to return the min value of
        /// </summary>
        /// <param name="predicate">The expression that returns the Proerty to retrieve the value from<</param>
        /// <returns>ISelectQueryProvider containing the maps</returns>
        public ISelectQueryExpression<T> Min(Expression<Func<T, object>> predicate, string alias = null)
        {
            var parent = QueryPartsMap.Parts.OfType<IQueryPartDecorator>().LastOrDefault(p => p.OperationType == OperationType.Select);
            if (parent != null)
            {
                var field = FieldHelper.TryExtractPropertyName(predicate);
                alias = alias ?? field;
                var id = Guid.NewGuid().ToString();
                var part = new DelegateQueryPart(OperationType.Min, () => string.Format("MIN({0}) AS {1}{2} ", field, alias, parent.Parts.Last().ID != id ? "," : ""), id);

                parent.Add(part);
                parent.IsSealded = true;
            }

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Marks a field to return the count of
        /// </summary>
        /// <param name="predicate">The expression that returns the Proerty to retrieve the value from<</param>
        /// <returns>ISelectQueryProvider containing the maps</returns>
        public ISelectQueryExpression<T> Count(Expression<Func<T, object>> predicate, string alias = null)
        {
            var parent = QueryPartsMap.Parts.OfType<IQueryPartDecorator>().LastOrDefault(p => p.OperationType == OperationType.Select);
            if (parent != null)
            {
                var field = FieldHelper.TryExtractPropertyName(predicate);
                alias = alias ?? field;
                var id = Guid.NewGuid().ToString();
                var part = new DelegateQueryPart(OperationType.Count, () => string.Format("COUNT({0}) AS {1}{2} ", field, alias, parent.Parts.Last().ID != id ? "," : ""), id);

                parent.Add(part);
                parent.IsSealded = true;
            }

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        #endregion

        #region Where Expressions

        public IWhereQueryExpression<T> Where(Expression<Func<T, bool>> operation)
        {
            var part = SelectQueryPartsBuilder.Instance.AddExpressionQueryPart(QueryPartsMap, operation, OperationType.Where);

            // check if the last part that was added containes a alias
            var last = QueryPartsMap.Parts.Last(l => 
                l.OperationType == OperationType.From || 
                l.OperationType == OperationType.Join ||
                l.OperationType == OperationType.FullJoin ||
                l.OperationType == OperationType.LeftJoin ||
                l.OperationType == OperationType.RightJoin) as IEntityQueryPart;

            if (last != null && !string.IsNullOrEmpty(last.EntityAlias) && last.Entity == typeof(T).Name)
                part.AliasMap.Add(typeof(T), last.EntityAlias);

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        public IWhereQueryExpression<T> Where<T2>(Expression<Func<T2, bool>> operation)
        {
            SelectQueryPartsBuilder.Instance.AddExpressionQueryPart(QueryPartsMap, operation, OperationType.Where);

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        public IWhereQueryExpression<T> Where<T2, T3>(Expression<Func<T2, T3, bool>> operation)
        {
            SelectQueryPartsBuilder.Instance.AddExpressionQueryPart(QueryPartsMap, operation, OperationType.Where);

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        #endregion

        #region OrderBy Expressions

        /// <summary>
        /// Marks a field to be ordered by ascending
        /// </summary>
        /// <param name="predicate">The property to order by</param>
        /// <returns></returns>
        public IOrderQueryExpression<T> OrderBy(Expression<Func<T, object>> predicate)
        {
            return CreateExpressionQueryPart<T>(OperationType.OrderBy, predicate);
        }

        /// <summary>
        /// Marks a field to be ordered by ascending
        /// </summary>
        /// <typeparam name="T2">The type containing the member to order by</typeparam>
        /// <param name="predicate">The property to order by</param>
        /// <returns></returns>
        public IOrderQueryExpression<T2> OrderBy<T2>(Expression<Func<T2, object>> predicate)
        {
            return CreateExpressionQueryPart<T2>(OperationType.OrderBy, predicate);
        }

        /// <summary>
        /// Marks a field to be ordered by descending
        /// </summary>
        /// <param name="predicate">The property to order by</param>
        /// <returns></returns>
        public IOrderQueryExpression<T> OrderByDesc(Expression<Func<T, object>> predicate)
        {
            return CreateExpressionQueryPart<T>(OperationType.OrderByDesc, predicate);
        }

        /// <summary>
        /// Marks a field to be ordered by descending
        /// </summary>
        /// <typeparam name="T2">The type containing the member to order by</typeparam>
        /// <param name="predicate">The property to order by</param>
        /// <returns></returns>
        public IOrderQueryExpression<T2> OrderByDesc<T2>(Expression<Func<T2, object>> predicate)
        {
            return CreateExpressionQueryPart<T2>(OperationType.OrderByDesc, predicate);
        }

        #endregion

        #region GroupBy Expressions

        /// <summary>
        /// Marks a field to be grouped by
        /// </summary>
        /// <param name="predicate">The property to group by</param>
        /// <returns></returns>
        public IGroupQueryExpression<T> GroupBy(Expression<Func<T, object>> predicate)
        {
            return GroupBy<T>(predicate);
        }

        /// <summary>
        /// Marks a field to be grouped by
        /// </summary>
        /// <typeparam name="T2">The type containing the member to group by</typeparam>
        /// <param name="predicate">The property to group by</param>
        /// <returns></returns>
        public IGroupQueryExpression<T> GroupBy<T2>(Expression<Func<T2, object>> predicate)
        {
            var field = FieldHelper.TryExtractPropertyName(predicate);
            var part = new DelegateQueryPart(OperationType.ThenBy, () => string.Format("GROUP BY {0}", field));
            QueryPartsMap.Add(part);

            return new SelectQueryBuilder<T>(Context, QueryPartsMap);
        }

        #endregion

        #region Select Expressions

        public IEnumerable<T2> Select<T2>()
        {
            var expr = Context.ConnectionProvider.QueryCompiler;
            var query = expr.Compile<T2>(QueryPartsMap);

            // extract all fields with valueConverter
            var selector = QueryPartsMap.Parts.OfType<IQueryPartDecorator>().FirstOrDefault(p => p.OperationType == OperationType.Select && p.Parts.OfType<FieldQueryPart>().Any(f => f.Converter != null));
            query.Converters = selector != null ? selector.Parts.OfType<FieldQueryPart>().Where(p => p.Converter != null).Select(p => new MapValueConverter { Converter = p.Converter, ID = p.ID }) : null;

            return Context.Kernel.Execute<T2>(query);
        }

        public IEnumerable<T> Select()
        {
            return Select<T>();
        }

        public IEnumerable<TSelect> Select<TSelect>(Expression<Func<TSelect>> anonym)
        {
            return Select<TSelect>();
        }

        public IEnumerable<TSelect> Select<TSelect>(Expression<Func<T, TSelect>> anonym)
        {
            var expr = Context.ConnectionProvider.QueryCompiler;
            var query = expr.Compile<T>(QueryPartsMap);

            var elements = Context.Kernel.Execute<T>(query);
            var expression = anonym.Compile();

            foreach (var item in elements)
            {
                yield return expression.Invoke(item);
            }
        }

        /// <summary>
        /// Defines the fields that will be used in the query
        /// </summary>
        /// <typeparam name="TNew"></typeparam>
        /// <returns></returns>
        public IAfterMapQueryExpression<TNew> For<TNew>()
        {
            var members = typeof(TNew).GetSelectionMembers();
            var fields = members.Select(m => m.ToFieldQueryPart(null, null));

            SelectQueryPartsBuilder.Instance.AddFiedlParts(QueryPartsMap, fields.ToArray());

            foreach (var part in QueryPartsMap.Parts.Where(p => p.OperationType == OperationType.Select))
            {
                // seal part to disalow other parts to be added to selectmaps
                var map = part as IQueryPartDecorator;
                if (map != null)
                    map.IsSealded = true;
            }

            return new SelectQueryBuilder<TNew>(Context, QueryPartsMap);
        }

        /// <summary>
        /// Defines the fields that will be used in the query
        /// </summary>
        /// <typeparam name="TAno"></typeparam>
        /// <param name="anonym"></param>
        /// <returns></returns>
        public IAfterMapQueryExpression<TAno> For<TAno>(Expression<Func<TAno>> anonym)
        {
            return For<TAno>();
        }

        /// <summary>
        /// Compiles the Query to a sql statement for the given type
        /// </summary>
        /// <typeparam name="T">The select type</typeparam>
        /// <returns>The sql string</returns>
        public string CompileQuery<T2>()
        {
            var expr = Context.ConnectionProvider.QueryCompiler;
            var query = expr.Compile<T2>(QueryPartsMap);

            return query.QueryString;
        }

        /// <summary>
        /// Compiles the Query to a sql statement
        /// </summary>
        /// <returns>The sql string</returns>
        public string CompileQuery()
        {
            return CompileQuery<T>();
        }

        #endregion

        #endregion
    }
}
