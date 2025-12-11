using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;

#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    /// <summary>
    /// Fake Message executor for Upsert requests
    /// </summary>
    public class UpsertRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// Returns true if this message executor can execute the specified request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is UpsertRequest;
        }


        /// <summary>
        /// Executes the current request with the given context
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var upsertRequest = (UpsertRequest)request;
            var service = ctx.GetOrganizationService();
            
            var target = upsertRequest.Target;
            var entityLogicalName = target.LogicalName;
            
            var entityId = ctx.GetRecordUniqueId(target.ToEntityReferenceWithKeyAttributes(), validate: false);
            
            bool exists = false;
            
            if (entityId != Guid.Empty)
            {
                if (ctx is XrmFakedContext concreteContext)
                {
                    exists = concreteContext.ContainsEntity(entityLogicalName, entityId);
                }
                else
                {
                    try
                    {
                        service.Retrieve(entityLogicalName, entityId, new Microsoft.Xrm.Sdk.Query.ColumnSet(false));
                        exists = true;
                    }
                    catch (System.ServiceModel.FaultException)
                    {
                        exists = false;
                    }
                }
            }
            
            bool recordCreated;
            if (exists)
            {
                if (target.Id == Guid.Empty)
                {
                    target.Id = entityId;
                }
                
                service.Update(target);
                recordCreated = false;
            }
            else
            {
                if (target.KeyAttributes.Count > 0)
                {
                    target.KeyAttributes.Clear();
                }
                entityId = service.Create(target);
                recordCreated = true;
            }

            var result = new UpsertResponse();
            result.Results.Add("RecordCreated", recordCreated);
            result.Results.Add("Target", new EntityReference(entityLogicalName, entityId));
            return result;
        }

        /// <summary>
        /// Gets request type that will execute this request
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(UpsertRequest);
        }
    }
}
#endif
