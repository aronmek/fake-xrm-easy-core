using System;
using Xunit;
using Microsoft.Xrm.Sdk;
using System.Linq;
using Crm;
using FakeXrmEasy.Core.Exceptions;

namespace FakeXrmEasy.Core.Tests.FakeContextTests
{
    public class AmbiguousProxyTypeReproductionTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_multiple_proxy_types_are_enabled_and_entity_is_ambiguous_it_should_still_be_possible_to_resolve_metadata()
        {
            _context.EnableProxyTypes(typeof(Crm.Account).Assembly);
            _context.EnableProxyTypes(typeof(DataverseEntities.Account).Assembly);

            // This should trigger EnsureEntityNameExistsInMetadata via QueryExpressionExtensions
            var qe = new Microsoft.Xrm.Sdk.Query.QueryExpression("account");
            
            var service = _context.GetOrganizationService();
            service.RetrieveMultiple(qe);
        }
    }
}
