using System.IO;

namespace AzureFluentException
{
    public class Resources
    {
        public static string GetAzureResponse()
        {
            const string resourceName = "AzureFluentException.AzureResponse.json";
            var asm = typeof(Resources).Assembly;
            using (var stream = asm.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
