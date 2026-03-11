using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions
    {
        internal static Expression ToUnderExpression(this TypedConditionExpression c, IXrmFakedContext context, ParameterExpression entity)
        {
            var ancestorId = (Guid)c.GetSingleConditionValue();
            var methodInfo = typeof(ConditionExpressionExtensions).GetMethod("IsUnder", BindingFlags.NonPublic | BindingFlags.Static);
            
            var entityNameExpr = Expression.Constant(c.EntityName);
            var ancestorIdExpr = Expression.Constant(ancestorId);
            var contextExpr = Expression.Constant(context, typeof(IXrmFakedContext));
            
            var getValueMethod = typeof(ConditionExpressionExtensions).GetMethod("GetEntityIdOrAttributeValue", BindingFlags.NonPublic | BindingFlags.Static);
            var attrNameExpr = Expression.Constant(c.GetAttributeName());
            var valueExpr = Expression.Call(getValueMethod, entity, attrNameExpr);

            return Expression.Call(methodInfo, valueExpr, entityNameExpr, ancestorIdExpr, contextExpr);
        }

        internal static Expression ToUnderOrEqualExpression(this TypedConditionExpression c, IXrmFakedContext context, ParameterExpression entity)
        {
            var ancestorId = (Guid)c.GetSingleConditionValue();
            var methodInfo = typeof(ConditionExpressionExtensions).GetMethod("IsUnderOrEqual", BindingFlags.NonPublic | BindingFlags.Static);
            
            var entityNameExpr = Expression.Constant(c.EntityName);
            var ancestorIdExpr = Expression.Constant(ancestorId);
            var contextExpr = Expression.Constant(context, typeof(IXrmFakedContext));
            
            var getValueMethod = typeof(ConditionExpressionExtensions).GetMethod("GetEntityIdOrAttributeValue", BindingFlags.NonPublic | BindingFlags.Static);
            var attrNameExpr = Expression.Constant(c.GetAttributeName());
            var valueExpr = Expression.Call(getValueMethod, entity, attrNameExpr);
            
            return Expression.Call(methodInfo, valueExpr, entityNameExpr, ancestorIdExpr, contextExpr);
        }

        internal static Expression ToAboveExpression(this TypedConditionExpression c, IXrmFakedContext context, ParameterExpression entity)
        {
             var descendantId = (Guid)c.GetSingleConditionValue();
             var methodInfo = typeof(ConditionExpressionExtensions).GetMethod("IsAbove", BindingFlags.NonPublic | BindingFlags.Static);
             
             var entityNameExpr = Expression.Constant(c.EntityName);
             var descendantIdExpr = Expression.Constant(descendantId);
             var contextExpr = Expression.Constant(context, typeof(IXrmFakedContext));
             
             var getValueMethod = typeof(ConditionExpressionExtensions).GetMethod("GetEntityIdOrAttributeValue", BindingFlags.NonPublic | BindingFlags.Static);
             var attrNameExpr = Expression.Constant(c.GetAttributeName());
             var valueExpr = Expression.Call(getValueMethod, entity, attrNameExpr);
             
             return Expression.Call(methodInfo, valueExpr, entityNameExpr, descendantIdExpr, contextExpr);
        }

        internal static Expression ToAboveOrEqualExpression(this TypedConditionExpression c, IXrmFakedContext context, ParameterExpression entity)
        {
             var descendantId = (Guid)c.GetSingleConditionValue();
             var methodInfo = typeof(ConditionExpressionExtensions).GetMethod("IsAboveOrEqual", BindingFlags.NonPublic | BindingFlags.Static);
             
             var entityNameExpr = Expression.Constant(c.EntityName);
             var descendantIdExpr = Expression.Constant(descendantId);
             var contextExpr = Expression.Constant(context, typeof(IXrmFakedContext));
             
             var getValueMethod = typeof(ConditionExpressionExtensions).GetMethod("GetEntityIdOrAttributeValue", BindingFlags.NonPublic | BindingFlags.Static);
             var attrNameExpr = Expression.Constant(c.GetAttributeName());
             var valueExpr = Expression.Call(getValueMethod, entity, attrNameExpr);
             
             return Expression.Call(methodInfo, valueExpr, entityNameExpr, descendantIdExpr, contextExpr);
        }

        internal static Expression ToNotUnderExpression(this TypedConditionExpression c, IXrmFakedContext context, ParameterExpression entity)
        {
            // NotUnder is simply !IsUnder
            var underExpr = ToUnderExpression(c, context, entity);
            return Expression.Not(underExpr);
        }
        
        private static object GetEntityIdOrAttributeValue(Entity entity, string attributeName)
        {
            if (entity == null) return null;
            
            bool isId = attributeName.Equals(entity.LogicalName + "id", StringComparison.OrdinalIgnoreCase);
            
            if (isId)
            {
                return entity.Id;
            }
            if (entity.Attributes.ContainsKey(attributeName))
            {
                var val = entity[attributeName];
                if (val is AliasedValue)
                {
                    return (val as AliasedValue).Value;
                }
                return val;
            }
            return null;
        }

        private static bool IsUnder(object startNode, string entityName, Guid ancestorId, IXrmFakedContext context)
        {
            if (startNode is EntityReference er)
            {
                entityName = er.LogicalName;
            }

            var startId = ResolveId(startNode);
            
            if (startId == Guid.Empty) return false;

            var currentId = startId;
            var path = new System.Collections.Generic.HashSet<Guid>();
                
            while(true)
            {
                if(path.Contains(currentId)) return false; // Cycle detected
                path.Add(currentId);

                var parentRef = GetParentReference(currentId, entityName, context);
                
                if(parentRef == null) 
                {
                    return false; 
                }

                if(parentRef.Id == ancestorId) return true; // Found ancestor

                currentId = parentRef.Id;
            }
        }

        private static bool IsUnderOrEqual(object startNode, string entityName, Guid ancestorId, IXrmFakedContext context)
        {
            var startId = ResolveId(startNode);
            if (startId == ancestorId) return true;
            return IsUnder(startNode, entityName, ancestorId, context);
        }

        private static bool IsAbove(object startNode, string entityName, Guid descendantId, IXrmFakedContext context)
        {
             // IsAbove(A, B) means A is above B. B is descendant of A.
             // Equivalent to IsUnder(B, A).
             // But here startNode is A. descendantId is B.
             
             if (startNode is EntityReference er)
             {
                 entityName = er.LogicalName;
             }

             // So we check if B is under A.
             return IsUnder(descendantId, entityName, (Guid)ResolveId(startNode), context);
        }

        private static bool IsAboveOrEqual(object startNode, string entityName, Guid descendantId, IXrmFakedContext context)
        {
            var startId = ResolveId(startNode);
            if (startId == descendantId) return true;
            return IsAbove(startNode, entityName, descendantId, context);
        }

        private static Guid ResolveId(object node)
        {
            if (node is Guid g) return g;
            if (node is EntityReference er) return er.Id;
            if (node is Entity e) return e.Id;
            return Guid.Empty;
        }

        private static EntityReference GetParentReference(Guid entityId, string entityName, IXrmFakedContext context)
        {
             // We use a simple iteration to find the record to avoid potential LINQ provider limitations
             // when calling CreateQuery inside a query execution.
             var query = context.CreateQuery(entityName);
             foreach(var entity in query)
             {
                 if (entity.Id == entityId)
                 {
                     string parentAttrName = "parent" + entityName + "id";
             
                     if (entityName == "systemuser") parentAttrName = "parentsystemuserid";
                     if (entityName == "position") parentAttrName = "parentpositionid";
                     
                     if (entity.Attributes.ContainsKey(parentAttrName))
                     {
                         return entity[parentAttrName] as EntityReference;
                     }
                     return null;
                 }
             }
             return null;
        }

        // BindingFlags helper for Current class
        // private const BindingFlags Current = BindingFlags.NonPublic | BindingFlags.Static;
    }
}
