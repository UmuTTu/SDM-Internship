namespace DemandTrack.Api.DTOS;

//Durum geçmişindeki tek bir değişiklik
public record Status_Log_DTO(
    string OldStatus,
    string NewStatus,
    DateTime ChangedAt
);
