using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace VirtualHandSharp
{
    /// <summary>
    /// The <see cref="VirtualHandSharp"/> namespace containing the classes required for
    /// the creation and usage of a Hand object.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    /*
    /// <summary>
    /// This class serves as a little debug program for the Hand classes.
    /// It is a console application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The hand.
        /// </summary>
        private static Hand hand;
        /// <summary>
        /// The list of functions that can dynamically be added to the parser
        /// </summary>
        private static Dictionary<string, Func<string, bool>> parser;
        static void Main(string[] args)
        {
            try
            {
                hand = new Hand();
                Angle.UseAngleUnit = Angle.AngleUnit.DEGREES;
                //hand.startPolling();
                hand.HandGrasped += HasGrasped;
                hand.HandReleased += HasReleased;
            }
            catch (ConnectionFailedException e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
                return;
            }
            initParser();
            //hand.StatusChanged += new Hand.StatusChangedHandler(handStatusHasChanged);
            string input = Console.ReadLine();
            string cmd = null;
            bool result = true;
            do
            {
                string[] para = input.Split(new char[] { ' ' }, 2);
                cmd = para[0];
                if (parser.ContainsKey(cmd))
                {
                    parser[cmd].DynamicInvoke(para.Length > 1 ? para[1] : "");
                }
                else
                {
                    unknownCommand();
                }
                input = Console.ReadLine();
            } while(input != "q!" && result);

        }

        static void unknownCommand()
        {
            Console.WriteLine("Unknown command. Known commands:");
            foreach (string key in parser.Keys)
            {
                Console.WriteLine("    " + key);
            }
        }

        static bool timer(string p)
        {
            try
            {
                int freq = 1;
                if (p != null && Int32.TryParse(p, out freq))
                {
                    freq = freq >= 9 ? freq : 1;
                }

                Stopwatch sw = new Stopwatch();

                sw.Start();
                for (int ii = 0; ii < freq; ii++)
                {
                    hand.update();
                }
                sw.Stop();

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
        /// Prints the hand's debug info.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static bool poll(string p)
        {
            if (hand == null)
                return false;
            hand.update();
            Console.WriteLine(positionToString(hand.Position));
            Console.WriteLine(hand.DebugString);
            return true;
        }

        /// <summary>
        /// Initiates the parser.
        /// </summary>
        /// <returns></returns>
        static bool initParser()
        {
            parser = new Dictionary<string, Func<string, bool>>();
            parser["p"] = poll;
            parser["q!"] = exitAfterKeyPress;
            parser["t"] = timer;
            parser["v1"] = vibrate;
            parser["v0"] = stopVibrate;

            return true;
        }

        /// <summary>
        /// Exits the application once a key has been pressed.
        /// </summary>
        /// <author>Arno Sluismans</author>
        static bool exitAfterKeyPress(string p)
        {
            Console.WriteLine("Quitting. Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
            return true;
        }

        /// <summary>
        /// The event handler for when this.hand has received new data from the CyberGlove.
        /// This is supposed to happen at a set interval after the Hand object has been initialised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <author>Arno Sluismans</author>
        private static void handStatusHasChanged(object sender, StatusEventArgs e)
        {
            Console.WriteLine(positionToString(hand.Position));
        }

        /// <summary>
        /// Gets a Hand.RPSPosition's name.
        /// </summary>
        /// <param name="pos">The Hand.RPSPosition.</param>
        /// <returns>The Hand.RPSPosition's string presentation.</returns>
        /// <author>Arno Sluismans</author>
        private static string positionToString(Hand.RPSPosition pos)
        {
            switch (pos)
            {
                case Hand.RPSPosition.PAPER:
                    return "Paper";
                case Hand.RPSPosition.ROCK:
                    return "Rock";
                case Hand.RPSPosition.SCISSORS:
                    return "Scissors";
                default:
                    return "???";
            }
        }

        private static bool vibrate(string p)
        {
            // hand.vibrate(0.5);
            Console.WriteLine("Disabled.");
            return true;
        }

        private static bool stopVibrate(string p)
        {
            // hand.vibrate(0);
            Console.WriteLine("Disabled.");
            return true;
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

        /// <summary>
        /// Creates a new CyberHand object.
        /// </summary>
        /// <returns>Returns a pointer to the newly created object.</returns>
        /// <author>Arno Sluismans</author>
        [DllImport("C:\\Program Files (x86)\\Unity\\Editor\\VirtualHandBridge")]
        static public extern IntPtr CreateHand();

        /// <summary>
        /// Deletes the CyberHand object.
        /// </summary>
        /// <param name="hand">The pointer to the object we need to dispose of.</param>
        /// <author>Arno Sluismans</author>
        [DllImport("C:\\Program Files (x86)\\Unity\\Editor\\VirtualHandBridge")]
        static public extern void DeleteHand(IntPtr hand);
        
    }
    */
}
