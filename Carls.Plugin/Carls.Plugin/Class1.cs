using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Training.Plugins
{
    public class ValidationPlugin : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)
            serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            //on create chat post, get user-id(guid) to populate lookup of jobseeker/consultant

            Entity chatPost = (Entity)context.InputParameters["Target"]; //chatPost Entity


            EntityReference chatUserReference = chatPost.GetAttributeValue<EntityReference>("resco_chatuserid");
            Entity chatUser = service.Retrieve(chatUserReference.LogicalName, chatUserReference.Id, new ColumnSet(true));

            if (chatUser.GetAttributeValue<String>("resco_userentity").Equals("xrm_applicant"))
            {
                chatPost["xrm_jobseeker"] = new EntityReference("xrm_applicant", new Guid(chatUser.GetAttributeValue<String>("resco_userid")));
                EntityReference consultant = new EntityReference("systemuser", getConsultant(service, chatPost.GetAttributeValue<String>("resco_name")).Id);
                chatPost["xrm_consultant"] = consultant;
            }
            else if (chatUser.GetAttributeValue<String>("resco_userentity").Equals("systemuser"))
            {
                chatPost["xrm_consultant"] = new EntityReference("systemuser", new Guid(chatUser.GetAttributeValue<String>("resco_userid")));
                EntityReference jobseeker = new EntityReference("xrm_applicant", getJobseeker(service, chatPost.GetAttributeValue<String>("resco_name")).Id);
                chatPost["xrm_jobseeker"] = jobseeker;
            }


            try
            {
                getChatlog(service, chatPost.GetAttributeValue<EntityReference>("xrm_consultant"), chatPost.GetAttributeValue<EntityReference>("xrm_jobseeker"));
                chatPost["xrm_chat"] = new EntityReference
                    ("xrm_chatlog", getChatlog(service, chatPost.GetAttributeValue<EntityReference>("xrm_consultant"), chatPost.GetAttributeValue<EntityReference>("xrm_jobseeker")).Id);
            }

            catch (ArgumentOutOfRangeException)
            {
                createChatlog(service, chatPost.GetAttributeValue<EntityReference>("xrm_consultant"), chatPost.GetAttributeValue<EntityReference>("xrm_jobseeker"));
                chatPost["xrm_chat"] = new EntityReference
                    ("xrm_chatlog", getChatlog(service, chatPost.GetAttributeValue<EntityReference>("xrm_consultant"), chatPost.GetAttributeValue<EntityReference>("xrm_jobseeker")).Id);
            }


        }

        private void createChatlog(IOrganizationService service, EntityReference consultant, EntityReference jobseeker)
        {
            Entity chatlog = new Entity("xrm_chatlog");
            chatlog["xrm_consultant"] = consultant;
            chatlog["xrm_jobseeker"] = jobseeker;
            service.Create(chatlog);


        }

        private static Entity getChatlog(IOrganizationService service, EntityReference consultant, EntityReference jobseeker)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_chatlog";
            query.ColumnSet = new ColumnSet("xrm_consultant", "xrm_jobseeker");

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
            (
                new ConditionExpression("xrm_consultant", ConditionOperator.Equal, consultant)
            );
            query.Criteria.Conditions.Add
            (
                new ConditionExpression("xrm_jobseeker", ConditionOperator.Equal, jobseeker)
            );   
            EntityCollection entities = service.RetrieveMultiple(query);

            return entities[0];
        }

        private static Entity getJobseeker(IOrganizationService service, String jobseekerIdText)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = "xrm_applicant";
            query.ColumnSet = new ColumnSet("xrm_name");

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
            (
                new ConditionExpression("xrm_name", ConditionOperator.Equal, jobseekerIdText)
            );
            EntityCollection entities = service.RetrieveMultiple(query);

            return entities[0];
        }

        private static Entity getConsultant(IOrganizationService service, String consultantName)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = "systemuser";
            query.ColumnSet = new ColumnSet("fullname");

            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.Conditions.Add
            (
                new ConditionExpression("fullname", ConditionOperator.Equal, consultantName)
            );
            EntityCollection entities = service.RetrieveMultiple(query);

            return entities[0];
        }
    }
}