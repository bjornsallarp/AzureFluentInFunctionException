using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace AzureFluentException
{
    public static class Function
    {
        [FunctionName("Function")]
        public static async Task Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)]TimerInfo myTimer, TraceWriter log)
        {
            // TransformationJsonConverter is the failing component. See issue where bug was solved:
            // https://github.com/Azure/azure-sdk-for-net/issues/2501
            // 
            // But Azure runtime pulls in a different package than refenced by Microsoft.Azure.Management.Fluent
            // and also 'Microsoft.Rest.ClientRuntime'-package explicitly referenced by this project. Appears to
            // be an existing issue, see: https://github.com/Azure/azure-functions-host/issues/1743
            var microsoftRestClientRuntime = typeof(TransformationJsonConverter).Assembly.Location;
            log.Info($"Using dll: {microsoftRestClientRuntime}");
            log.Info($"Version: {System.Diagnostics.FileVersionInfo.GetVersionInfo(microsoftRestClientRuntime).FileVersion}");

            // Deserialize with latest TransformationJsonConverter from github
            Deserialize(WorkingDeserializationSettings(), log);

            // Deserialize the way it's done in fluent API
            Deserialize(FluentPackaeDeserializationSettings(), log);
        }

        private static void Deserialize(JsonSerializerSettings settings, TraceWriter log)
        {
            try
            {
                var response = Resources.GetAzureResponse();
                JsonConvert.DeserializeObject<Page<SiteInner>>(response, settings);
                log.Info("Deserialization works!");
            }
            catch (Exception ex)
            {
                log.Error("Deserialization failed", ex);
                throw;
            }
        }

        /// <summary>
        /// This is code from WebSiteManagementClient in fluent package
        /// </summary>
        /// <returns></returns>
        private static JsonSerializerSettings FluentPackaeDeserializationSettings()
        {
            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter()
                }
            };
            settings.Converters.Add(new TransformationJsonConverter());
            settings.Converters.Add(new CloudErrorJsonConverter());

            return settings;
        }

        /// <summary>
        /// This is code from WebSiteManagementClient in fluent package but instead uses
        /// the TransformationJsonConverter from latest azure-sdk-for-net repo.
        /// https://github.com/Azure/azure-sdk-for-net/blob/3f736b5af3851ab99bbaa98483ae537de0d48cfb/src/SdkCommon/ClientRuntime/ClientRuntime/Serialization/TransformationJsonConverter.cs
        /// </summary>
        /// <returns></returns>
        private static JsonSerializerSettings WorkingDeserializationSettings()
        {
            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter()
                }
            };

            // Use code copied from https://github.com/Azure/azure-sdk-for-net/blob/3f736b5af3851ab99bbaa98483ae537de0d48cfb/src/SdkCommon/ClientRuntime/ClientRuntime/Serialization/TransformationJsonConverter.cs
            settings.Converters.Add(new TransformationJsonConverter2());
            settings.Converters.Add(new CloudErrorJsonConverter());

            return settings;
        }
    }
}
