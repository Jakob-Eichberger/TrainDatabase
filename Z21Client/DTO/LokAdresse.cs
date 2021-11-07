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

namespace TrainDatabase.Z21Client.DTO
{
    public class LokAdresse
    {
        private int val = 0;

        public LokAdresse(int adresse)
        {
            Value = adresse;
        }

        public LokAdresse(long adresse)
        {
            Value = (int)adresse;
        }

        public LokAdresse(ValueBytesStruct valueBytes)
        {
            ValueBytes = valueBytes;
        }

        public int Value
        {
            set
            {
                if (value >= 1 & value <= 9999)
                    val = value;
                else
                    val = -99;
            }
            get
            {
                return val;
            }
        }

        public ValueBytesStruct ValueBytes
        {
            set
            {
                Value = ((value.Adr_MSB & 0x3F) << 8) + value.Adr_LSB;
            }
            get
            {
                ValueBytesStruct vbs;

                try
                {
                    var b = Convert.ToByte(val >> 8);
                    if (val >= 128)
                        b += 192;
                    vbs.Adr_MSB = b;
                    vbs.Adr_LSB = Convert.ToByte(val % 256);

                }
                catch
                {
                    vbs.Adr_MSB = 0;
                    vbs.Adr_LSB = 0;

                }
                return vbs;
            }
        }

        public override string ToString()
        {
            return Value.ToString();

        }

    }
}
