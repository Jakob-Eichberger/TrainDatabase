using Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using WPF_Application.CentralStation.DTO;
using WPF_Application.CentralStation.Enum;
using WPF_Application.CentralStation.Events;

namespace WPF_Application.CentralStation
{
    public abstract class CentralStationClient : UdpClient
    {
        public const int maxDccStep = 127;

        internal IPAddress lanAdresse;

        internal string lanAdresseS;

        internal int lanPort;

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

        public abstract event EventHandler<FirmwareVersionInfoEventArgs> OnGetFirmwareVersion;

        public abstract event EventHandler<HardwareInfoEventArgs> OnGetHardwareInfo;

        public abstract event EventHandler<GetLocoInfoEventArgs> OnGetLocoInfo;

        public abstract event EventHandler<GetSerialNumberEventArgs> OnGetSerialNumber;

        public abstract event EventHandler<VersionInfoEventArgs> OnGetVersion;

        public abstract event EventHandler<DataEventArgs> OnReceive;

        public abstract event EventHandler<StateEventArgs> OnStatusChanged;

        public abstract event EventHandler OnStopped;

        public abstract event EventHandler<SystemStateEventArgs> OnSystemStateDataChanged;

        public abstract event EventHandler<TrackPowerEventArgs> TrackPowerChanged;

        internal string LanAdresse
        {
            get
            {
                return lanAdresseS;
            }
        }

        internal int LanPort
        {
            get
            {
                return lanPort;
            }
        }

        public abstract new void Dispose();

        public abstract void GetFirmwareVersion();

        public abstract void GetHardwareInfo();

        public abstract void GetLocoInfo(LokAdresse adresse);

        public abstract void GetSerialNumber();

        public abstract void GetStatus();

        public abstract void GetVersion();

        public abstract void LogOFF();

        public abstract void LogOn();

        public abstract void SetLocoDrive(LokInfoData data);

        public abstract void SetLocoDrive(List<LokInfoData> data);

        public abstract void SetLocoFunction(LokAdresse adresse, Function function, ToggleType toggelType);

        public abstract void SetLocoFunction(List<(ToggleType toggle, Function Func)> list);

        public abstract void SetStop();

        public abstract void SetTrackPowerOFF();

        public abstract void SetTrackPowerON();

        public abstract void SystemStateGetData();

        internal abstract void Empfang(IAsyncResult res);

        internal abstract void EndConnect(IAsyncResult res);
    }
}
