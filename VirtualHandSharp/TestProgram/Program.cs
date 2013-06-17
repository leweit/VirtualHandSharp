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
using System.Diagnostics;
using VirtualHandSharp;
using VirtualHandSharp.Position;
using VirtualHandSharp.Motion;
using VirtualHandSharp.Emulator;
namespace TestProgram
{
    /// <summary>
    /// This class serves as a little debug program for the Hand classes.
    /// It is a console application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The TestSettings.ini path.
        /// </summary>
        private static string SETTINGSPATH = "./TestSettings.ini";
        /// <summary>
        /// The command for polling once.
        /// </summary>
        private static string POLL = "p";
        /// <summary>
        /// The command for starting to poll.
        /// </summary>
        private static string STARTPOLLING = "p1";
        /// <summary>
        /// The command to stop polling.
        /// </summary>
        private static string STOPPOLLING = "p0";
        /// <summary>
        /// The command to create a new emulation record.
        /// </summary>
        private static string MAKEEMULATION = "e";
        /// <summary>
        /// The command to quit.
        /// </summary>
        private static string QUIT = "q!";
        /// <summary>
        /// The command to create a new position record.
        /// </summary>
        private static string RECORD = "r";
        /// <summary>
        /// The command to do a speedtest.
        /// </summary>
        private static string TIMER = "t";
        /// <summary>
        /// The command to toggle the current angle unit.
        /// </summary>
        private static string TOGGLEUNIT = "tu";
        /// <summary>
        /// The command to set the refresh interval.
        /// </summary>
        private static string INTERVAL = "i";
        /// <summary>
        /// Whether to use an emulator (true) or the actual hand.
        /// </summary>
        private static bool emulating = true;
        /// <summary>
        /// The path to the positions file.
        /// </summary>
        private static string POSITIONS;
        /// <summary>
        /// The path to the motions file.
        /// </summary>
        private static string MOTIONS;
        /// <summary>
        /// The path to the mock file.
        /// </summary>
        private static string MOCK;
        /// <summary>
        /// The hand.
        /// </summary>
        private static Hand hand;
        /// <summary>
        /// The list of functions that can dynamically be added to the parser
        /// </summary>
        private static Dictionary<string, Func<string, bool>> commandParser;
        /// <summary>
        /// Gives a short string of help info per command. The key is the command name.
        /// </summary>
        private static Dictionary<string, string> help;
        /// <summary>
        /// Main method for execution of the test program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            // Read TestSettings.ini first.
            initSettings();
            try
            {
                // Make a new hand object that we will be using throughout this program.
                // This can either be an emulated one or an actual object.
                hand = emulating ? new HandEmulator(MOCK) : new Hand();
                // Make the hand poll 24 times per second.
                hand.FPS = 24;
                // Add event handlers to the position and motion events.
                hand.PositionChanged += positionChanged;
                hand.MotionDetected += MotionDetected;
                Console.WriteLine("Initialisation went fine.");
            }
            catch (ConnectionFailedException e)
            {
                // In connection can't be made, so not much can be done.
                // Exit time!
                Console.WriteLine(e.Message);
                Console.ReadKey(true);
                return;
            }
            try
            {
                // Parse the positions file's contents and print a debugstring.
                PositionParser positionParser = new PositionParser(POSITIONS);
                positionParser.Parse();
                Console.WriteLine(positionParser.DebugString);
                // Parse the motions file.
                MotionParser motionParser = new MotionParser(MOTIONS);
                motionParser.Parse();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("ArgumentException: " + e.Message);
            }
            catch (MalformedException e)
            {
                Console.WriteLine("MalformedException in {0}: {1}", e.Path, e.Message);
            }

            // Begin the main part of the program. First create the input
            // command parser.
            initCommandParser();

            // Ask for input.
            Console.Write(">>> ");
            string input = Console.ReadLine();
            string cmd = null;

