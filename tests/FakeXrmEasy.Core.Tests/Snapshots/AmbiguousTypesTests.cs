using FakeXrmEasy.Snapshots;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Snapshots
{
    public class AmbiguousTypesTests
    {
        [Fact]
        public void When_Restoring_OptionSetValue_It_Should_Not_Be_Confused_With_Money()
        {
            // Arrange
            var entity = new Entity("test");
            entity["statuscode"] = new OptionSetValue(1);
            
            // Act
            var snapshot = EntitySnapshotConverter.ToSnapshot(entity);
            
            // Serialize and Deserialize to simulate the full cycle which produces JsonElements
            var json = JsonSerializer.Serialize(snapshot);
            var deserializedSnapshot = JsonSerializer.Deserialize<EntitySnapshot>(json);
            
            var restoredEntity = EntitySnapshotConverter.FromSnapshot(deserializedSnapshot);

            // Assert
            Assert.IsType<OptionSetValue>(restoredEntity["statuscode"]);
            Assert.Equal(1, ((OptionSetValue)restoredEntity["statuscode"]).Value);
        }

        [Fact]
        public void Check_Decimal_Serialization()
        {
            decimal d = 100m; // No decimal point
            string json = JsonSerializer.Serialize(d);
            // Console.WriteLine($"Decimal 100 serialized: {json}");
            
            Assert.Equal("100", json); 
            
            var doc = JsonDocument.Parse(json);
            bool isInt = doc.RootElement.TryGetInt32(out _);
            Assert.True(isInt, "100 should be parsed as int32");
        }

        [Fact]
        public void When_Restoring_Decimal_It_Should_Preserve_Type_Even_If_Whole_Number()
        {
            // Arrange
            var entity = new Entity("test");
            entity["quantity"] = 10m; // Whole number decimal
            
            // Act
            var snapshot = EntitySnapshotConverter.ToSnapshot(entity);
            
            // Serialize and Deserialize
            var json = JsonSerializer.Serialize(snapshot);
            var deserializedSnapshot = JsonSerializer.Deserialize<EntitySnapshot>(json);
            
            var restoredEntity = EntitySnapshotConverter.FromSnapshot(deserializedSnapshot);

            // Assert
            Assert.IsType<decimal>(restoredEntity["quantity"]);
            Assert.Equal(10m, (decimal)restoredEntity["quantity"]);
        }

    }
}
