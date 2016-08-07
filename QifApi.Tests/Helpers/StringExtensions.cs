using System.IO;
using System.Text;

namespace QifApi.Tests.Helpers
{
    public static class StringExtensions
    {
        public static Stream ToUTF8MemoryStream(this string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source ?? ""));
        }
    }
}