            do
            {
                // After the first space, some parameters may follow. Separate the input
                // in the command, and the parameters.
                string[] parts = input.Split(new char[] { ' ' }, 2);
                cmd = parts[0];
                if (commandParser.ContainsKey(cmd))
                {
                    // Invoke the function represented by this command.
                    bool rv = (bool)commandParser[cmd].DynamicInvoke(parts.Length > 1 ? parts[1] : "");
                    // Based on the function's return value, give some feedback.
                    Console.WriteLine("Command exited " + (rv ? "without" : "with") + " errors.");
                }
                else
                {
                    // Give the user some feedback about the list of existing commands.
                    unknownCommand();
                }
                // Read next input command.
                Console.Write(">>> ");
                input = Console.ReadLine();
            } while (true);
        }
        /// <summary>
        /// Reads the input files from TestSettings.ini.
        /// </summary>
        private static void initSettings()
        {
            IniFile ini = new IniFile(SETTINGSPATH);
            POSITIONS = ini.IniReadValue("input", "position");
            MOTIONS = ini.IniReadValue("input", "motion");
            MOCK = ini.IniReadValue("input", "mock");
            emulating = Boolean.Parse(ini.IniReadValue("emulator", "enable"));
        }

        public static void MotionDetected(Hand sender, MotionRecord motion)
        {
            Console.WriteLine("Motion detected: {0}", motion.Name);
        }

        /// <summary>
        /// Gives the error message when an unknown command was entered, and gives
        /// a list of possible commands.
        /// </summary>
        private static void unknownCommand()
        {
            Console.WriteLine("Unknown command. Known commands:");
            foreach (string key in commandParser.Keys)
            {
                Console.WriteLine("  {0,-5}: {1}", key, help[key]);
            }
        }
        /// <summary>
        /// Runs a speedtest on a number of updates, specified by the argument.
        /// </summary>
        /// <param name="p">How many updates should be executed.</param>
        /// <returns>Whether the command executed without errors.</returns>
        private static bool timer(string p)
        {
            try
            {
                int freq = 1;
                if (p != null && Int32.TryParse(p, out freq))
                {
                    freq = freq >= 9 ? freq : 1;
                }
                bool polling = hand.Polling;
                hand.StopPolling();
                Stopwatch sw = new Stopwatch();

                sw.Start();
                for (int ii = 0; ii < freq; ii++)
                {
                    hand.Update();
                }
                sw.Stop();
                if (polling)
                    hand.StartPolling();
                Console.WriteLine("{0} updates took us {1} to request.", freq, sw.Elapsed);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        /// <summary>
        /// Toggles the Angle unit between radians and degrees. 
        /// </summary>
        /// <param name="p">Unused.</param>
        /// <returns>Whether the command executed without errors.</returns>
        private static bool switchUnit(string p)
        {
            if (Angle.UseAngleUnit == Angle.AngleUnit.DEGREES)
            {
                Angle.UseAngleUnit = Angle.AngleUnit.RADIANS;
                Console.WriteLine("Now using Radians.");
            }
            else
            {
                Angle.UseAngleUnit = Angle.AngleUnit.DEGREES;
                Console.WriteLine("Now using Degrees.");
            }
            return true;
        }
        /// <summary>
        /// Prints the hand's debug info.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool poll(string p)
        {
            if (hand == null)
                return false;
            if (hand.Polling == false)
                hand.Update();
            Console.WriteLine(hand.DebugString);
            if (hand.CurrentPosition != null)
            {
                Console.WriteLine("Position: {0}", hand.CurrentPosition.Name);
            }
            return true;
        }
        /// <summary>
        /// Creates a PositionRecord.
        /// </summary>
        /// <param name="p">Parameter, should be the name of the new record.</param>
        /// <returns></returns>
        private static bool record(string p)
        {
            try
            {
                // This requires one parameter that will serve as a name.
                if (p == null || p == "")
                {
                    Console.WriteLine("A name for the position in required as parameter.");
                    return false;
                }

                PositionRecord record = new PositionRecord(p);
                record.CloneData(hand);

                editRecord(record);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Lets the user edit the record in command line.
        /// </summary>
        /// <param name="record">The PositionRecord that has been recorded.</param>
        private static void editRecord(PositionRecord record)
        {
            string input = "";
            do
            {
                Console.WriteLine(record.Name);
                Console.WriteLine(record.CSV);
                Console.WriteLine("Anything you still want to do?");
                Console.WriteLine("  s : Save record");
                Console.WriteLine("  q!: Discard record");
                Console.WriteLine("  w: Ignore wrist.");
                Console.WriteLine("  a: Ignore abductions.");
                Console.WriteLine("  t: Ignore thumb.");
                Console.WriteLine("  Enter any number from 0 to 21 to");
                Console.WriteLine("  add a joint to the ignore list.");

                Console.Write(">>> ");
                input = Console.ReadLine().Trim();
                int joint = -1;
                if (input == "q!")
                    break;
                else if (input == "w")
                    record.IgnoreWrist();
                else if (input == "t")
                    record.IgnoreThumb();
                else if (input == "a")
                    record.IgnoreAbductions();
                else if (input == "s")
                {
                    PositionParser p = new PositionParser("positions.txt");
                    Console.WriteLine("Saving record!");
                    for (int i = 0; i < 22; i++)
                    {
                        Console.WriteLine("[{0,2}]: {1,5} : {2,5}", i, record[i].Value, hand[i].Value);
                    }
                    p.Save(record);
                    break;
                }
                else if (Int32.TryParse(input, out joint))
                {
                    record.Ignored[joint] = true;
                }
                else
                {
                    Console.WriteLine("Unrecognized command!!");
                }
            } while (true);
        }
        /// <summary>
        /// Initiates the parser.
        /// </summary>
        /// <returns>Whether the function executed without errors.</returns>
        private static bool initCommandParser()
        {
            // Make a dictionary of strings and pointers. The strings are
            // the commands thatwill be entered by the user.
            commandParser = new Dictionary<string, Func<string, bool>>();
            commandParser[MAKEEMULATION] = makeEmulationRecord;
            commandParser[INTERVAL] = setInterval;
            commandParser[POLL] = poll;
            commandParser[STARTPOLLING] = beginPolling;
            commandParser[STOPPOLLING] = stopPolling;
            commandParser[QUIT] = exitAfterKeyPress;
            commandParser[RECORD] = record;
            commandParser[TIMER] = timer;
            commandParser[TOGGLEUNIT] = switchUnit;

            // Also make a list of helpful explanations for each command.
            help = new Dictionary<string, string>();
            help[MAKEEMULATION] = "Makes an emulation record.";
            help[INTERVAL] = "Requires one integer argument. Sets the hand's refresh interval to n milliseconds.";
            help[POLL] = "Tells the hand to poll for data, then prints the hand's debug info.";
            help[STARTPOLLING] = "Tells the hand to start polling for data.";
            help[STOPPOLLING] = "Tells the hand to stop polling for data.";
            help[QUIT] = "Quits the program.";
            help[RECORD] = "Records a hand position. Takes one string argument that will be the record's name.";
            help[TIMER] = "Requires one integer argument. Does a speedtest on n updates.";
            help[TOGGLEUNIT] = "Toggles the unit that angles will be returned in. (Radians vs Degrees)";
            
            return true;
        }
        /// <summary>
        /// Sets the hand's refresh interval to a number given as parameter.
        /// </summary>
        /// <param name="p">The refresh interval in milliseconds. 
        /// Should be positive and no higher than 1000.</param>
        /// <returns>Whether the command executed without errors.</returns>
        private static bool setInterval(string p)
        {
            try
            {
                int i = 0;
                bool success = (p != null && Int32.TryParse(p, out i));
                if (i <= 0 || i > 1000 || !success)
                {
                    throw new ArgumentException("Expected: 0 < i <= 1000");
                }
                hand.Interval = i;
                return true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Wrong argument. " + e.Message);
                return false;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Hand was not initialised yet.");
                return false;
            }
        }
        /// <summary>
        /// Exits the application once a key has been pressed.
        /// </summary>
        private static bool exitAfterKeyPress(string p)
        {
            if (hand != null) hand.StopPolling();
            Console.WriteLine("Quitting. Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);

            return true;
        }
        /// <summary>
        /// Handler for the hand's PositionChanged event.
        /// </summary>
        /// <param name="sender">The hand whose position has changed.</param>
        /// <param name="position">The newly matched position.</param>
        private static void positionChanged(Hand sender, PositionRecord position)
        {
            if (position.Standalone)
                Console.WriteLine("New position: {0}", position.Name);
        }
        /// <summary>
        /// Makes the hand vibrate.
        /// </summary>
        /// <param name="p">Unused.</param>
        /// <returns>Whether the command executed without errors.</returns>
        private static bool vibrate(string p)
        {
            // hand.vibrate(0.5);
            Console.WriteLine("Disabled.");
            return false;
        }
        /// <summary>
        /// Makes the hand stop vibrating.
        /// </summary>
        /// <param name="p">Unused.</param>
        /// <returns>Whether the command executed without errors.</returns>
        private static bool stopVibrate(string p)
        {
            // hand.vibrate(0);
            Console.WriteLine("Disabled.");
            return false;
        }
        /// <summary>
        /// Makes the hand begin polling for data.
        /// </summary>
        /// <param name="p">Unused.</param>
        /// <returns>Whether the command executed without errors.</returns>
        private static bool beginPolling(string p)
        {
            try
            {
                hand.StartPolling();
                Console.WriteLine("Polling!");
                return true;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Hand was not connected yet...");
                return false;
            }
        }
        /// <summary>
        /// Makes the hand stop polling for data.
        /// </summary>
        /// <param name="p">Unused.</param>
        /// <returns>Whether the command executed without errors.</returns>
        private static bool stopPolling(string p)
        {
            try
            {
                hand.StopPolling();
                Console.WriteLine("Not polling anymore.");
                return true;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Hand was not connected yet...");
                return false;
            }
        }
        /// <summary>
        /// Creates an emulator record file. Path is the file to use.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool makeEmulationRecord(string p)
        {
            try
            {
                if (p == null || p == "")
                {
                    return false;
                }
                Recorder he = new Recorder(hand, 24, p);
                Console.WriteLine("Press any key to start...");
                Console.ReadKey(true);
                he.Start();
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
                he.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
