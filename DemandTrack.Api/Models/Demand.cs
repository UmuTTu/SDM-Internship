using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Api.Models;


[Index(nameof(TalepNo), IsUnique = true)]// Talep no benzersiz
public class Demand
{
    [Key]// Veritabanı id'si, sadece backend içinde kullanılır
    public int Id { get; set; }
    public string TalepNo { get; set; } = string.Empty;// TN + gün + saat + dakika (ör. TN121435)
    public string Başlık { get; set; } = string.Empty;
    public string Açıklama { get; set; } = string.Empty;
    public string Oluşturan_Kişi { get; set; } = string.Empty;
    public string Durum { get; set; } = string.Empty;
    public DateOnly Oluşturma_Tarihi { get; set; }

    public Demand() { }
    public Demand(string talepNo, string başlık, string açıklama, string oluşturan_Kişi, string durum, DateOnly oluşturma_Tarihi)
    {
        TalepNo = talepNo;
        Başlık = başlık;
        Açıklama = açıklama;
        Oluşturan_Kişi = oluşturan_Kişi;
        Durum = durum;
        Oluşturma_Tarihi = oluşturma_Tarihi;
    }
}