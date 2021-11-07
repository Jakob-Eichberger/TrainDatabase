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

namespace WPF_Application.CentralStation.DTO
{
    public class Z21ClientData
    {
        public bool EmergencyStop { get; set; } = true;

        public bool HighTemperature { get; set; } = true;

        public bool PowerLost { get; set; } = true;

        public bool ProgrammingModeActive { get; set; } = true;

        public bool ShortCircuit { get; set; } = true;

        public bool ShortCircuitExternal { get; set; } = true;

        public bool ShortCircuitInternal { get; set; } = true;

        public bool TrackVoltageOff { get; set; } = true;
    }
}
