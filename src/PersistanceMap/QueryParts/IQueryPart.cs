﻿
using System;

namespace PersistanceMap.QueryParts
{
    public interface IQueryPart
    {
        /// <summary>
        /// The ID of the QueryPart
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// Defines the type of operation that this part is
        /// </summary>
        OperationType OperationType { get; }

        /// <summary>
        /// Gets the type of the entity/table
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Compile the part to a query string
        /// </summary>
        /// <returns></returns>
        string Compile();
    }
}
