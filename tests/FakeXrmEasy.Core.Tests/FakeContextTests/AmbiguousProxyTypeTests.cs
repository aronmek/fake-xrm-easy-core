using System;
using Xunit;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using System.Linq;
using Crm;
using FakeXrmEasy.Core.Exceptions;

namespace FakeXrmEasy.Core.Tests.FakeContextTests
{
    public class AmbiguousProxyTypeTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_multiple_proxy_types_are_enabled_FindReflectedType_should_return_null_by_default()
        {
            _context.EnableProxyTypes(typeof(Crm.Account).Assembly);
            _context.EnableProxyTypes(typeof(DataverseEntities.Account).Assembly);

            Assert.Null(_context.FindReflectedType("account"));
        }

        [Fact]
        public void When_multiple_proxy_types_are_enabled_CreateQuery_with_generic_type_should_succeed_even_if_ambiguous()
        {
            _context.EnableProxyTypes(typeof(Crm.Account).Assembly);
            _context.EnableProxyTypes(typeof(DataverseEntities.Account).Assembly);

            // This should now succeed because CreateQuery uses the generic type directly
            var query = _context.CreateQuery<Crm.Account>();
            Assert.NotNull(query);
        }
    }
}
