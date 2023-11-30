# BellGuardian

BellGuardian is an open-source implementation of BellMinder, a school bell system created by Techenz. This repo is a work in progress.

## Why?

The original software is 20 years old and doesn't run on Windows 11 without a lot of messing around due to it being written in Visual C++ 5 or 6. Some schools still use the bell system, so an open-source implementation of the software is pretty much a necessity at this point.

## What's left to do?

- Implement a UI
  - Way to set term times (e.g. pick start date, pick end date, click "Add", repeat)
  - Import dates and times from CSV, JSON etc.
- Test on actual hardware and make sure all features are 1:1
 
## Contributing

There's a few ways you can help:

- Pull Requests to add new features and fix bugs
- Create issues for feature requests or bugs you come across
- Test this on actual hardware. We've got three controllers boxes, but only cables for two of them, meaning I can only do non-disruptive testing outside of work hours.
  - Load up a schedule and send the plan to the controller, then do the same using the official software and compare the results using serial port capture software.
- Let me know if you use this product and what your experiences with it are.
 
## Building

BellGuardian was made using Visual Studio 2022, so build with that. 

## Learn more about BellMinder

Check the [Wiki](https://github.com/graydav1/BellGuardian/wiki) for all the information I have about the BellMinder software and hardware.
