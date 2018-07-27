using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Training.Plugins
{
    public class UpdateJobseekerForm : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity jobseeker = (Entity)context.InputParameters["Target"];
            Entity preJobseeker = (Entity)context.PreEntityImages["Image2"];
            int numberOfWeeksEmployed = preJobseeker.GetAttributeValue<int>("xrm_weeksemployed");

            jobseeker["xrm_previousweeksemployed"] = numberOfWeeksEmployed; //.ToString();
        }

    }


}