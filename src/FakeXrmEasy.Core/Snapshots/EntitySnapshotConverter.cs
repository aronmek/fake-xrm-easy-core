#if !NET452
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Snapshots
{
    /// <summary>
    /// Converts between Entity and EntitySnapshot for serialization
    /// </summary>
    public static class EntitySnapshotConverter
    {
        /// <summary>
        /// Converts an Entity to a serializable EntitySnapshot
        /// </summary>
        public static EntitySnapshot ToSnapshot(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var snapshot = new EntitySnapshot
            {
                LogicalName = entity.LogicalName,
                Id = entity.Id
            };

            // Convert attributes
            foreach (var attr in entity.Attributes)
            {
                snapshot.Attributes[attr.Key] = ConvertAttributeValue(attr.Value);
            }

            // Copy formatted values
            foreach (var fv in entity.FormattedValues)
            {
                snapshot.FormattedValues[fv.Key] = fv.Value;
            }

            return snapshot;
        }

        /// <summary>
        /// Converts an EntitySnapshot back to an Entity
        /// </summary>
        public static Entity FromSnapshot(EntitySnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            var entity = new Entity(snapshot.LogicalName)
            {
                Id = snapshot.Id
            };

            // Convert attributes back
            foreach (var attr in snapshot.Attributes)
            {
                entity[attr.Key] = RestoreAttributeValue(attr.Value);
            }

            // Restore formatted values
            foreach (var fv in snapshot.FormattedValues)
            {
                entity.FormattedValues[fv.Key] = fv.Value;
            }

            return entity;
        }

        /// <summary>
        /// Converts an attribute value to a serializable representation
        /// </summary>
        private static object ConvertAttributeValue(object value)
        {
            if (value == null)
                return null;

            // EntityReference → EntityReferenceSnapshot
            if (value is EntityReference entityRef)
            {
                return new EntityReferenceSnapshot
                {
                    LogicalName = entityRef.LogicalName,
                    Id = entityRef.Id,
                    Name = entityRef.Name
                };
            }

            // Money → MoneySnapshot
            if (value is Money money)
            {
                return new MoneySnapshot
                {
                    Value = money.Value
                };
            }

            // OptionSetValue → OptionSetValueSnapshot
            if (value is OptionSetValue optionSet)
            {
                return new OptionSetValueSnapshot
                {
                    Value = optionSet.Value
                };
            }

            // Enum → OptionSetValueSnapshot
            if (value.GetType().IsEnum)
            {
                return new OptionSetValueSnapshot
                {
                    Value = (int)value
                };
            }

            // Decimal → DecimalSnapshot
            if (value is decimal decimalValue)
            {
                return new DecimalSnapshot
                {
                    Value = decimalValue
                };
            }

            // Simple types: string, int, bool, Guid, DateTime, double, long
            if (value is string || value is int || value is bool ||
                value is Guid || value is DateTime || value is double || value is long ||
                value is short || value is byte)
            {
                return value;
            }

            // Unsupported type - throw for now to identify during testing
            throw new NotSupportedException($"Attribute value type {value.GetType().Name} is not supported for snapshot serialization. Please add support or file an issue.");
        }

        /// <summary>
        /// Restores an attribute value from its serializable representation
        /// </summary>
        private static object RestoreAttributeValue(object value)
        {
            if (value == null)
                return null;

            // Handle System.Text.Json's JsonElement deserialization
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                return RestoreFromJsonElement(jsonElement);
            }

            // EntityReferenceSnapshot → EntityReference
            if (value is EntityReferenceSnapshot entityRefSnapshot)
            {
                return new EntityReference(entityRefSnapshot.LogicalName, entityRefSnapshot.Id)
                {
                    Name = entityRefSnapshot.Name
                };
            }

            // MoneySnapshot → Money
            if (value is MoneySnapshot moneySnapshot)
            {
                return new Money(moneySnapshot.Value);
            }

            // OptionSetValueSnapshot → OptionSetValue
            if (value is OptionSetValueSnapshot optionSetSnapshot)
            {
                return new OptionSetValue(optionSetSnapshot.Value);
            }

            // DecimalSnapshot → decimal
            if (value is DecimalSnapshot decimalSnapshot)
            {
                return decimalSnapshot.Value;
            }

            // Newtonsoft.Json sometimes deserializes numbers as long/int64
            if (value is long longValue)
            {
                // Keep as long if it doesn't fit in int
                if (longValue >= int.MinValue && longValue <= int.MaxValue)
                {
                    return (int)longValue;
                }
                return longValue;
            }

            // Simple types pass through
            if (value is string || value is int || value is decimal || value is bool ||
                value is Guid || value is DateTime || value is double || 
                value is short || value is byte)
            {
                return value;
            }

            // Unsupported type
            throw new NotSupportedException($"Attribute value type {value.GetType().FullName} cannot be restored from snapshot. Please add support or file an issue.");
        }

        /// <summary>
        /// Restores a value from System.Text.Json.JsonElement
        /// </summary>
        private static object RestoreFromJsonElement(System.Text.Json.JsonElement element)
        {
            switch (element.ValueKind)
            {
                case System.Text.Json.JsonValueKind.String:
                    // Could be a string or a Guid or DateTime
                    var stringValue = element.GetString();
                    if (Guid.TryParse(stringValue, out var guidValue))
                        return guidValue;
                    if (DateTime.TryParse(stringValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateValue))
                        return dateValue;
                    return stringValue;

                case System.Text.Json.JsonValueKind.Number:
                    // Try to get as various numeric types
                    if (element.TryGetInt32(out var intValue))
                        return intValue;
                    if (element.TryGetInt64(out var longValue))
                        return longValue;
                    if (element.TryGetDecimal(out var decimalValue))
                        return decimalValue;
                    if (element.TryGetDouble(out var doubleValue))
                        return doubleValue;
                    return element.GetRawText();

                case System.Text.Json.JsonValueKind.True:
                    return true;

                case System.Text.Json.JsonValueKind.False:
                    return false;

                case System.Text.Json.JsonValueKind.Object:
                    // Check if it's one of our snapshot types
                    if (element.TryGetProperty("LogicalName", out var logicalNameProp) && 
                        element.TryGetProperty("Id", out var idProp))
                    {
                        // EntityReferenceSnapshot
                        var logicalName = logicalNameProp.GetString();
                        var id = Guid.Parse(idProp.GetString());
                        var name = element.TryGetProperty("Name", out var nameProp) ? nameProp.GetString() : null;
                        
                        return new EntityReference(logicalName, id) { Name = name };
                    }

                    // Check for explicit Type property (added to disambiguate Money vs OptionSetValue)
                    if (element.TryGetProperty("Type", out var typeProp))
                    {
                        var type = typeProp.GetString();
                        if (type == "Money" && element.TryGetProperty("Value", out var moneyValProp))
                        {
                             if (moneyValProp.TryGetDecimal(out var moneyValue))
                                return new Money(moneyValue);
                        }
                        if (type == "OptionSetValue" && element.TryGetProperty("Value", out var osvValProp))
                        {
                             if (osvValProp.TryGetInt32(out var osvValue))
                                return new OptionSetValue(osvValue);
                        }
                        if (type == "Decimal" && element.TryGetProperty("Value", out var decimalValProp))
                        {
                             if (decimalValProp.TryGetDecimal(out var decValue))
                                return decValue;
                        }
                    }

                    if (element.TryGetProperty("Value", out var valueProp))
                    {
                        // Could be MoneySnapshot or OptionSetValueSnapshot
                        if (valueProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            // Check if it's decimal (Money) or int (OptionSetValue)
                            // Prioritize OptionSetValue (int) because TryGetDecimal succeeds for integers too.
                            // This fixes the issue where OptionSetValue was deserialized as Money.
                            // Note: Money(100) (integer value) will be deserialized as OptionSetValue(100) in this fallback path,
                            // but new snapshots will have the Type property to disambiguate.
                            if (valueProp.TryGetInt32(out var optionSetValue))
                            {
                                return new OptionSetValue(optionSetValue);
                            }
                            
                            if (valueProp.TryGetDecimal(out var moneyValue))
                            {
                                return new Money(moneyValue);
                            }
                        }
                    }

                    throw new NotSupportedException($"Cannot restore JsonElement object: {element.GetRawText()}");

                case System.Text.Json.JsonValueKind.Null:
                    return null;

                default:
                    throw new NotSupportedException($"JsonElement ValueKind {element.ValueKind} is not supported");
            }
        }

        /// <summary>
        /// Converts a list of entities to snapshots
        /// </summary>
        public static List<EntitySnapshot> ToSnapshots(IEnumerable<Entity> entities)
        {
            if (entities == null)
                return new List<EntitySnapshot>();

            return entities.Select(ToSnapshot).ToList();
        }

        /// <summary>
        /// Converts a list of snapshots to entities
        /// </summary>
        public static List<Entity> FromSnapshots(IEnumerable<EntitySnapshot> snapshots)
        {
            if (snapshots == null)
                return new List<Entity>();

            return snapshots.Select(FromSnapshot).ToList();
        }
    }
}
#endif
