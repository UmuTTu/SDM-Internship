namespace DemandTrack.Api.DTOS;
using System.ComponentModel.DataAnnotations;

public record Update_Demand_DTO(
    [Required(ErrorMessage = "Başlık zorunlu.")]
    [StringLength(20, ErrorMessage = "Başlık en fazla 20 karakter olabilir.")]
    string Başlık,

    [Required(ErrorMessage = "Açıklama zorunlud.")]
    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    string Açıklama,

    [Required(ErrorMessage = "Oluşturan kişi zorunludur.")]
    [StringLength(20, ErrorMessage = "Oluşturan kişi en fazla 20 karakter olabilir.")]
    string Oluşturan_Kişi,

    [Required(ErrorMessage = "Durum zorunludur.")]
    [AllowedValues("Yeni", "Onaylandı", "Reddedildi", "Revizyon Bekliyor", // AllowedValues sadece bu 4 değere sınırlar
        ErrorMessage = "Durum sadece 'Yeni), (Onaylandı), (Reddedildi) veya (Revizyon Bekliyor)' olabilir.")]
    string Durum,
    [Validate_Date]
    DateOnly Oluşturma_Tarihi
);
