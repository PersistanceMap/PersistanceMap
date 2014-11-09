﻿using PersistanceMap.Factories;
using PersistanceMap.QueryBuilder.Commands;
using PersistanceMap.QueryParts;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Linq;
using PersistanceMap.Sqlite.Internal;
using System.Text;

namespace PersistanceMap.Sqlite.QueryBuilder
{
    internal class DatabaseQueryBuilderBase : IQueryExpression
    {
        public DatabaseQueryBuilderBase(SqliteDatabaseContext context)
        {
            _context = context;
        }

        public DatabaseQueryBuilderBase(SqliteDatabaseContext context, IQueryPartsMap container)
        {
            _context = context;
            _queryPartsMap = container;
        }

        #region IQueryProvider Implementation

        readonly SqliteDatabaseContext _context;
        public SqliteDatabaseContext Context
        {
            get
            {
                return _context;
            }
        }

        IDatabaseContext IQueryExpression.Context
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

    }

    internal class DatabaseQueryBuilder : DatabaseQueryBuilderBase, IDatabaseQueryExpression, IQueryExpression
    {
        public DatabaseQueryBuilder(SqliteDatabaseContext context)
            : base(context)
        {
        }

        public DatabaseQueryBuilder(SqliteDatabaseContext context, IQueryPartsMap container)
            : base(context, container)
        {
        }
        
        #region IDatabaseQueryExpression Implementation

        public virtual void Create()
        {
            Context.AddQuery(new DelegateQueryCommand(() =>
            {
                var provider = Context.ConnectionProvider as SqliteConnectionProvider;
                var db = provider.ConnectionString.Replace("data source=", "").Replace("Data Source=", "");
                File.Create(db);
            }));
        }

        public ITableQueryExpression<T> Table<T>()
        {
            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        #endregion
    }

    internal class TableQueryBuilder<T> : DatabaseQueryBuilderBase, ITableQueryExpression<T>
    {
        public TableQueryBuilder(SqliteDatabaseContext context, IQueryPartsMap container)
            : base(context, container)
        {
        }

        #region ITableQueryExpression Implementation

        public void Create()
        {
            var createPart = new DelegateQueryPart(OperationType.CreateTable, () => string.Format("CREATE TABLE IF NOT EXISTS {0} (", typeof(T).Name));
            QueryPartsMap.AddBefore(createPart, OperationType.None);

            var fields = TypeDefinitionFactory.GetFieldDefinitions<T>();
            foreach (var field in fields)
            {
                var existing = QueryPartsMap.Parts.Where(p => p.OperationType == OperationType.Column && p.ID == field.MemberName);
                if (existing.Any())
                    continue;

                var fieldPart = new DelegateQueryPart(OperationType.Column, () => string.Format("{0} {1}{2}{3}", field.MemberName, field.MemberType.ToSqlDbType(), !field.IsNullable ? " NOT NULL" : "", QueryPartsMap.Parts.Last(p => p.OperationType == OperationType.Column).ID == field.MemberName ? "" : ", "), field.MemberName);

                QueryPartsMap.AddAfter(fieldPart, QueryPartsMap.Parts.Any(p => p.OperationType == OperationType.Column) ? OperationType.Column : OperationType.CreateTable);
            }

            // add closing bracked
            QueryPartsMap.Add(new DelegateQueryPart(OperationType.None, () => ")"));

            Context.AddQuery(new MapQueryCommand(QueryPartsMap));
        }

        public void Alter()
        {
            throw new NotImplementedException();
        }

        public ITableQueryExpression<T> Ignore(Expression<Func<T, object>> field)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Marks a column to be a primary key column
        /// </summary>
        /// <param name="key">The field that marks the key</param>
        /// <param name="isAutoIncrement">Is the column a auto incrementing column</param>
        /// <returns></returns>
        public ITableQueryExpression<T> Key(Expression<Func<T, object>> key, bool isAutoIncrement = false)
        {
            //
            // id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
            // 

            var memberName = FieldHelper.TryExtractPropertyName(key);
            var fields = TypeDefinitionFactory.GetFieldDefinitions<T>();
            var field = fields.FirstOrDefault(f => f.MemberName == memberName);

            var fieldPart = new DelegateQueryPart(OperationType.Column, () => string.Format("{0} {1} PRIMARY KEY{2}{3}", field.MemberName, field.MemberType.ToSqlDbType(), !field.IsNullable ? " NOT NULL" : "", isAutoIncrement ? " AUTOINCREMENT" : ""), field.MemberName);
            QueryPartsMap.AddBefore(fieldPart, OperationType.TableKeys);

            return new TableQueryBuilder<T>(Context, QueryPartsMap);
        }

        public ITableQueryExpression<T> Key(params Expression<Func<T, object>>[] keyFields)
        {
            //
            // CREATE TABLE something (column1, column2, column3, PRIMARY KEY (column1, column2));
            //
            
            var fields = TypeDefinitionFactory.GetFieldDefinitions<T>();

            var last = keyFields.Last();

            var sb = new StringBuilder();
            sb.Append("PRIMARY KEY (");
            foreach (var key in keyFields)
            {
                var memberName = FieldHelper.TryExtractPropertyName(keyFields.First());
                var field = fields.FirstOrDefault(f => f.MemberName == memberName);

                sb.Append(string.Format("{0}{1}", field.MemberName, key == last ? "" : ", "));                
            }

            sb.Append(")");

            var fieldPart = new DelegateQueryPart(OperationType.TableKeys, () => sb.ToString(), OperationType.TableKeys.ToString());
            QueryPartsMap.Add(fieldPart);

            throw new NotImplementedException();
        }

        public ITableQueryExpression<T> Key<TRef>(Expression<Func<T, object>> field, Expression<Func<TRef, object>> reference)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Drops the key definition.
        /// </summary>
        /// <param name="keyFields">All items that make the key</param>
        /// <returns></returns>
        public ITableQueryExpression<T> DropKey(params Expression<Func<T, object>>[] keyFields)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Drops the table
        /// </summary>
        public void Drop()
        {
            var part = new DelegateQueryPart(OperationType.Drop, () => string.Format("DROP TABLE {0}", typeof(T).Name));
            QueryPartsMap.Add(part);
        }

        #endregion
    }
}