﻿
namespace PersistanceMap
{
    public enum MapOperationType
    {
        None,

        From,

        Join,

        //InnerJoin,

        LeftJoin,

        RightJoin,

        FullJoin,

        /// <summary>
        /// Defines the Operation as an alias of a table, a field or a storedprocedure parameter
        /// </summary>
        As,

        /// <summary>
        /// Defines the operation that the field is included in the resultset
        /// </summary>
        Include,

        /// <summary>
        /// defines the operation as a join operation
        /// </summary>
        JoinOn,

        /// <summary>
        /// 
        /// </summary>
        AndOn,

        /// <summary>
        /// 
        /// </summary>
        OrOn,

        /// <summary>
        /// defines the value of a storeprocedure parameter
        /// </summary>
        Value,

        Parameter,

        OutParameterPrefix,

        OutParameterSufix
    }
}
