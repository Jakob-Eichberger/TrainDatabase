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
using System;
using System.Collections.Generic;

namespace ModelTrainController.Z21
{
    public class LokInfoData
    {
        public LokAdresse Adresse = default!;
        public bool Besetzt;
        public DrivingDirection drivingDirection;
        private byte fahrstufe = 2;
        // TODO: Change the max value
        public byte Fahrstufe { get { return fahrstufe; } set { fahrstufe = value; } }

        /// <summary>
        /// int -> <see cref="Function.FunctionIndex"/> |
        /// bool -> TRUE: ON, FALSE, OFF
        /// </summary>
        public List<(int functionIndex, bool state)> Functions = new();
    }
}
