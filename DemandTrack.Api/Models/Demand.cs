using System.ComponentModel.DataAnnotations;

namespace DemandTrack.Api.Models;


public class Demand
{
    [Key]// Talep no benzersiz 
    public int TalepNo { get; set; }
    public string Başlık { get; set; } = string.Empty;
    public string Açıklama { get; set; } = string.Empty;
    public string Oluşturan_Kişi { get; set; } = string.Empty;
    public string Durum { get; set; } = string.Empty;
    public DateOnly Oluşturma_Tarihi { get; set; }

    public Demand() { }
    public Demand(int talepNo, string başlık, string açıklama, string oluşturan_Kişi, string durum, DateOnly oluşturma_Tarihi)
    {
        TalepNo = talepNo;
        Başlık = başlık;
        Açıklama = açıklama;
        Oluşturan_Kişi = oluşturan_Kişi;
        Durum = durum;
        Oluşturma_Tarihi = oluşturma_Tarihi;
    }
}