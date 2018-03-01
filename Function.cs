using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFluentException
{
    public static class Function
    {
        private static string tenantId = "";
        private static string clientId = "";
        private static string clientSecret = "";

        [FunctionName("Function")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *", RunOnStartup = true)]TimerInfo myTimer, 
            TraceWriter log)
        {
            var credentials = GetCredentials();
            var azure = Azure.Authenticate(credentials).WithDefaultSubscription();

            try
            {
                // This throws error
                var apps = await azure.WebApps.ListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static AzureCredentials GetCredentials()
        {
            var env = AzureEnvironment.AzureGlobalCloud;
            var principalLogin = new ServicePrincipalLoginInformation
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            return new AzureCredentials(principalLogin, tenantId, env);
        }
    }
}
