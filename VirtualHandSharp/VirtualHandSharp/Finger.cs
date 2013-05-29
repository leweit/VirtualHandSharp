/*
 * Copyright (C) 2013 Rovaniemi University of Applied Sciences (Rovaniemen Ammattikorkeakoulu)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights 
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
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
