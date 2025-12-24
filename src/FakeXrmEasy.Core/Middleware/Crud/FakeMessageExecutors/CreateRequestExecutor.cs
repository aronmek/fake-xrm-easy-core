using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    /// <summary>
    /// CreateRequest Executor
    /// </summary>
    public class CreateRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is CreateRequest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var createRequest = (CreateRequest)request;

            var target = createRequest.Target;
            
            // Clone the target to avoid modifying the caller's object instance.
            // This simulates the server-side behavior where the object is deserialized (new instance).
            // Only clone if target is not null (null will be handled by CreateEntity validation)
            Entity clonedTarget = target != null ? target.Clone(ctx) : target;
            
            // Update the request target to the clone so that:
            // 1. The clone is what gets processed by CreateEntity (and gets an Id)
            // 2. PostOperation plugins accessing the request Target will see the clone (with Id)
            if (clonedTarget != null)
            {
                createRequest.Target = clonedTarget;
            }

            var guid = ctx.CreateEntity(clonedTarget);

            // Ensure the cloned target has the ID populated, so PostOperation plugins can see it
            if (clonedTarget != null && clonedTarget.Id == Guid.Empty)
            {
                clonedTarget.Id = guid;
            }

            return new CreateResponse()
            {
                ResponseName = "Create",
                Results = new ParameterCollection { { "id", guid } }
            };
        }

        /// <summary>
        /// Returns CreateRequest
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(CreateRequest);
        }
    }
}