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
#include "StdAfx.h"
#include "Hand.h"
#include <string>
#include <iostream>
#include <sstream>
#include <vhtIOConn.h>
#include <vhtTracker.h>
#include <vhtCyberGlove.h>
#include <vht6DofDevice.h>
#include <vhtHandMaster.h>
#include <vhtHumanHand.h>
#include <vhtBadLogic.h>
#include <vhtTrackerEmulator.h>
#include <vhtTransform3D.h>
#include <vhtVector3d.h>
#include <vhtQuaternion.h>
#include <vhtGenHandModel.h>
#include "VirtualHandBridge.h"

// Turn off if you want a tracker emulator
// #define USE_REAL_TRACKER


namespace CyberHand 
{
	using namespace std;
	int const Hand::NR_FINGERS = 5;
	int const Hand::NR_JOINTS = 4;
	Hand::Hand(void)
	{
		// Instantiate 5 fingers and put them in the array of fingers.
		for (int i=0;i<5;i++) {
			fingers[i] = new Finger();
		}
		// Make sure there are no dangling pointers.
		hand = NULL;
		glove = NULL;
		tracker = NULL;
		wristPitch = 0;
		wristYaw = 0;
	}

	Hand::~Hand(void)
	{
		try 
		{
			// Delete the hand, tracker, glove and fingers.
			hand->disconnect();
			glove->disconnect();
			tracker->disconnect();
			for (int i=0;i<5;i++)
			{
				delete fingers[i];
			}
		}
		catch (vhtBadLogicExcp* e) 
		{
			// If something goes wrong, we print that error to the console.
			std::cerr << "[vhtBadLogicExcp]: " << e->getMessage() << std::endl;
			delete e;
		}
	}

	void Hand::init()
	{
		try 
		{
			// Get a connection to the default glove defice.
			vhtIOConn *ioconn = vhtIOConn::getDefault(vhtIOConn::glove);
			// Get a glove using this connection.
			this->glove = new vhtCyberGlove(ioconn);
			// We happen to not have a tracker device available, so we emulate one.
			// If you have this device, you can use the following line instead:
			// this->tracker = new vhtTracker(vhtIOConn::getDefault(vhtIOConn::tracker);
			this->tracker = new vhtTrackerEmulator();
			// Using this tracker, we create a vht6DofDevice.
			vht6DofDevice *rcvr = tracker->getLogicalDevice(0);
			// Using the 6Dof and the glove, we can create a HandMaster...
			vhtHandMaster *master = new vhtHandMaster(glove, rcvr);

			// And last but not least, we create the HumanHand object
			// that offers easy access to all data.
			this->hand = new vhtHumanHand(master);

			// Update the data for a first time.
			updateData();
		} 
		catch (vhtBadLogicExcp* e) 
		{
			// If something goes wrong, we print that error to the console.
#ifdef ENABLE_DEBUG_OUTPUT
			std::cerr << "[vhtBadLogicExcp]: " << e->getMessage() << std::endl;
#else
#endif
			throw e;
		}
	}

	void Hand::updateData()
	{
		// If the hand is not initialised, we can not update the data.
		if (this->hand == NULL) 
			return;

		// Ask the hand to update
		hand->update();
		// Go through the array of fingers and populate them with corresponding data.
		for (int i = 0;i<GHM::nbrFingers; i++)
		{
			// Populate the finger's data with the glove's joint angles.
			this->fingers[i]->setJoints(
					glove->getAngle((GHM::Fingers)i, (GHM::Joints)0),
					glove->getAngle((GHM::Fingers)i, (GHM::Joints)1),
					glove->getAngle((GHM::Fingers)i, (GHM::Joints)2)
				);
		}
		fingers[0]->setAbduction(glove->getData(3));
		fingers[1]->setAbduction(glove->getData(11));
		fingers[2]->setAbduction(glove->getData(15));
		fingers[3]->setAbduction(glove->getData(19));
		fingers[4]->setAbduction(glove->getData(20));
		wristPitch = glove->getData(21);
		wristYaw = glove->getData(22);
	}

	double* Hand::getJoints()
	{
		// Thumb indexed, inner joint indexed.
		int max = NR_FINGERS * (NR_JOINTS) + 2;
		double* rv = new double[max]; // Return value
		// Go through the fingers array and get all the joints' data.
		for (int i=0;i<NR_FINGERS;i++)
		{
			// Possibly print debug output
#ifdef ENABLE_DEBUG_OUTPUT
			cout << "Starting index: " << i*NR_JOINTS << "\n";
#endif
			// Add the data to the array
			rv[i*NR_JOINTS+0] = fingers[i]->getInner();
			rv[i*NR_JOINTS+1] = fingers[i]->getMiddle();
			rv[i*NR_JOINTS+2] = fingers[i]->getOuter();
			rv[i*NR_JOINTS+3] = fingers[i]->getAbduction();
		}
		rv[max-2] = wristPitch;
		rv[max-1] = wristYaw;
		return rv;
	}
}