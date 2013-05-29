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

namespace VirtualHandSharp
{
    /// <summary>
    /// An Exception thrown by the parsers when an input file contains incorrect syntax or semantics. 
    /// </summary>
    public class MalformedException : Exception
    {
        /// <summary>
        /// The path to the erroneous input file.
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// Specifies the number of the line on which the error occurred.
        /// </summary>
        public readonly int LineNumber;
        /// <summary>
        /// The error message, including the line number.
        /// </summary>
        public override string Message
        {
            get
            {
                return base.Message + (LineNumber >= 0 ? " (line " + LineNumber + ")" : "");
            }
        }
        /// <summary>
        /// Creates a custom MalformedException.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="path">The input file's path.</param>
        /// <param name="lineNumber">The number of the line on which the error occurred.</param>
        public MalformedException(string message, string path, int lineNumber)
            : base(message)
        {
            LineNumber = lineNumber;
            Path = path;
        }
        /// <summary>
        /// Constructs a MalformedException with the given message and file name.
        /// </summary>
        /// <param name="message">The specific error.</param>
        /// <param name="path">The path to the input file.</param>
        public MalformedException(string message, string path)
            : this(message, path, -1)
        {
        }

        /// <summary>
        /// Constructs a MalformedException with the given message.
        /// </summary>
        /// <param name="message"></param>
        public MalformedException(string message)
            : this(message, (string)null)
        {
        }
    }
}
