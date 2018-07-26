using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Training.Plugins
{
    public class ValidationPlugin : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity jobseeker = (Entity)context.InputParameters["Target"];

            // Creating Images
            Entity postJobseeker = (Entity)context.PostEntityImages["Image"];
            Entity preJobseeker = (Entity)context.PreEntityImages["Image"];

            // From Target
            String jobseekerId = jobseeker.GetAttributeValue<String>("xrm_name");
            int numberOfServiceWeeks = jobseeker.GetAttributeValue<int>("xrm_weeksemployed");

            // From Pre Image
            int currentWeeks = postJobseeker.GetAttributeValue<int>("xrm_weeksemployed");
            int previousWeeks = postJobseeker.GetAttributeValue<int>("xrm_previousweeksemployed");

            // From Post Image
            int numberOfIterations = postJobseeker.GetAttributeValue<int>("xrm_ppsiterations");

            // Creation
            if (postJobseeker.Contains("xrm_pps") && postJobseeker.GetAttributeValue<bool>("xrm_pps") && previousWeeks < currentWeeks)
            {
                for (int i = 0; i < numberOfServiceWeeks; i++)
                {
                    try
                    {  // If record exists, it will do nothing
                        getTheRecord(service, jobseeker, i, numberOfIterations);
                   
                    } // When the record doesn't exist
                    catch (ArgumentOutOfRangeException)
                    {
                        createEntity(service, jobseeker, jobseekerId, i, numberOfIterations);
                    }

                }
            }
            // Deletion
            else if (currentWeeks < previousWeeks)
            {
                jobseeker["xrm_contracttype"] = "Minutes";
                service.Update(jobseeker);
                Entity nonActualizedRecords = getTheNonActualizedRecords(service, jobseeker, numberOfIterations);
                service.Update(nonActualizedRecords);
            }
        }

        // Filter for record (generic)
        private static Entity getTheRecord(IOrganizationService service, Entity jobseeker, int week, int numberOfIterations)
        {
            // Snippet for querying records
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_postplacementsupport";
            query.ColumnSet = new ColumnSet("xrm_weekplugin", "xrm_jobseeker", "xrm_ppsiterations");

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_weekplugin", ConditionOperator.Equal, week)
                );
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_jobseeker", ConditionOperator.Equal, jobseeker.Id)
                );
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_ppsiterations", ConditionOperator.Equal, numberOfIterations)
                );
        
            EntityCollection entities = service.RetrieveMultiple(query);

            return entities[0];
        }

        // Filter for non-actualized records
        private static Entity getTheNonActualizedRecords(IOrganizationService service, Entity jobseeker, int numberOfIterations)
        {
            // Snippet for querying records
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_postplacementsupport";
            query.ColumnSet = new ColumnSet("xrm_weekplugin", "xrm_jobseeker", "xrm_ppsiterations", "xrm_actualized");

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_jobseeker", ConditionOperator.Equal, jobseeker.Id)
                );
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_ppsiterations", ConditionOperator.Equal, numberOfIterations)
                );
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_actualized", ConditionOperator.Equal, false)
                );

            EntityCollection entities = service.RetrieveMultiple(query);

            return entities[0];
        }

        private void createEntity(IOrganizationService service, Entity jobseeker, String jobseekerId, int week, int numberOfIterations)
        {
            Entity PPSWeek = new Entity("xrm_postplacementsupport");
            // Creating & connecting the lookup
            EntityReference jobseekerValue = new EntityReference("xrm_applicant", jobseeker.Id);
            PPSWeek["xrm_jobseeker"] = jobseekerValue;
            PPSWeek["xrm_name"] = "PPS WEEK " + (week + 1);
            PPSWeek["xrm_weekplugin"] = week;
            PPSWeek["xrm_weekactual"] = (week + 1);
            PPSWeek["xrm_ppsiterations"] = numberOfIterations;
            PPSWeek["xrm_iterationstatus"] = false;
            PPSWeek["xrm_date"] = DateTime.Now; 
            // Linking the PPSTable
            EntityReference ppstableValue = new EntityReference("xrm_ppstable", new Guid("7876a1fe-b188-e811-a964-000d3ad1c715"));
            PPSWeek["xrm_ppstable"] = ppstableValue;

            service.Create(PPSWeek);
        }
    }
}