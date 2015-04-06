This project can build a DLL for windows that reads from a WildDiving IOM device.

The library exposes low-level functions to interact with the device.

Higher level code is needed to get usable data from the readings.

IOMPlugin.MacOSX contains an Xcode project for building a corresponding Max OS X bundle that is based on the hidapi library.

TODO:

* Code should be shared between the Mac OS and the Windows version to avoid the current code duplication.
* Build the libusb1 based version for Linux. This does not work on Mac OS, as the device reports as a HID device and cannot be used in a pure libusb way. However, that version should be usable on Linux.
