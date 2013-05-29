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
namespace CyberHand
{
	class Finger
	{
	public:
		// Default constructor
		Finger(void);
		// Default destructor
		~Finger(void);
		// Sets the joints to the new values. (inner joint, middle joint, outer joint)
		void setJoints(double i, double m, double o);
		// Gets the inner joint's value.
		double getInner() {return inner;}
		// Gets the middle joint's value.
		double getMiddle() {return middle;}
		// Gets the outer joint's value.
		double getOuter() {return outer;}
		// Gets the abduction
		double getAbduction() {return abduction;}
		// Sets the abduction
		void setAbduction(double v) {abduction = v;}
	private:
		double inner;
		double middle;
		double outer;
		// This is the angle between this finger and the finger to the right 
		// (on your right hand, back of the hand facing you)
		double abduction;
	};

}