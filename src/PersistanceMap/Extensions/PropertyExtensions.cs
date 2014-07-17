﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace PersistanceMap
{
    public delegate object PropertyGetterDelegate(object instance);

    public delegate void PropertySetterDelegate(object instance, object value);

    internal static class PropertyExtensions
    {
        public static PropertyGetterDelegate GetPropertyGetter(this PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod();
            if (getMethodInfo == null) 
                return null;

            try
            {
                var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
                var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.DeclaringType);

                var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
                var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

                var propertyGetFn = Expression.Lambda<PropertyGetterDelegate>
                    (
                        oExprCallPropertyGetFn,
                        oInstanceParam
                    ).Compile();

                return propertyGetFn;

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
        }

        public static PropertySetterDelegate GetPropertySetter(this PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.GetSetMethod();
            if (propertySetMethod == null) 
                return null;

            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var instanceParam = Expression.Convert(instance, propertyInfo.DeclaringType);
            var valueParam = Expression.Convert(argument, propertyInfo.PropertyType);

            var setterCall = Expression.Call(instanceParam, propertyInfo.GetSetMethod(), valueParam);

            return Expression.Lambda<PropertySetterDelegate>(setterCall, instance, argument).Compile();
        }

        public static FieldDefinition ToFieldDefinition(this PropertyInfo propertyInfo)
        {
            var isNullableType = propertyInfo.PropertyType.IsNullableType();

            var isNullable = !propertyInfo.PropertyType.IsValueType /*&& !propertyInfo.HasAttributeNamed(typeof(RequiredAttribute).Name))*/ || isNullableType;

            var propertyType = isNullableType ? Nullable.GetUnderlyingType(propertyInfo.PropertyType) : propertyInfo.PropertyType;

            //propertyInfo.DeclaringType
            return new FieldDefinition
            {
                FieldName = propertyInfo.Name,
                Name = propertyInfo.Name/*.ToLower()*/,
                EntityName = propertyInfo.DeclaringType.Name,
                FieldType = propertyType,
                EntityType = propertyInfo.DeclaringType,
                IsNullable = isNullable,
                PropertyInfo = propertyInfo,
                GetValueFunction = propertyInfo.GetPropertyGetter(),
                SetValueFunction = propertyInfo.GetPropertySetter(),
            };
        }
    }
}