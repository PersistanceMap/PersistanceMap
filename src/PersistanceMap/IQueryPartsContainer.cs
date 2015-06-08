﻿using PersistanceMap.QueryBuilder;
using PersistanceMap.QueryParts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersistanceMap
{
    /// <summary>
    /// A container for all queryparts needed for a sql statement
    /// </summary>
    public interface IQueryPartsContainer
    {
        /// <summary>
        /// Add a querypart
        /// </summary>
        /// <param name="part"></param>
        void Add(IQueryPart part);

        /// <summary>
        /// Add a querypart before the last operation
        /// </summary>
        /// <param name="part"></param>
        /// <param name="operation"></param>
        void AddBefore(IQueryPart part, OperationType operation);

        /// <summary>
        /// Add a querypart after the last operation
        /// </summary>
        /// <param name="part"></param>
        /// <param name="operation"></param>
        void AddAfter(IQueryPart part, OperationType operation);

        /// <summary>
        /// Add a querypart to the query part with the operation
        /// </summary>
        /// <param name="part"></param>
        /// <param name="operation"></param>
        void AddToLast(IQueryPart part, OperationType operation);

        /// <summary>
        /// Add a querypart
        /// </summary>
        /// <param name="part"></param>
        /// <param name="predicate"></param>
        void AddToLast(IQueryPart part, Func<IQueryPart, bool> predicate);

        /// <summary>
        /// The list of queryparts in the container
        /// </summary>
        IEnumerable<IQueryPart> Parts { get; }

        /// <summary>
        /// Compile the queryparts to a sql statement
        /// </summary>
        /// <returns></returns>
        CompiledQuery Compile();

        bool IsSealed { get; }
    }
}