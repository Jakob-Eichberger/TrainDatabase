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

namespace TrainDatabase.Z21
{
    public class SystemStateData
    {
        public int MainCurrent = -1;
        public int ProgCurrent = -1;
        public int FilteredMainCurrent = -1;
        public int Temperature = -1;
        public int SupplyVoltage = -1;
        public int VCCVoltage = -1;
        public CentralStateData CentralState;
        public SystemStateData()
        {
            CentralState = new CentralStateData();
        }
    }
}
