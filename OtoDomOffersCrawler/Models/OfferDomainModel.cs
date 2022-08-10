using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoDomOffersCrawler.Models
{
    public class OfferDomainModelList
    {
        public List<DateTime> PricePerDateCaptionList { get; set; } = new List<DateTime>();

        public List<OfferDomainModel> Offers { get; set; } = new List<OfferDomainModel>();

        public string GetCsvHeader() => $"Link do oferty;Tytuł;Cena za m2;Metraż;Czynsz;Pokoje;Ogrzewanie;{string.Join(';', PricePerDateCaptionList.Select(p => $"Cena z {p.ToShortDateString()}"))}";        

        private static int NumberOfNonPriceDataColumns = 7;

        public static OfferDomainModelList CreateFromCsvHeaderStringLine(string csvHeaderLine)
        {
            var values = csvHeaderLine.Split(';') ?? throw new Exception("Error during csv file header read");
            var csvDateHeaders = values.TakeLast(values.Length - NumberOfNonPriceDataColumns);

            return new OfferDomainModelList()
            {
                PricePerDateCaptionList = csvDateHeaders.Select(p => DateTime.Parse(p.Replace("Cena z ", string.Empty))).ToList(),
                Offers = new List<OfferDomainModel>(),
            };
        }

        public void AddOfferFromCsvStringLine(string csvLine)
        {
            var values = csvLine.Split(';') ?? throw new Exception("Error during csv file line read");
            IEnumerable<string?> csvDateValues = values.TakeLast(values.Length - NumberOfNonPriceDataColumns);

            Offers.Add(new OfferDomainModel()
            {
                Url = values[0],
                Title = values[1],
                Surface = float.Parse(values[3]),
                RentAmount = int.Parse(values[4]),
                NumberOfRooms = int.Parse(values[5]),
                HeatingType = values[6],
                PricePerDateList = csvDateValues.Select(p => ToNullableInt(p)).ToList(),
            });
        }

        public void AddOffer(OfferDomainModel offerToAdd)
        {
            if(Offers.Select(p => p.Title).Contains(offerToAdd.Title))
            {
                var offer = Offers.Where(p => p.Title == offerToAdd.Title).First() ?? throw new Exception();

                offer.PricePerDateList.Add(offerToAdd.PricePerDateList.Last());
            }
            else
            {
                var priceToAdd = offerToAdd.PricePerDateList.Last();
                offerToAdd.PricePerDateList = PricePerDateCaptionList.Select(p => (int?)null).ToList();
                offerToAdd.PricePerDateList[offerToAdd.PricePerDateList.Count - 1] = priceToAdd;
                Offers.Add(offerToAdd);
            }
        }

        public static int? ToNullableInt(string? s)
        {
            int i;
            if (s == null) return null;
            if (int.TryParse(s, out i)) return i;
            return null;
        }
    }

    public class OfferDomainModel
    {
        public string Url { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        //public int PricePerSquareMeter { get; set; }

        public float Surface { get; set; }

        public int RentAmount { get; set; }

        public int NumberOfRooms { get; set; }

        public string HeatingType { get; set; } = string.Empty;

        public List<int?> PricePerDateList { get; set; } = new List<int?>();

        public string GetCsvRow() => $"{Url};{Title};=ROUND([@Cena]/[@Metraż], 0);{Surface};{RentAmount};{NumberOfRooms};{HeatingType};{string.Join(';', PricePerDateList.Select(p => p.ToString()))}";        
    }
}
