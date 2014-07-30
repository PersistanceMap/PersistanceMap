﻿using PersistanceMap.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using PersistanceMap.QueryBuilder.Decorators;
using PersistanceMap.Internals;

namespace PersistanceMap.QueryProvider
{
    /// <summary>
    /// MapOption for simple select expressions like From{T}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SelectMapOption<T> : IJoinMapOption<T>, ISelectMapOption<T>
    {
        #region Include

        /// <summary>
        /// Creates a include expression to mark fields of the curent join that have to be included in the select statement
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public IQueryMap Include<T2>(Expression<Func<T, T2>> field)
        {
            return new QueryMap(MapOperationType.Include, field);
        }

        public IQueryMap Include<T2>(Expression<Func<T, T2>> alias, string source)
        {
            throw new NotImplementedException();
        }

        public IQueryMap Include<T2, T3, T4>(Expression<Func<T, T2>> alias, Expression<Func<T3, T4>> source)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region As

        /// <summary>
        /// Adds an alias for the entity to the fields of the curent join
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap As(Expression<Func<string>> predicate)
        {
            return new QueryMap(MapOperationType.As, predicate);
        }

        #endregion

        #region MapTo

        public IQueryMap MapTo<TOut>(Expression<Func<T, TOut>> source, string alias)
        {
            // create a expression that returns the field with a alias
            var entity = typeof(T).Name;
            return new FieldQueryPart(FieldHelper.TryExtractPropertyName(source), alias, null /*EntityAlias*/, entity/*, expression*/)
            {
                MapOperationType = MapOperationType.Include
            };
        }

        public IQueryMap MapTo<TAlias, TOut>(Expression<Func<T, TOut>> source, Expression<Func<TAlias, TOut>> alias)
        {
            var aliasField = FieldHelper.TryExtractPropertyName(alias);
            var sourceField = FieldHelper.TryExtractPropertyName(source);

            // create a expression that returns the field with a alias
            var entity = typeof(T).Name;
            return new FieldQueryPart(sourceField, aliasField, null /*EntityAlias*/, entity/*, expression*/)
            {
                MapOperationType = MapOperationType.Include
            };
        }

        public IQueryMap MapTo<TAlias, TOut>(Expression<Func<TAlias, TOut>> source, Expression<Func<T, TOut>> alias)
        {
            var aliasField = FieldHelper.TryExtractPropertyName(alias);
            var sourceField = FieldHelper.TryExtractPropertyName(source);

            // create a expression that returns the field with a alias
            var entity = typeof(TAlias).Name;
            return new FieldQueryPart(sourceField, aliasField, null /*EntityAlias*/, entity/*, expression*/)
            {
                MapOperationType = MapOperationType.Include
            };
        }

        #endregion
    }

    /// <summary>
    /// MapOption for extended select expressions like Join{T,T2}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    internal class SelectMapOption<T, T2> : SelectMapOption<T>, IJoinMapOption<T, T2>, IJoinMapOption<T>
    {
        #region On

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap On(Expression<Func<T, T2, bool>> predicate)
        {
            return new QueryMap(MapOperationType.JoinOn, predicate);
        }

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap On<T3>(Expression<Func<T, T3, bool>> predicate)
        {
            return new QueryMap(MapOperationType.JoinOn, predicate);
        }

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap On(string alias, Expression<Func<T, T2, bool>> predicate)
        {
            var part = new QueryMap(MapOperationType.JoinOn, predicate);
            part.AddEntityAlias(typeof(T2), alias);

            return part;
        }

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="alias"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap On<T3>(string alias, Expression<Func<T, T3, bool>> predicate)
        {
            var part = new QueryMap(MapOperationType.JoinOn, predicate);
            part.AddEntityAlias(typeof(T3), alias);

            return part;
        }

        #endregion

        #region And

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together with a and expression
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap And(Expression<Func<T, T2, bool>> predicate)
        {
            return new QueryMap(MapOperationType.AndOn, predicate);
        }

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together with a and expression
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap And(string alias, Expression<Func<T, T2, bool>> predicate)
        {
            var part = new QueryMap(MapOperationType.AndOn, predicate);
            part.AddEntityAlias(typeof(T2), alias);

            return part;
        }

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together with a and expression
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap And<T3>(Expression<Func<T, T3, bool>> predicate)
        {
            return new QueryMap(MapOperationType.AndOn, predicate);
        }

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together with a and expression
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="alias"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap And<T3>(string alias, Expression<Func<T, T3, bool>> predicate)
        {
            var part = new QueryMap(MapOperationType.AndOn, predicate);
            part.AddEntityAlias(typeof(T3), alias);

            return part;
        }

        #endregion

        #region Or

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together with a or expression
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap Or(Expression<Func<T, T2, bool>> predicate)
        {
            return new QueryMap(MapOperationType.OrOn, predicate);
        }

        /// <summary>
        /// Provides an expression to mark the fields that have to be joined together with a or expression
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryMap Or<T3>(Expression<Func<T, T3, bool>> predicate)
        {
            return new QueryMap(MapOperationType.OrOn, predicate);
        }

        #endregion
    }
}