using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ATeam.ScoreRankPlugin
{
    public class ScoreRank : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            EntityCollection allScoreboards = getScoreboards(service);

            for (int i = 0; i < allScoreboards.Entities.Count; i++)
            {
                Entity currentScoreboard = allScoreboards[i];
                currentScoreboard["xrm_rank"] = i + 1;
                service.Update(currentScoreboard);
            }
        }

        private static EntityCollection getScoreboards(IOrganizationService service)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_scoreboard";
            query.ColumnSet = new ColumnSet("xrm_rank","xrm_score");

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
    }
}
