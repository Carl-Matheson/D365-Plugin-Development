using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Training.Plugins
{
    /* Occurs onCreate of a PPS Week Record */
    public class PPSWeek : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity PPSWeek = (Entity)context.InputParameters["Target"];
            Guid tableId = ((EntityReference)PPSWeek.Attributes["xrm_ppstable"]).Id;
            // Grabs the PPSTable with all columns

            // Variable definitions
            Entity PPSTable = service.Retrieve("xrm_ppstable", tableId, new ColumnSet(true));
            int currentWeek = PPSWeek.GetAttributeValue<int>("xrm_weekactual");
            int iteration = PPSWeek.GetAttributeValue<int>("xrm_ppsiterations");
            Guid jobseekerId = ((EntityReference)PPSWeek.Attributes["xrm_jobseeker"]).Id;
            // Old, but some code uses the 'complete' values
            Entity jobseeker = service.Retrieve("xrm_applicant", jobseekerId, new ColumnSet(new string[] { "xrm_4weekcomplete", "xrm_13weekcomplete", "xrm_26weekcomplete", "xrm_52weekcomplete" })); 

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

            if (currentWeek < firstPeriod)
            {
                PPSWeek["xrm_reportableamount"] = firstBreakdown;
                PPSWeek["xrm_actualamount"] = new Money(0m);
            }

            else if (currentWeek == firstPeriod)
            {
                PPSWeek["xrm_reportableamount"] = firstBreakdown;
                PPSWeek["xrm_actualamount"] = firstAmount;
                PPSWeek["xrm_actualized"] = true;

                for (int i = 1; i < 4; i++)
                {
                    Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseekerId);
                    fetchedJobseeker["xrm_actualized"] = true;
                    service.Update(fetchedJobseeker);

                    jobseeker["xrm_4weekcomplete"] = true;
                    service.Update(jobseeker);
                }
            }
        
           else if (currentWeek < secondPeriod)
            {
                PPSWeek["xrm_reportableamount"] = secondBreakdown;
                PPSWeek["xrm_actualamount"] = new Money(0m);
            }

            else if (currentWeek == secondPeriod)
            {
                PPSWeek["xrm_reportableamount"] = secondBreakdown;
                PPSWeek["xrm_actualamount"] = secondAmount;
                PPSWeek["xrm_actualized"] = true;

                for (int i = 1; i < 13; i++)
                {
                    Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseekerId);
                    fetchedJobseeker["xrm_actualized"] = true;
                    service.Update(fetchedJobseeker);

                    jobseeker["xrm_13weekcomplete"] = true;
                    service.Update(jobseeker);
                }
            }
            else if (currentWeek < thirdPeriod)
            {
                PPSWeek["xrm_reportableamount"] = thirdBreakdown;
                PPSWeek["xrm_actualamount"] = new Money(0m);
            }
            else if (currentWeek == thirdPeriod)
            {
                PPSWeek["xrm_reportableamount"] = thirdBreakdown;
                PPSWeek["xrm_actualamount"] = thirdAmount;
                PPSWeek["xrm_actualized"] = true;

                for (int i = 1; i < 26; i++)
                {
                    Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseekerId);
                    fetchedJobseeker["xrm_actualized"] = true;
                    service.Update(fetchedJobseeker);

                    jobseeker["xrm_26weekcomplete"] = true;
                    service.Update(jobseeker);
                }
            }
            else if (currentWeek < fourthPeriod)
            {
                PPSWeek["xrm_reportableamount"] = fourthBreakdown;
                PPSWeek["xrm_actualamount"] = new Money(0m);
            }
            else if (currentWeek == fourthPeriod)
            {
                PPSWeek["xrm_reportableamount"] = fourthBreakdown;
                PPSWeek["xrm_actualamount"] = fourthAmount;
                PPSWeek["xrm_actualized"] = true;

                for (int i = 1; i < 52; i++)
                {
                    Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseekerId);
                    fetchedJobseeker["xrm_actualized"] = true;
                    service.Update(fetchedJobseeker);

                    jobseeker["xrm_52weekcomplete"] = true;
                    service.Update(jobseeker);
                }
            }
        }

        private static Entity getTheRecord(IOrganizationService service, int week, int numberOfIterations, Guid jobseeker)
        {
            // Snippet for querying records
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_postplacementsupport";
            query.ColumnSet = new ColumnSet("xrm_weekplugin", "xrm_ppsiterations", "xrm_jobseeker");

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_weekactual", ConditionOperator.Equal, week)
                );
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_ppsiterations", ConditionOperator.Equal, numberOfIterations)
                );
            query.Criteria.Conditions.Add
                (
                new ConditionExpression("xrm_jobseeker", ConditionOperator.Equal, jobseeker)
                );

            EntityCollection entities = service.RetrieveMultiple(query);

            return entities[0];
        }

    }
}