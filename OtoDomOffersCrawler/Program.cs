using CommandLine;
using HtmlAgilityPack;
using OtoDomOffersCrawler.Models;
using OtoDomOffersCrawler.Verbs;
using RestSharp;
using System;

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

            using (var stream = new StreamWriter(verb.FilePath))
            {
                await stream.WriteLineAsync(OfferDomainModel.GetCsvHeader());

                foreach (var href in offersAnchorHrefs)
                {
                    var url = $"{verb.MainUrl}{href.Attributes.Where(p => p.Name == "href").Select(p => p.Value).FirstOrDefault()}";
                    Console.WriteLine(url);
                    await DumpOffer(stream, url);
                }
            }
        }

        private static async Task DumpOffer(StreamWriter stream, string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var model = new OfferDomainModel();
            model.Title = doc.DocumentNode.SelectNodes("//h1[@data-cy='adPageAdTitle']").First().InnerText;

            var price = doc.DocumentNode.SelectNodes("//strong[@data-cy='adPageHeaderPrice']").First().InnerText.Replace("zł", string.Empty).Replace(" ", string.Empty);
            model.Price = int.Parse(price);

            //var pricePerSquareMeter = doc.DocumentNode.SelectNodes("//div[@aria-label='Cena za metr kwadratowy']").First().InnerText.Replace("zł/m²", string.Empty).Replace(" ", string.Empty);
            //model.PricePerSquareMeter = int.Parse(pricePerSquareMeter);

            var numberOfRooms = doc.DocumentNode.SelectNodes("//div[@aria-label='Liczba pokoi']/div[2]/div").First().InnerText.Replace(" ", string.Empty);
            model.NumberOfRooms = int.Parse(numberOfRooms);

            var surface = doc.DocumentNode.SelectNodes("//div[@aria-label='Powierzchnia']/div[2]/div").First().InnerText.Replace("m²", string.Empty).Replace(" ", string.Empty);
            model.Surface = float.Parse(surface.Replace(",", "."));

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

            await stream.WriteLineAsync(model.GetCsvRow(url));
        }
    }
}