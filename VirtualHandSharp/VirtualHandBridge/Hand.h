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
