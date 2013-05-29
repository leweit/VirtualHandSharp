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
// VirtualHandBridge.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include"Hand.h"
#include<string>
#include<comutil.h>
#include "VirtualHandBridge.h"
#include <iostream>
#include<vhtBadLogic.h>

// Creates a new hand and returns a pointer to it.
HAND_EXT CyberHand::Hand* __stdcall CreateHand()
{
	try
	{
		// Instantiate a hand
		CyberHand::Hand* rv = new CyberHand::Hand(); // return value
		// Initialise the hand's properties (connection and whatsoever)
		rv->init();
		// return the pointer to the new hand.
		return rv;
	}
	catch (vhtBadLogicExcp* e)
	{
		std::cerr << "[vhtBadLogicExcp]: " << e->getMessage() << std::endl;
		delete e;
		return NULL;
	}
}

// Deletes a hand object.
HAND_EXT void __stdcall DeleteHand(CyberHand::Hand* hand)
{
	// We can only delete it if it's not null.
	if (hand != NULL)
	{
		// Delete it.
		delete hand;
		// Set the pointer to null.
		hand = NULL;
	}
}

// Populates an array of doubles with data returned by the hand.
HAND_EXT size_t __stdcall Poll(CyberHand::Hand* hand, double* buffer, size_t buffersize)
{
	try
	{
		// Ask the hand to update its data.
		hand->updateData();
		// Get the array of data from the hand.
		double* arr = hand->getJoints();
		// Copy the array over into the pre-allocated buffer (sent to us as an argument).
		for (size_t i=0;i<buffersize;i++)
		{
	#ifdef ENABLE_DEBUG_OUTPUT
			std::cout << arr[i] << ", ";
	#endif
			buffer[i] = arr[i];
		}
	
	#ifdef ENABLE_DEBUG_OUTPUT
		std::cout << "\n";
	#endif

		// Delete the array we received from the hand.
		delete[] arr;
		// Return the size of the new array.

		return buffersize;
	} catch(vhtBadLogicExcp* e) {
		std::cerr << "[vhtBadLogicExcp]: " << e->getMessage() << std::endl;
		delete e;
		return 0;
	}
}

// Whether we are currently debugging. To change this, edit the ApiFunctions.h file.
HAND_EXT bool __stdcall Debugging()
{
#ifdef ENABLE_DEBUG_OUTPUT // This is defined (or not defined) in VirtualHandBridge.h
	return true;
#else
	return false;
#endif
}

// Makes all vibrators in the glove vibrate.
HAND_EXT void __stdcall VibrateAll(CyberHand::Hand* hand, double vibration)
{
	// Not implemented yet.
}