﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceMap.Internals
{
    internal static class ExpressionFactory
    {
        public static LambdaExpression CreateExpression(string value)
        {
            Expression<Func<string>> expression = () => value;
            return expression;
        }

        public static Expression ExtractKeyExpression<T>(Expression<Func<T>> entity)
        {
            var fields = TypeDefinitionFactory.GetFieldDefinitions<T>();
            var pk = fields.FirstOrDefault(f => f.IsPrimaryKey);
            if (pk == null)
                return null;

            var obj = entity.Compile().Invoke();
            var value = pk.PropertyInfo.GetValue(obj);


            ParameterExpression pe = Expression.Parameter(typeof(T), "exp");

            // ***** Where(company => (company.ToLower() == "coho winery" || company.Length > 16)) *****
            // Create an expression tree that represents the expression 'company.ToLower() == "coho winery"'.
            var left = Expression.Property(pe, pk.PropertyInfo);
            var right = Expression.Constant(value);
            var e1 = Expression.Equal(left, right);

            return e1;
        }
    }
}
