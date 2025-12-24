#if !NET452
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Snapshots
{
    public class SnapshotCleanInitializationTests : IDisposable
    {
        private readonly string _testSnapshotDir;

        public SnapshotCleanInitializationTests()
        {
            _testSnapshotDir = Path.Combine(Path.GetTempPath(), "FakeXrmEasyTests", Guid.NewGuid().ToString());
            if (!Directory.Exists(_testSnapshotDir))
            {
                Directory.CreateDirectory(_testSnapshotDir);
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_testSnapshotDir))
            {
                Directory.Delete(_testSnapshotDir, true);
            }
        }

        [Fact]
        public void InitializeFromSnapshotOrCreate_Should_Use_A_Different_Context_For_Setup_Callback()
        {
            // Arrange
            var context = new XrmFakedContext(FakeXrmEasyLicense.NonCommercial);
            IXrmFakedContext callbackContext = null;
            var snapshotName = "test_clean_init.json";

            // Act
            context.InitializeFromSnapshotOrCreate(_testSnapshotDir, (ctx) =>
            {
                callbackContext = ctx;
            }, snapshotName);

            // Assert
            Assert.NotNull(callbackContext);
            Assert.NotSame(context, callbackContext);
        }

        [Fact]
        public void InitializeFromSnapshotOrCreate_Should_Initialize_Main_Context_From_Callback_Context()
        {
            // Arrange
            var context = new XrmFakedContext(FakeXrmEasyLicense.NonCommercial);
            var contactId = Guid.NewGuid();
            var snapshotName = "test_init_transfer.json";

            // Act
            context.InitializeFromSnapshotOrCreate(_testSnapshotDir, (ctx) =>
            {
                var service = ctx.GetOrganizationService();
                service.Create(new Entity("contact") { Id = contactId });
            }, snapshotName);

            // Assert
            var contact = context.CreateQuery("contact").FirstOrDefault();
            Assert.NotNull(contact);
            Assert.Equal(contactId, contact.Id);
        }

        [Fact]
        public void InitializeFromSnapshotOrCreate_Should_Preserve_Proxy_Types_In_Callback_Context()
        {
            // Arrange
            var context = new XrmFakedContext(FakeXrmEasyLicense.NonCommercial);
            context.EnableProxyTypes(typeof(Entity).Assembly); // Just an example assembly
            var snapshotName = "test_proxy_transfer.json";
            bool hasProxyTypes = false;

            // Act
            context.InitializeFromSnapshotOrCreate(_testSnapshotDir, (ctx) =>
            {
                hasProxyTypes = ctx.ProxyTypesAssemblies.Any();
            }, snapshotName);

            // Assert
            Assert.True(hasProxyTypes, "Callback context should have proxy types enabled if the main context has them.");
        }
    }
}
#endif
