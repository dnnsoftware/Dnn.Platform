// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.IO;
    using System.Net;

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

        private static long CountryBegin = 16776960;

        public CountryLookup(MemoryStream ms)
        {
            this.m_MemoryStream = ms;
        }

        public CountryLookup(string FileLocation)
        {
            //------------------------------------------------------------------------------------------------
            // Load the passed in GeoIP Data file to the memorystream
            //------------------------------------------------------------------------------------------------
            using (var _FileStream = new FileStream(FileLocation, FileMode.Open, FileAccess.Read))
            {
                this.m_MemoryStream = new MemoryStream();
                var _Byte = new byte[256];
                while (_FileStream.Read(_Byte, 0, _Byte.Length) != 0)
                {
                    this.m_MemoryStream.Write(_Byte, 0, _Byte.Length);
                }

                _FileStream.Close();
            }
        }

        public MemoryStream m_MemoryStream { get; }

        public static MemoryStream FileToMemory(string FileLocation)
        {
            // Read a given file into a Memory Stream to return as the result
            var _MemStream = new MemoryStream();
            var _Byte = new byte[256];
            try
            {
                FileStream _FileStream;
                using (_FileStream = new FileStream(FileLocation, FileMode.Open, FileAccess.Read))
                {
                    while (_FileStream.Read(_Byte, 0, _Byte.Length) != 0)
                    {
                        _MemStream.Write(_Byte, 0, _Byte.Length);
                    }

                    _FileStream.Close();
                }
            }
            catch (FileNotFoundException exc)
            {
                throw new Exception(exc.Message +
                                    "  Please set the \"GeoIPFile\" Property to specify the location of this file.  The property value must be set to the virtual path to GeoIP.dat (i.e. \"/controls/CountryListBox/Data/GeoIP.dat\")");
            }

            return _MemStream;
        }

        public string LookupCountryCode(IPAddress _IPAddress)
        {
            // Look up the country code, e.g. US, for the passed in IP Address
            return CountryCode[Convert.ToInt32(this.SeekCountry(0, this.ConvertIPAddressToNumber(_IPAddress), 31))];
        }

        public string LookupCountryCode(string _IPAddress)
        {
            // Look up the country code, e.g. US, for the passed in IP Address
            IPAddress _Address;
            try
            {
                _Address = IPAddress.Parse(_IPAddress);
            }
            catch (FormatException)
            {
                return "--";
            }

            return this.LookupCountryCode(_Address);
        }

        public string LookupCountryName(IPAddress addr)
        {
            // Look up the country name, e.g. United States, for the IP Address
            return CountryName[Convert.ToInt32(this.SeekCountry(0, this.ConvertIPAddressToNumber(addr), 31))];
        }

        public string LookupCountryName(string _IPAddress)
        {
            // Look up the country name, e.g. United States, for the IP Address
            IPAddress _Address;
            try
            {
                _Address = IPAddress.Parse(_IPAddress);
            }
            catch (FormatException)
            {
                return "N/A";
            }

            return this.LookupCountryName(_Address);
        }

        public int SeekCountry(int Offset, long Ipnum, short Depth)
        {
            try
            {
                var Buffer = new byte[6];
                var X = new int[2];
                short I;
                short J;
                byte Y;
                if (Depth == 0)
                {
                    throw new Exception();
                }

                this.m_MemoryStream.Seek(6 * Offset, 0);
                var len = this.m_MemoryStream.Read(Buffer, 0, 6);
                if (len == 6)
                {
                    for (I = 0; I <= 1; I++)
                    {
                        X[I] = 0;
                        for (J = 0; J <= 2; J++)
                        {
                            Y = Buffer[(I * 3) + J];
                            if (Y < 0)
                            {
                                Y = Convert.ToByte(Y + 256);
                            }

                            X[I] = Convert.ToInt32(X[I] + (Y << (J * 8)));
                        }
                    }
                }
                else
                {
                    for (I = 0; I < 6; I++)
                    {
                        X[I] = 0;
                    }
                }

                if ((Ipnum & (1 << Depth)) > 0)
                {
                    if (X[1] >= CountryBegin)
                    {
                        return Convert.ToInt32(X[1] - CountryBegin);
                    }

                    return this.SeekCountry(X[1], Ipnum, Convert.ToInt16(Depth - 1));
                }
                else
                {
                    if (X[0] >= CountryBegin)
                    {
                        return Convert.ToInt32(X[0] - CountryBegin);
                    }

                    return this.SeekCountry(X[0], Ipnum, Convert.ToInt16(Depth - 1));
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Error seeking country: " + exc.Message);
            }
        }

        private long ConvertIPAddressToNumber(IPAddress _IPAddress)
        {
            // Convert an IP Address, (e.g. 127.0.0.1), to the numeric equivalent
            string[] _Address = _IPAddress.ToString().Split('.');
            if (_Address.Length == 4)
            {
                return Convert.ToInt64((16777216 * Convert.ToDouble(_Address[0])) + (65536 * Convert.ToDouble(_Address[1])) + (256 * Convert.ToDouble(_Address[2])) + Convert.ToDouble(_Address[3]));
            }
            else
            {
                return 0;
            }
        }

        private string ConvertIPNumberToAddress(long _IPNumber)
        {
            // Convert an IP Number to the IP Address equivalent
            string _IPNumberPart1 = Convert.ToString(((int)(_IPNumber / 16777216)) % 256);
            string _IPNumberPart2 = Convert.ToString(((int)(_IPNumber / 65536)) % 256);
            string _IPNumberPart3 = Convert.ToString(((int)(_IPNumber / 256)) % 256);
            string _IPNumberPart4 = Convert.ToString(((int)_IPNumber) % 256);
            return _IPNumberPart1 + "." + _IPNumberPart2 + "." + _IPNumberPart3 + "." + _IPNumberPart4;
        }
    }
}
