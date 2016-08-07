using System;
using System.Globalization;
using System.Threading;

namespace QifApi.Helpers
{
    public class SpoofCulture : IDisposable
    {
        private readonly CultureInfo _previousCultureInfo;
        private readonly Thread _threadToSpoof;

        public SpoofCulture(string nameOfCultureToUse, Thread threadToSpoof = null)
            : this(new CultureInfo(nameOfCultureToUse), threadToSpoof) { }

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