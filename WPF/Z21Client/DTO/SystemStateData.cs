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

namespace TrainDatabase.Z21Client.DTO
{
    public class SystemStateData
    {
        public SystemStateData()
        {
            ClientData = new Z21ClientData();
        }

        public Z21ClientData ClientData { get; set; }

        public int FilteredMainCurrent { get; set; } = -1;

        public int MainCurrent { get; set; } = -1;

        public int ProgCurrent { get; set; } = -1;

        public int SupplyVoltage { get; set; } = -1;

        public int Temperature { get; set; } = -1;

        public int VCCVoltage { get; set; } = -1;
    }
}
