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
            initSettings();
            PositionRecord pr = new PositionRecord(null);
            try
            {
                // Make a new hand object that we will be using throughout this program.
                if (emulating)
                {
                    hand = new HandEmulator(MOCK, true);
                }
                else
                {
                    hand = new Hand();
                }
                hand.Interval = 10;
                hand.PositionChanged += positionChanged;
                hand.MotionDetected += MotionDetected;
                Console.WriteLine("Initialisation went fine.");
            }
            catch (ConnectionFailedException e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
                return;
            }
            try
            {
                // Parse the positions.txt file's contents.
                PositionParser positionParser = new PositionParser(POSITIONS);
                positionParser.Parse();
                Console.WriteLine(positionParser.DebugString);

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

            initCommandParser();

            Console.Write(">>> ");
            string input = Console.ReadLine();
            string cmd = null;
            bool result = true;
            do
            {
                string[] para = input.Split(new char[] { ' ' }, 2);
                cmd = para[0];
                if (commandParser.ContainsKey(cmd))
                {
                    bool rv = (bool)commandParser[cmd].DynamicInvoke(para.Length > 1 ? para[1] : "");
                    Console.WriteLine("Command exited " + (rv ? "without" : "with") + " errors.");
                }
                else
                {
                    unknownCommand();
                }
                Console.Write(">>> ");
                input = Console.ReadLine();
            } while (input != "q!" && result);
        }

        private static void initSettings()
        {
            IniFile ini = new IniFile("./TestSettings.ini");
            POSITIONS = ini.IniReadValue("input", "position");
            MOTIONS = ini.IniReadValue("input", "motion");
            MOCK = ini.IniReadValue("input", "mock");
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
                Console.WriteLine("  {0,-"+"toggleunit".Length+"}: {1}", key, help[key]);
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
            commandParser = new Dictionary<string, Func<string, bool>>();
            help = new Dictionary<string, string>();

            commandParser["emake"] = makeEmulationRecord;
            commandParser["interval"] = setInterval;
            commandParser["poll"] = poll;
            commandParser["pstart"] = beginPolling;
            commandParser["pstop"] = stopPolling;
            commandParser["q!"] = exitAfterKeyPress;
            commandParser["record"] = record;
            commandParser["timer"] = timer;
            commandParser["toggleunit"] = switchUnit;

            help["emake"] = "Makes an emulation record.";
            help["interval"] = "Requires one integer argument. Sets the hand's refresh interval to n milliseconds.";
            help["poll"] = "Tells the hand to poll for data, then prints the hand's debug info.";
            help["pstart"] = "Tells the hand to start polling for data.";
            help["pstop"] = "Tells the hand to stop polling for data.";
            help["q!"] = "Quits the program.";
            help["record"] = "Records a hand position. Takes one string argument that will be the record's name.";
            help["timer"] = "Requires one integer argument. Does a speedtest on n updates.";
            help["toggleunit"] = "Toggles the unit that angles will be returned in. (Radians vs Degrees)";

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
        /// <author>Arno Sluismans</author>
        private static bool exitAfterKeyPress(string p)
        {
            Console.WriteLine("Quitting. Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
            return true;
        }
        /// <summary>
        /// Handler for the hand's PositionChanged event.
        /// </summary>
        /// <param name="sender">The hand whose position has changed.</param>
        /// <param name="position"></param>
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
                Console.ReadKey(false);
                he.Start();
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(false);
                he.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        /// <summary>
        /// Event handler for the hand's HandGrasped event.
        /// </summary>
        /// <param name="sender">The event's sender.</param>
        /// <param name="e">Event arguments.</param>
        public static void HasGrasped(object sender, EventArgs e)
        {
            Console.WriteLine("Grasped!");
        }
        /// <summary>
        /// Event handler for the hand's HandReleased event.
        /// </summary>
        /// <param name="sender">The event's sender.</param>
        /// <param name="e">Event arguments.</param>
        public static void HasReleased(object sender, EventArgs e)
        {
            Console.WriteLine("Released!");
        }
    }
}
