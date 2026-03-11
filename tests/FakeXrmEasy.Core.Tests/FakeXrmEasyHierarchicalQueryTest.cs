using Xunit;
using FakeXrmEasy;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware; // Added
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace FakeXrmEasy.Core.Tests
{
    public class FakeXrmEasyHierarchicalQueryTest
    {
        [Fact]
        public void EqualOrUnder_Operator_Not_Supported_Returns_No_Results()
        {
            // Arrange: Setup FakeXrmEasy context
            var context = XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial); // Changed
            var service = context.GetOrganizationService();

            // Create a parent business unit
            var parentBU = new Entity("businessunit")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Parent Business Unit"
            };

            // Create a child business unit
            var childBU = new Entity("businessunit")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Child Business Unit",
                ["parentbusinessunitid"] = parentBU.ToEntityReference()
            };

            // Create a test contact owned by the child business unit
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["owningbusinessunit"] = childBU.ToEntityReference()
            };

            // Initialize the context with test data
            context.Initialize(new[] { parentBU, childBU, contact });

            // Act: Query for contacts using EqualOrUnder operator on the parent BU
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "lastname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("owningbusinessunit", ConditionOperator.UnderOrEqual, parentBU.Id) 
                    }
                }
            };

            var results = service.RetrieveMultiple(query);
            
            // Expected: 1 contact (the one in child BU)
            Assert.Single(results.Entities);
        }

        [Fact]
        public void EqualOrUnder_Operator_With_FetchXml_Not_Supported()
        {
            // Arrange: Setup FakeXrmEasy context
            var context = XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var service = context.GetOrganizationService();

            // Create a parent business unit
            var parentBU = new Entity("businessunit")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Parent Business Unit"
            };

            // Create a child business unit
            var childBU = new Entity("businessunit")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Child Business Unit",
                ["parentbusinessunitid"] = parentBU.ToEntityReference()
            };

            // Create a test contact owned by the child business unit
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["owningbusinessunit"] = childBU.ToEntityReference()
            };

            // Initialize the context with test data
            context.Initialize(new[] { parentBU, childBU, contact });

            // Act: Query using FetchXML with eq-or-under operator
            var fetchXml = $@"
                <fetch>
                    <entity name='contact'>
                        <attribute name='firstname' />
                        <attribute name='lastname' />
                        <filter>
                            <condition attribute='owningbusinessunit' operator='eq-or-under' value='{parentBU.Id}' />
                        </filter>
                    </entity>
                </fetch>";

            var fetchQuery = new FetchExpression(fetchXml);

            var results = service.RetrieveMultiple(fetchQuery);

            Assert.Single(results.Entities);
        }

        [Fact]
        public void EqualOrUnder_Operator_In_LinkEntity_Returns_Results()
        {
            // Arrange
            var context = XrmFakedContextFactory.New(FakeXrmEasyLicense.NonCommercial);
            var service = context.GetOrganizationService();

            var parentBU = new Entity("businessunit")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Parent Business Unit"
            };

            var childBU = new Entity("businessunit")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Child Business Unit",
                ["parentbusinessunitid"] = parentBU.ToEntityReference()
            };

            // User linked to Child BU
            var user = new Entity("systemuser")
            {
                Id = Guid.NewGuid(),
                ["businessunitid"] = childBU.ToEntityReference(),
                ["firstname"] = "Test User"
            };

            context.Initialize(new[] { parentBU, childBU, user });

            // Query: Find users where the linked Business Unit is UnderOrEqual the Parent BU
            var query = new QueryExpression("systemuser");
            query.ColumnSet = new ColumnSet("firstname");
            
            // Link to Business Unit
            var buLink = query.AddLink("businessunit", "businessunitid", "businessunitid");
            buLink.EntityAlias = "bu";
            
            // Add Condition on the Linked Entity (Business Unit)
            // "bu.businessunitid" UnderOrEqual the parentBU.Id
            // Note: In real Dataverse, we usually query on 'businessunitid' of the BU entity.
            buLink.LinkCriteria.AddCondition("businessunitid", ConditionOperator.UnderOrEqual, parentBU.Id);

            // Act
            var results = service.RetrieveMultiple(query);

            // Assert
            Assert.Single(results.Entities);
            Assert.Equal(user.Id, results.Entities[0].Id);
        }
    }
}
