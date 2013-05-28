#include "StdAfx.h"
#include "Finger.h"

namespace CyberHand
{
	// Default constructor
	Finger::Finger(void)
	{
		// Initialise variables
		inner = 0;
		middle = 0;
		outer = 0;
		abduction = 0;
	}


	Finger::~Finger(void)
	{
	}
	
	void Finger::setJoints(double i, double m, double o)
	{
		inner = i;
		middle = m;
		outer = o;
	}
}