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

        [Option('i', "inputfilepath", Required = false, Default = "input.csv", HelpText = "path for the csv file to read previous reads from")]
        public string InputFilePath { get; set; } = "input.csv";

        [Option('o', "outputfilepath", Required = false, Default = "output.csv", HelpText = "path for the csv file to generate")]
        public string OutputFilePath { get; set; } = "output.csv";
    }
}
