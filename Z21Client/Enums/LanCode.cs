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

namespace TrainDatabase.Z21Client.Enums
{
    public enum LanCode
    {
        ///<summary>
        ///Keine Features gesperrt
        ///</summary>
        Z21NoLock = 0x00,

        ///<summary>
        ///„z21 start”: Fahren und Schalten per LAN gesperrt
        ///</summary>
        z21StartLocked = 0x01,

        ///<summary>
        /// „z21 start”: alle Feature-Sperren aufgehoben
        ///</summary>
        z21StartUnlocked = 0x02
    }
}
