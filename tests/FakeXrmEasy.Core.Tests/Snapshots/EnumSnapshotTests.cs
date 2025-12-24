#if !NET452
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Snapshots
{
    public enum TestEnum
    {
        Value1 = 1,
        Value2 = 2
    }

    public class EnumSnapshotTests : FakeXrmEasyTestsBase
    {
        private readonly string _testSnapshotDir;

        public EnumSnapshotTests()
        {
            _testSnapshotDir = Path.Combine(Path.GetTempPath(), "FakeXrmEasyTests", Guid.NewGuid().ToString());
        }

        [Fact]
        public void Should_serialize_enum_as_optionsetvalue_in_snapshot()
        {
            // Arrange
            var context = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var entity = new Entity("test_entity")
            {
                Id = Guid.NewGuid(),
                ["statuscode"] = TestEnum.Value2 // Set as Enum directly
            };
            context.Initialize(entity);

            var snapshotName = "enum_test.json";

            // Act
            context.SaveSnapshot(_testSnapshotDir, snapshotName);

            // Assert - Load it back and check if it's an OptionSetValue (standard Xrm behavior after deserialization)
            var loadContext = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            loadContext.InitializeFromSnapshot(_testSnapshotDir, snapshotName);

            var service = loadContext.GetOrganizationService();
            var loadedEntity = service.Retrieve("test_entity", entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            Assert.NotNull(loadedEntity);
            Assert.True(loadedEntity.Contains("statuscode"));
            
            // The snapshot deserializer should have converted it to OptionSetValue or int, 
            // but definitely not left it as an Enum type that might not exist in the loading context if it were dynamic.
            // However, since we are in the same assembly, the Enum exists.
            // But the goal of the fix was to ensure it's serialized as OptionSetValueSnapshot.
            
            var value = loadedEntity["statuscode"];
            Assert.IsType<OptionSetValue>(value);
            Assert.Equal((int)TestEnum.Value2, ((OptionSetValue)value).Value);
        }
    }
}
#endif
