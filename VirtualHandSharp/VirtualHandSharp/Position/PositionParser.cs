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
using System.IO;

namespace VirtualHandSharp.Position
{
    /// <summary>
    /// A class that parses an input file in search for PositionRecords, and/or write
    /// newly created PositionRecords to the file.
    /// </summary>
    public class PositionParser
    {
        #region Static
        /// <summary>
        /// The list of known positions, used for position recognition.
        /// </summary>
        private static Dictionary<string, PositionRecord> positions;
        /// <summary>
        /// Static constructor.
        /// </summary>
        static PositionParser()
        {
            positions = new Dictionary<string, PositionRecord>();
        }
        /// <summary>
        /// Adds a record to the list of known records.
        /// </summary>
        /// <param name="pr">The PositionRecord that needs to be added.</param>
        public static void AddPosition(PositionRecord pr)
        {
            positions.Add(pr.Name, pr);
        }
        /// <summary>
        /// Searches the saved positions for a match with the given HandData object.
        /// </summary>
        /// <param name="needle">The HandData object that we have to search a match for.</param>
        /// <returns>The first match. Null if no matches were found.</returns>
        public static PositionRecord GetMatch(HandData needle)
        {
            foreach (PositionRecord pr in positions.Values)
            {
                if (pr.IsSimilar(needle))
                {
                    return pr;
                }
            }
            return null;
        }
        /// <summary>
        /// Returns a list of matches with the given HandData object.
        /// </summary>
        /// <param name="needle">The HandData object that we have to search a match for.</param>
        /// <returns>A List of matches. Will be empty if none were found.</returns>
        public static List<PositionRecord> GetAllMatches(HandData needle)
        {
            List<PositionRecord> rv = new List<PositionRecord>();

            foreach (PositionRecord pr in positions.Values)
            {
                if (pr.IsSimilar(needle))
                {
                    rv.Add(pr);
                }
            }
            return rv;
        }
        /// <summary>
        /// Returns a list of matches with the given HandData object, with the precision altered by the given argument.
        /// </summary>
        /// <param name="needle">The HandData object that we have to search a match for.</param>
        /// <param name="precision">The normal precision will be multiplied by this number. 
        /// Low number increases precision, high number decreases precision.</param>
        /// <returns>A List of matches. Will be empty if none were found.</returns>
        public static List<PositionRecord> GetAllMatches(HandData needle, double precision)
        {
            List<PositionRecord> rv = new List<PositionRecord>();
            foreach (PositionRecord pr in positions.Values)
            {
                if (pr.IsSimilar(needle, precision))
                {
                    rv.Add(pr);
                }
            }
            return rv;
        }
        /// <summary>
        /// Returns a list of matches with the given HandData object, with the precision altered by the given argument.
        /// </summary>
        /// <param name="needle">The HandData object that we have to search a match for.</param>
        /// <param name="precision">The normal precision will be multiplied by this number. 
        /// Low number increases precision, high number decreases precision.</param>
        /// <returns>A List of matches. Will be empty if none were found.</returns>
        public static List<PositionRecord> GetAllMatches(PositionRecord needle, double precision)
        {
            List<PositionRecord> rv = new List<PositionRecord>();
            foreach (PositionRecord pr in positions.Values)
            {
                if (pr.IsSimilar(needle, precision))
                {
                    rv.Add(pr);
                }
            }
            return rv;
        }
        /// <summary>
        /// The separator character for "comma" separated values.
        /// </summary>
        private static char SEPARATOR = ':';
        #endregion
        #region Properties
        /// <summary>
        /// The path to the file that will be used for writing and/or reading.
        /// </summary>
        public string Path;
        /// <summary>
        /// Gets the DebugString of this parser, which contains the path and the known positions.
        /// </summary>
        public string DebugString
        {
            get
            {
                string rv = "Parsed " + Path + "\n";
                if (positions.Count == 0)
                    rv += "    No records.";
                foreach (PositionRecord pr in positions.Values)
                {
                    rv += pr.Name + " matches ";
                    List<PositionRecord> matches = GetAllMatches(pr);
                    if (matches.Count == 0)
                        rv += "nothing";
                    foreach (PositionRecord match in matches)
                    {
                        rv += match.Name + ", ";
                    }
                    rv += "\n";
                }
                return rv;
            }
        }
        #endregion
        #region Public Functions
        /// <summary>
        /// Saves a record to the text file.
        /// </summary>
        /// <param name="record">The record that should be saved.</param>
        public void Save(PositionRecord record)
        {
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(Path, true);
                writer.WriteLine(record.Name + (record.Standalone ? "" : "-"));
                writer.WriteLine(record.CSV);
                AddPosition(record);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
            }
        }
        /// <summary>
        /// Tells whether the name already exists for a position.
        /// </summary>
        /// <param name="name">The requested name.</param>
        /// <returns>Whether the name is already used for a different position.</returns>
        public bool NameExists(string name)
        {
            name = name.ToUpper().Trim();
            if (positions.Keys.Contains<string>(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Gets a record by its name.
        /// </summary>
        /// <param name="name">The name of the requested record.</param>
        /// <returns>The record that matches the given name.</returns>
        public static PositionRecord GetByName(string name)
        {
            return positions[name];
        }
        /// <summary>
        /// Creates a parser, that will parse the file in the given path.
        /// </summary>
        /// <param name="path">The path to the file that needs to be read.</param>
        public PositionParser(string path)
        {
            this.Path = path;
        }
        /// <summary>
        /// Parses the contents of the given file. Recognized positions will be saved in 
        /// Hand.positions.
        /// </summary>
        public void Parse()
        {
            // If the file does not exist, we create it (for writing). Its contents
            // will of course be empty, so we can just exit immediately.
            if (!File.Exists(Path))
            {
                File.Create(Path);
                return;
            }
            // name and csv strings, for file input.
            string name, csv;
            StreamReader reader = new StreamReader(Path);
            try
            {
                
                while (reader.Peek() != -1)
                {
                    // Read two lines. First line is always a name, possibly with the Dependency modifier....
                    name = reader.ReadLine();
                    // ... followed by the array of values
                    csv = reader.ReadLine();
                    PositionRecord record = makeRecord(name, csv);
                    AddPosition(record);
                }
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
        #endregion
        #region Private Functions
        /// <summary>
        /// Creates a record from the file input.
        /// </summary>
        /// <param name="name">The name of the record.</param>
        /// <param name="csv">The CSV string of values.</param>
        /// <returns></returns>
        private PositionRecord makeRecord(string name, string csv)
        {
            // Parse the name line
            bool standalone = !name.EndsWith("-");
            name = name.TrimEnd(new char[] { '-' });
            PositionRecord rv = new PositionRecord(name);
            rv.Standalone = standalone;
            // Parse the CSV line
            string[] values = csv.Split(SEPARATOR);
            if (values.Length != values.Length)
            {
                throw new ArgumentException("The CSV needs to contain exactly 22 values.", "csv");
            }
            for (int i = 0; i < values.Length; i++)
            {
                string v = values[i].Trim();
                if (v == "*")
                    rv.Ignored[i] = true;
                else
                    rv[i].Value = Double.Parse(values[i].Trim());
            }
            return rv;
        }
        #endregion
    }
}
