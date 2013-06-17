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
using System.Linq;
using System.Text;

namespace VirtualHandSharp.Position
{
    /// <summary>
    /// Compares by matching all angles separately.
    /// </summary>
    public class AllIndividualMatching : MatchingStrategy
    {
        /// <summary>
        /// If this one is higher, it will be easier to match stuff.
        /// </summary>
        public static double TOLLERANCE { get; set; }
        /// <summary>
        /// Static constructor.
        /// </summary>
        static AllIndividualMatching()
        {
            TOLLERANCE = 1.0;
        }
        /// <summary>
        /// Compares two handdatas and tells whether they match.
        /// </summary>
        /// <param name="position">The position that should be matched.</param>
        /// <param name="current">The current data.</param>
        /// <returns>Whether the data matches.</returns>
        public bool AreSimilar(PositionRecord position, HandData current, double tollerance = 0)
        {
            tollerance = tollerance > 0 ? tollerance : TOLLERANCE;
            int max = (int)HandData.Joint.MAX;
            for (int i = 0; i < max; i++)
            {
                if (position.Ignored[i])
                    continue;
                else if (!position[i].IsSimilar(current[i], tollerance))
                    return false;
            }
            return true;
        }
    }
}
