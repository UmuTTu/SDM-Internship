using System.Data;
namespace DemandTrack.Api.DTOS;
//DTO's are contract between the client and the server
//Shared agreement of data transfer and use of it
public record /*clasas*/ DemandDTO
(
    int TalepNo,
    string  Başlık,
    string Açıklama,
    string Oluşturan_Kişi,
    string Durum,//Options: Yeni-Onaylandı-Reddedildi-Revizyon bekliyor
    DateOnly Oluşturma_Tarihi
    
    
);