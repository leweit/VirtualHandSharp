using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualHandSharp.Glove
{
    /// <summary>
    /// StatusEventsArgs is a class that wraps a status message which will be passed on the
    /// StatusChanged event (or similar events). 
    /// </summary>
    /// <author>Arno Sluismans</author>
    public class StatusEventArgs : EventArgs
    {
        /// <summary>
        /// The new status.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// The default constructor, whose usage is discouraged. It simply sets Status to a standard
        /// meaningless message.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public StatusEventArgs()
        {
            this.Status = "Nothing out of the ordinary.";
        }
        /// <summary>
        /// A constructor that sets the status message.
        /// </summary>
        /// <param name="status">The new status message.</param>
        /// <author>Arno Sluismans</author>
        public StatusEventArgs(string status)
        {
            this.Status = status;
        }

    }
}
