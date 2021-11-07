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
    public class FirmwareVersionInfo
    {
        public FirmwareVersionInfo(int major, int minor)
        {
            Major = major;
            Minor = minor;
            Firmware = major.ToString("X") + "." + minor.ToString("X");
        }

        public string Firmware { get; set; } = string.Empty;

        public int Major { get; set; } = 0;

        public int Minor { get; set; } = 0;
    }
}
