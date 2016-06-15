using System;
using System.Globalization;
using System.Threading;

namespace QifApi.Tests.Helpers
{
    public class SpoofCulture : IDisposable
    {
        private readonly CultureInfo _previousCultureInfo;
        private readonly Thread _threadToSpoof;

        public SpoofCulture(CultureInfo cultureToUse, Thread threadToSpoof = null)
        {
            _threadToSpoof = threadToSpoof ?? Thread.CurrentThread;

            _previousCultureInfo = CultureInfo.CurrentCulture;

            _threadToSpoof.CurrentCulture = cultureToUse;
        }

        public void Dispose()
        {
            _threadToSpoof.CurrentCulture = _previousCultureInfo;
        }
    }
}