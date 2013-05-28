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
            records = new Dictionary<string, MotionRecord>();
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

        public static List<MotionRecord> GetMatches(PositionRecord needle)
        {
            List<MotionRecord> rv = new List<MotionRecord>();
            foreach (MotionRecord mr in records.Values)
            {
                if (mr.CheckMatched(needle))
                    rv.Add(mr);
            }
            return rv;
        }
    }
}
