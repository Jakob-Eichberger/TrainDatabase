/* Z21 - C#-Implementierung des Protokolls der Kommunikation mit der digitalen
 * Steuerzentrale Z21 oder z21 von Fleischmann/Roco
 * ---------------------------------------------------------------------------
 * Datei:     z21.cs
 * Version:   16.06.2014
 * Besitzer:  Mathias Rentsch (rentsch@online.de)
 * Lizenz:    GPL
 *
 * Die Anwendung und die Quelltextdateien sind freie Software und stehen unter der
 * GNU General Public License. Der Originaltext dieser Lizenz kann eingesehen werden
 * unter http://www.gnu.org/licenses/gpl.html.
 * 
 */

using Model;
using ModelTrainController;
using ModelTrainController.Z21;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace Helper
{
    public class Z21 : ModelTrainController.ModelTrainController
    {
        public Z21(StartData startData) : base(startData)
        {

            BeginReceive(new AsyncCallback(Empfang), null);
            Console.WriteLine("Z21 initialisiert.");
        }

        public override event EventHandler<DataEventArgs> OnReceive;                         //  Allgemeiner Empfang von Daten
        public override event EventHandler<GetSerialNumberEventArgs> OnGetSerialNumber;      //  10    LAN GET SERIAL NUMBER  2.1 (10)  
        public override event EventHandler<VersionInfoEventArgs> OnGetVersion;               //  40 21 LAN X GET VERSION  2.3 (xx)
        public override event EventHandler OnTrackPowerOFF;                                  //  40 61 LAN X BC TRACK POWER OFF 2.7 (12) 
        public override event EventHandler OnTrackPowerON;                                   //  40 61 LAN X BC TRACK POWER ON  2.8 (12) 
        public override event EventHandler OnProgrammingMode;                                //  40 61 LAN X BC PROGRAMMING MODE 2.9 (12) 
        public override event EventHandler OnTrackShortCircuit;                              //  40 61 LAN X BC TRACK SHORT CIRCUIT 2.10 (12) 
        public override event EventHandler<StateEventArgs> OnStatusChanged;                  //  40 62 LAN X STATUS CHANGED 2.12 (13)
        public override event EventHandler OnStopped;                                        //  40 81 LAN X BC STOPPED 2.14 (14)
        public override event EventHandler<FirmwareVersionInfoEventArgs> OnGetFirmwareVersion;// 40 F3 LAN X GET FIRMWARE VERSION 2.15 (xx)
        public override event EventHandler<SystemStateEventArgs> OnSystemStateDataChanged;   //  84    LAN SYSTEMSTATE_DATACHANGED 2.18 (16) 
        public override event EventHandler<HardwareInfoEventArgs> OnGetHardwareInfo;         //  1A    LAN GET HWINFO
        public override event EventHandler<GetLocoInfoEventArgs> OnGetLocoInfo;              //  40 EF LAN X LOCO INFO   4.4 (22)
        public override event EventHandler<TrackPowerEventArgs> OnTrackPower;                //  ist Zusammenfassung von 

        internal override void Empfang(IAsyncResult res)
        {
            try
            {
                IPEndPoint RemoteIpEndPoint = null;
                byte[] received = EndReceive(res, ref RemoteIpEndPoint);
                BeginReceive(new AsyncCallback(Empfang), null);
                if (OnReceive != null) OnReceive(this, new DataEventArgs(received));
                CutTelegramm(received);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Empfang  " + ex.Message);
            }
        }

        internal override void EndConnect(IAsyncResult res)
        {
            Console.WriteLine("Reconnection abgeschlossen");
            Client.EndConnect(res);
        }

        private void CutTelegramm(byte[] bytes)
        {
            if (bytes == null) return;
            int z = 0;
            int length = 0;
            int max = bytes.GetLength(0);
            while (z < max)
            {
                length = bytes[z];
                if ((length > 3) & ((z + length) <= max))
                {
                    byte[] einzelbytes = new byte[length];
                    Array.Copy(bytes, z, einzelbytes, 0, length);
                    Evaluation(einzelbytes);
                    z += length;
                }
                else
                {
                    z = max;  //Notausgang, falls ungültige Länge, Restliche Daten werden verworfen
                    Console.WriteLine("> Fehlerhaftes Telegramm.");
                }
            }

        }

        private void Evaluation(byte[] received)
        {
            bool b;
            int i, j;

            switch (received[2])
            {
                case 0x1A:           //  LAN GET HWINFO  2.2 (xx)
                    Console.WriteLine("> LAN GET HWINFO " + getByteString(received));
                    HardwareTyp hardwareTyp;
                    i = (received[7] << 24) + (received[6] << 16) + (received[5] << 8) + (received[4]);
                    j = (received[11] << 24) + (received[10] << 16) + (received[9] << 8) + (received[8]);
                    switch (i)
                    {
                        case 0x00000200: hardwareTyp = HardwareTyp.Z21_OLD; break;
                        case 0x00000201: hardwareTyp = HardwareTyp.Z21_NEW; break;
                        case 0x00000202: hardwareTyp = HardwareTyp.SMARTRAIL; break;
                        case 0x00000203: hardwareTyp = HardwareTyp.z21_SMALL; break;
                        default: hardwareTyp = HardwareTyp.None; break;
                    }
                    if (OnGetHardwareInfo != null) OnGetHardwareInfo(this, new HardwareInfoEventArgs(new HardwareInfo(hardwareTyp, j)));
                    break;
                case 0x10:           //  LAN GET SERIAL NUMBER  2.1 (10)
                    Console.WriteLine("> LAN GET SERIAL NUMBER " + getByteString(received));
                    i = (received[7] << 24) + (received[6] << 16) + (received[5] << 8) + (received[4]);
                    if (OnGetSerialNumber != null) OnGetSerialNumber(this, new GetSerialNumberEventArgs(i));

                    break;
                case 0x40:           //  X-Bus-Telegramm
                    switch (received[4])
                    {
                        case 0x61:
                            switch (received[5])
                            {
                                case 0x00:           //  LAN X BC TRACK POWER OFF  2.7 (12)
                                    Console.WriteLine("> LAN X BC TRACK POWER OFF " + getByteString(received));
                                    if (OnTrackPowerOFF != null) OnTrackPowerOFF(this, new EventArgs());
                                    if (OnTrackPower != null) OnTrackPower(this, new TrackPowerEventArgs(false));
                                    break;
                                case 0x01:           //  LAN X BC TRACK POWER ON  2.8 (12)
                                    Console.WriteLine("> LAN X BC TRACK POWER ON " + getByteString(received));
                                    if (OnTrackPowerON != null) OnTrackPowerON(this, new EventArgs());
                                    if (OnTrackPower != null) OnTrackPower(this, new TrackPowerEventArgs(true));
                                    break;
                                case 0x02:           //  LAN X BC PROGRAMMING MODE  2.9 (12)
                                    Console.WriteLine("> LAN X BC PROGRAMMING MODE " + getByteString(received));
                                    if (OnProgrammingMode != null) OnProgrammingMode(this, new EventArgs());
                                    break;
                                case 0x08:           //  LAN X BC TRACK SHORT CIRCUIT  2.10 (12)
                                    Console.WriteLine("> LAN X BC TRACK SHORT CIRCUIT " + getByteString(received));
                                    if (OnTrackShortCircuit != null) OnTrackShortCircuit(this, new EventArgs());
                                    break;
                                default:
                                    Console.WriteLine("> Unbekanntes X-Bus-Telegramm Header 61" + getByteString(received));
                                    break;
                            }
                            break;
                        case 0x62:           //  LAN X STATUS CHANGED  2.12 (13)
                            Console.WriteLine("> LAN X STATUS CHANGED " + getByteString(received));
                            CentralStateData centralStateData = GetCentralStateData(received);
                            if (OnStatusChanged != null) OnStatusChanged(this, new StateEventArgs(centralStateData));
                            break;
                        case 0x63:
                            switch (received[5])
                            {
                                case 0x21:           //  LAN X GET VERSION  2.3 (10)
                                    Console.WriteLine("> LAN X GET VERSION " + getByteString(received));
                                    VersionTyp versionTyp;
                                    switch (received[7])
                                    {
                                        case 0x00:
                                            versionTyp = VersionTyp.None;
                                            break;
                                        case 0x12:
                                            versionTyp = VersionTyp.Z21;
                                            break;
                                        case 0x13:
                                            versionTyp = VersionTyp.z21;  // 0x13 ist keine gesicherte Erkenntnis aus dem LAN-Protokoll, wird aber von meiner z21 so praktiziert
                                            break;
                                        default:
                                            versionTyp = VersionTyp.Other;
                                            break;
                                    }
                                    if (OnGetVersion != null) OnGetVersion(this, new VersionInfoEventArgs(new VersionInfo(received[6], versionTyp)));
                                    break;
                                default:
                                    Console.WriteLine("> Unbekanntes X-Bus-Telegramm Header 63" + getByteString(received));
                                    break;
                            }
                            break;

                        case 0x81:           //  LAN X BC STOPPED  2.14 (14)
                            Console.WriteLine("> LAN X BC STOPPED " + getByteString(received));
                            if (OnStopped != null) OnStopped(this, new EventArgs());
                            break;
                        case 0xEF:           //  LAN X LOCO INFO  4.4 (22)

                            ValueBytesStruct vbs = new ValueBytesStruct();
                            vbs.Adr_MSB = received[5];
                            vbs.Adr_LSB = received[6];
                            LokInfoData infodata = new LokInfoData();
                            infodata.Adresse = new LokAdresse(vbs);
                            infodata.Besetzt = ((received[7] & 8) == 8);
                            infodata.Fahrstufe = (byte)(received[8] & 0x7F);
                            b = ((received[8] & 0x80) == 0x80);
                            if (b) infodata.drivingDirection = DrivingDirection.F; else infodata.drivingDirection = DrivingDirection.R;

                            int functionIndexCount = 5;
                            for (int index = 9; index < received.Length && index <= 12; index++)
                            {
                                var functionBits = new BitArray(new byte[] { received[index] });
                                if (index == 9)
                                {
                                    infodata.Functions.Add(new(0, Convert.ToBoolean(functionBits.Get(4))));
                                    infodata.Functions.Add(new(1, Convert.ToBoolean(functionBits.Get(0))));
                                    infodata.Functions.Add(new(2, Convert.ToBoolean(functionBits.Get(1))));
                                    infodata.Functions.Add(new(3, Convert.ToBoolean(functionBits.Get(2))));
                                    infodata.Functions.Add(new(4, Convert.ToBoolean(functionBits.Get(3))));
                                }
                                else
                                {
                                    for (int temp = 0; temp < 8; temp++)
                                    {
                                        infodata.Functions.Add(new(functionIndexCount, Convert.ToBoolean(functionBits.Get(temp))));
                                        functionIndexCount++;
                                    }
                                }
                            }
                            if (OnGetLocoInfo != null) OnGetLocoInfo(this, new GetLocoInfoEventArgs(infodata));
                            break;
                        case 0xF3:
                            switch (received[5])
                            {
                                case 0x0A:           //  LAN X GET FIRMWARE VERSION 2.15 (xx)
                                    Console.WriteLine("> LAN X GET FIRMWARE VERSION " + getByteString(received));
                                    if (OnGetFirmwareVersion != null) OnGetFirmwareVersion(this, new FirmwareVersionInfoEventArgs(new FirmwareVersionInfo(received[6], received[7])));
                                    // Achtung: die z21 bringt die Minor-Angabe hexadezimal !!!!!!!!    z.B. Firmware 1.23 = Minor 34
                                    break;
                                default:
                                    Console.WriteLine("> Unbekanntes X-Bus-Telegramm Header F3" + getByteString(received));
                                    break;
                            }
                            break;
                        default:
                            Console.WriteLine("> Unbekanntes X-Bus-Telegramm " + getByteString(received));
                            break;
                    }
                    break;
                case 0x84:            // LAN SYSTEMSTATE DATACHANGED    2.18 (16)
                    Console.WriteLine("> LAN SYSTEMSTATE DATACHANGED " + getByteString(received));
                    SystemStateData systemStateData = GetSystemStateData(received);
                    if (OnSystemStateDataChanged != null) OnSystemStateDataChanged(this, new SystemStateEventArgs(systemStateData));

                    break;
                default:
                    Console.WriteLine("> Unbekanntes Telegramm " + getByteString(received));
                    break;
            }
        }

        private CentralStateData GetCentralStateData(byte[] received)
        {
            CentralStateData statedata = new CentralStateData();
            statedata.EmergencyStop = ((received[6] & 0x01) == 0x01);
            statedata.TrackVoltageOff = ((received[6] & 0x02) == 0x02);
            statedata.ShortCircuit = ((received[6] & 0x04) == 0x04);
            statedata.ProgrammingModeActive = ((received[6] & 0x20) == 0x20);
            return statedata;
        }

        private SystemStateData GetSystemStateData(byte[] received)
        {
            SystemStateData statedata = new SystemStateData();
            statedata.MainCurrent = (received[4] << 8) + (received[5]);
            statedata.ProgCurrent = (received[6] << 8) + (received[7]);
            statedata.FilteredMainCurrent = (received[8] << 8) + (received[9]);
            statedata.Temperature = (received[10] << 8) + (received[11]);
            statedata.SupplyVoltage = (received[12] << 8) + (received[13]);
            statedata.VCCVoltage = (received[14] << 8) + (received[15]);
            statedata.CentralState.EmergencyStop = ((received[16] & 0x01) == 0x01);
            statedata.CentralState.TrackVoltageOff = ((received[16] & 0x02) == 0x02);
            statedata.CentralState.ShortCircuit = ((received[16] & 0x04) == 0x04);
            statedata.CentralState.ProgrammingModeActive = ((received[16] & 0x20) == 0x20);
            statedata.CentralState.HighTemperature = ((received[17] & 0x01) == 0x01);
            statedata.CentralState.PowerLost = ((received[17] & 0x02) == 0x02);
            statedata.CentralState.ShortCircuitExternal = ((received[17] & 0x04) == 0x04);
            statedata.CentralState.ShortCircuitInternal = ((received[17] & 0x08) == 0x08);
            return statedata;
        }

        /// <summary>
        /// LAN_GET_SERIAL_NUMBER()    
        /// </summary>
        public override void GetSerialNumber()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0;
            bytes[2] = 0x10;
            bytes[3] = 0;
            Console.WriteLine("LAN GET SERIAL NUMBER " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_GET_VERSION
        /// </summary>
        public override void GetVersion()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x21;
            bytes[5] = 0x21;
            //bytes[6] = 0x47;   // = XOR-Byte  selbst ausgerechnet, in der LAN-Doku steht 0 ?!
            bytes[6] = 0;
            Console.WriteLine("LAN X GET VERSION " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_GET_STATUS
        /// </summary>
        public override void GetStatus()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x21;
            bytes[5] = 0x24;
            bytes[6] = 0x05;   // = XOR-Byte
            Console.WriteLine("LAN X GET STATUS " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_SET_TRACK_POWER_OFF
        /// </summary>
        public override void SetTrackPowerOFF()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x21;
            bytes[5] = 0x80;
            bytes[6] = 0xA1;   // = XOR-Byte
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_SET_TRACK_POWER_ON
        /// </summary>
        public override void SetTrackPowerON()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x21;
            bytes[5] = 0x81;
            bytes[6] = 0xA0;   // = XOR-Byte
            Console.WriteLine("LAN X SET TRACK POWER OFF " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_SET_STOP
        /// </summary>
        public override void SetStop()
        {
            byte[] bytes = new byte[6];
            bytes[0] = 0x06;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x80;
            bytes[5] = 0x80;   // = XOR-Byte
            Console.WriteLine("LAN X SET STOP " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_GET_FIRMWARE_VERSION
        /// </summary>
        public override void GetFirmwareVersion()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0xF1;
            bytes[5] = 0x0A;
            bytes[6] = 0xFB;   // = XOR-Byte
            Console.WriteLine("LAN X GET FIRMWARE VERSION " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_SET_BROADCASTFLAGS
        /// </summary>
        public override void LogOn()
        {
            byte[] bytes = new byte[8];
            bytes[0] = 0x08;
            bytes[1] = 0;
            bytes[2] = 0x50;
            bytes[3] = 0;
            bytes[4] = 1;         //  0x0000001   (Byte 4+5) 
            bytes[5] = 0;         //  0x0000100   (Byte 4+5) 
            bytes[6] = 0;
            bytes[7] = 0;
            Console.WriteLine("LAN SET BROADCASTFLAGS " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_SYSTEMSTATE_GETDATA
        /// </summary>
        public override void SystemStateGetData()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0;
            bytes[2] = 0x85;
            bytes[3] = 0;
            Console.WriteLine("LAN SYSTEMSTATE GETDATA " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_GET_HWINFO
        /// </summary>
        public override void GetHardwareInfo()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0;
            bytes[2] = 0x1A;
            bytes[3] = 0;      // kein XOR-Byte  ???
            Console.WriteLine("LAN GET HWINFO " + getByteString(bytes));
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_GET_LOCO_INFO
        /// </summary>
        /// <param name="adresse"></param>
        public override void GetLocoInfo(LokAdresse adresse)
        {
            if (adresse is null) return;
            byte[] bytes = new byte[9];
            bytes[0] = 0x09;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0xE3;
            bytes[5] = 0xF0;
            bytes[6] = adresse.ValueBytes.Adr_MSB;
            bytes[7] = adresse.ValueBytes.Adr_LSB;
            bytes[8] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7]);
            Console.WriteLine("LAN X GET LOCO INFO " + getByteString(bytes) + " (#" + adresse.Value.ToString() + ")");
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_SET_LOCO_DRIVE
        /// </summary>
        /// <param name="data"></param>
        public override void SetLocoDrive(LokInfoData data)
        {
            if (data.drivingDirection == DrivingDirection.F) data.Fahrstufe |= 0x080;

            byte[] bytes = new byte[10];
            bytes[0] = 0x0A;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0xE4;
            bytes[5] = 0x13; //  = 128 Fahrstufen
            bytes[6] = data.Adresse.ValueBytes.Adr_MSB;
            bytes[7] = data.Adresse.ValueBytes.Adr_LSB;
            bytes[8] = data.Fahrstufe;
            bytes[9] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7] ^ bytes[8]);
            Console.WriteLine("LAN X SET LOCO DRIVE " + getByteString(bytes) + "  (" + data.Adresse + " - " + data.Fahrstufe.ToString() + ")");
            Senden(bytes);
        }

        /// <summary>
        /// LAN_X_SET_LOCO_FUNCTION
        /// </summary>
        /// <param name="adresse"></param>
        /// <param name="function"></param>
        /// <param name="toggelType"></param>
        public override void SetLocoFunction(LokAdresse adresse, Function function, ToggleType toggelType)
        {
            byte[] bytes = new byte[10];
            bytes[0] = 0x0A;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0xE4;
            bytes[5] = 0xF8;
            bytes[6] = adresse.ValueBytes.Adr_MSB;
            bytes[7] = adresse.ValueBytes.Adr_LSB;
            bytes[8] = (byte)function.FunctionIndex;

            var bitarray = new BitArray(new byte[] { bytes[8] });
            switch (toggelType)
            {
                case ToggleType.off:
                    break;
                case ToggleType.on:
                    bitarray.Set(6, true);
                    break;
                case ToggleType.@switch:
                    bitarray.Set(7, true);
                    break;

            }
            bitarray.CopyTo(bytes, 8);

            bytes[9] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7] ^ bytes[8]);
            Senden(bytes);
        }

        /// <summary>
        /// LAN_LOGOFF
        /// </summary>
        public override void LogOFF()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0;
            bytes[2] = 0x30;
            bytes[3] = 0;
            Senden(bytes);
        }

        private string getByteString(byte[] bytes)
        {
            string s = "";
            foreach (byte b in bytes)
            {
                s += b.ToString("X") + ",";
            }
            return s;
        }

        private void Senden(byte[] bytes)
        {
            try
            {
                Send(bytes, bytes.GetLength(0));
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("Fehler beim Senden. Zu sendende Bytes waren null.");
                Console.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine("Fehler beim Senden. Der UdpClient ist geschlossen.");
                Console.WriteLine(e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Fehler beim Senden. Der UdpClient hat bereits einen Standardremotehost eingerichtet.");
                Console.WriteLine(e.Message);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Fehler beim Senden. Socket-Exception.");
                Console.WriteLine("Versuche es erneut.");
                Client.BeginConnect(lanAdresse, lanPort, new AsyncCallback(EndConnect), null);
                Console.WriteLine(e.Message);

            }
        }

        public override void Reconnect()
        {
            try
            {
                Client.BeginConnect(lanAdresse, lanPort, new AsyncCallback(EndConnect), null);
            }
            catch
            {
                Console.WriteLine("Fehler beim Reconnection.");
            }
        }

        public override void Dispose()
        {
            //LogOFF();
            Close();
        }

        public override void SetBroadcastFlag()
        {
            byte[] bytes = new byte[8];
            bytes[0] = 0x08;
            bytes[1] = 0;
            bytes[2] = 0x50;
            bytes[3] = 0;
            //bytes[4] = 0x00010000;
            var x = unchecked((int)0x00010000);
            //Senden(bytes);
        }
    }
}
