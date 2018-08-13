using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;

namespace ATeam.ScoreRankPlugin
{
    public class ScoreRank : IPlugin
    {
        private static EntityCollection getScoreboards(IOrganizationService service)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_scoreboard";
            query.ColumnSet = new ColumnSet() { AllColumns = true };

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
            (
                new ConditionExpression("xrm_score", ConditionOperator.NotNull)
            );
            query.Criteria.Conditions.Add
            (
                new ConditionExpression("xrm_score", ConditionOperator.NotEqual, 0)
            );
            OrderExpression order = new OrderExpression();
            order.AttributeName = "xrm_score";
            order.OrderType = OrderType.Descending;
            query.Orders.Add(order);
            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = 50; 
            query.PageInfo.PageNumber = 1;

            EntityCollection scoreboards = service.RetrieveMultiple(query);
            

            return scoreboards;
        }

        public void Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity jobseeker = (Entity)context.InputParameters["Target"];

            EntityCollection allScoreboards = getScoreboards(service);

            for (int i = 0; i < allScoreboards.Entities.Count; i++)
            {
                Entity currentScoreboard = allScoreboards[i];
                currentScoreboard["xrm_rank"] = i + 1;
                service.Update(currentScoreboard);
            }



        }

        //protected override void Execute(CodeActivityContext executionContext)
        //{
        //    //Create the context
        //    IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
        //    IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
        //    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);



        //    EntityCollection allScoreboards = getScoreboards(service);

        //    for (int i = 0; i < allScoreboards.Entities.Count; i++)
        //    {
        //        Entity currentScoreboard = allScoreboards[i];
        //        currentScoreboard["xrm_rank"] = i + 1;
        //        service.Update(currentScoreboard);
        //    }
        //}
    }
}
