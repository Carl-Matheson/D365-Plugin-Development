using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Training.Plugins
{
    public class PPSWeek : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity PPSWeek = (Entity)context.InputParameters["Target"];
            EntityReference PPSTableValue = new EntityReference("xrm_ppstable", new Guid("7876a1fe-b188-e811-a964-000d3ad1c715"));
            // Grabs the PPSTable with all columns
            Entity PPSTable = service.Retrieve("xrm_ppstable", new Guid("7876a1fe-b188-e811-a964-000d3ad1c715"), new ColumnSet(true));

            // Variable definitions
            int currentWeek = PPSWeek.GetAttributeValue<int>("xrm_weekactual");
            int iteration = PPSWeek.GetAttributeValue<int>("xrm_ppsiterations");
            Guid jobseeker = ((EntityReference)PPSWeek.Attributes["xrm_jobseeker"]).Id;
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
                        Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseeker);
                        fetchedJobseeker["xrm_actualized"] = true;
                        service.Update(fetchedJobseeker);                    
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
                    Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseeker);
                    fetchedJobseeker["xrm_actualized"] = true;
                    service.Update(fetchedJobseeker);
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
                    Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseeker);
                    fetchedJobseeker["xrm_actualized"] = true;
                    service.Update(fetchedJobseeker);
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
                    Entity fetchedJobseeker = getTheRecord(service, i, iteration, jobseeker);
                    fetchedJobseeker["xrm_actualized"] = true;
                    service.Update(fetchedJobseeker);
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