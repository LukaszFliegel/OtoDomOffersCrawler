using CommandLine;
using HtmlAgilityPack;
using OtoDomOffersCrawler.Models;
using OtoDomOffersCrawler.Verbs;
using System.Globalization;

namespace OtoDomOffersCrawler 
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<CsvVerb>(args)
                   .WithParsedAsync<CsvVerb>(async verb =>
                   {
                       await DumpOffersToCsv(verb);
                   });
        }

        private static async Task DumpOffersToCsv(CsvVerb verb)
        {
            var web = new HtmlWeb();
            var doc = web.Load($"{verb.MainUrl}{verb.OffersUrl}");
            var offersAnchorHrefs = doc.DocumentNode.SelectNodes("//div[2][@data-cy='search.listing']/ul/li/a");

            var readOfferModelList = new OfferDomainModelList();

            if (File.Exists(verb.InputFilePath))
            {
                using (var stream = new StreamReader(verb.InputFilePath))
                {
                    var headerLine = await stream.ReadLineAsync() ?? throw new InvalidOperationException("Csv file does not contain proper header");

                    readOfferModelList = OfferDomainModelList.CreateFromCsvHeaderStringLine(headerLine);

                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync() ?? throw new InvalidOperationException("Error eading line from csv file");
                        
                        readOfferModelList.AddOfferFromCsvStringLine(line);
                    }
                }
            }

            var todayDate = DateTime.Today.ToShortDateString();

            if(readOfferModelList.PricePerDateCaptionList.Contains(DateTime.Today))
            {
                Console.WriteLine("Csv file contains offers with today's date");
                return;
            }

            readOfferModelList.PricePerDateCaptionList.Add(DateTime.Today);

            foreach (var href in offersAnchorHrefs)
            {
                var url = $"{verb.MainUrl}{href.Attributes.Where(p => p.Name == "href").Select(p => p.Value).FirstOrDefault()}";
                Console.WriteLine(url);
                var model = await DumpOffer(url);

                readOfferModelList.AddOffer(model);
            }

            using (var stream = new StreamWriter(verb.OutputFilePath))
            {
                await stream.WriteLineAsync(readOfferModelList.GetCsvHeader());

                foreach (var offer in readOfferModelList.Offers)
                {
                    await stream.WriteLineAsync(offer.GetCsvRow());
                }
            }
        }

        private static async Task<OfferDomainModel> DumpOffer(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            
            // sanity check code - sometimes title is null, looks like page is not alway loaded by HtmlAgilityPack
            if (doc.DocumentNode.SelectNodes("//h1[@data-cy='adPageAdTitle']") == null)
            {
                Console.WriteLine($"null title on {url}, retrying");

                await Task.Delay(3000);

                doc = await web.LoadFromWebAsync(url);
                if (doc.DocumentNode.SelectNodes("//h1[@data-cy='adPageAdTitle']") == null)
                {
                    throw new Exception($"Surprising exception on {url}");
                }

                Console.WriteLine($"After retry title is loaded");
            }

            var model = new OfferDomainModel();
            model.Url = url;

            model.Title = doc.DocumentNode.SelectNodes("//h1[@data-cy='adPageAdTitle']").First().InnerText;

            var price = doc.DocumentNode.SelectNodes("//strong[@data-cy='adPageHeaderPrice']").First().InnerText.Replace("zł", string.Empty).Replace(" ", string.Empty);
            model.PricePerDateList.Add(int.Parse(price));

            //var pricePerSquareMeter = doc.DocumentNode.SelectNodes("//div[@aria-label='Cena za metr kwadratowy']").First().InnerText.Replace("zł/m²", string.Empty).Replace(" ", string.Empty);
            //model.PricePerSquareMeter = int.Parse(pricePerSquareMeter);

            var numberOfRooms = doc.DocumentNode.SelectNodes("//div[@aria-label='Liczba pokoi']/div[2]/div").First().InnerText.Replace(" ", string.Empty);
            model.NumberOfRooms = int.Parse(numberOfRooms);

            var surface = doc.DocumentNode.SelectNodes("//div[@aria-label='Powierzchnia']/div[2]/div").First().InnerText.Replace("m²", string.Empty).Replace(" ", string.Empty);
            model.Surface = float.Parse(surface, NumberStyles.Any, CultureInfo.CurrentCulture);

            var rentAmountNode = doc.DocumentNode.SelectNodes("//div[@aria-label='Czynsz']/div[2]/div");
            if (rentAmountNode != null)
            { 
                var rentAmount = rentAmountNode.First().InnerText.Replace("zł", string.Empty).Replace(" ", string.Empty);
                if (int.TryParse(rentAmount, out var rentAmountInt))
                {
                    model.RentAmount = rentAmountInt;
                }
            }

            var heatingTypeNode = doc.DocumentNode.SelectNodes("//div[@aria-label='Ogrzewanie']/div[2]/div");
            if (heatingTypeNode != null)
            {
                model.HeatingType = heatingTypeNode.First().InnerText.Replace("Zapytaj", string.Empty).Replace(" ", string.Empty);
            }

            return model;
        }
    }
}