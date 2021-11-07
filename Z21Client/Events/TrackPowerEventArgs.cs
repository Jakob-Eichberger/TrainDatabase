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
using TrainDatabase.Z21Client.Enum;

namespace TrainDatabase.Z21Client.Events
{
    public class TrackPowerEventArgs : EventArgs
    {
        public TrackPowerEventArgs(TrackPower trackPower)
        {
            TrackPower = trackPower;
        }

        public TrackPower TrackPower { get; set; }
    }
}
