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

        public override string Message
        {
            get
            {
                return base.Message + (LineNumber >= 0 ? " (line " + LineNumber + ")" : "");
            }
        }

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
