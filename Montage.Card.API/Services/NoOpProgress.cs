using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Services
{
    /// <summary>
    /// Represents a Progress class that does nothing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NoOpProgress<T> : IProgress<T>
    {
        /// <summary>
        /// Represents a static instance of the class.
        /// </summary>
        public static NoOpProgress<T> Instance { get; } = new NoOpProgress<T>();

        private NoOpProgress()
        {

        }

        public void Report(T value)
        {
            // Do nothing
        }
    }
}
