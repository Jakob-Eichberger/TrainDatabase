# TrainDatabase
TrainDatabase is a software that lets you control your model trains with your PC. Whether you want to run a single train or a whole network, TrainDatabase gives you an easy and intuitive way to do it. With TrainDatabase, you can:

- Control your model trains with your PC using TrainDatabase
- Run a single train or a whole network with ease and flexibility
- Enjoy realistic and immersive simulation of train operations and scenarios
- Support double traction for more power and speed
- Import a z21 Database for seamless integration with your existing Roco/Fleischman system

*Note: TrainDatabase and its documentation is written in German. An English language package is **not** planned.*

## Getting Started

### Installing the software

1. Make sure that you have a z21/Z21 from Roco/Fleischmann. (No other digital control center is currently supported)
2. Go to this [link](https://github.com/Jakob-Eichberger/TrainDatabase/releases) to download the installer.
3. Install the software using the provided installer!
4. When the app starts for the first time you get the option to import your Z21 layout (from the new Z21 Android/IOS App).

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
Joystick support|Not done
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
