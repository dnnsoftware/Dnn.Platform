/* eslint-disable spellcheck/spell-checker */
const timeZones = [{
    value: "Dateline Standard Time",
    label: "(UTC-12:00) International Date Line West",
    ianaId: "Etc/GMT+12"
},
{
    value: "UTC-11",
    label: "(UTC-11:00) Coordinated Universal Time-11",
    ianaId: "Etc/GMT+11"
},
{
    value: "Hawaiian Standard Time",
    label: "(UTC-10:00) Hawaii",
    ianaId: "Pacific/Honolulu"
},
{
    value: "Alaskan Standard Time",
    label: "(UTC-09:00) Alaska",
    ianaId: "America/Anchorage"
},
{
    value: "Pacific Standard Time (Mexico)",
    label: "(UTC-08:00) Baja California",
    ianaId: "America/Tijuana"
},
{
    value: "Pacific Standard Time",
    label: "(UTC-08:00) Pacific Time (US & Canada)",
    ianaId: "America/Los_Angeles"
},
{
    value: "US Mountain Standard Time",
    label: "(UTC-07:00) Arizona",
    ianaId: "America/Phoenix"
},
{
    value: "Mountain Standard Time (Mexico)",
    label: "(UTC-07:00) Chihuahua, La Paz, Mazatlan",
    ianaId: "America/Chihuahua"
},
{
    value: "Mountain Standard Time",
    label: "(UTC-07:00) Mountain Time (US & Canada)",
    ianaId: "America/Denver"
},
{
    value: "Central America Standard Time",
    label: "(UTC-06:00) Central America",
    ianaId: "America/Guatemala"
},
{
    value: "Central Standard Time",
    label: "(UTC-06:00) Central Time (US & Canada)",
    ianaId: "America/Chicago"
},
{
    value: "Central Standard Time (Mexico)",
    label: "(UTC-06:00) Guadalajara, Mexico City, Monterrey",
    ianaId: "America/Mexico_City"
},
{
    value: "Canada Central Standard Time",
    label: "(UTC-06:00) Saskatchewan",
    ianaId: "America/Regina"
},
{
    value: "SA Pacific Standard Time",
    label: "(UTC-05:00) Bogota, Lima, Quito, Rio Branco",
    ianaId: "America/Bogota"
},
{
    value: "Eastern Standard Time (Mexico)",
    label: "(UTC-05:00) Chetumal",
    ianaId: "America/Cancun"
},
{
    value: "Eastern Standard Time",
    label: "(UTC-05:00) Eastern Time (US & Canada)",
    ianaId: "America/New_York"
},
{
    value: "US Eastern Standard Time",
    label: "(UTC-05:00) Indiana (East)",
    ianaId: "America/Indiana/Indianapolis"
},
{
    value: "Venezuela Standard Time",
    label: "(UTC-04:30) Caracas",
    ianaId: "America/Caracas"
},
{
    value: "Paraguay Standard Time",
    label: "(UTC-04:00) Asuncion",
    ianaId: "America/Asuncion"
},
{
    value: "Atlantic Standard Time",
    label: "(UTC-04:00) Atlantic Time (Canada)",
    ianaId: "America/Halifax"
},
{
    value: "Central Brazilian Standard Time",
    label: "(UTC-04:00) Cuiaba",
    ianaId: "America/Cuiaba"
},
{
    value: "SA Western Standard Time",
    label: "(UTC-04:00) Georgetown, La Paz, Manaus, San Juan",
    ianaId: "America/La_Paz"
},
{
    value: "Newfoundland Standard Time",
    label: "(UTC-03:30) Newfoundland",
    ianaId: "America/St_Johns"
},
{
    value: "E. South America Standard Time",
    label: "(UTC-03:00) Brasilia",
    ianaId: "America/Sao_Paulo"
},
{
    value: "SA Eastern Standard Time",
    label: "(UTC-03:00) Cayenne, Fortaleza",
    ianaId: "America/Cayenne"
},
{
    value: "Argentina Standard Time",
    label: "(UTC-03:00) City of Buenos Aires",
    ianaId: "America/Argentina/Buenos_Aires"
},
{
    value: "Greenland Standard Time",
    label: "(UTC-03:00) Greenland",
    ianaId: "America/Godthab"
},
{
    value: "Montevideo Standard Time",
    label: "(UTC-03:00) Montevideo",
    ianaId: "America/Montevideo"
},
{
    value: "Bahia Standard Time",
    label: "(UTC-03:00) Salvador",
    ianaId: "America/Bahia"
},
{
    value: "Pacific SA Standard Time",
    label: "(UTC-03:00) Santiago",
    ianaId: "America/Santiago"
},
{
    value: "UTC-02",
    label: "(UTC-02:00) Coordinated Universal Time-02",
    ianaId: "Etc/GMT+2"
},
{
    value: "Mid-Atlantic Standard Time",
    label: "(UTC-02:00) Mid-Atlantic - Old",
    ianaId: "Etc/GMT+2"
},
{
    value: "Azores Standard Time",
    label: "(UTC-01:00) Azores",
    ianaId: "Atlantic/Azores"
},
{
    value: "Cape Verde Standard Time",
    label: "(UTC-01:00) Cabo Verde Is.",
    ianaId: "Atlantic/Cape_Verde"
},
{
    value: "Morocco Standard Time",
    label: "(UTC) Casablanca",
    ianaId: "Africa/Casablanca"
},
{
    value: "UTC",
    label: "(UTC) Coordinated Universal Time",
    ianaId: "Etc/UTC"
},
{
    value: "GMT Standard Time",
    label: "(UTC) Dublin, Edinburgh, Lisbon, London",
    ianaId: "Europe/London"
},
{
    value: "Greenwich Standard Time",
    label: "(UTC) Monrovia, Reykjavik",
    ianaId: "Atlantic/Reykjavik"
},
{
    value: "W. Europe Standard Time",
    label: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna",
    ianaId: "Europe/Berlin"
},
{
    value: "Central Europe Standard Time",
    label: "(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague",
    ianaId: "Europe/Budapest"
},
{
    value: "Romance Standard Time",
    label: "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris",
    ianaId: "Europe/Paris"
},
{
    value: "Central European Standard Time",
    label: "(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb",
    ianaId: "Europe/Warsaw"
},
{
    value: "W. Central Africa Standard Time",
    label: "(UTC+01:00) West Central Africa",
    ianaId: "Africa/Lagos"
},
{
    value: "Namibia Standard Time",
    label: "(UTC+01:00) Windhoek",
    ianaId: "Africa/Windhoek"
},
{
    value: "Jordan Standard Time",
    label: "(UTC+02:00) Amman",
    ianaId: "Asia/Amman"
},
{
    value: "GTB Standard Time",
    label: "(UTC+02:00) Athens, Bucharest",
    ianaId: "Europe/Bucharest"
},
{
    value: "Middle East Standard Time",
    label: "(UTC+02:00) Beirut",
    ianaId: "Asia/Beirut"
},
{
    value: "Egypt Standard Time",
    label: "(UTC+02:00) Cairo",
    ianaId: "Africa/Cairo"
},
{
    value: "Syria Standard Time",
    label: "(UTC+02:00) Damascus",
    ianaId: "Asia/Damascus"
},
{
    value: "E. Europe Standard Time",
    label: "(UTC+02:00) E. Europe",
    ianaId: "Europe/Chisinau"
},
{
    value: "South Africa Standard Time",
    label: "(UTC+02:00) Harare, Pretoria",
    ianaId: "Africa/Johannesburg"
},
{
    value: "FLE Standard Time",
    label: "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius",
    ianaId: "Europe/Kiev"
},
{
    value: "Turkey Standard Time",
    label: "(UTC+02:00) Istanbul",
    ianaId: "Europe/Istanbul"
},
{
    value: "Israel Standard Time",
    label: "(UTC+02:00) Jerusalem",
    ianaId: "Asia/Jerusalem"
},
{
    value: "Kaliningrad Standard Time",
    label: "(UTC+02:00) Kaliningrad (RTZ 1)",
    ianaId: "Europe/Kaliningrad"
},
{
    value: "Libya Standard Time",
    label: "(UTC+02:00) Tripoli",
    ianaId: "Africa/Tripoli"
},
{
    value: "Arabic Standard Time",
    label: "(UTC+03:00) Baghdad",
    ianaId: "Asia/Baghdad"
},
{
    value: "Arab Standard Time",
    label: "(UTC+03:00) Kuwait, Riyadh",
    ianaId: "Asia/Riyadh"
},
{
    value: "Belarus Standard Time",
    label: "(UTC+03:00) Minsk",
    ianaId: "Europe/Minsk"
},
{
    value: "Russian Standard Time",
    label: "(UTC+03:00) Moscow, St. Petersburg, Volgograd (RTZ 2)",
    ianaId: "Europe/Moscow"
},
{
    value: "E. Africa Standard Time",
    label: "(UTC+03:00) Nairobi",
    ianaId: "Africa/Nairobi"
},
{
    value: "Iran Standard Time",
    label: "(UTC+03:30) Tehran",
    ianaId: "Asia/Tehran"
},
{
    value: "Arabian Standard Time",
    label: "(UTC+04:00) Abu Dhabi, Muscat",
    ianaId: "Asia/Dubai"
},
{
    value: "Azerbaijan Standard Time",
    label: "(UTC+04:00) Baku",
    ianaId: "Asia/Baku"
},
{
    value: "Russia Time Zone 3",
    label: "(UTC+04:00) Izhevsk, Samara (RTZ 3)",
    ianaId: "Europe/Samara"
},
{
    value: "Mauritius Standard Time",
    label: "(UTC+04:00) Port Louis",
    ianaId: "Indian/Mauritius"
},
{
    value: "Georgian Standard Time",
    label: "(UTC+04:00) Tbilisi",
    ianaId: "Asia/Tbilisi"
},
{
    value: "Caucasus Standard Time",
    label: "(UTC+04:00) Yerevan",
    ianaId: "Asia/Yerevan"
},
{
    value: "Afghanistan Standard Time",
    label: "(UTC+04:30) Kabul",
    ianaId: "Asia/Kabul"
},
{
    value: "West Asia Standard Time",
    label: "(UTC+05:00) Ashgabat, Tashkent",
    ianaId: "Asia/Tashkent"
},
{
    value: "Ekaterinburg Standard Time",
    label: "(UTC+05:00) Ekaterinburg (RTZ 4)",
    ianaId: "Asia/Yekaterinburg"
},
{
    value: "Pakistan Standard Time",
    label: "(UTC+05:00) Islamabad, Karachi",
    ianaId: "Asia/Karachi"
},
{
    value: "India Standard Time",
    label: "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi",
    ianaId: "Asia/Kolkata"
},
{
    value: "Sri Lanka Standard Time",
    label: "(UTC+05:30) Sri Jayawardenepura",
    ianaId: "Asia/Colombo"
},
{
    value: "Nepal Standard Time",
    label: "(UTC+05:45) Kathmandu",
    ianaId: "Asia/Kathmandu"
},
{
    value: "Central Asia Standard Time",
    label: "(UTC+06:00) Astana",
    ianaId: "Asia/Almaty"
},
{
    value: "Bangladesh Standard Time",
    label: "(UTC+06:00) Dhaka",
    ianaId: "Asia/Dhaka"
},
{
    value: "N. Central Asia Standard Time",
    label: "(UTC+06:00) Novosibirsk (RTZ 5)",
    ianaId: "Asia/Novosibirsk"
},
{
    value: "Myanmar Standard Time",
    label: "(UTC+06:30) Yangon (Rangoon)",
    ianaId: "Asia/Yangon"
},
{
    value: "SE Asia Standard Time",
    label: "(UTC+07:00) Bangkok, Hanoi, Jakarta",
    ianaId: "Asia/Bangkok"
},
{
    value: "North Asia Standard Time",
    label: "(UTC+07:00) Krasnoyarsk (RTZ 6)",
    ianaId: "Asia/Krasnoyarsk"
},
{
    value: "China Standard Time",
    label: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi",
    ianaId: "Asia/Shanghai"
},
{
    value: "North Asia East Standard Time",
    label: "(UTC+08:00) Irkutsk (RTZ 7)",
    ianaId: "Asia/Irkutsk"
},
{
    value: "Singapore Standard Time",
    label: "(UTC+08:00) Kuala Lumpur, Singapore",
    ianaId: "Asia/Singapore"
},
{
    value: "W. Australia Standard Time",
    label: "(UTC+08:00) Perth",
    ianaId: "Australia/Perth"
},
{
    value: "Taipei Standard Time",
    label: "(UTC+08:00) Taipei",
    ianaId: "Asia/Taipei"
},
{
    value: "Ulaanbaatar Standard Time",
    label: "(UTC+08:00) Ulaanbaatar",
    ianaId: "Asia/Ulaanbaatar"
},
{
    value: "North Korea Standard Time",
    label: "(UTC+08:30) Pyongyang",
    ianaId: "Asia/Pyongyang"
},
{
    value: "Tokyo Standard Time",
    label: "(UTC+09:00) Osaka, Sapporo, Tokyo",
    ianaId: "Asia/Tokyo"
},
{
    value: "Korea Standard Time",
    label: "(UTC+09:00) Seoul",
    ianaId: "Asia/Seoul"
},
{
    value: "Yakutsk Standard Time",
    label: "(UTC+09:00) Yakutsk (RTZ 8)",
    ianaId: "Asia/Yakutsk"
},
{
    value: "Cen. Australia Standard Time",
    label: "(UTC+09:30) Adelaide",
    ianaId: "Australia/Adelaide"
},
{
    value: "AUS Central Standard Time",
    label: "(UTC+09:30) Darwin",
    ianaId: "Australia/Darwin"
},
{
    value: "E. Australia Standard Time",
    label: "(UTC+10:00) Brisbane",
    ianaId: "Australia/Brisbane"
},
{
    value: "AUS Eastern Standard Time",
    label: "(UTC+10:00) Canberra, Melbourne, Sydney",
    ianaId: "Australia/Sydney"
},
{
    value: "West Pacific Standard Time",
    label: "(UTC+10:00) Guam, Port Moresby",
    ianaId: "Pacific/Port_Moresby"
},
{
    value: "Tasmania Standard Time",
    label: "(UTC+10:00) Hobart",
    ianaId: "Australia/Hobart"
},
{
    value: "Magadan Standard Time",
    label: "(UTC+10:00) Magadan",
    ianaId: "Asia/Magadan"
},
{
    value: "Vladivostok Standard Time",
    label: "(UTC+10:00) Vladivostok, Magadan (RTZ 9)",
    ianaId: "Asia/Vladivostok"
},
{
    value: "Russia Time Zone 10",
    label: "(UTC+11:00) Chokurdakh (RTZ 10)",
    ianaId: "Asia/Srednekolymsk"
},
{
    value: "Central Pacific Standard Time",
    label: "(UTC+11:00) Solomon Is., New Caledonia",
    ianaId: "Pacific/Guadalcanal"
},
{
    value: "Russia Time Zone 11",
    label: "(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky (RTZ 11)",
    ianaId: "Asia/Kamchatka"
},
{
    value: "New Zealand Standard Time",
    label: "(UTC+12:00) Auckland, Wellington",
    ianaId: "Pacific/Auckland"
},
{
    value: "UTC+12",
    label: "(UTC+12:00) Coordinated Universal Time+12",
    ianaId: "Etc/GMT-12"
},
{
    value: "Fiji Standard Time",
    label: "(UTC+12:00) Fiji",
    ianaId: "Pacific/Fiji"
},
{
    value: "Kamchatka Standard Time",
    label: "(UTC+12:00) Petropavlovsk-Kamchatsky - Old",
    ianaId: "Asia/Kamchatka"
},
{
    value: "Tonga Standard Time",
    label: "(UTC+13:00) Nuku'alofa",
    ianaId: "Pacific/Tongatapu"
},
{
    value: "Samoa Standard Time",
    label: "(UTC+13:00) Samoa",
    ianaId: "Pacific/Apia"
},
{
    value: "Line Islands Standard Time",
    label: "(UTC+14:00) Kiritimati Island",
    ianaId: "Pacific/Kiritimati"
}
];


export default timeZones;