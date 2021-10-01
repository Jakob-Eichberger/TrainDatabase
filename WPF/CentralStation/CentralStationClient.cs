using Helper;
using Model;
using ModelTrainController.Z21;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using WPF_Application.CentralStation;

namespace ModelTrainController
{
    public abstract class CentralStationClient : UdpClient
    {
        public const int maxDccStep = 127;

        public CentralStationClient(IPAddress address, int port) : base(port)
        {
            DontFragment = false;
            lanAdresse = address;
            lanAdresseS = address.ToString();
            lanPort = port;
            Connect(address, lanPort);
            DontFragment = false;
            EnableBroadcast = false;
        }

        internal abstract void Empfang(IAsyncResult res);
        
        internal abstract void EndConnect(IAsyncResult res);

        internal IPAddress lanAdresse;
        
        internal string lanAdresseS;
        
        internal string LanAdresse
        {
            get
            {
                return lanAdresseS;
            }
        }

        internal int lanPort;
        internal int LanPort
        {
            get
            {
                return lanPort;
            }
        }

        public abstract event EventHandler<DataEventArgs> OnReceive;                         //  Allgemeiner Empfang von Daten
        public abstract event EventHandler<GetSerialNumberEventArgs> OnGetSerialNumber;      //  10    LAN GET SERIAL NUMBER  2.1 (10)  
        public abstract event EventHandler<VersionInfoEventArgs> OnGetVersion;               //  40 21 LAN X GET VERSION  2.3 (xx)
        public abstract event EventHandler<TrackPowerEventArgs> TrackPowerChanged;
        public abstract event EventHandler<StateEventArgs> OnStatusChanged;                  //  40 62 LAN X STATUS CHANGED 2.12 (13)
        public abstract event EventHandler OnStopped;                                        //  40 81 LAN X BC STOPPED 2.14 (14)
        public abstract event EventHandler<FirmwareVersionInfoEventArgs> OnGetFirmwareVersion;// 40 F3 LAN X GET FIRMWARE VERSION 2.15 (xx)
        public abstract event EventHandler<SystemStateEventArgs> OnSystemStateDataChanged;   //  84    LAN SYSTEMSTATE_DATACHANGED 2.18 (16) 
        public abstract event EventHandler<HardwareInfoEventArgs> OnGetHardwareInfo;         //  1A    LAN GET HWINFO
        public abstract event EventHandler<GetLocoInfoEventArgs> OnGetLocoInfo;              //  40 EF LAN X LOCO INFO   4.4 (22)

        public abstract void GetSerialNumber();

        public abstract void GetVersion();

        public abstract void GetStatus();

        public abstract void SetTrackPowerOFF();

        public abstract void SetTrackPowerON();

        public abstract void SetStop();

        public abstract void GetFirmwareVersion();

        public abstract void LogOn();

        public abstract void SystemStateGetData();

        public abstract void GetHardwareInfo();

        public abstract void GetLocoInfo(LokAdresse adresse);
      
        public abstract void SetLocoDrive(LokInfoData data);

        public abstract void SetLocoDrive(List<LokInfoData> data);

        public abstract void SetLocoFunction(LokAdresse adresse, Function function, ToggleType toggelType);

        public abstract void SetLocoFunction(List<(ToggleType toggle, Function Func)> list);

        public abstract void LogOFF();

        public abstract new void Dispose();

    }
}
