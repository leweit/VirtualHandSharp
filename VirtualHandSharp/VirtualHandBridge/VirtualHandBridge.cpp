// VirtualHandBridge.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include"Hand.h"
#include<string>
#include<comutil.h>
#include "VirtualHandBridge.h"
#include <iostream>

// Creates a new hand and returns a pointer to it.
HAND_EXT CyberHand::Hand* __stdcall CreateHand()
{
	// Instantiate a hand
	CyberHand::Hand* rv = new CyberHand::Hand(); // return value
	// Initialise the hand's properties (connection and whatsoever)
	rv->init();
	// return the pointer to the new hand.
	return rv;
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
	/*
	if (0 >= vibration && vibration <= 1)
		hand->vibrate(vibration);
	else
		return;
	*/
}