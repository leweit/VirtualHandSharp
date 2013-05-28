#pragma once
#include<string>
#include<vhtHumanHand.h>
#include<vhtCyberGlove.h>
#include<vhtTracker.h>
#include<vhtCyberTouch.h>
#include"Finger.h"


namespace CyberHand 
{
	class Hand
	{
	public:
		// Default constructor
		Hand(void);
		// Default destructor
		~Hand(void);
		// Initialises the hand's connection to a vhtHumanHand.
		void init();
		// Asks the vhtHumanHand for its current data, and saves it in the fingers array.
		void updateData();
		// Returns a list of joint values. 0 = Thumb inner, 1 = thumb middle, 2 = thumb outer, 3 = index inner,...
		double* getJoints(); 
	private:
		// The vhtHumanHand object that we will be connected to.
		vhtHumanHand* hand;
		// The glove used in vhtHumanHand.
		vhtCyberGlove *glove;
		// The tracker used in vhtHumanHand.
		vhtTracker *tracker;
		// The array of fingers, thumb indexed.
		Finger* fingers[5];
		// Remaining data here.
		double wristPitch;
		double wristYaw;
		// The number of fingers; presumably 5 unless you are Mickey Mouse.
		static const int NR_FINGERS;
		// The number of joints per finger; presumably 3.
		static const int NR_JOINTS;
	};
}
