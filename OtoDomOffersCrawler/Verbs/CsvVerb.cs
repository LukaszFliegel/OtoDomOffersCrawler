using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoDomOffersCrawler.Verbs
{
    [Verb("csv", HelpText = "Generates CSV file with parsed offers")]
    public class CsvVerb
    {
        [Option('u', "url", Required = true, HelpText = "Url of main page")]
        public string MainUrl { get; set; } = string.Empty;

        [Option('r', "resources", Required = true, HelpText = "Resources url to parse")]
        public string OffersUrl { get; set; } = string.Empty;

        //[Option('d', "darker", Required = false, HelpText = "Make hover colors darker")]
        //public bool Darker { get; set; }

        [Option('f', "filepath", Required = false, Default = "Result.csv", HelpText = "path for the csv file to generate.")]
        public string FilePath { get; set; } = "Result.csv";

        //[Option('o', "openFile", Required = false, HelpText = "If true (which is default) generated file will be opened at the end", Default = true)]
        //public bool OpenFile { get; set; }

        //[Option('s', "strength", Required = false, HelpText = "Strength of making the hover color (darker or lighter depends on -d option). Takes values between 0 and 1.", Default = 0.5)]
        //public double Strength { get; set; }
    }
}
