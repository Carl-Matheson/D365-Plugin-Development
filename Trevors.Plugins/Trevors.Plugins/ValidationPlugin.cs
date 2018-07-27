using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Training.Plugins
{
    /* Occurs onUpdate of Current Week field */

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

            //PPS Table Reference
            EntityReference PPSTableValue = new EntityReference("xrm_ppstable", new Guid("7876a1fe-b188-e811-a964-000d3ad1c715"));
            Entity PPSTable = service.Retrieve("xrm_ppstable", new Guid("7876a1fe-b188-e811-a964-000d3ad1c715"), new ColumnSet(true));

            // 4 Week
            int firstPeriod = PPSTable.GetAttributeValue<int>("xrm_outcomeperiod1");
            Money firstBreakdown = PPSTable.GetAttributeValue<Money>("xrm_4wkbreakdown");
            Money firstAmount = PPSTable.GetAttributeValue<Money>("xrm_4wkoutcome");

            // 13 Week
            int secondPeriod = PPSTable.GetAttributeValue<int>("xrm_outcomeperiod2");
            Money secondBreakdown = PPSTable.GetAttributeValue<Money>("xrm_13wkbreakdown");
            Money secondAmount = PPSTable.GetAttributeValue<Money>("xrm_13wkoutcome");
           
            // 26 Week
            int thirdPeriod = PPSTable.GetAttributeValue<int>("xrm_outcomeperiod3");
            Money thirdBreakdown = PPSTable.GetAttributeValue<Money>("xrm_26wkbreakdown");
            Money thirdAmount = PPSTable.GetAttributeValue<Money>("xrm_26wkoutcome");
            
            // 52 Week
            int fourthPeriod = PPSTable.GetAttributeValue<int>("xrm_outcomeperiod4");
            Money fourthBreakdown = PPSTable.GetAttributeValue<Money>("xrm_52wkbreakdown");
            Money fourthAmount = PPSTable.GetAttributeValue<Money>("xrm_52wkoutcome");

            jobseeker["xrm_contracttype"] = previousWeeks.ToString();

            // Creation of PPS Weeks records
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
            // Deletion, JS has exited PPS - calculate profit-loss
            else if (currentWeeks < previousWeeks)
            {
                if(previousWeeks > 0 && previousWeeks < firstPeriod) // Exit in week 1-3
                {

                    decimal loss = previousWeeks * firstBreakdown.Value;
                    Money economicLoss = new Money(loss);
                    try
                    {
                        Entity PPSWeek = getTheRecord(service, jobseeker, previousWeeks, numberOfIterations);
                        PPSWeek["xrm_economiclossimmediate"] = economicLoss;
                        service.Update(PPSWeek);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new InvalidPluginExecutionException("Out of range exception");
                    }
                }
                else if(previousWeeks > firstPeriod && previousWeeks < secondPeriod) // Exit in week 5 - 12
                {
                    decimal loss = (previousWeeks - firstPeriod) * secondBreakdown.Value;
                    Money economicLoss = new Money(loss);
                    try
                    {
                        Entity PPSWeek = getTheRecord(service, jobseeker, previousWeeks, numberOfIterations);
                        PPSWeek["xrm_economiclossimmediate"] = economicLoss;
                        service.Update(PPSWeek);
                    }
                    catch (ArgumentOutOfRangeException)
                    {

                    }
                }
                else if (previousWeeks > secondPeriod && previousWeeks < thirdPeriod) // Exit in week 13 - 25
                {
                    decimal loss = (previousWeeks - secondPeriod) * thirdBreakdown.Value;
                    Money economicLoss = new Money(loss);
                    Entity PPSWeek = getTheRecord(service, jobseeker, previousWeeks, numberOfIterations);
                    PPSWeek["xrm_economiclossimmediate"] = economicLoss;
                    service.Update(PPSWeek);
                }
                else if(previousWeeks > thirdPeriod && previousWeeks < fourthPeriod) // Exit in week 27 - 51
                {
                    decimal loss = (previousWeeks - thirdPeriod) * fourthBreakdown.Value;
                    Money economicLoss = new Money(loss);
                    Entity PPSWeek = getTheRecord(service, jobseeker, previousWeeks, numberOfIterations);
                    PPSWeek["xrm_economiclossimmediate"] = economicLoss;
                    service.Update(PPSWeek);
                }
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
            PPSWeek["xrm_iterationstatus"] = true;
            PPSWeek["xrm_date"] = DateTime.Now;
            PPSWeek["xrm_actualhours"] = 0m; 
            // Linking the PPSTable
            EntityReference ppstableValue = new EntityReference("xrm_ppstable", new Guid("7876a1fe-b188-e811-a964-000d3ad1c715"));
            PPSWeek["xrm_ppstable"] = ppstableValue;

            service.Create(PPSWeek);
        }
    }
}