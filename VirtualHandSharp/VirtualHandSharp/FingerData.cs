﻿/*
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

namespace VirtualHandSharp
{
    /// <summary>
    /// A wrapper class for a finger's joint data.
    /// </summary>
    public class FingerData
    {
        #region Members
        /// <summary>
        /// The inner joint angle.
        /// </summary>
        protected Angle inner;
        /// <summary>
        /// The middle joint angle.
        /// </summary>
        protected Angle middle;
        /// <summary>
        /// The outer joint angle.
        /// </summary>
        protected Angle outer;
        #endregion
        #region Properties
        /// <summary>
        /// The joint closest to the palm.
        /// 
        /// Close to 0 means that the joint is stretched.
        /// Negative means that the joint is bent towards the palm.
        /// Positive means that the joint is bent backwards (barely possible).
        /// </summary>
        public Angle Inner
        {
            get { return inner; }
        }
        /// <summary>
        /// The middle joint.
        /// 
        /// Close to 0 means that the joint is stretched.
        /// Negative means that the joint is bent towards the palm.
        /// Positive means that the joint is bent backwards (barely possible).
        /// </summary>
        public Angle Middle
        {
            get { return middle; }
        }
        /// <summary>
        /// The joint closest to the fingertip.
        /// 
        /// Close to 0 means that the joint is stretched.
        /// Negative means that the joint is bent towards the palm.
        /// Positive means that the joint is bent backwards (barely possible).
        /// </summary>
        public Angle Outer
        {
            get { return outer; }
        }
        /// <summary>
        /// Gets or sets the array of joints. Inner indexed.
        /// </summary>
        public Angle[] Joints { get; protected set; }
        /// <summary>
        /// Gets or sets the abduction between this finger and the one previous to it.
        /// (i.e.: closer to the thumb)
        /// </summary>
        public Angle PreviousAbduction { get; set; }
        /// <summary>
        /// Gets or sets the abduction between this finger and the one next to it.
        /// (i.e.: away from the thumb)
        /// </summary>
        public Angle NextAbduction { get; set; }
        #endregion
        #region Public Functions
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id">The index of the finger. Thumb is 0, index is 1, etc.</param>
        public FingerData(int id)
        {
            if (id > 0)
            {
                PreviousAbduction = new Angle();
            }
            if (id < HandData.NR_FINGERS - 1)
            {
                NextAbduction = new Angle();
            }
            inner = new Angle();
            middle = new Angle();
            outer = new Angle();
            Joints = new Angle[] { inner, middle, outer };
        }
        /// <summary>
        /// Populates the joints' data with the given data. 
        /// </summary>
        /// <param name="inner">The inner joint's angle data.</param>
        /// <param name="middle">The middle joint's angle data.</param>
        /// <param name="outer">The outer joint's angle data.</param>
        public void SetJoints(double inner, double middle, double outer)
        {
            // Set the inner joint
            this.inner.Value = inner;
            // Set the middle joint
            this.middle.Value = middle;
            // Set the outer joint
            this.outer.Value = outer;
        }
        #endregion
    }
}
