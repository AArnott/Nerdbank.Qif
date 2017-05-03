using System;
using System.Globalization;
using System.Threading;

namespace QifApi
{
    /// <summary>
    /// A culture swapping context. When instantiated, the <see cref="CultureInfo.CurrentCulture"/>
    /// is saved, then replaced by the specified culture.
    /// </summary>
    public class CultureContext : IDisposable
    {
        private readonly CultureInfo _previousCulture;

        /// <summary>
        /// Creates a new instance of <see cref="CultureContext"/>
        /// </summary>
        /// <param name="nameOfCultureToUse">The language tag of the culture to use (e.g., en-US, es-MX, ar-SA, etc).</param>
        public CultureContext(string nameOfCultureToUse)
            : this(new CultureInfo(nameOfCultureToUse)) { }

        /// <summary>
        /// Creates a new instance of <see cref="CultureContext"/>
        /// </summary>
        /// <param name="cultureToUse">The <see cref="CultureInfo"/> to replace <see cref="CultureInfo.CurrentCulture"/>.</param>
        public CultureContext(CultureInfo cultureToUse)
        {
            _previousCulture = CultureInfo.CurrentCulture;

            CultureInfo.CurrentCulture = cultureToUse;
        }

        /// <summary>
        /// This method restores the <see cref="CultureInfo.CurrentCulture"/> instance that had been replaced
        /// when this instance was created.
        /// </summary>
        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
        }
    }
}