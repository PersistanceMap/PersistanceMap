﻿using System.Text;
using PersistanceMap.QueryBuilder;
using System.Collections.Generic;
using System.Linq;
using PersistanceMap.QueryBuilder.Decorators;
using System.Linq.Expressions;
using System.Reflection;
using System;
using PersistanceMap.Internals;

namespace PersistanceMap
{
    public class SelectQueryPartsMap : QueryPartsMap, IQueryPartsMap
    {
        #region Properties

        public IEnumerable<IEntityQueryPart> Joins
        {
            get
            {
                return Parts.Where(p => 
                    (p.OperationType == OperationType.From || p.OperationType == OperationType.Join || p.OperationType == OperationType.LeftJoin || p.OperationType == OperationType.RightJoin || p.OperationType == OperationType.FullJoin)
                    && p is IEntityQueryPart).Cast<IEntityQueryPart>();
            }
        }

        #endregion

        #region IQueryPartsMap Implementation

        public override void Add(IQueryPart map)
        {
            switch (map.OperationType)
            {
                case OperationType.From:
                case OperationType.Join:
                case OperationType.LeftJoin:
                case OperationType.RightJoin:
                case OperationType.FullJoin:
                    var entity = map as IEntityQueryPart;
                    entity.EnsureArgumentNotNull("map");

                    Parts.Add(entity);

                    break;

                case OperationType.Include:
                    var field = map as FieldQueryPart;
                    if (field == null)
                    {
                        // try to create a field query part
                        var expr = map as IQueryMap;
                        if (expr != null)
                        {
                            var last = Joins.LastOrDefault();
                            var id = last != null ? string.IsNullOrEmpty(last.EntityAlias) ? last.Entity : last.EntityAlias : null;
                            var ent = last != null ? last.Entity : null;

                            field = new FieldQueryPart(FieldHelper.TryExtractPropertyName(expr.Expression), id, ent)
                            {
                                OperationType = OperationType.Include
                            };
                        }
                    }

                    if (field != null)
                    {
                        // add the field to the last QueryPart of type SelectionMap (select a,b,c...)
                        AddToLast(field, OperationType.SelectMap);
                    }
                    break;
                    
                default:
                    Parts.Add(map);
                    break;
            }
        }

        public override CompiledQuery Compile()
        {
            var sb = new StringBuilder(100);

            // loop all parts and compile
            foreach (var part in Parts)
            {
                sb.AppendLine(part.Compile());
            }

            // where

            // order...

            return new CompiledQuery
            {
                QueryString = sb.ToString(),
                QueryParts = this
            };
        }

        #endregion
    }
}
