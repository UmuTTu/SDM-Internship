namespace DemandTrack.Api.Models;

// Bir talebin durum değişikliği kaydı (ör. Yeni -> Onaylandı). Talep silinince kayıtları da silinir.
public class DemandStatusLog
{
    public int Id { get; set; }
    public int DemandId { get; set; }
    public Demand Demand { get; set; } = null!;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
