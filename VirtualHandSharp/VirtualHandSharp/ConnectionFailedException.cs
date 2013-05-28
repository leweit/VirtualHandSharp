using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualHandSharp
{
    /// <summary>
    /// Exception thrown when no connection can be made to the CyberGlove.
    /// The Message property contains the reason.
    /// </summary>
    public class ConnectionFailedException : Exception
    {
        #region Public Functions
        /// <summary>
        /// Creates a custom ConnectionFailedException instance, with a reason for its occurrence.
        /// </summary>
        /// <param name="message">The reason for this exception's occurrence.</param>
        public ConnectionFailedException(string message)
            : base(message)
        {
        }
        #endregion
    }
}
