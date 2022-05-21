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

using Helper;
using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using TrainDatabase.Z21Client.DTO;
using TrainDatabase.Z21Client.Enums;
using TrainDatabase.Z21Client.Events;

namespace TrainDatabase.Z21Client
{
    public class Z21Client : UdpClient
    {
        public const int maxDccStep = 127;

        public Z21Client() : base(Configuration.ClientPort)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                    AllowNatTraversal(true);
                Address = Configuration.ClientIP;
                Port = Configuration.ClientPort;
                Connect(Address, Port);
                BeginReceive(new AsyncCallback(Empfang), null);
                Logger.LogInformation($"Z21 initialisiert.");
                RenewClientSubscription.Elapsed += (a, b) => GetStatus();
                LogOn();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fehler beim connecten zur Z21.");
            }
        }

        public event EventHandler<FirmwareVersionInfoEventArgs> OnGetFirmwareVersion = default!;

        public event EventHandler<HardwareInfoEventArgs> OnGetHardwareInfo = default!;

        public event EventHandler<LanCode> OnGetLanCode = default!;

        public event EventHandler<GetLocoInfoEventArgs> OnGetLocoInfo = default!;

        public event EventHandler<GetSerialNumberEventArgs> OnGetSerialNumber = default!;

        public event EventHandler<VersionInfoEventArgs> OnGetVersion = default!;

        public event EventHandler<DataEventArgs> OnReceive = default!;

        public event EventHandler<StateEventArgs> OnStatusChanged = default!;

        public event EventHandler OnStopped = default!;

        public event EventHandler<SystemStateEventArgs> OnSystemStateDataChanged = default!;

        public event EventHandler<TrackPowerEventArgs> TrackPowerChanged = default!;

        public IPAddress Address { get; }

        public int Port { get; }

        private Timer RenewClientSubscription { get; } = new Timer() { AutoReset = true, Enabled = true, Interval = new TimeSpan(0, 0, 50).TotalMilliseconds, };

        public new void Dispose()
        {
            LogOFF();
            Close();
            base.Dispose();
        }

        public void GetFirmwareVersion()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0xF1;
            bytes[5] = 0x0A;
            bytes[6] = 0xFB;
            Logger.LogByteArray($"GET FIRMWARE VERSION", bytes);
            Senden(bytes);
        }

        public void GetHardwareInfo()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0;
            bytes[2] = 0x1A;
            bytes[3] = 0;
            Logger.LogByteArray($"GET HWINFO ", bytes);
            Senden(bytes);
        }

        public void GetLanCode()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0x00;
            bytes[2] = 0x18;
            bytes[3] = 0x00;
            Logger.LogByteArray($"GET LAN CODE ", bytes);
            Senden(bytes);
        }

        public void GetLocoInfo(LokAdresse adresse)
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
            Logger.LogByteArray($"LAN X GET LOCO INFO  (#{adresse.Value})", bytes);
            Senden(bytes);
        }

        public void GetSerialNumber()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0;
            bytes[2] = 0x10;
            bytes[3] = 0;
            Logger.LogByteArray($"GET SERIAL NUMBER ", bytes);
            Senden(bytes);
        }

        public void GetStatus()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x21;
            bytes[5] = 0x24;
            bytes[6] = 0x05;
            Logger.LogByteArray($"GET STATUS ", bytes);
            Senden(bytes);
        }

        public void GetVersion()
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
            Logger.LogByteArray($"GET VERSION ", bytes);
            Senden(bytes);
        }

        public void LogOFF()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0x00;
            bytes[2] = 0x30;
            bytes[3] = 0x00;
            Logger.LogByteArray("LOG OFF", bytes);
            Senden(bytes);
        }

        public void LogOn()
        {
            var flags = BitConverter.GetBytes(0x00000001 | 0x00010000);
            byte[] bytes = new byte[8];
            bytes[0] = 0x08;
            bytes[1] = 0;
            bytes[2] = 0x50;
            bytes[3] = 0;
            bytes[4] = flags[0];
            bytes[5] = flags[1];
            bytes[6] = flags[2];
            bytes[7] = flags[3];
            Logger.LogByteArray($"SET BROADCASTFLAGS ", bytes);
            Senden(bytes);
        }

        public void SetLocoDrive(List<LokInfoData> data)
        {
            var array = new byte[10 * data.Count];
            for (int i = 0, currentIndex = 0; i < data.Count; i++, currentIndex += 10)
                Array.Copy(GetLocoDriveByteArray(data[i]), 0, array, currentIndex, 10);
            Senden(array);
        }

        public void SetLocoDrive(LokInfoData data) => Senden(GetLocoDriveByteArray(data));

        public void SetLocoFunction(LokAdresse adresse, Function function, ToggleType toggelType)
        {
            byte[] bytes = GetLocoFunctionByteArray(adresse, function, toggelType);
            Senden(bytes);
        }

        public void SetLocoFunction(List<(ToggleType toggle, Function Func)> data)
        {
            var array = new byte[10 * data.Count];
            for (int i = 0, currentIndex = 0; i < data.Count; i++, currentIndex += 10)
            {
                (ToggleType toggleType, Function function) = data[i];
                Array.Copy(GetLocoFunctionByteArray(new(function.Vehicle.Address), function, toggleType), 0, array, currentIndex, 10);
            }
            Senden(array);
        }

        public void SetStop()
        {
            byte[] bytes = new byte[6];
            bytes[0] = 0x06;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x80;
            bytes[5] = 0x80;
            Logger.LogByteArray($"SET STOP ", bytes);
            Senden(bytes);
        }

        public void SetTrackPowerOFF()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x21;
            bytes[5] = 0x80;
            bytes[6] = 0xA1;
            Senden(bytes);
            Logger.LogByteArray($"SET TRACK POWER OFF ", bytes);
        }

        public void SetTrackPowerON()
        {
            byte[] bytes = new byte[7];
            bytes[0] = 0x07;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0x21;
            bytes[5] = 0x81;
            bytes[6] = 0xA0;
            Logger.LogByteArray($"SET TRACK POWER ON ", bytes);
            Senden(bytes);
        }

        public void SystemStateGetData()
        {
            byte[] bytes = new byte[4];
            bytes[0] = 0x04;
            bytes[1] = 0;
            bytes[2] = 0x85;
            bytes[3] = 0;
            Logger.LogByteArray($"SYSTEMSTATE GETDATA ", bytes);
            Senden(bytes);
        }

        private static string GetByteString(byte[] bytes) => string.Join("", bytes.Select(e => $"{e:D2} "));

        private static TrackPower GetCentralStateData(byte[] received)
        {
            TrackPower state = TrackPower.ON;
            bool isEmergencyStop = (received[6] & 0x01) == 0x01;
            bool isTrackVoltageOff = (received[6] & 0x02) == 0x02;
            bool isShortCircuit = (received[6] & 0x04) == 0x04;
            bool isProgrammingModeActive = (received[6] & 0x20) == 0x20;
            if (isEmergencyStop || isTrackVoltageOff)
                state = TrackPower.OFF;
            else if (isShortCircuit)
                state = TrackPower.Short;
            else if (isProgrammingModeActive)
                state = TrackPower.Programing;
            Logger.LogByteArray($"LAN_X_STATUS_CHANGED \n\t{nameof(isEmergencyStop)}: {isEmergencyStop}\n\t{nameof(isTrackVoltageOff)}: {isTrackVoltageOff}\n\t{nameof(isShortCircuit)}: {isShortCircuit}\n\t{nameof(isProgrammingModeActive)}: {isProgrammingModeActive}", received);
            return state;
        }

        private static byte[] GetLocoDriveByteArray(LokInfoData data)
        {
            if (data.DrivingDirection) data.Speed |= 0x080;
            byte[] bytes = new byte[10];
            bytes[0] = 0x0A;
            bytes[1] = 0;
            bytes[2] = 0x40;
            bytes[3] = 0;
            bytes[4] = 0xE4;
            bytes[5] = 0x13; //  = 128 Fahrstufen
            bytes[6] = data.Adresse.ValueBytes.Adr_MSB;
            bytes[7] = data.Adresse.ValueBytes.Adr_LSB;
            bytes[8] = (byte)data.Speed;
            bytes[9] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7] ^ bytes[8]);
            Logger.LogByteArray($"SET LOCO DRIVE: \n\tAdresse:'{data.Adresse.Value:D3}' \n\tDirection: '{data.DrivingDirection}'\t \n\tSpeed:'{data.Speed:D3}'", bytes);
            return bytes;
        }

        private static byte[] GetLocoFunctionByteArray(LokAdresse adresse, Function function, ToggleType toggelType)
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
                case ToggleType.Off:
                    break;
                case ToggleType.On:
                    bitarray.Set(6, true);
                    break;
                case ToggleType.Toggle:
                    bitarray.Set(7, true);
                    break;
            }
            bitarray.CopyTo(bytes, 8);
            bytes[9] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7] ^ bytes[8]);
            Logger.LogByteArray($"SET LOCO FUNCTION ({adresse} - index: {function.FunctionIndex} - {toggelType})", bytes);
            return bytes;
        }

        private static SystemStateData GetSystemStateData(byte[] received)
        {
            SystemStateData statedata = new SystemStateData();
            statedata.MainCurrent = (received[4] << 8) + received[5];
            statedata.ProgCurrent = (received[6] << 8) + received[7];
            statedata.FilteredMainCurrent = (received[8] << 8) + received[9];
            statedata.Temperature = (received[10] << 8) + received[11];
            statedata.SupplyVoltage = (received[12] << 8) + received[13];
            statedata.VCCVoltage = (received[14] << 8) + received[15];
            statedata.ClientData.EmergencyStop = (received[16] & 0x01) == 0x01;
            statedata.ClientData.TrackVoltageOff = (received[16] & 0x02) == 0x02;
            statedata.ClientData.ShortCircuit = (received[16] & 0x04) == 0x04;
            statedata.ClientData.ProgrammingModeActive = (received[16] & 0x20) == 0x20;
            statedata.ClientData.HighTemperature = (received[17] & 0x01) == 0x01;
            statedata.ClientData.PowerLost = (received[17] & 0x02) == 0x02;
            statedata.ClientData.ShortCircuitExternal = (received[17] & 0x04) == 0x04;
            statedata.ClientData.ShortCircuitInternal = (received[17] & 0x08) == 0x08;
            return statedata;
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
                if (length > 3 & z + length <= max)
                {
                    byte[] einzelbytes = new byte[length];
                    Array.Copy(bytes, z, einzelbytes, 0, length);
                    Evaluation(einzelbytes);
                    z += length;
                }
                else
                {
                    z = max;
                    Logger.LogByteArray($"Fehlerhaftes Telegramm.", bytes);
                }
            }

        }

        private void Empfang(IAsyncResult res)
        {
            try
            {
                IPEndPoint RemoteIpEndPoint = null!;
                byte[] received = EndReceive(res, ref RemoteIpEndPoint!);
                BeginReceive(new AsyncCallback(Empfang), null);
                if (OnReceive != null) OnReceive(this, new DataEventArgs(received));
                Logger.LogByteArray("Received", received);
                CutTelegramm(received);
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Fehler beim Empfang  " + ex.Message);
            }
        }

        private void EndConnect(IAsyncResult res)
        {
            Logger.LogInformation($"Reconnection abgeschlossen");
            Client.EndConnect(res);
        }

        private void Evaluation(byte[] received)
        {
            int i, j;

            switch (received[2])
            {
                case 0x1A:
                    Logger.LogByteArray($"GET HWINFO ", received);
                    HardwareTyp hardwareTyp;
                    i = (received[7] << 24) + (received[6] << 16) + (received[5] << 8) + received[4];
                    j = (received[11] << 24) + (received[10] << 16) + (received[9] << 8) + received[8];
                    switch (i)
                    {
                        case 0x00000200: hardwareTyp = HardwareTyp.Z21Old; break;
                        case 0x00000201: hardwareTyp = HardwareTyp.Z21New; break;
                        case 0x00000202: hardwareTyp = HardwareTyp.SmartRail; break;
                        case 0x00000203: hardwareTyp = HardwareTyp.z21Small; break;
                        default: hardwareTyp = HardwareTyp.None; break;
                    }
                    OnGetHardwareInfo?.Invoke(this, new HardwareInfoEventArgs(new HardwareInfo(hardwareTyp, j)));
                    break;
                case 0x10:
                    Logger.LogByteArray($"GET SERIAL NUMBER ", received);
                    i = (received[7] << 24) + (received[6] << 16) + (received[5] << 8) + received[4];
                    OnGetSerialNumber?.Invoke(this, new GetSerialNumberEventArgs(i));
                    break;
                case 0x18:
                    if (Enum.IsDefined(typeof(LanCode), received[4]))
                    {
                        Logger.LogByteArray($"GET LAN CODE ", received);
                        OnGetLanCode?.Invoke(this, (LanCode)received[4]);
                    }
                    else
                    {
                        Logger.LogByteArray($"RECEIVED INVALID LAN CODE", received);
                    }
                    break;
                case 0x40:
                    switch (received[4])
                    {
                        case 0x61:
                            switch (received[5])
                            {
                                case 0x00:
                                    Logger.LogByteArray($"TRACK POWER OFF ", received);
                                    TrackPowerChanged?.Invoke(this, new(TrackPower.OFF));
                                    break;
                                case 0x01:
                                    Logger.LogByteArray($"TRACK POWER ON ", received);
                                    TrackPowerChanged?.Invoke(this, new(TrackPower.ON));
                                    break;
                                case 0x02:
                                    Logger.LogByteArray($"PROGRAMMING MODE ", received);
                                    TrackPowerChanged?.Invoke(this, new(TrackPower.Programing));
                                    break;
                                case 0x08:
                                    Logger.LogByteArray($"TRACK SHORT CIRCUIT ", received);
                                    TrackPowerChanged?.Invoke(this, new(TrackPower.Short));
                                    break;
                                default:
                                    Logger.LogByteArray($"Unbekanntes X-Bus-Telegramm Header 61", received);
                                    break;
                            }
                            break;
                        case 0x62:           //  LAN X STATUS CHANGED  2.12 (13)
                            OnStatusChanged?.Invoke(this, new StateEventArgs(GetCentralStateData(received)));
                            break;
                        case 0x63:
                            switch (received[5])
                            {
                                case 0x21:           //  LAN X GET VERSION  2.3 (10)
                                    Logger.LogByteArray($"GET VERSION ", received);
                                    var versionTyp = received[7] switch
                                    {
                                        0x00 => VersionTyp.None,
                                        0x12 => VersionTyp.Z21,
                                        0x13 => VersionTyp.z21,// 0x13 ist keine gesicherte Erkenntnis aus dem LAN-Protokoll, wird aber von meiner z21 so praktiziert
                                        _ => VersionTyp.Other,
                                    };
                                    OnGetVersion?.Invoke(this, new VersionInfoEventArgs(new VersionInfo(received[6], versionTyp)));
                                    break;
                                default:
                                    Logger.LogByteArray($"Unbekanntes X-Bus-Telegramm Header 63", received);
                                    break;
                            }
                            break;

                        case 0x81:           //  LAN X BC STOPPED  2.14 (14)
                            Logger.LogByteArray($"BC STOPPED ", received);
                            OnStopped?.Invoke(this, new EventArgs());
                            break;
                        case 0xEF:           //  LAN X LOCO INFO  4.4 (22)

                            ValueBytesStruct vbs = new();
                            vbs.Adr_MSB = received[5];
                            vbs.Adr_LSB = received[6];
                            LokInfoData infodata = new();
                            infodata.Adresse = new LokAdresse(vbs);
                            infodata.InUse = (received[7] & 8) == 8;
                            infodata.Speed = (byte)(received[8] & 0x7F);
                            infodata.DrivingDirection = (received[8] & 0x80) == 0x80;
                            int functionIndexCount = 5;
                            for (int index = 9; index < received.Length && index <= 12; index++)
                            {
                                BitArray functionBits = new(new byte[] { received[index] });
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
                            OnGetLocoInfo?.Invoke(this, new GetLocoInfoEventArgs(infodata));
                            Logger.LogByteArray($"GET LOCO DRIVE: \n\tAdresse:'{infodata.Adresse.Value:D3}' \n\tDirection: '{infodata.DrivingDirection}'\n\tSpeed:'{infodata.Speed:D3}'", received);
                            break;
                        case 0xF3:
                            switch (received[5])
                            {
                                case 0x0A:
                                    Logger.LogByteArray($"GET FIRMWARE VERSION ", received);
                                    OnGetFirmwareVersion?.Invoke(this, new FirmwareVersionInfoEventArgs(new FirmwareVersionInfo(received[6], received[7])));
                                    // Achtung: die z21 bringt die Minor-Angabe hexadezimal !!!!!!!!    z.B. Firmware 1.23 = Minor 34
                                    break;
                                default:
                                    Logger.LogByteArray($"Unbekanntes X-Bus-Telegramm Header F3", received);
                                    break;
                            }
                            break;
                        default:
                            Logger.LogByteArray($"Unbekanntes X-Bus-Telegramm ", received);
                            break;
                    }
                    break;
                case 0x84:            // LAN SYSTEMSTATE DATACHANGED    2.18 (16)
                    Logger.LogByteArray($"LAN SYSTEMSTATE DATACHANGED ", received);
                    SystemStateData systemStateData = GetSystemStateData(received);
                    OnSystemStateDataChanged?.Invoke(this, new SystemStateEventArgs(systemStateData));
                    break;
                default:
                    Logger.LogByteArray($"Unbekanntes Telegramm ", received);
                    break;
            }
        }

        private async void Senden(byte[] bytes)
        {
            try
            {
                await SendAsync(bytes, bytes.GetLength(0));
            }
            catch (ArgumentNullException ex)
            {
                Logger.LogError(ex, "Fehler beim Senden. Zu sendende Bytes waren null.");
            }
            catch (ObjectDisposedException ex)
            {
                Logger.LogError(ex, "Fehler beim Senden. Der UdpClient ist geschlossen.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogError(ex, "Fehler beim Senden. Der UdpClient hat bereits einen Standardremotehost eingerichtet.");
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                    Client.BeginConnect(Address, Port, new AsyncCallback(EndConnect), null);
                Logger.LogError(ex, "Fehler beim Senden");
            }
        }
    }
}
