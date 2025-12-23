using System;
using System.Collections.Generic;

namespace FakeXrmEasy.Snapshots
{
    /// <summary>
    /// Simplified DTO for serializing Entity data without complex Xrm SDK collection types
    /// </summary>
    public class EntitySnapshot
    {
        public string LogicalName { get; set; }
        public Guid Id { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public Dictionary<string, string> FormattedValues { get; set; }

        public EntitySnapshot()
        {
            Attributes = new Dictionary<string, object>();
            FormattedValues = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Serializable representation of EntityReference
    /// </summary>
    public class EntityReferenceSnapshot
    {
        public string LogicalName { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Serializable representation of Money
    /// </summary>
    public class MoneySnapshot
    {
        public decimal Value { get; set; }
        public string Type { get; set; } = "Money";
    }

    /// <summary>
    /// Serializable representation of OptionSetValue
    /// </summary>
    public class OptionSetValueSnapshot
    {
        public int Value { get; set; }
        public string Type { get; set; } = "OptionSetValue";
    }

    /// <summary>
    /// Serializable representation of Decimal to preserve type fidelity during JSON serialization
    /// </summary>
    public class DecimalSnapshot
    {
        public decimal Value { get; set; }
        public string Type { get; set; } = "Decimal";
    }

}
