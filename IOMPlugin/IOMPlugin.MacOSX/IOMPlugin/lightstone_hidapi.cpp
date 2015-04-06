/***
 * @file lightstone_libusb1.c
 * @brief LibUSB based implementation of lightstone communication
 * @author Kyle Machulis (kyle@nonpolynomial.com)
 * @copyright (c) 2006-2011 Nonpolynomial Labs/Kyle Machulis
 * @license BSD License
 *
 * Project info at http://liblightstone.nonpolynomial.com/
 *
 */


#include <stdlib.h>
#include "lightstone.h"


LIGHTSTONE_DECLSPEC lightstone* lightstone_create()
{
	lightstone* s = (lightstone*)malloc(sizeof(lightstone));
    s->_is_open = 0;
    s->_is_inited = 0;
    if(hid_init() < 0)
    {
        return NULL;
    }
    s->_is_inited = 1;	
	return s;
}

LIGHTSTONE_DECLSPEC int lightstone_get_count(lightstone* s)
{
    // Enumerate the HID devices on the system
    struct hid_device_info *devs, *cur_dev;
    int count = 0;
    
    int j = 0;
    for(j = 0; j < LIGHTSTONE_VID_PID_PAIRS_COUNT; ++j)
    {
        devs = hid_enumerate(lightstone_vid_pid_pairs[j][0], lightstone_vid_pid_pairs[j][1]);
        cur_dev = devs;
        while (cur_dev) {
            ++count;
            cur_dev = cur_dev->next;
        }
        hid_free_enumeration(devs);
    }
	return count;
}

LIGHTSTONE_DECLSPEC int lightstone_open(lightstone* s, unsigned int device_index)
{
    // Open the HID device that has the index when enumerating
    struct hid_device_info *devs, *cur_dev;
    int count = 0;
    
    int j = 0;
    for(j = 0; j < LIGHTSTONE_VID_PID_PAIRS_COUNT; ++j)
    {
        devs = hid_enumerate(lightstone_vid_pid_pairs[j][0], lightstone_vid_pid_pairs[j][1]);
        cur_dev = devs;
        while (cur_dev) {
            if (count == device_index) {
                s->_device = hid_open_path(cur_dev->path);
                hid_free_enumeration(devs);
                if (s->_device == NULL) {
                    return E_LIGHTSTONE_NOT_OPENED;
                }
                s->_is_open = 1;
                // Set the hid_read() function to be non-blocking.
                //hid_set_nonblocking(s->_device, 1);
                return 0;
            }
            ++count;
            cur_dev = cur_dev->next;
        }
        hid_free_enumeration(devs);
    }
    return E_LIGHTSTONE_NOT_INITED;
}

LIGHTSTONE_DECLSPEC int lightstone_close(lightstone* s)
{
	if(!s->_is_open)
	{
		return E_LIGHTSTONE_NOT_OPENED;
	}
	hid_close(s->_device);
	s->_is_open = 0;
	return 0;
}

LIGHTSTONE_DECLSPEC void lightstone_delete(lightstone* dev)
{
    lightstone_close(dev);
	free(dev);
    hid_exit(); // maybe prematurely, but better no memory leaks in unity...
}

int lightstone_read(lightstone* dev, unsigned char* input_report)
{
    return hid_read_timeout(dev->_device, input_report, 8, 0x10);
}
