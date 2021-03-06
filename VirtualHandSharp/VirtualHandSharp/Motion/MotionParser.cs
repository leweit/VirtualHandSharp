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
using VirtualHandSharp.Position;
using System.IO;

namespace VirtualHandSharp.Motion
{
    /// <summary>
    /// A class that can parse a file of MotionRecords. It is not yet possible to write 
    /// existing MotionRecords to a file.
    /// </summary>
    public class MotionParser
    {
        /// <summary>
        /// Static constructor.
        /// </summary>
        static MotionParser()
        {
            records = new Dictionary<string, MotionRecord>();
        }
        /// <summary>
        /// The list of saved MotionRecords.
        /// </summary>
        private static Dictionary<string, MotionRecord> records;
        /// <summary>
        /// Adds a record to the list of known records.
        /// </summary>
        /// <param name="record">The new record.</param>
        public static void AddRecord(MotionRecord record)
        {
            records.Add(record.Name, record);
        }
        /// <summary>
        /// The path to the input file.
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// Gets a MotionRecord by its name (specified in the input file).
        /// </summary>
        /// <param name="name">The name of the record.</param>
        /// <returns></returns>
        public static MotionRecord GetByName(string name)
        {
            return records[name];
        }
        /// <summary>
        /// Creates a default parser that will read the given file.
        /// </summary>
        /// <param name="path">The path to the input file.</param>
        public MotionParser(string path)
        {
            Path = path;
        }
        private string read(StreamReader reader, ref int lineNumber)
        {
            string line = null;
            while (reader.Peek() != -1)
            {
                line = reader.ReadLine().Trim();
                lineNumber++;
                if (line == "" || line.StartsWith("#")) // Empty line or comment line
                    continue;

                return line;
            }
            return null;
        }
        /// <summary>
        /// Parses the input file.
        /// </summary>
        public void Parse()
        {
            // If the file does not exist, throw an exception.
            if (!File.Exists(Path))
            {
                throw new FileNotFoundException("The input file does not exist.", Path);
            }
            // name and csv strings, for file input.
            string line;
            StreamReader reader = new StreamReader(Path);
            int lineNumber = -1; // For potential error messages.
            try
            {
                while (reader.Peek() != -1)
                {
                    // Read line per line. Each line is one record.
                    line = read(reader, ref lineNumber);
                    if (line == null) return;
                    MotionRecord record = makeRecord(line);
                    AddRecord(record);
                }
            }
            catch (MalformedException e)
            {
                // Add linenumber and path to the exception.
                throw new MalformedException(e.Message, Path, lineNumber);
            }
            catch (KeyNotFoundException e)
            {
                throw new MalformedException(e.Message, Path, lineNumber);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }
        private MotionRecord makeRecord(string line)
        {
            string[] tokens = line.Split(new char[] { ' ' });
            // There must be room for at least a name and two positions.
            if (tokens.Length < 3)
            {
                throw new MalformedException("Need at least three tokens per line.");
            }
            // The first token should be the name. If this is not the correct, throw exception.
            string name = tokens[0].Trim();
            if (!name.EndsWith("="))
            {
                throw new MalformedException("Name was not specified with = operator.");
            }
            // Finally create the returnvalue.
            MotionRecord rv = new MotionRecord(name.Trim(new char[] {'=', ' ', '\t'}));
            // And fill it up!
            for (int i = 1; i < tokens.Length; i++)
            {
                rv.Add(SequenceItem.FromToken(tokens[i]));
            }

            // We still need to enable the modifiers.
            rv.InitModifiers();
            return rv;
        }
        /// <summary>
        /// Checks for matches in the entire records list. Matches will invoke events.
        /// </summary>
        /// <param name="needle">The PositionRecord that needs to be matched.</param>
        public static void Match(PositionRecord needle)
        {
            foreach (MotionRecord mr in records.Values)
            {
                mr.CheckMatched(needle);
            }
        }
        /// <summary>
        /// Checks for matches based on the current PositionRecord.
        /// </summary>
        /// <param name="curr">The current PositionRecord</param>
        /// <returns>A list of all MotionRecords that have been matched.</returns>
        public static List<MotionRecord> GetMatches(PositionRecord curr)
        {
            List<MotionRecord> rv = new List<MotionRecord>();
            foreach (MotionRecord mr in records.Values)
            {
                if (mr.CheckMatched(curr))
                    rv.Add(mr);
            }
            return rv;
        }
    }
}
