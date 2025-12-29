#if !NET452 && !FAKE_XRM_EASY_365
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Snapshots
{
    public class SnapshotExtensionsTests : FakeXrmEasyTestsBase
    {
        private readonly string _testSnapshotDir;

        public SnapshotExtensionsTests()
        {
            _testSnapshotDir = Path.Combine(Path.GetTempPath(), "FakeXrmEasyTests", Guid.NewGuid().ToString());
        }

        [Fact]
        public void Should_save_snapshot_with_explicit_name()
        {
            // Arrange
            var context = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Doe"
            };
            context.Initialize(contact);

            var snapshotName = "test_snapshot.json";

            // Act
            context.SaveSnapshot(_testSnapshotDir, snapshotName);

            // Assert
            var expectedPath = Path.Combine(_testSnapshotDir, snapshotName);
            Assert.True(File.Exists(expectedPath), $"Snapshot file should exist at {expectedPath}");
        }

        [Fact]
        public void Should_load_snapshot_with_explicit_name()
        {
            // Arrange
            var setupContext = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var contactId = Guid.NewGuid();
            var contact = new Entity("contact")
            {
                Id = contactId,
                ["firstname"] = "John",
                ["lastname"] = "Doe"
            };
            setupContext.Initialize(contact);

            var snapshotName = "test_load_snapshot.json";
            setupContext.SaveSnapshot(_testSnapshotDir, snapshotName);

            // Act
            var loadContext = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            loadContext.InitializeFromSnapshot(_testSnapshotDir, snapshotName);

            // Assert
            var service = loadContext.GetOrganizationService();
            var loadedContact = service.Retrieve("contact", contactId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Assert.NotNull(loadedContact);
            Assert.Equal("John", loadedContact.GetAttributeValue<string>("firstname"));
            Assert.Equal("Doe", loadedContact.GetAttributeValue<string>("lastname"));
        }

        [Fact]
        public void Should_generate_snapshot_path_with_caller_attributes()
        {
            // Arrange
            var context = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Jane"
            };
            context.Initialize(contact);

            // Act - CallerMemberName, CallerFilePath, and CallerLineNumber should auto-populate
            context.SaveSnapshot(_testSnapshotDir);

            // Assert - should create file at {baseDir}/{ClassName}/{TestMethodName}_{LineNumber}.json
            var files = Directory.GetFiles(_testSnapshotDir, "*.json", SearchOption.AllDirectories);
            Assert.Single(files);
            Assert.Contains("SnapshotExtensionsTests", files[0]);
            Assert.Contains("Should_generate_snapshot_path_with_caller_attributes", files[0]);
        }

        [Fact]
        public void Should_round_trip_entities_with_various_attribute_types()
        {
            // Arrange
            var setupContext = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var accountId = Guid.NewGuid();
            var contactId = Guid.NewGuid();
            
            var account = new Entity("account")
            {
                Id = accountId,
                ["name"] = "Test Account",
                ["revenue"] = new Money(100000.50m),
                ["createdon"] = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                ["numberofemployees"] = 150
            };

            var contact = new Entity("contact")
            {
                Id = contactId,
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["parentcustomerid"] = new EntityReference("account", accountId),
                ["donotemail"] = true
            };

            setupContext.Initialize(new List<Entity> { account, contact });

            var snapshotName = "roundtrip_test.json";

            // Act
            setupContext.SaveSnapshot(_testSnapshotDir, snapshotName);
            var loadContext = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            loadContext.InitializeFromSnapshot(_testSnapshotDir, snapshotName);

            // Assert
            var service = loadContext.GetOrganizationService();
            var loadedAccount = service.Retrieve("account", accountId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var loadedContact = service.Retrieve("contact", contactId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            Assert.Equal("Test Account", loadedAccount.GetAttributeValue<string>("name"));
            Assert.Equal(100000.50m, loadedAccount.GetAttributeValue<Money>("revenue").Value);
            Assert.Equal(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc), loadedAccount.GetAttributeValue<DateTime>("createdon"));
            Assert.Equal(150, loadedAccount.GetAttributeValue<int>("numberofemployees"));

            Assert.Equal("John", loadedContact.GetAttributeValue<string>("firstname"));
            Assert.Equal("Doe", loadedContact.GetAttributeValue<string>("lastname"));
            Assert.True(loadedContact.GetAttributeValue<bool>("donotemail"));
            Assert.Equal(accountId, loadedContact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
        }

        [Fact]
        public void Should_throw_when_snapshot_file_does_not_exist()
        {
            // Arrange
            var context = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var nonExistentSnapshot = "does_not_exist.json";

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() =>
                context.InitializeFromSnapshot(_testSnapshotDir, nonExistentSnapshot)
            );
            Assert.Contains(nonExistentSnapshot, exception.Message);
        }

        [Fact]
        public void Should_use_callback_when_snapshot_does_not_exist()
        {
            // Arrange
            var context = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var snapshotName = "callback_test.json";
            var callbackInvoked = false;
            var contactId = Guid.NewGuid();

            // Act
            context.InitializeFromSnapshotOrCreate(_testSnapshotDir, (ctx) =>
            {
                callbackInvoked = true;
                var newContact = new Entity("contact")
                {
                    Id = contactId,
                    ["firstname"] = "Created",
                    ["lastname"] = "ByCallback"
                };
                ctx.Initialize(newContact);
            }, snapshotName);

            // Assert
            Assert.True(callbackInvoked, "Callback should have been invoked");
            var expectedPath = Path.Combine(_testSnapshotDir, snapshotName);
            Assert.True(File.Exists(expectedPath), "Snapshot should have been saved");

            var service = context.GetOrganizationService();
            var contact = service.Retrieve("contact", contactId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Assert.Equal("Created", contact.GetAttributeValue<string>("firstname"));
            Assert.Equal("ByCallback", contact.GetAttributeValue<string>("lastname"));
        }

        [Fact]
        public void Should_load_from_snapshot_when_it_exists_and_skip_callback()
        {
            // Arrange - First create a snapshot
            var setupContext = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var contactId = Guid.NewGuid();
            var contact = new Entity("contact")
            {
                Id = contactId,
                ["firstname"] = "FromSnapshot",
                ["lastname"] = "NotCallback"
            };
            setupContext.Initialize(contact);
            var snapshotName = "existing_snapshot.json";
            setupContext.SaveSnapshot(_testSnapshotDir, snapshotName);

            // Act - Try to load with a callback that should NOT be invoked
            var loadContext = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var callbackInvoked = false;
            loadContext.InitializeFromSnapshotOrCreate(_testSnapshotDir, (ctx) =>
            {
                callbackInvoked = true;
            }, snapshotName);

            // Assert
            Assert.False(callbackInvoked, "Callback should NOT have been invoked when snapshot exists");
            var service = loadContext.GetOrganizationService();
            var loadedContact = service.Retrieve("contact", contactId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Assert.Equal("FromSnapshot", loadedContact.GetAttributeValue<string>("firstname"));
            Assert.Equal("NotCallback", loadedContact.GetAttributeValue<string>("lastname"));
        }

        [Fact]
        public void Should_create_directory_structure_when_saving_snapshot()
        {
            // Arrange
            var context = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var contact = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Test" };
            context.Initialize(contact);

            var deepPath = Path.Combine(_testSnapshotDir, "level1", "level2", "level3");
            var snapshotName = "deep_snapshot.json";

            // Act
            context.SaveSnapshot(deepPath, snapshotName);

            // Assert
            Assert.True(Directory.Exists(deepPath), "Directory structure should be created");
            var expectedPath = Path.Combine(deepPath, snapshotName);
            Assert.True(File.Exists(expectedPath), "Snapshot file should exist in deep directory");
        }

        [Fact]
        public void Should_throw_when_initialize_called_twice()
        {
            // Arrange
            var context = (XrmFakedContext)XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var snapshotName = "double_init.json";
            var contact = new Entity("contact") { Id = Guid.NewGuid() };
            context.Initialize(contact);
            context.SaveSnapshot(_testSnapshotDir, snapshotName);

            // Act & Assert - Try to initialize from snapshot on already-initialized context
            var exception = Assert.Throws<Exception>(() =>
                context.InitializeFromSnapshot(_testSnapshotDir, snapshotName)
            );
            Assert.Contains("Initialize should be called only once", exception.Message);
        }
    }
}
#endif
