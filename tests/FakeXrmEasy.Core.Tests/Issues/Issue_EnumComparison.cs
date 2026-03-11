using System;
using System.Linq;
using System.Runtime.Serialization;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Xunit;
using System.Reflection;

namespace FakeXrmEasy.Core.Tests.Issues
{
    public class FakeXrmEasyReproTests
    {
        [Fact]
        public void RetrieveMultiple_WithFetchXml_AndEnumProperty_ShouldThrowArithmeticException_IfBugExists()
        {
            // ARRANGES
            var context = MiddlewareBuilder
                .New()
                .AddCrud()
                .UseMessages()
                .UseCrud()
                .SetLicense(FakeXrmEasyLicense.NonCommercial)
                .Build();

            if (context is FakeXrmEasy.XrmFakedContext fakedContext)
            {
                // Matches TestFixtureBase configuration
                fakedContext.SuppressProxyTypesForRetrieveMultiple = true;
            }

            // Enable Proxy Types to force mapping to Strong Types
            context.EnableProxyTypes(typeof(ReproEntity).Assembly);
            
            var entity = new ReproEntity
            {
                Id = Guid.NewGuid(),
            };
            entity["statecode"] = new OptionSetValue(0);
            context.Initialize(new[] { entity });
            
            var service = context.GetOrganizationService();

            // ACT
            // Use QueryExpression directly to mimic what LINQ/FetchXml translates to
            var qe = new QueryExpression("repro_entity");
            qe.ColumnSet = new ColumnSet(true);
            // This is the key: Passing an INT to a condition on an Enum field
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            var results = service.RetrieveMultiple(qe);
            Assert.Single(results.Entities);
        }

        [Fact]
        public void RetrieveMultiple_WithFetchXml_AndEnumProperty_ShouldWorkWithEnumValues()
        {
            // ARRANGES
            var context = MiddlewareBuilder
                .New()
                .AddCrud()
                .UseMessages()
                .UseCrud()
                .SetLicense(FakeXrmEasyLicense.NonCommercial)
                .Build();

            if (context is FakeXrmEasy.XrmFakedContext fakedContext)
            {
                fakedContext.SuppressProxyTypesForRetrieveMultiple = true;
            }

            context.EnableProxyTypes(typeof(ReproEntity).Assembly);
            
            var entity = new ReproEntity
            {
                Id = Guid.NewGuid(),
            };
            entity["statecode"] = new OptionSetValue(0);
            context.Initialize(new[] { entity });
            
            var service = context.GetOrganizationService();

            // ACT
            var qe = new QueryExpression("repro_entity");
            qe.ColumnSet = new ColumnSet(true);
            // Using Enum value directly
            qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, ReproEntity_StateCode.Active);

            var results = service.RetrieveMultiple(qe);
            Assert.Single(results.Entities);
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class OptionSetMetadataAttribute : Attribute
    {
        public string Name { get; private set; }
        public int Index { get; set; }
        public int Lcid { get; set; }

        public OptionSetMetadataAttribute(string name, int index = 0, int lcid = 1033)
        {
            Name = name;
            Index = index;
            Lcid = lcid;
        }
    }

    [EntityLogicalName("repro_entity")]
    [DataContract(Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    public class ReproEntity : Entity
    {
        public ReproEntity() : base("repro_entity") { }

        [AttributeLogicalName("repro_entityid")]
        public override Guid Id { get => base.Id; set => base.Id = value; }

        [AttributeLogicalName("statecode")]
        public ReproEntity_StateCode? StateCode
        {
            get 
            {
                var val = this.GetAttributeValue<OptionSetValue>("statecode");
                if (val == null) return null;
                return (ReproEntity_StateCode)val.Value;
            }
            set 
            {
                if (value.HasValue)
                    this.SetAttributeValue("statecode", new OptionSetValue((int)value.Value));
                else
                    this.SetAttributeValue("statecode", null);
            }
        }
    }

    [DataContract]
    public enum ReproEntity_StateCode
    {
        [EnumMember]
        [OptionSetMetadata("Active", Index = 0, Lcid = 1033)]
        Active = 0,
        
        [EnumMember]
        [OptionSetMetadata("Inactive", Index = 1, Lcid = 1033)]
        Inactive = 1
    }
}
