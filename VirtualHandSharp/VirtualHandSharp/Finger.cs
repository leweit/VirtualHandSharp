using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualHandSharp
{
    /// <summary>
    /// Represents a finger, and allows for CyberGlove's joint data to be 
    /// saved in Angle objects (Inner, Middle and Outer) that are read-only.
    /// </summary>
    /// <author>Arno Sluismans</author>
    public class Finger : FingerData
    {
        #region Properties
        /// <summary>
        /// Returns a debug string, containing the joints' data.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public string DebugString
        {
            get
            {
                return Inner.Value + ", " + Middle.Value + ", " + Outer.Value +
                    (NextAbduction == null ? "" : (", " + NextAbduction.Value));
            }
        }
        #endregion
        #region Public Functions
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id">The index of the finger. Thumb is 0, index is 1, etc.</param>
        /// <author>Arno Sluismans</author>
        public Finger(int id) : base(id)
        {
        }
        #endregion
    }
}
