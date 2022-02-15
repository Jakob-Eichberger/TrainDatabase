# TrainDatabase
TrainDatabase is software for managing and controlling your model trains. The software was created because no pc software currently exists that provides a modern, simple and intuitive interface for controlling your trains. 
TrainDatabase fully supports multi traction. 

The features of this software are as followed: 
- Controlling your trains with either a mouse, hotkeys, or throttle.
- Fully supports double traction for control cars, unmeasured and measured locomotives. 
- Measuring the vehicle's speed using a raspberry pi. 
- Database can be imported from the new Roco Z21 app. 

*Note: TrainDatabase and its documentation is written in German. An English language package is **not** planned.*

## Getting Started

### Installing the software

1. Make sure that you have a z21/Z21 from Roco/Fleischmann. (No other digital control center is currently supported)
2. Go to this [link](https://github.com/Jakob-Eichberger/TrainDatabase/releases) to download the software.
3. Unpack the .zip file, copy the folder to its final directory and start the TrainDB.exe. (*Note: A wix based installer is planned*)
4. When it's your first time starting the app you get the option to import your Z21 Layout (from the new Z21 Android/IOS App).

### Import existing z21 Layout. 
1. Open the Roco/Fleischmann Z21 App.
2. Go to "Layouts"
3. Select the layout you want to export. 
4. Scroll down and click "Export". 
5. Select the "Share" option. 
6. Save the file to your device or send it to yourself via Email.
7. In the TrainDatabase go to "Database".
8. Click "Neue Datenbank importieren".
9. Select the .z21 File.
10. Click "Jetzt importieren".
11. The Software imports the database, and it will show a "Import erfolgreich" dialog, if the import was successful.

## Examples of use

### Main window
In the main window you can manage your vehicles and search for them:

**Note:** In the search bar you can search for any attribute that the vehicle might have. (Like name, railway transport company, epoch, etc)

![image](https://user-images.githubusercontent.com/53713395/130352358-c94851f8-9904-4193-a374-727b4c68bfb4.png)

### Vehicle edit window

In the edit window, the vehicle and its functions can be changed.

**Note:** The shown fields are not yet final. 

![image](https://user-images.githubusercontent.com/53713395/140822639-1f07bcd9-de62-45f9-afd3-61c8f13acb3f.png)
![image](https://user-images.githubusercontent.com/53713395/140822682-2b9de754-85eb-48f0-8cbd-5c29560fdb69.png)
![image](https://user-images.githubusercontent.com/53713395/140822719-24a59654-d83a-4c33-ab6a-9fa798324ce1.png)
![image](https://user-images.githubusercontent.com/53713395/140822897-d496ea8d-cfb1-48f3-9ab0-40a8a7ab9b6b.png)

### Vehicle control window

In the control window the speed, direction of travel, and functions of a vehicle can be controlled.

![image](https://user-images.githubusercontent.com/53713395/140822127-be8e3e04-c2da-49e1-aedb-21cd6444b2fe.png)

### Speeed measurement window

TrainDatabase (with the help of a [raspberry pi](https://www.raspberrypi.org/products/raspberry-pi-3-model-b/) and two [infrared sensors](https://amazon.de/gp/product/B07D924JHT)) allows you to measure the speed of your vehicle so that it can be used in trains with multiple locomotives.

![image](https://user-images.githubusercontent.com/53713395/130366046-f6c6b504-1d95-458e-a21c-57f4ed6ee224.png)

## Project Status

Feature Name | Status
------------ | -------------
Import database from Roco Z21 App|Complete
Manage vehicles|Complete
Control vehicle speed|Complete
Control vehicle functions|Complete
Multi traction support|In development
Joystick support|In development
Hotykey support|Not done

## Sources
TBD

## FAQ
TBD

## Technologies

- .Net 5.0 WPF
- EF Core 5.0.0
- OxyPlot.WPF 2.0.0
- SharpDX 4.2.0
