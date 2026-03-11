using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Reflection;
using Xunit;
using FakeXrmEasy.Query;
using FakeXrmEasy.Core.Tests.FakeContextTests;

namespace FakeXrmEasy.Core.Tests.Query.FetchXml
{
    public class FetchXmlHierarchyOperatorTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void FetchXml_Operator_Under_Translation()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='accountid' operator='under' value='{B05EC79D-93D3-4248-B5BB-ED6D3A0A093E}' />
                                    </filter>
                              </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("accountid", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Under, query.Criteria.Conditions[0].Operator);
            Assert.Equal(new Guid("B05EC79D-93D3-4248-B5BB-ED6D3A0A093E"), query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void FetchXml_Operator_Eq_Or_Under_Translation()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='accountid' operator='eq-or-under' value='{B05EC79D-93D3-4248-B5BB-ED6D3A0A093E}' />
                                    </filter>
                              </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("accountid", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.UnderOrEqual, query.Criteria.Conditions[0].Operator);
            Assert.Equal(new Guid("B05EC79D-93D3-4248-B5BB-ED6D3A0A093E"), query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void FetchXml_Operator_Not_Under_Translation()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='accountid' operator='not-under' value='{B05EC79D-93D3-4248-B5BB-ED6D3A0A093E}' />
                                    </filter>
                              </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("accountid", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.NotUnder, query.Criteria.Conditions[0].Operator);
            Assert.Equal(new Guid("B05EC79D-93D3-4248-B5BB-ED6D3A0A093E"), query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void FetchXml_Operator_Above_Translation()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='accountid' operator='above' value='{B05EC79D-93D3-4248-B5BB-ED6D3A0A093E}' />
                                    </filter>
                              </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("accountid", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Above, query.Criteria.Conditions[0].Operator);
            Assert.Equal(new Guid("B05EC79D-93D3-4248-B5BB-ED6D3A0A093E"), query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void FetchXml_Operator_Eq_Or_Above_Translation()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='account'>
                                    <attribute name='name' />
                                    <filter type='and'>
                                        <condition attribute='accountid' operator='eq-or-above' value='{B05EC79D-93D3-4248-B5BB-ED6D3A0A093E}' />
                                    </filter>
                              </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("accountid", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.AboveOrEqual, query.Criteria.Conditions[0].Operator);
            Assert.Equal(new Guid("B05EC79D-93D3-4248-B5BB-ED6D3A0A093E"), query.Criteria.Conditions[0].Values[0]);
        }
    }
}
