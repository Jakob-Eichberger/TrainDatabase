using Helper;
using Model;
using ModelTrainController.Z21;
using System;
using System.Net;
using System.Net.Sockets;

namespace ModelTrainController
{
    public abstract class ModelTrainController : UdpClient
    {
        public ModelTrainController(StartData startData) : base(startData.LanPort)
        {
            lanAdresse = IPAddress.Parse(startData.LanAdresse);
            lanAdresseS = startData.LanAdresse;
            lanPort = startData.LanPort;
            Connect(lanAdresse, lanPort);
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
        public abstract event EventHandler OnTrackPowerOFF;                                  //  40 61 LAN X BC TRACK POWER OFF 2.7 (12) 
        public abstract event EventHandler OnTrackPowerON;                                   //  40 61 LAN X BC TRACK POWER ON  2.8 (12) 
        public abstract event EventHandler OnProgrammingMode;                                //  40 61 LAN X BC PROGRAMMING MODE 2.9 (12) 
        public abstract event EventHandler OnTrackShortCircuit;                              //  40 61 LAN X BC TRACK SHORT CIRCUIT 2.10 (12) 
        public abstract event EventHandler<StateEventArgs> OnStatusChanged;                  //  40 62 LAN X STATUS CHANGED 2.12 (13)
        public abstract event EventHandler OnStopped;                                        //  40 81 LAN X BC STOPPED 2.14 (14)
        public abstract event EventHandler<FirmwareVersionInfoEventArgs> OnGetFirmwareVersion;// 40 F3 LAN X GET FIRMWARE VERSION 2.15 (xx)
        public abstract event EventHandler<SystemStateEventArgs> OnSystemStateDataChanged;   //  84    LAN SYSTEMSTATE_DATACHANGED 2.18 (16) 
        public abstract event EventHandler<HardwareInfoEventArgs> OnGetHardwareInfo;         //  1A    LAN GET HWINFO
        public abstract event EventHandler<GetLocoInfoEventArgs> OnGetLocoInfo;              //  40 EF LAN X LOCO INFO   4.4 (22)
        public abstract event EventHandler<TrackPowerEventArgs> OnTrackPower;                //  ist Zusammenfassung von 

        public abstract void GetSerialNumber();

        //  LAN_X_GET_VERSION     // 2.3 (10)
        public abstract void GetVersion();

        //  LAN_X_GET_STATUS     // 2.4 (11)
        public abstract void GetStatus();

        //  LAN_X_SET_TRACK_POWER_OFF   // 2.5 (11)
        public abstract void SetTrackPowerOFF();

        //  LAN_X_SET_TRACK_POWER_ON   // 2.6 (11)
        public abstract void SetTrackPowerON();

        //  LAN_X_SET_STOP   // 2.13 (14)
        public abstract void SetStop();

        //  LAN_X_GET_FIRMWARE_VERSION   // 2.15 (xx)
        public abstract void GetFirmwareVersion();

        //  LAN_SET_BROADCASTFLAGS()    // 2.16 (15)
        public abstract void SetBroadcastFlags();

        //  LAN_SYSTEMSTATE_GETDATA()     // 2.19 (17)
        public abstract void SystemStateGetData();

        //  LAN_GET_HWINFO   // 2.20 (xx)
        public abstract void GetHardwareInfo();

        //  LAN X GET LOCO INFO         // 4.1 (20)
        public abstract void GetLocoInfo(LokAdresse adresse);
        //  LAN_X_SET_LOCO_DRIVE  4.2  (21)
        public abstract void SetLocoDrive(LokInfoData data);
        public abstract void SetLocoFunction(LokAdresse adresse, Function function, ToggleType toggelType);

        public abstract void Nothalt();
        //  LAN_LOGOFF            2.2 (10)         
        public abstract void LogOFF();

        public abstract void Reconnect();

        public abstract new void Dispose();
    }
}
