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
using System.Collections.Generic;

namespace ModelTrainController.Z21
{
    public class LokInfoData
    {
        private int speed = 0;

        public LokAdresse Adresse { get; set; } = default!;

        public bool InUse { get; set; } = default!;

        public bool DrivingDirection { get; set; } = true;

        public int Speed
        {
            get => speed; set
            {
                speed = value == 1 ? (speed > 1 ? 0 : 2) : value;
            }
        }

        /// <summary>
        /// int -> <see cref="Function.FunctionIndex"/> |
        /// bool -> TRUE: ON, FALSE, OFF
        /// </summary>
        public List<(int functionIndex, bool state)> Functions = new();

        public LokInfoData(LokAdresse adresse)
        {
            Adresse = adresse;
        }

        public LokInfoData(int adresse) => Adresse = new(adresse);

        public LokInfoData(long adresse) => Adresse = new((int)adresse);

        public LokInfoData()
        {
        }
    }
}
