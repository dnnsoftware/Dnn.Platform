// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.IO;
    using System.Net;

    /// <summary>Provides the ability to lookup a country from an IP address.</summary>
    public class CountryLookup
    {
        private static readonly string[] CountryName = new[]
                                                           {
                                                               "N/A", "Asia/Pacific Region", "Europe", "Andorra", "United Arab Emirates", "Afghanistan", "Antigua and Barbuda", "Anguilla", "Albania",
                                                               "Armenia", "Netherlands Antilles", "Angola", "Antarctica", "Argentina", "American Samoa", "Austria", "Australia", "Aruba", "Azerbaijan",
                                                               "Bosnia and Herzegovina", "Barbados", "Bangladesh", "Belgium", "Burkina Faso", "Bulgaria", "Bahrain", "Burundi", "Benin", "Bermuda",
                                                               "Brunei Darussalam", "Bolivia", "Brazil", "Bahamas", "Bhutan", "Bouvet Island", "Botswana", "Belarus", "Belize", "Canada",
                                                               "Cocos (Keeling) Islands", "Congo, The Democratic Republic of the", "Central African Republic", "Congo", "Switzerland", "Cote D'Ivoire",
                                                               "Cook Islands", "Chile", "Cameroon", "China", "Colombia", "Costa Rica", "Cuba", "Cape Verde", "Christmas Island", "Cyprus", "Czech Republic",
                                                               "Germany", "Djibouti", "Denmark", "Dominica", "Dominican Republic", "Algeria", "Ecuador", "Estonia", "Egypt", "Western Sahara", "Eritrea",
                                                               "Spain", "Ethiopia", "Finland", "Fiji", "Falkland Islands (Malvinas)", "Micronesia, Federated States of", "Faroe Islands", "France",
                                                               "France, Metropolitan", "Gabon", "United Kingdom", "Grenada", "Georgia", "French Guiana", "Ghana", "Gibraltar", "Greenland", "Gambia",
                                                               "Guinea", "Guadeloupe", "Equatorial Guinea", "Greece", "South Georgia and the South Sandwich Islands", "Guatemala", "Guam", "Guinea-Bissau",
                                                               "Guyana", "Hong Kong", "Heard Island and McDonald Islands", "Honduras", "Croatia", "Haiti", "Hungary", "Indonesia", "Ireland", "Israel",
                                                               "India", "British Indian Ocean Territory", "Iraq", "Iran, Islamic Republic of", "Iceland", "Italy", "Jamaica", "Jordan", "Japan", "Kenya",
                                                               "Kyrgyzstan", "Cambodia", "Kiribati", "Comoros", "Saint Kitts and Nevis", "Korea, Democratic People's Republic of", "Korea, Republic of",
                                                               "Kuwait", "Cayman Islands", "Kazakstan", "Lao People's Democratic Republic", "Lebanon", "Saint Lucia", "Liechtenstein", "Sri Lanka",
                                                               "Liberia", "Lesotho", "Lithuania", "Luxembourg", "Latvia", "Libyan Arab Jamahiriya", "Morocco", "Monaco", "Moldova, Republic of",
                                                               "Madagascar", "Marshall Islands", "Macedonia, the Former Yugoslav Republic of", "Mali", "Myanmar", "Mongolia", "Macau",
                                                               "Northern Mariana Islands", "Martinique", "Mauritania", "Montserrat", "Malta", "Mauritius", "Maldives", "Malawi", "Mexico", "Malaysia",
                                                               "Mozambique", "Namibia", "New Caledonia", "Niger", "Norfolk Island", "Nigeria", "Nicaragua", "Netherlands", "Norway", "Nepal", "Nauru",
                                                               "Niue", "New Zealand", "Oman", "Panama", "Peru", "French Polynesia", "Papua New Guinea", "Philippines", "Pakistan", "Poland",
                                                               "Saint Pierre and Miquelon", "Pitcairn", "Puerto Rico", "Palestinian Territory, Occupied", "Portugal", "Palau", "Paraguay", "Qatar",
                                                               "Reunion", "Romania", "Russian Federation", "Rwanda", "Saudi Arabia", "Solomon Islands", "Seychelles", "Sudan", "Sweden", "Singapore",
                                                               "Saint Helena", "Slovenia", "Svalbard and Jan Mayen", "Slovakia", "Sierra Leone", "San Marino", "Senegal", "Somalia", "Suriname",
                                                               "Sao Tome and Principe", "El Salvador", "Syrian Arab Republic", "Swaziland", "Turks and Caicos Islands", "Chad",
                                                               "French Southern Territories", "Togo", "Thailand", "Tajikistan", "Tokelau", "Turkmenistan", "Tunisia", "Tonga", "East Timor", "Turkey",
                                                               "Trinidad and Tobago", "Tuvalu", "Taiwan, Province of China", "Tanzania, United Republic of", "Ukraine", "Uganda",
                                                               "United States Minor Outlying Islands", "United States", "Uruguay", "Uzbekistan", "Holy See (Vatican City State)",
                                                               "Saint Vincent and the Grenadines", "Venezuela", "Virgin Islands, British", "Virgin Islands, U.S.", "Vietnam", "Vanuatu",
                                                               "Wallis and Futuna", "Samoa", "Yemen", "Mayotte", "Yugoslavia", "South Africa", "Zambia", "Zaire", "Zimbabwe", "Anonymous Proxy",
                                                               "Satellite Provider",
                                                           };

        private static readonly string[] CountryCode = new[]
                                                           {
                                                               "--", "AP", "EU", "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AN", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AZ", "BA", "BB", "BD", "BE",
                                                               "BF", "BG", "BH", "BI", "BJ", "BM", "BN", "BO", "BR", "BS", "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK",
                                                               "CL", "CM", "CN", "CO", "CR", "CU", "CV", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE", "EG", "EH", "ER", "ES", "ET",
                                                               "FI", "FJ", "FK", "FM", "FO", "FR", "FX", "GA", "GB", "GD", "GE", "GF", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU",
                                                               "GW", "GY", "HK", "HM", "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IN", "IO", "IQ", "IR", "IS", "IT", "JM", "JO", "JP", "KE", "KG", "KH",
                                                               "KI", "KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", "MD", "MG",
                                                               "MH", "MK", "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA", "NC", "NE", "NF", "NG", "NI",
                                                               "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE",
                                                               "RO", "RU", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "ST", "SV", "SY", "SZ", "TC",
                                                               "TD", "TF", "TG", "TH", "TJ", "TK", "TM", "TN", "TO", "TP", "TR", "TT", "TV", "TW", "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC",
                                                               "VE", "VG", "VI", "VN", "VU", "WF", "WS", "YE", "YT", "YU", "ZA", "ZM", "ZR", "ZW", "A1", "A2",
                                                           };

        private static long countryBegin = 16776960;

        /// <summary>Initializes a new instance of the <see cref="CountryLookup"/> class.</summary>
        /// <param name="ms">A memory stream with the GeoIP data file.</param>
        public CountryLookup(MemoryStream ms)
        {
            this.m_MemoryStream = ms;
        }

        /// <summary>Initializes a new instance of the <see cref="CountryLookup"/> class.</summary>
        /// <param name="fileLocation">The file path to the GeoIP data file.</param>
        public CountryLookup(string fileLocation)
        {
            //------------------------------------------------------------------------------------------------
            // Load the passed in GeoIP Data file to the memorystream
            //------------------------------------------------------------------------------------------------
            using (var fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
            {
                this.m_MemoryStream = new MemoryStream();
                var bytes = new byte[256];
                while (fileStream.Read(bytes, 0, bytes.Length) != 0)
                {
                    this.m_MemoryStream.Write(bytes, 0, bytes.Length);
                }

                fileStream.Close();
            }
        }

        /// <summary>Gets the GeoIP data file stream.</summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public MemoryStream m_MemoryStream { get; }
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>Reads the file into memory.</summary>
        /// <param name="fileLocation">The path to the file.</param>
        /// <returns>A <see cref="MemoryStream"/> instance.</returns>
        public static MemoryStream FileToMemory(string fileLocation)
        {
            // Read a given file into a Memory Stream to return as the result
            var memStream = new MemoryStream();
            var bytes = new byte[256];
            try
            {
                FileStream fileStream;
                using (fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
                {
                    while (fileStream.Read(bytes, 0, bytes.Length) != 0)
                    {
                        memStream.Write(bytes, 0, bytes.Length);
                    }

                    fileStream.Close();
                }
            }
            catch (FileNotFoundException exc)
            {
                throw new Exception(exc.Message +
                                    "  Please set the \"GeoIPFile\" Property to specify the location of this file.  The property value must be set to the virtual path to GeoIP.dat (i.e. \"/controls/CountryListBox/Data/GeoIP.dat\")");
            }

            return memStream;
        }

        /// <summary>
        /// Get CountryCode by CountryName.
        /// </summary>
        /// <param name="stringValue">CountryName.</param>
        /// <returns>CountryCode.</returns>
        public static string CodeByName(string stringValue)
        {
            for (int i = 0; i < CountryName.Length; i++)
            {
                if (stringValue == CountryName[i])
                {
                    return CountryCode[i];
                }
            }

            return "--";
        }

        /// <summary>Looks up the country code from an IP address.</summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <returns>The country code, e.g. <c>"US"</c>.</returns>
        public string LookupCountryCode(IPAddress ipAddress)
        {
            // Look up the country code, e.g. US, for the passed in IP Address
            return CountryCode[Convert.ToInt32(this.SeekCountry(0, this.ConvertIPAddressToNumber(ipAddress), 31))];
        }

        /// <summary>Looks up the country code from an IP address.</summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <returns>The country code, e.g. <c>"US"</c>, or <c>"--"</c>.</returns>
        public string LookupCountryCode(string ipAddress)
        {
            // Look up the country code, e.g. US, for the passed in IP Address
            IPAddress address;
            try
            {
                address = IPAddress.Parse(ipAddress);
            }
            catch (FormatException)
            {
                return "--";
            }

            return this.LookupCountryCode(address);
        }

        /// <summary>Looks up the country name from an IP address.</summary>
        /// <param name="addr">The IP address.</param>
        /// <returns>The country name.</returns>
        public string LookupCountryName(IPAddress addr)
        {
            // Look up the country name, e.g. United States, for the IP Address
            return CountryName[Convert.ToInt32(this.SeekCountry(0, this.ConvertIPAddressToNumber(addr), 31))];
        }

        /// <summary>Looks up the country name from an IP address.</summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <returns>The country name, or <c>"N/A"</c>.</returns>
        public string LookupCountryName(string ipAddress)
        {
            // Look up the country name, e.g. United States, for the IP Address
            IPAddress address;
            try
            {
                address = IPAddress.Parse(ipAddress);
            }
            catch (FormatException)
            {
                return "N/A";
            }

            return this.LookupCountryName(address);
        }

        /// <summary>Gets the country lookup index.</summary>
        /// <param name="offset">The offset.</param>
        /// <param name="ipNum">The IP address as a number.</param>
        /// <param name="depth">The depth.</param>
        /// <returns>The index.</returns>
        public int SeekCountry(int offset, long ipNum, short depth)
        {
            try
            {
                var buffer = new byte[6];
                var x = new int[2];
                short i;
                short j;
                byte y;
                if (depth == 0)
                {
                    throw new Exception();
                }

                this.m_MemoryStream.Seek(6 * offset, 0);
                var len = this.m_MemoryStream.Read(buffer, 0, 6);
                if (len == 6)
                {
                    for (i = 0; i <= 1; i++)
                    {
                        x[i] = 0;
                        for (j = 0; j <= 2; j++)
                        {
                            y = buffer[(i * 3) + j];
                            if (y < 0)
                            {
                                y = Convert.ToByte(y + 256);
                            }

                            x[i] = Convert.ToInt32(x[i] + (y << (j * 8)));
                        }
                    }
                }
                else
                {
                    for (i = 0; i < 6; i++)
                    {
                        x[i] = 0;
                    }
                }

                if ((ipNum & (1 << depth)) > 0)
                {
                    if (x[1] >= countryBegin)
                    {
                        return Convert.ToInt32(x[1] - countryBegin);
                    }

                    return this.SeekCountry(x[1], ipNum, Convert.ToInt16(depth - 1));
                }
                else
                {
                    if (x[0] >= countryBegin)
                    {
                        return Convert.ToInt32(x[0] - countryBegin);
                    }

                    return this.SeekCountry(x[0], ipNum, Convert.ToInt16(depth - 1));
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Error seeking country: " + exc.Message);
            }
        }

        private long ConvertIPAddressToNumber(IPAddress ipAddress)
        {
            // Convert an IP Address, (e.g. 127.0.0.1), to the numeric equivalent
            string[] address = ipAddress.ToString().Split('.');
            if (address.Length == 4)
            {
                return Convert.ToInt64((16777216 * Convert.ToDouble(address[0])) + (65536 * Convert.ToDouble(address[1])) + (256 * Convert.ToDouble(address[2])) + Convert.ToDouble(address[3]));
            }
            else
            {
                return 0;
            }
        }

        private string ConvertIPNumberToAddress(long ipNumber)
        {
            // Convert an IP Number to the IP Address equivalent
            string ipNumberPart1 = Convert.ToString(((int)(ipNumber / 16777216)) % 256);
            string ipNumberPart2 = Convert.ToString(((int)(ipNumber / 65536)) % 256);
            string ipNumberPart3 = Convert.ToString(((int)(ipNumber / 256)) % 256);
            string ipNumberPart4 = Convert.ToString(((int)ipNumber) % 256);
            return ipNumberPart1 + "." + ipNumberPart2 + "." + ipNumberPart3 + "." + ipNumberPart4;
        }
    }
}
