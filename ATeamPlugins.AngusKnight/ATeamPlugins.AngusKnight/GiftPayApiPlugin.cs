using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace ATeamPlugins.AngusKnight
{
    public class GiftPayApiPlugin : IPlugin
    {

        public string SendGiftByEmail(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {

            

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            String ApiKey = "8199B41C-AFD6-480C-A702-D4F01434A403";

            String sendGiftRequest = "https://sandbox.express.giftpay.com/api/gift.svc/send?key=";
            Entity rewardClaim = (Entity)context.InputParameters["Target"];

            

            EntityReference jobseekerReference = rewardClaim.GetAttributeValue<EntityReference>("xrm_jobseekercopy");

            Entity jobseeker = service.Retrieve(jobseekerReference.LogicalName, jobseekerReference.Id, new ColumnSet(true));

            String recipientEmail = jobseeker.GetAttributeValue<String>("xrm_emailaddressurl");

            EntityReference rewardReference = rewardClaim.GetAttributeValue<EntityReference>("xrm_rewardcopy");

            Entity reward = service.Retrieve(rewardReference.LogicalName, rewardReference.Id, new ColumnSet(true));

            String rewardValue = reward.GetAttributeValue<String>("xrm_realdollarvalue");

            sendGiftRequest = sendGiftRequest + ApiKey + "&to=" + recipientEmail + "&value=" + rewardValue + "&clientref=" + rewardClaim.Id.ToString(); 

            String result = rewardClaim.GetAttributeValue<String>("xrm_resulttext");



            if (result == "Purchased")
            {
                rewardClaim["xrm_httpresponse"] = SendGiftByEmail(sendGiftRequest);
            }





        }
    }
}

//execute on update of result field in reward claim

//if result equals approved
//    get request to send appropriate gift card to jobseekers email address

