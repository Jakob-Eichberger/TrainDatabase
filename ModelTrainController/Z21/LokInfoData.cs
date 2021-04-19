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
using System;
namespace ModelTrainController.Z21
{
    public class LokInfoData
    {
        public LokAdresse Adresse;
        public bool Besetzt;
        public DrivingDirection drivingDirection;
        private byte fahrstufe = 0;
        public byte Fahrstufe { get { return fahrstufe; } set { fahrstufe = value <= 254 ? value : throw new ArgumentException($"Max Value für Fahrstufe ist 254! Ist: {value}"); } }
    }
}
