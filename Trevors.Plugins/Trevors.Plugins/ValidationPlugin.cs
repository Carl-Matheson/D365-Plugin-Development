using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Training.Plugins
{
    /* Occurs onUpdate of weeks employed */

    public class ValidationPlugin : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity jobseeker = (Entity)context.InputParameters["Target"];
            Entity preJobseeker = (Entity)context.PreEntityImages["Image"];

            // From Target
            String jobseekerId = jobseeker.GetAttributeValue<String>("xrm_name");
            int currentWeeks = jobseeker.GetAttributeValue<int>("xrm_weeksemployed");

            // From Pre Image
            int previousWeeks = preJobseeker.GetAttributeValue<int>("xrm_weeksemployed");
            jobseeker["xrm_previousweeksemployed"] = previousWeeks;
            int numberOfIterations = preJobseeker.GetAttributeValue<int>("xrm_ppsiterations");
            Guid tableId = ((EntityReference) preJobseeker.Attributes["xrm_fundinglevel"]).Id;
            decimal employmentBenchmark = preJobseeker.GetAttributeValue<decimal>("xrm_employmentbenchmarkdecimal");
            DateTime anchorDate = preJobseeker.GetAttributeValue<DateTime>("xrm_anchor_date");


            // Creation of PPS Weeks records
            if (preJobseeker.Contains("xrm_pps") && preJobseeker.GetAttributeValue<bool>("xrm_pps") && previousWeeks < currentWeeks)
            {
                for (int i = 1; i <= currentWeeks; i++)
                {
                    try
                    {  // If record exists, it will do nothing
                        getTheRecord(service, jobseeker, i, numberOfIterations);

                    } // When the record doesn't exist
                    catch (ArgumentOutOfRangeException)
                    {
                        createEntity(service, jobseeker, jobseekerId, i, numberOfIterations, tableId, employmentBenchmark, anchorDate);
                    }

                }
            }

            // Deletion, JS has exited PPS - calculate profit-loss
            else if (currentWeeks < previousWeeks)
            {
                jobseeker["xrm_4weekactionedlookup"] = null;
                jobseeker["xrm_4weekactioned"] = false;
                jobseeker["xrm_4weekcomment"] = null;

                jobseeker["xrm_13weekactionedlookup"] = null;
                jobseeker["xrm_13weekactioned"] = false;
                jobseeker["xrm_13weekcomment"] = null;

                jobseeker["xrm_26weekactionedlookup"] = null;
                jobseeker["xrm_26weekactioned"] = false;
                jobseeker["xrm_26weekcomment"] = null;

                jobseeker["xrm_52weekactionedlookup"] = null;
                jobseeker["xrm_52weekactioned"] = false;
                jobseeker["xrm_52weekcomment"] = null;

                try
                {
                    Entity PPSWeek = getTheRecord(service, jobseeker, previousWeeks, numberOfIterations);
                    // If statement to prevent the code from happening on the threshold weeks
       
                    // Intialize loss, turns into money later. 
                    decimal totalLoss = 0m;
                    for (int i = 0; i < getTheNonActualizedRecords(service, jobseeker, numberOfIterations).Entities.Count; i++)
                    {
                        Entity currentWeek = getTheNonActualizedRecords(service, jobseeker, numberOfIterations)[i];
                        decimal loss = currentWeek.GetAttributeValue<Money>("xrm_reportableamount").Value;
                        totalLoss = totalLoss + loss;
                        service.Update(currentWeek);

                    }
                    Money economicLoss = new Money(totalLoss);

                    PPSWeek["xrm_economiclossimmediate"] = economicLoss;
                    service.Update(PPSWeek);


                    // Creates static count
                    int count = getThecurrentIterationRecords(service, jobseeker, numberOfIterations).Entities.Count;
                    jobseeker["xrm_contracttype"] = count.ToString();
                    // Setting the current iteration to old
                    for (int i = 0; i < count; i++)
                    {
                        Entity currentWeek = getThecurrentIterationRecords(service, jobseeker, numberOfIterations)[0];
                        currentWeek["xrm_comments"] = count.ToString();
                        currentWeek["xrm_iterationstatus"] = false;
                        service.Update(currentWeek);
                    }
                }
                // Unknown error occurs; Mainly that the records don't exist. 
                catch (ArgumentOutOfRangeException)
                {

                }

            }
        }

        // Filter for record (generic)
        private static Entity getTheRecord(IOrganizationService service, Entity jobseeker, int week, int numberOfIterations)
        {
            // Snippet for querying records
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_postplacementsupport";
            query.ColumnSet = new ColumnSet("xrm_weekactual", "xrm_jobseeker", "xrm_ppsiterations", "xrm_actualamount");

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;

            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_weekactual", ConditionOperator.Equal, week)
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
        private static EntityCollection getTheNonActualizedRecords(IOrganizationService service, Entity jobseeker, int numberOfIterations)
        {
            // Snippet for querying records
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_postplacementsupport";
            query.ColumnSet = new ColumnSet("xrm_jobseeker", "xrm_ppsiterations", "xrm_actualized", "xrm_reportableamount", "xrm_actualamount");

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

            return entities;
        }

        // Filter for current iteration records
        private static EntityCollection getThecurrentIterationRecords(IOrganizationService service, Entity jobseeker, int numberOfIterations)
        {
            // Snippet for querying records
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_postplacementsupport";
            query.ColumnSet = new ColumnSet("xrm_jobseeker", "xrm_ppsiterations", "xrm_iterationstatus");

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
                new ConditionExpression("xrm_iterationstatus", ConditionOperator.Equal, true)
                );

            EntityCollection entities = service.RetrieveMultiple(query);

            return entities;
        }

        private void createEntity(IOrganizationService service, Entity jobseeker, String jobseekerId, int week, int numberOfIterations, Guid tableId, decimal employmentBenchmark, DateTime anchorDate)
        {
            Entity PPSWeek = new Entity("xrm_postplacementsupport");
            // Creating & connecting the lookup
            EntityReference jobseekerValue = new EntityReference("xrm_applicant", jobseeker.Id);
            PPSWeek["xrm_jobseeker"] = jobseekerValue;
            PPSWeek["xrm_name"] = jobseekerValue.Name + "PPS WEEK " + week;
            PPSWeek["xrm_weekactual"] = week;
            PPSWeek["xrm_ppsiterations"] = numberOfIterations;
            PPSWeek["xrm_iterationstatus"] = true;
            PPSWeek["xrm_date"] = DateTime.Now;
            PPSWeek["xrm_actualhours"] = 0m;
            PPSWeek["xrm_employmentbenchmark"] = employmentBenchmark;
            PPSWeek["xrm_economiclossimmediate"] = new Money(0m); 
            PPSWeek["xrm_anchordate"] = anchorDate;
            // Linking the PPSTable
            EntityReference ppstableValue = new EntityReference("xrm_ppstable", tableId);
            PPSWeek["xrm_ppstable"] = ppstableValue;

            if (week != 52) {
                PPSWeek["xrm_date"] = anchorDate.AddDays(week * 7);
            } else {
                PPSWeek["xrm_date"] = anchorDate.AddDays(365);
            }

            service.Create(PPSWeek);
        }
    }
}