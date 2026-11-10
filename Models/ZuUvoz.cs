using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UporediPlate.Models
{
    [Table("ZuUvozi")]
    public class ZuUvoz
    {
        [Key]
        public int Id { get; set; }

        public string Filijala { get; set; } = string.Empty;
        public string SifraZu { get; set; } = string.Empty;
        public string NazivZu { get; set; } = string.Empty;
        public string Nivo { get; set; } = string.Empty;
        public int Mesec { get; set; }
        public string Jmbg { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string RadnoMesto { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 4)")]
        public decimal OsnovniKoef { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal UkupniKoef { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal SatiUkupno { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal PrekovremeniSati { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal DezurstvoSati { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal PoPozivuSati { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal PrekovremeniNeto { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal DezurstvoNeto { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal PoPozivuNeto { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal PripravnostSati { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal PripravnostNeto { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal UkupanNeto { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Bruto1 { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Bruto2 { get; set; }

        // Системске колоне за праћење историје увоза (идентично као за РФЗО)
        public DateTime PeriodUvoza { get; set; }
        public DateTime DatumUvozaPodataka { get; set; }
        public string NazivFajla { get; set; } = string.Empty;
    }
}
