// IOMPlugin.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include "lightstone.h"

#if _MSC_VER // this is defined when compiling with Visual Studio
#define EXPORT_API __declspec(dllexport) // Visual Studio needs annotating exported functions with this
#else
#define EXPORT_API // XCode does not need annotating exported functions, so define is empty
#endif

// ------------------------------------------------------------------------
// Plugin itself

// global variable:
lightstone* iom_dev;

// Link following functions C-style (required for plugins to be called from Unity)
extern "C"
{
	int EXPORT_API iom_setup() {
		int count;
		int ret;
		iom_dev = lightstone_create();
		count = lightstone_get_count(iom_dev);
		if (count > 0) {
			ret = lightstone_open(iom_dev, 0);
			if (ret < 0) {
				return ret; // open failed
			}
		}
		return count; // opened successfully
	}

	int EXPORT_API iom_close() {
		if (iom_dev == NULL) {
			return -1;
		}
		lightstone_delete(iom_dev);
		iom_dev = NULL;
		return 1;
	}

	lightstone_info EXPORT_API iom_get_hrvscl() { // both hrv and scl
		lightstone_info bad;
		bad.hrv = -1;
		bad.scl = -1;
		if (iom_dev == NULL) {
			return bad;
		}
		return lightstone_get_info(iom_dev);
	}

} // end of export C block
