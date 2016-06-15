using System.IO;
using System.Reflection;

namespace QifApi.Tests.Helpers
{
    public static class ResourceHelpers
    {
        public static string ExtractResourceToString(string resourceName)
        {
            var asm = Assembly.GetCallingAssembly();
            var resourceInfo = asm.GetManifestResourceInfo(resourceName);
            if (resourceInfo == null)
            {
                throw new FileNotFoundException(string.Format("Cannot find manifest resource: '{0}'", resourceName));
            }

            using (var reader = new StreamReader(asm.GetManifestResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}