# TrainDatabase
With this software, you can control your entire model train layout comfortably from your windows computer!



## Tables of Contents
- [General Information](#general-information)
- [Features](#features)
- [Examples of use](#examples-of-use)
- [Getting Started](#getting-started)
- [Project Status](#project-status)
- [Sources](#sources)
- [FAQ](#faq)
- [Technologies](#technologies)

## General Information
This software was created to give the user a simple interface from where his/her model trains can be controlled. The program offers the ability to not only control trains and their functions but also to control switches and signals, it even supports multi traction. For people that want a bit more feedback when controlling their trains, this software allows the user to control their trains with a Joystick/Trottel. 

*Note: The software itself is in German. An English language package is planned.*

## Features
- Manage and control vehicles and functions.
- Manage and control your layout. (Switches, signals, etc)
- Usage of Joystick to control vehicle speed, direction, and functions.
- Multitraction support
- Support to measure the vehicle speed with a raspberry pi.
- Import a layout from your (new) Z21 smartphone app.


## Examples of use

#### Main window

This is the main window of the application where you can manage your vehicles and search for them: 

**Note:** In the search bar you can search for any attribute that the vehicle might have. (Like name, railway transport company, epoch, etc)

![image](https://user-images.githubusercontent.com/53713395/130352358-c94851f8-9904-4193-a374-727b4c68bfb4.png)

#### Vehicle edit window

In this window, the vehicle can be edited. 

**Note:** The shown fields are not yet final. 

![image](https://user-images.githubusercontent.com/53713395/130353666-2aa8d178-da2f-47da-a955-128ddf3118be.png)

#### Vehicle control window

In this window, the speed, direction of travel, and functions can be controlled.

![image](https://user-images.githubusercontent.com/53713395/130352398-85260549-59de-4edd-8550-6c56cf23b666.png)

#### Speeed measurement window

This software (with the help of a [raspberry pi](https://www.raspberrypi.org/products/raspberry-pi-3-model-b/) and two [infrared sensors](https://amazon.de/gp/product/B07D924JHT)) allows you to measure the speed of your vehicle so that it can be used in trains with multiple locomotives.

![image](https://user-images.githubusercontent.com/53713395/130352710-73c0cc13-ab67-46e2-94ec-f4809c4c2db6.png)


## Getting Started
Getting started is simple. Just follow these steps:

1. Make sure that you have a Z21 from Roco/Fleischmann. (No other controller is currently supported)
2. Go to this [link](https://github.com/Jakob-Eichberger/TrainDatabase/releases) to download the software.
3. Unpack the .zip file, copy the folder to its final directory and start the TrainDB.exe.
4. When it's your first time starting the app you get the option to import your Z21 Layout (from the new Z21 Android/IOS App).
5. Done!

## Project Status

Feature Name | Status
------------ | -------------
Manage vehicles|Complete
Joystick support|Complete
Control vehicle speed|Complete
Control vehicle functions | Complete
Multi traction support | In development
CV Programming | Not done
Controlling Switches/Signals|Not done

## Sources
TBD

## FAQ
TBD

## Technologies

- .Net 5.0 WPF
- EF Core 5.0.0
- OxyPlot.WPF 2.0.0
- SharpDX 4.2.0
