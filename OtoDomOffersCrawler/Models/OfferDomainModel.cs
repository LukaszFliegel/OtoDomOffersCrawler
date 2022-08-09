using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoDomOffersCrawler.Models
{
    public class OfferDomainModel
    {
        public string Title { get; set; } = string.Empty;

        public int Price { get; set; }

        //public int PricePerSquareMeter { get; set; }

        public float Surface { get; set; }

        public int NumberOfRooms { get; set; }

        public int RentAmount { get; set; }

        public string HeatingType { get; set; } = string.Empty;

        public static string GetCsvHeader() => $"Link do oferty;Tytuł;Cena;Cena z m2;Metraż;Czynsz;Pokoje;Ogrzewanie";

        public string GetCsvRow(string url) => $"{url};{Title};{Price};=ROUND([@Cena]/[@Metraż], 0);{Surface};{RentAmount};{NumberOfRooms};{HeatingType}";
    }
}
