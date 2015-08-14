using System;
using Microsoft.SPOT;
using System.Collections;

namespace uPLibrary.IoT.ThingSpeak
{
    public class DataEntry
    {
        // const string separator
        private const char CR = '\r';
        private const char LF = '\n';

        /// <summary>
        /// Date/Time
        /// </summary>
        public DateTime DateTime { get; internal set; }

        /// <summary>
        /// Entry Id returned from the server after channel update
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Fields values
        /// </summary>
        public string[] Fields { get; internal set; }

        /// <summary>
        /// Status update message
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Twitter username linked to ThingTweet
        /// </summary>
        public string Twitter { get; set; }

        /// <summary>
        /// Twitter status update
        /// </summary>
        public string Tweet { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DataEntry()
        {
            this.Fields = new string[ThingSpeakClient.THING_SPEAK_MAX_FIELDS];
        }

        /// <summary>
        /// Parse data entries in CSV format
        /// </summary>
        /// <param name="data">Data entries in CSV format</param>
        /// <returns>List of all data entries</returns>
        public static ArrayList ParseCsv(string data)
        {
            ArrayList dataEntries = new ArrayList();
            int i = 0;
            
            // split response lines on line feed
            string[] lines = data.Split(LF);
            
            // extract column headers
            string[] headers = lines[i++].Split(',');

            while (i < lines.Length)
            {
                // extract token on current line
                string[] tokens = lines[i].Split(',');
                DataEntry dataEntry = new DataEntry();

                // parse all tokens
                for (int j = 0; j < tokens.Length; j++)
                {
                    // date/time
                    if (headers[j] == "created_at")
                    {
                        // date/time in UTC format
                        dataEntry.DateTime = DataEntry.Parse(tokens[j]);
                    }
                    // entry id
                    else if (headers[j] == "entry_id")
                    {
                        dataEntry.Id = Convert.ToInt32(tokens[j]);
                    }
                    // any field (field1, field2, .... field8)
                    else if (headers[j].Substring(0, 5) == "field")
                    {
                        int fieldIdx = Convert.ToInt32(headers[j].Substring(5, 1));
                        dataEntry.Fields[fieldIdx - 1] = tokens[j];
                    }
                    // latitude
                    else if (headers[j] == "latitude")
                    {
                        if (tokens[j] != String.Empty)
                        {
                            if (dataEntry.Location == null)
                                dataEntry.Location = new Location();
                            dataEntry.Location.Latitude = Convert.ToDouble(tokens[j]);
                        }
                    }
                    // longitude
                    else if (headers[j] == "longitude")
                    {
                        if (tokens[j] != String.Empty)
                        {
                            if (dataEntry.Location == null)
                                dataEntry.Location = new Location();
                            dataEntry.Location.Longitude = Convert.ToDouble(tokens[j]);
                        }
                    }
                    // elevation
                    else if (headers[j] == "elevation")
                    {
                        if (tokens[j] != String.Empty)
                        {
                            if (dataEntry.Location == null)
                                dataEntry.Location = new Location();
                            dataEntry.Location.Elevation = Convert.ToDouble(tokens[j]);
                        }
                    }
                    else if (headers[j] == "location")
                    {
                        // TODO
                    }
                    // status
                    else if (headers[j] == "status")
                    {
                        if (tokens[j] != String.Empty)
                            dataEntry.Status = tokens[j];
                    }
                }

                dataEntries.Add(dataEntry);
                i++;
            }

            return dataEntries;
        }

        /// <summary>
        /// Parse date/time in UTC format from server
        /// </summary>
        /// <param name="dateTime">Date/Time in UTC format</param>
        /// <returns>DateTime object in UTC format</returns>
        private static DateTime Parse(string dateTime)
        {
            // split parts
            string[] tokens = dateTime.Split(' ');

            // split date parts
            string[] date = tokens[0].Split('-');
            // split time parts
            string[] time = tokens[1].Split(':');

            return new DateTime(Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2]),
                Convert.ToInt32(time[0]), Convert.ToInt32(time[1]), Convert.ToInt32(time[2]));
        }
    }
}
