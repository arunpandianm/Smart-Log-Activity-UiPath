using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DeveloperLog
{
    [MetadataType(typeof(SmartLog))]
    [DisplayName("Smart Log")]
    public class SmartLog : CodeActivity
    {
        [Category("File")]
        [RequiredArgument]
        [DisplayName("Unique Name")]
        [Description("Enter the Transaction ID or Key, which will be Unique for the each transaction")]
        public InArgument<string> UniqueName { get; set; }

        [Category("File")]
        [RequiredArgument]
        [DisplayName("Path")]
        [Description("Enter the Valid Path or Sharepoint path")]
        public InArgument<string> Path { get; set; }

        [Category("Log Message")]
        [RequiredArgument]
        [DisplayName("Message")]
        [Description("Enter the Log Message")]
        public InArgument<string> Message { get; set; }

        [Category("Log Message")]
        [RequiredArgument]
        [DisplayName("Level")]
        [Description("Select the Level based on the Severity of the Log Message")]
        public LevelOption Level { get; set; }

        //Core Logic
        protected override void Execute(CodeActivityContext context)
        {
            //Variable Initializing
            var strPath = Path.Get(context);
            var strHealthyPath = strPath + "\\Healthy";
            var strErrorPath = strPath + "\\Quarantine";

            var strUniqueName = UniqueName.Get(context);
            var strMessage = Message.Get(context);
            var strLevel = this.Level;

            //Check whether the given directory/path exist or not
            //Create User Defined Path - if not exist
            if (!Directory.Exists(strPath))
                Directory.CreateDirectory(strPath);

            //Create Healthy folder -  if not exist
            if (!Directory.Exists(strHealthyPath))
                Directory.CreateDirectory(strHealthyPath);

            //Log message is converted to HTML format and saved as HTML file
            StringBuilder buildHtml = new StringBuilder();

            buildHtml.Append("<div id='container' style='font-size:15px;font-family:arial;width: 100%;margin: 0;display: table;'><div id='row' style='display: table-row;'><div style='max-width:400px;float:left;'><div id='left' style='padding:2px;display: table-cell;text-transform: uppercase;font-weight: bold;'>");
            buildHtml.Append("<p>[ " + System.DateTime.Now + " : " + GetTzAbbreviation(TimeZone.CurrentTimeZone.StandardName) + " ]</p>");
            buildHtml.Append("</div><div style='padding:2px;display: table-cell;text-transform: uppercase;font-weight: bold;'>--</div><div id='middle' style='width:75px;text-align:center;padding: 2px;display: table-cell;text-transform: uppercase;font-weight: bold;'>");
            buildHtml.Append("<p>[ " + Convert.ToString(strLevel) + " ]");
            buildHtml.Append("</div><div style='padding:2px;display: table-cell;text-transform: uppercase;font-weight: bold;'>--</div></div><div>");

            switch (Convert.ToString(strLevel))
            {
                case "Error":
                    {
                        if (!Directory.Exists(strErrorPath))
                            Directory.CreateDirectory(strErrorPath);

                        if (File.Exists(strHealthyPath + "\\" + strUniqueName + ".html"))
                        {
                            File.Move(strHealthyPath + "\\" + strUniqueName + ".html", strErrorPath + "\\" + strUniqueName + ".html");
                        }
                        buildHtml.Append("<div id='right' style='margin:0;width:50%;padding:2px;display: table-cell;text-align:justify;text-transform: capitalize;font-weight: bold;color:red;'>");
                        break;
                    }

                case "Fatal":
                    {
                        if (!Directory.Exists(strErrorPath))
                            Directory.CreateDirectory(strErrorPath);

                        if (File.Exists(strHealthyPath + "\\" + strUniqueName + ".html"))
                        {
                            File.Move(strHealthyPath + "\\" + strUniqueName + ".html", strErrorPath + "\\" + strUniqueName + ".html");
                        }
                        buildHtml.Append("<div id='right' style='margin:0;width:50%;padding:2px;display: table-cell;text-align:justify;text-transform: capitalize;font-weight: bold;color:#770be2;'>");
                        break;
                    }
                case "Trace":
                    {
                        buildHtml.Append("<div id='right' style='margin:0;width:50%;padding:2px;display: table-cell;text-align:justify;text-transform: capitalize;font-weight: bold;color:#2283D8;'>");
                        break;
                    }
                case "Warn":
                    {
                        buildHtml.Append("<div id='right' style='margin:0;width:50%;padding:2px;display: table-cell;text-align:justify;text-transform: capitalize;font-weight: bold;color:#f97822;'>");
                        break;
                    }
                default:
                    {
                        buildHtml.Append("<div id='right' style='margin:0;width:50%;padding:2px;display: table-cell;text-align:justify;text-transform: capitalize;font-weight: bold;color:#8E8991;'>");
                        break;
                    }
            }

            buildHtml.Append("<p>" + strMessage + "</p></div></div></div></div>");
            string sHtml = buildHtml.ToString();

            //This block is used when the log file are created for first time
            if (File.Exists(strHealthyPath + "\\" + strUniqueName + ".html"))
            {
                System.IO.File.AppendAllText(strHealthyPath + "\\" + strUniqueName + ".html", sHtml);
            }
            else if (File.Exists(strErrorPath + "\\" + strUniqueName + ".html") || Convert.ToString(strLevel).Contains("Error") || Convert.ToString(strLevel).Contains("Fatal"))
            {
                System.IO.File.AppendAllText(strErrorPath + "\\" + strUniqueName + ".html", sHtml);
            }
            else
            {
                System.IO.File.AppendAllText(strHealthyPath + "\\" + strUniqueName + ".html", sHtml);
            }
        }

        //Method used to find the Abbreviation of time zone
        static string GetTzAbbreviation(string timeZoneName)
        {
            string output = string.Empty;

            string[] timeZoneWords = timeZoneName.Split(' ');
            foreach (string timeZoneWord in timeZoneWords)
            {
                if (timeZoneWord[0] != '(')
                {
                    output += timeZoneWord[0];
                }
                else
                {
                    output += timeZoneWord;
                }
            }
            return output;
        }

        //Options in Level Dropdown list
        public enum LevelOption
        {
            Info, Trace, Warn, Error, Fatal
        }
    }
}




