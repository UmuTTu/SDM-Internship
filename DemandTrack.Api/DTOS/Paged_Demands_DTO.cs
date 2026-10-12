namespace DemandTrack.Api.DTOS;

//For paging
public record Paged_Demands_DTO(
    List<DemandDTO> Items,
    int TotalCount,
    int Page,
    int PageSize
);
