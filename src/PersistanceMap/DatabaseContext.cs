﻿using PersistanceMap.Tracing;
using PersistanceMap.QueryBuilder;
using PersistanceMap.QueryBuilder.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PersistanceMap
{
    /// <summary>
    /// Internal implementation of the DatabaseContext
    /// </summary>
    public class DatabaseContext : IDatabaseContext
    {
        readonly IList<IQueryCommand> _queryCommandStore;
        readonly ILoggerFactory _loggerFactory;

        public DatabaseContext(IConnectionProvider provider, ILoggerFactory loggerFactory)
        {
            ConnectionProvider = provider;
            _queryCommandStore = new List<IQueryCommand>();
            _loggerFactory = loggerFactory;
        }

        #region IDatabaseContext Implementation

        /// <summary>
        /// Provides a connection to a specific RDBMS
        /// </summary>
        public IConnectionProvider ConnectionProvider { get; private set; }

        /// <summary>
        /// Commit the queries contained in the commandstore
        /// </summary>
        public void Commit()
        {
            var command = QueryCommandStore.FirstOrDefault();
            while (command != null)
            {
                command.Execute(this);

                _queryCommandStore.Remove(command);
                command = QueryCommandStore.FirstOrDefault();
            }
        }

        /// <summary>
        /// Add a query to the commandstore
        /// </summary>
        /// <param name="command"></param>
        public void AddQuery(IQueryCommand command)
        {
            _queryCommandStore.Add(command);
        }

        /// <summary>
        /// The commandstore containing all queries that have not been executed
        /// </summary>
        public IEnumerable<IQueryCommand> QueryCommandStore
        {
            get
            {
                return _queryCommandStore;
            }
        }
        

        private QueryKernel _kernel;
        /// <summary>
        /// The kernel providing the execution of the query and mapping of the data
        /// </summary>
        public QueryKernel Kernel
        {
            get
            {
                if (_kernel == null)
                    _kernel = new QueryKernel(ConnectionProvider, _loggerFactory);

                return _kernel;
            }
        }

        #endregion

        #region QueryEpressions

        #region Execute

        public IEnumerable<T> Execute<T>(string queryString)
        {
            var query = new CompiledQuery
            {
                QueryString = queryString
            };

            return Kernel.Execute<T>(query);
        }

        public IEnumerable<T> Execute<T>(string queryString, Expression<Func<T>> anonymobject)
        {
            var query = new CompiledQuery
            {
                QueryString = queryString
            };

            return Kernel.Execute<T>(query);
        }

        public void Execute(string queryString)
        {
            var query = new CompiledQuery
            {
                QueryString = queryString
            };

            Kernel.Execute(query);
        }

        #endregion

        #region Select Expressions

        public IEnumerable<T> Select<T>()
        {
            var query = new SelectQueryBuilder<T>(this)
                .From<T>();

            return query.Select<T>();
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> predicate)
        {
            var query = new SelectQueryBuilder<T>(this)
                .From<T>()
                .Where(predicate);

            return query.Select<T>();
        }

        public ISelectQueryExpression<T> From<T>()
        {
            return new SelectQueryBuilder<T>(this)
                .From<T>();
        }

        public ISelectQueryExpression<T> From<T>(string alias)
        {
            return new SelectQueryBuilder<T>(this)
                .From<T>(alias);
        }

        public ISelectQueryExpression<TJoin> From<T, TJoin>(Expression<Func<TJoin, T, bool>> predicate)
        {
            return new SelectQueryBuilder<T>(this)
                .From<T>()
                .Join<TJoin>(predicate);
        }

        public IWhereQueryExpression<T> From<T>(Expression<Func<T, bool>> predicate)
        {
            return new SelectQueryBuilder<T>(this)
                .From<T>()
                .Where(predicate);
        }

        #endregion

        #region Delete Expressions

        /// <summary>
        /// Deletes all records
        /// </summary>
        /// <typeparam name="T">The Type that defines the Table to delete from</typeparam>
        /// <returns></returns>
        public IDeleteQueryExpression Delete<T>()
        {
            return new DeleteQueryBuilder(this)
                .Delete<T>()
                .AddToStore();
        }

        /// <summary>
        /// Deletes a record based on the where expression
        /// </summary>
        /// <typeparam name="T">The Type that defines the Table to delete from</typeparam>
        /// <param name="where">The expression defining the where statement</param>
        public IDeleteQueryExpression Delete<T>(Expression<Func<T, bool>> where)
        {
            return new DeleteQueryBuilder(this)
                .Delete(where)
                .AddToStore();
        }

        /// <summary>
        /// Deletes a record based on the Properties and values of the given entity
        /// </summary>
        /// <typeparam name="T">The Type that defines the Table to delete from</typeparam>
        /// <param name="dataObject">The entity to delete</param>
        /// <param name="where">The property defining the key on the entity</param>
        public IDeleteQueryExpression Delete<T>(Expression<Func<T>> dataObject, Expression<Func<T, object>> where = null)
        {
            return new DeleteQueryBuilder(this)
                .Delete(dataObject, where)
                .AddToStore();
        }

        /// <summary>
        /// Delete a record based on the Properties and values passed in the anonym object
        /// </summary>
        /// <typeparam name="T">The Type that defines the Table to delete from</typeparam>
        /// <param name="anonym">The object that defines the properties and the values that mark the object to delete</param>
        /// <returns>IDeleteQueryProvider</returns>
        public IDeleteQueryExpression Delete<T>(Expression<Func<object>> anonym)
        {
            return new DeleteQueryBuilder(this)
                .Delete<T>(anonym)
                .AddToStore();
        }
        
        #endregion

        #region Update Expressions

        /// <summary>
        /// Updates a row with the values provided by the dataobject
        /// </summary>
        /// <typeparam name="T">Tabletype to update</typeparam>
        /// <param name="dataObject">Expression providing the object containing the data</param>
        /// <param name="where">The expression providing the where statement</param>
        /// <returns></returns>
        public IUpdateQueryExpression<T> Update<T>(Expression<Func<T>> dataObject, Expression<Func<T, object>> where = null)
        {
            return new UpdateQueryBuilder<T>(this)
                .Update(dataObject, where)
                .AddToStore();
        }

        /// <summary>
        /// Updates a row with the values provided by the dataobject
        /// </summary>
        /// <typeparam name="T">Tabletype to update</typeparam>
        /// <param name="anonym">Expression providing the anonym object containing the data1</param>
        /// <param name="where">The expression providing the where statement</param>
        /// <returns></returns>
        public IUpdateQueryExpression<T> Update<T>(Expression<Func<object>> anonym, Expression<Func<T, bool>> where = null)
        {
            return new UpdateQueryBuilder<T>(this)
                .Update(anonym, where)
                .AddToStore();
        }

        #endregion

        #region Insert Expressions

        /// <summary>
        /// Inserts a row with the values defined in the dataobject
        /// </summary>
        /// <typeparam name="T">Tabletype to insert</typeparam>
        /// <param name="dataObject">Expression providing the object containing the data</param>
        /// <returns></returns>
        public IInsertQueryExpression<T> Insert<T>(Expression<Func<T>> dataObject)
        {
            return new InsertQueryBuilder<T>(this)
                .Insert(dataObject)
                .AddToStore();
        }

        /// <summary>
        /// Inserts a row with the values defined in the anonym dataobject
        /// </summary>
        /// <typeparam name="T">Tabletype to insert</typeparam>
        /// <param name="anonym">Expression providing the anonym object containing the data</param>
        /// <returns></returns>
        public IInsertQueryExpression<T> Insert<T>(Expression<Func<object>> anonym)
        {
            return new InsertQueryBuilder<T>(this)
                .Insert(anonym)
                .AddToStore();
        }

        #endregion

        #endregion

        #region IDisposeable Implementation

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases resources held by the object.
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                if (disposing && !IsDisposed)
                {
                    // commit all uncommited transactions
                    Commit();

                    ConnectionProvider.Dispose();

                    IsDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Releases resources before the object is reclaimed by garbage collection.
        /// </summary>
        ~DatabaseContext()
        {
            Dispose(false);
        }

        #endregion
    }

    internal static class DatabaseContextExtensions
    {
        internal static IDeleteQueryExpression AddToStore(this IDeleteQueryExpression expression)
        {
            expression.Context.AddQuery(new MapQueryCommand(expression.QueryPartsMap));

            return expression;
        }

        internal static IUpdateQueryExpression<T> AddToStore<T>(this IUpdateQueryExpression<T> expression)
        {
            expression.Context.AddQuery(new MapQueryCommand(expression.QueryPartsMap));

            return expression;
        }

        internal static IInsertQueryExpression<T> AddToStore<T>(this IInsertQueryExpression<T> expression)
        {
            expression.Context.AddQuery(new MapQueryCommand(expression.QueryPartsMap));

            return expression;
        }
    }
}
