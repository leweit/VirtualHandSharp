#define VISUALHANDBRIDGE_EXPORTS

#ifdef VISUALHANDBRIDGE_EXPORTS
#define VISUALHAND_API __declspec(dllexport) 
#else
#define VISUALHAND_API __declspec(dllimport) 
#endif

#ifndef HAND_EXT
#define HAND_EXT extern "C" VISUALHAND_API
#endif

// Disable if not debugging.
//#define ENABLE_DEBUG_OUTPUT
