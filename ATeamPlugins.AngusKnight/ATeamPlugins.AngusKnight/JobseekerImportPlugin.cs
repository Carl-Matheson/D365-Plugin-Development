using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace ATeamPlugins.AngusKnight
{
    public class JobseekerImportPlugin : IPlugin
    {

        private static Entity getTheRecord(IOrganizationService service, String jobseekerIdText)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_applicant";
            query.ColumnSet = new ColumnSet() { AllColumns = true };

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
            (
                new ConditionExpression("xrm_name", ConditionOperator.Equal, jobseekerIdText)
            );
            EntityCollection entities = service.RetrieveMultiple(query);
            
            return entities[0];
            
            
        }

        private void createEntity(Entity jobseekerImport, IOrganizationService service, String jobseekerIdText)
        {
            Entity jobseeker = new Entity("xrm_applicant");
            jobseeker["xrm_name"] = jobseekerIdText;
            service.Create(jobseeker);

            
        }

        public void Execute(IServiceProvider serviceProvider)
        {

            

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity jobseekerImport = (Entity)context.InputParameters["Target"];

            String jobseekerIdText = jobseekerImport.GetAttributeValue<String>("xrm_jobseeker_id_text");

            try { getTheRecord(service, jobseekerIdText); }

            catch (ArgumentOutOfRangeException)
            {
                createEntity(jobseekerImport, service, jobseekerIdText);
            }

            EntityReference jobseekerId = new EntityReference("xrm_applicant", getTheRecord(service, jobseekerIdText).Id);

            jobseekerImport["xrm_jobseeker_id"] = jobseekerId;


        }
    }
}

