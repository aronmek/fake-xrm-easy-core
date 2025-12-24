using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Extensions
{
    /// <summary>
    /// Extension methods for IXrmFakedContext
    /// </summary>
    public static class IXrmFakedContextExtensions
    {
        /// <summary>
        /// Returns all the entities in the context
        /// </summary>
        /// <param name="context">The context to get entities from</param>
        /// <returns>A list of all entities in the context</returns>
        public static List<Entity> GetAllEntities(this IXrmFakedContext context)
        {
            var fakedContext = context as XrmFakedContext;
            if (fakedContext == null)
            {
                return new List<Entity>();
            }

            var entities = new List<Entity>();
            foreach (var table in fakedContext.Db._tables)
            {
                foreach (var e in table.Value.Rows)
                {
                    var type = fakedContext.FindReflectedType(e.LogicalName);
                    if (type != null)
                    {
                        entities.Add(e.Clone(type, fakedContext));
                    }
                    else
                    {
                        entities.Add(e.Clone(fakedContext));
                    }
                }
            }
            return entities;
        }
    }
}
