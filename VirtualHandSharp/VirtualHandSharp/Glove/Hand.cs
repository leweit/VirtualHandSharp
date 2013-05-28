using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace VirtualHandSharp.Glove
{
    /// <summary>
    /// An instance of this class can communicate with a connected CyberGlove. Its data gets updated
    /// at a set interval. The class is currently designed to be fit for Rock-Paper-Scissors usage, but
    /// can be edited and expanded as desired. The finger objects can either be accessed by their names (thumb, index, etc)
    /// or through the "fingers" array, which is thumb-indexed (meaning thumb's index is 0, pinkie's index is 4, etc).
    /// </summary>
    /// <author>Arno Sluismans</author>
    public class Hand
    {

        /// <summary>
        /// An enum that represents the three possible hand positions: Rock, paper and scissors.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public enum RPSPosition
        {
            PAPER = 0,
            ROCK = 1,
            SCISSORS = 2,
            UNRECOGNIZED = -1
        }
        #region Members

        /// <summary>
        /// The number of fingers per hand.
        /// </summary>
        public const int NR_FINGERS = 5;
        /// <summary>
        /// The number of joints per finger.
        /// </summary>
        public const int NR_JOINTS = 4;
        /// <summary>
        /// The interval at which we want the hand's data to be refreshed. (Expressed in milliseconds).
        /// This property is write-only.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public int Interval
        {
            set
            {
                if (pollTimer != null)
                {
                    // Only set the interval if the pollTimer object exists
                    pollTimer.Change(0, value);
                }
                interval = value;
            }
            get
            {
                return interval;
            }
        }
        private int interval;
        /// <summary>
        /// The Finger object representing the thumb.
        /// </summary>
        private Finger thumb = null;
        /// <summary>
        /// The Finger object representing the index finger.
        /// </summary>
        private Finger index = null;
        /// <summary>
        /// The Finger object representing the middle finger.
        /// </summary>
        private Finger middle = null;
        /// <summary>
        /// The Finger object representing the ring finger.
        /// </summary>
        private Finger ring = null;
        /// <summary>
        /// The Finger object representing the pinkie finger.
        /// </summary>
        private Finger pinkie = null;
        /// <summary>
        /// An array of the fingers; useful for loops that need to access all fingers.
        /// It is indexed from thumb to pinkie, meaning that the thumb is at the lowest index,
        /// and the pinkie is the highest index.
        /// </summary>
        public Finger[] Fingers { get; private set; }
        /// <summary>
        /// An IntPtr that points to address containing the C++ class of the CyberGlove. 
        /// Accessing its methods requires for them to be bridged in VirtualHandBridge.dll
        /// and should be done with caution. By any means, this is the most difficult part of this project.
        /// See below (at the end of this file) for a couple of such examples.
        /// </summary>
        private IntPtr vhtHand = IntPtr.Zero;
        /// <summary>
        /// The timer that handles the interval at which data will be polled.
        /// </summary>
        private Timer pollTimer = null;
        /// <summary>
        /// Gets or sets the Wrist's yaw.
        /// </summary>
        public double WristYaw
        {
            get { return wristYaw.Value; }
        }
        /// <summary>
        /// Gets or sets the Wrist's pitch.
        /// </summary>
        public double WristPitch
        {
            get { return wristPitch.Value; }
        }
        /// <summary>
        /// Gets or sets the Palm Arm. (Causes the pinkie to rotate across palm.
        /// </summary>
        public double PalmArch
        {
            get { return palmArch.Value; }
        }
        private Angle wristYaw;
        private Angle wristPitch;
        private Angle palmArch;
        #endregion
        #region Properties
        /// <summary>
        /// Gets some debug output. Specifically, it returns the fingers' names and whether they
        /// are respectively stretched or bent. It also returns said fingers' respective debug strings, which
        /// include the joints' data.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public string DebugString
        {
            get
            {
                string rv = ""; // Return value
                rv += "Glove: \n";
                // Add thumb data
                rv += "Thumb is " + (thumb.IsStretched ? "stretched" : "bent") + "\n";
                rv += "    [" + thumb.DebugString + "];\n";
                // Add index data
                rv += "Index is " + (index.IsStretched ? "stretched" : "bent") + "\n";
                rv += "    [" + index.DebugString + "];\n";
                // Add middle finger data
                rv += "Middle is " + (middle.IsStretched ? "stretched" : "bent") + "\n";
                rv += "    [" + middle.DebugString + "];\n";
                // Add ring finger data
                rv += "Ring is " + (ring.IsStretched ? "stretched" : "bent") + "\n";
                rv += "    [" + ring.DebugString + "];\n";
                // Add pinkie data
                rv += "Pinkie is " + (pinkie.IsStretched ? "stretched" : "bent") + "\n";
                rv += "    [" + pinkie.DebugString + "];\n";
                // Pitch and yaw:
                rv += "Wrist:\n";
                rv += "    [" + WristPitch + ", " + WristYaw + ", " + PalmArch + "]\n\n";
                return rv;
            }
        }
        /// <summary>
        /// Calculates and returns the current hand position (in accordance to Rock-Paper-Scissors' rules).
        /// </summary>
        /// <author>Arno Sluismans</author>
        public RPSPosition Position
        {
            get
            {
                if (thumb.IsStretched &&
                    index.IsStretched &&
                    middle.IsStretched &&
                    ring.IsStretched &&
                    pinkie.IsStretched
                )
                {
                    // In paper, all fingers are stretched.
                    return RPSPosition.PAPER;
                }
                else if (
                    index.IsBent &&
                    middle.IsBent &&
                    ring.IsBent &&
                    pinkie.IsBent
                )
                {
                    // In rock, all fingers (except maybe thumb) are bent 
                    return RPSPosition.ROCK;
                }
                else if (
                    index.IsStretched &&
                    middle.IsStretched &&
                    ring.IsBent &&
                    pinkie.IsBent
                )
                {
                    // In scissors, only index and middle (and maybe thumb) are stretched
                    return RPSPosition.SCISSORS;
                }
                else
                {
                    // If none of the patterns are matched, the position is unrecognised.
                    return RPSPosition.UNRECOGNIZED;
                }
            }
        }
        /// <summary>
        /// Gets or sets whether the polling for data is currently enabled.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public bool Polling { get; private set; }
        #endregion
        #region Events
        public delegate void StatusChangedHandler(object sender, StatusEventArgs e);
        public event StatusChangedHandler StatusChanged;

        #endregion
        /// <summary>
        /// Default constructor; creates all fingers and initiates connection with the CyberGlove. Basically,
        /// after the constructor has been called, this object is ready for use.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public Hand()
        {
            // Create finger objects.
            thumb = new Finger(0);
            index = new Finger(1);
            middle = new Finger(2);
            ring = new Finger(3);
            pinkie = new Finger(4);
            wristYaw = new Angle();
            wristPitch = new Angle();
            palmArch = new Angle();
            // Initialise the Interval to a standard value.
            Interval = 1000;
            // Fill the fingers array with the newly created fingers.
            Fingers = new Finger[NR_FINGERS] { thumb, index, middle, ring, pinkie };
            // Initialise the CyberHand connection.
            initVhtHand();
        }

        /// <summary>
        /// Can be used to manually request an update of data. This is especially useful if you want
        /// to disable the timer and decide manually when things should be updated.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public void update()
        {
            // request an update
            pollJointData();
        }

        /// <summary>
        /// Makes the hand vibrate.
        /// </summary>
        /// <param name="v">How hard it should vibrate.</param>
        public void vibrate(double v)
        {
            VibrateAll(vhtHand, v);
        }

        /// <summary>
        /// Requests renewed data from the CyberGlove and populates the finger objects with this
        /// new data.
        /// </summary>
        /// <author>Arno Sluismans</author>
        private void pollJointData()
        {
            // Calculate the size of the array.
            int max = NR_FINGERS * NR_JOINTS + 2;
            // Create an array of doubles, that will later be filled with joint data.
            double[] arr = new double[max];
            // Poll() fills arr with data and returns the size of the new array.
            // If that size is not 15, something weird is going on.
            int size = Poll(this.vhtHand, arr, max);
            // Go through the fingers array to populate the fingers with their data.
            for (int i = 0; i < NR_FINGERS; i++)
            {
                // Populate the finger with joint data.
                Fingers[i].setJoints(
                    arr[i * NR_JOINTS + 0], // Inner joint
                    arr[i * NR_JOINTS + 1], // Middle joint
                    arr[i * NR_JOINTS + 2]  // Outer joint
                );
                // Get the abduction between this finger and the next.
                double abd = arr[i * NR_JOINTS + 3];
                if (i >= NR_FINGERS - 1)
                {
                    palmArch.Value = abd;
                }
                else
                {
                    Fingers[i].NextAbduction.Value = abd;
                    Fingers[i + 1].PreviousAbduction.Value = abd;
                }
            }
            // Set wrist pitch and yaw.
            wristPitch.Value = arr[max - 2];
            wristYaw.Value = arr[max - 1];
            // Create arguments for the StatusChangedEvent
            StatusEventArgs args = new StatusEventArgs(DebugString);
            // Invoke the StatusChanged event.
            if (StatusChanged != null)
                StatusChanged(this, args);
        }

        /// <summary>
        /// Event handler for the pollTimer's Tick event. It calls pollJointData().
        /// </summary>
        /// <param name="state"></param>
        /// <author>Arno Sluismans</author>
        private void pollTimerTick(object state)
        {
            if (Polling)
            {
                // If we are still supposed to be polling for data, 
                // this is the moment to poll for it.
                pollJointData();
            }
            else
            {
                // If we're not supposed to be polling for data, we stop the
                // pollTimer's ticking as it has become useless.
                pollTimer.Change(System.Threading.Timeout.Infinite, Interval);
            }
        }
        /// <summary>
        /// Sets Polling to true, and (re)starts the pollTimer. If needed, it also creates
        /// a new Timer object to serve as pollTimer.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public void startPolling()
        {
            // Tell the application that we are actively polling for data.
            Polling = true;
            // If the timer has not been initialised, we initialise it.
            if (pollTimer == null)
            {
                // Instantiate the timer
                pollTimer = new Timer(pollTimerTick, null, 0, Interval);
            }
        }

        /// <summary>
        /// Calls the C++ function that constructs a CyberGlove. It stores the pointer in 
        /// vhtHand, but please be aware that this pointer can not be used as such inside C# code.
        /// </summary>
        /// <author>Arno Sluismans</author>
        private void initVhtHand()
        {
            // Call the C++ function that creates a CyberHand and returns the object as a pointer.
            vhtHand = CreateHand();
        }

        /// <summary>
        /// Calls the C++ function that deletes the CyberHand object.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public void delete()
        {
            // Delete the hand.
            DeleteHand(vhtHand);
            // Set our local hand variable to null.
            vhtHand = IntPtr.Zero;
        }
        #region Marshalled functions

        /// <summary>
        /// Creates a new CyberHand object.
        /// </summary>
        /// <returns>Returns a pointer to the newly created object.</returns>
        /// <author>Arno Sluismans</author>
        [DllImport("VirtualHandBridge.dll")]
        static public extern IntPtr CreateHand();

        /// <summary>
        /// Deletes the CyberHand object.
        /// </summary>
        /// <param name="hand">The pointer to the object we need to dispose of.</param>
        /// <author>Arno Sluismans</author>
        [DllImport("VirtualHandBridge.dll")]
        static public extern void DeleteHand(IntPtr hand);

        /// <summary>
        /// Polls the hand for its current data.
        /// </summary>
        /// <param name="hand">The CyberHand::Hand object.</param>
        /// <param name="buffer">The array of doubles that we will want to fill with data.</param>
        /// <param name="bufferSize">The length of the buffer. Normally this should be 15.</param>
        /// <returns>The new length of the buffer array. Normally this should be 15.</returns>
        [DllImport("VirtualHandBridge.dll")]
        static public extern int Poll(IntPtr hand, double[] buffer, int bufferSize);

        /// <summary>
        /// Makes the hand vibrate.
        /// </summary>
        /// <param name="hand">The hand object.</param>
        /// <param name="vibration">How hard it needs to vibrate.</param>
        [DllImport("VirtualHandBridge.dll")]
        static public extern void VibrateAll(IntPtr hand, double vibration);
        #endregion
    }
}
