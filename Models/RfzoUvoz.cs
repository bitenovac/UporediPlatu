using System;
using System.ComponentModel.DataAnnotations;

namespace UporediPlate.Models
{
    public class RfzoUvoz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(13)]
        public string Jmbg { get; set; } = string.Empty;

        public string? Lbo { get; set; }
        public string? Prezime { get; set; }
        public string? Ime { get; set; }
        public string? NazivSpreme { get; set; }
        public string? NazivSpecijalnosti { get; set; }
        public string? NazivSubSpecijalnosti { get; set; }
        public string? NazivZvanja { get; set; }
        
        // Датуми подешени као DateTime
        public DateTime? DatumZaposlenja { get; set; }
        
        public string? NazivZanimanja { get; set; }
        public string? NazivRV { get; set; }
        public string? NazivRukovodioc { get; set; }
        public string? NazivZZ { get; set; }
        public string? NazivRM { get; set; }
        
        public decimal? Koef1 { get; set; }
        public decimal? DodKzaRuk { get; set; }
        public decimal? DodKzaSiNzv { get; set; }
        public decimal? Dodatni036 { get; set; }
        public decimal? UkrsG { get; set; }
        public decimal? UkrsM { get; set; }
        
        public string? NapRadnik { get; set; }
        public string? NapRz { get; set; }
        
        public decimal? ProcRVr { get; set; }
        public string? NazivOJ { get; set; }
        public string? SifraZu { get; set; }
        public string? NazivZU { get; set; }
        public int? PovecanjeId { get; set; }
        
        public DateTime? DatumKapitacija { get; set; }
        public DateTime? DatumPrestankaRO { get; set; }
        public decimal? NaSpecijalizaciji { get; set; }

        // Поља за ревизију и обрачунски период
        public DateTime PeriodUvoza { get; set; }        // Месец и година за коју важи плата
        public DateTime DatumUvozaPodataka { get; set; } // Тачно време када је корисник учитао фајл
        public string? NazivFajla { get; set; } // Чува име фајла, нпр. exportLC_00220001.xls

    }
}
