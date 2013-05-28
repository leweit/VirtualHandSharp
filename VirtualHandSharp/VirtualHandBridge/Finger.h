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