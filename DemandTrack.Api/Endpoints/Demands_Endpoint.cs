namespace DemandTrack.Api.Endpoints;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using DemandTrack.Api.DTOS;
using  DemandTrack.Api.Data;
using DemandTrack.Api.Models;
using Microsoft.EntityFrameworkCore;
public static class Demands_Endpoints
{
    const string EndpointName = "getdemand";//Endpoint and name matched

    static DemandDTO ToDto(Demand d) =>
        new(d.TalepNo, d.Başlık, d.Açıklama, d.Oluşturan_Kişi, d.Durum, d.Oluşturma_Tarihi);

    static readonly int[] AllowedPageSizes = [20, 50, 100];

    // sortBy değerine göre sıralar; metin sütunları Türkçe alfabeye göre sıralanır. Geçersiz sortBy için null döner.
    static IOrderedQueryable<Demand>? SortDemands(IQueryable<Demand> query, string? sortBy, bool descending) =>
        sortBy?.ToLowerInvariant() switch
        {
            null or "" => OrderDemands(query, d => d.Id, descending),
            "demandno" => OrderDemands(query, d => d.TalepNo, descending),
            "title" => OrderDemands(query, d => EF.Functions.Collate(d.Başlık, TurkishCollationInterceptor.CollationName), descending),
            "description" => OrderDemands(query, d => EF.Functions.Collate(d.Açıklama, TurkishCollationInterceptor.CollationName), descending),
            "createdby" => OrderDemands(query, d => EF.Functions.Collate(d.Oluşturan_Kişi, TurkishCollationInterceptor.CollationName), descending),
            "status" => OrderDemands(query, d => EF.Functions.Collate(d.Durum, TurkishCollationInterceptor.CollationName), descending),
            "date" => OrderDemands(query, d => d.Oluşturma_Tarihi, descending),
            _ => null
        };

    static IOrderedQueryable<Demand> OrderDemands<TKey>(IQueryable<Demand> query, Expression<Func<Demand, TKey>> keySelector, bool descending) =>
        descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

    const string PythonNotFoundMessage = "Python bulunamadı. Python'un kurulu olduğundan ve 'py' komutunun çalıştığından emin olun.";

    // 'py' komutunun gerçekten bir Python çalıştırabildiğini kontrol eder.
    // 'py' hiç yoksa başlatılamaz; başlatıcı kurulu ama Python sürümü yoksa kendi hatasını verip sıfırdan farklı kodla çıkar.
    static async Task<bool> IsPythonAvailableAsync(ILogger logger)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "py",
            ArgumentList = { "-c", "pass" },
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        Process process;
        try
        {
            process = Process.Start(psi)!;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }

        using (process)
        {
            process.StandardInput.Close();// Başlatıcı bir onay sorusu sorarsa cevap beklemeden devam etsin
            var errorTask = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                logger.LogError("'py -c pass' 10 saniyede tamamlanmadı.");
                return false;
            }

            if (process.ExitCode == 0)
                return true;

            logger.LogError("'py' komutu Python çalıştıramadı ({ExitCode}):\n{Error}", process.ExitCode, await errorTask);
            return false;
        }
    }

    // Python traceback'ini kullanıcının anlayacağı tek satırlık bir mesaja çevirir
    static string RaporHatasiniAcikla(string stderr)
    {
        // Traceback'in son satırı asıl hatadır (ör. "PermissionError: [Errno 13] ...")
        var sonSatir = stderr.Trim().Split('\n').Last().Trim();

        if (sonSatir.StartsWith("PermissionError"))
            return "demands.csv dosyasına yazılamadı. Dosya başka bir programda (ör. Excel) açık olabilir; kapatıp tekrar deneyin.";
        if (sonSatir.Contains("no such table"))
            return "Veritabanında talepler tablosu bulunamadı. Veritabanının oluşturulduğundan emin olun.";
        if (sonSatir.StartsWith("sqlite3."))
            return "Veritabanı okunamadı: " + sonSatir;
        if (sonSatir.StartsWith("FileNotFoundError"))
            return "Rapor için gereken bir dosya ya da klasör bulunamadı: " + sonSatir;

        return sonSatir.Length > 0
            ? "Python betiği hata verdi: " + sonSatir
            : "Python betiği beklenmedik şekilde sonlandı.";
    }

public static void MapDemandsEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/demands").WithTags("Talepler");
    // GET DEMANDS?page=1&pageSize=20&sortBy=title&sortDirection=asc
group.MapGet("/", async (DemandContext dbContext, int page = 1, int pageSize = 20, string? sortBy = null, string? sortDirection = null) =>
{
    if (page < 1)
        return Results.BadRequest(new { mesaj = "Sayfa numarası 1 veya daha büyük olmalı." });
    if (!AllowedPageSizes.Contains(pageSize))
        return Results.BadRequest(new { mesaj = "Sayfa boyutu 20, 50 veya 100 olabilir." });

    var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
    var sortedDemands = SortDemands(dbContext.Demands, sortBy, descending);
    if (sortedDemands is null)
        return Results.BadRequest(new { mesaj = "Sıralama alanı demandNo, title, description, createdBy, status veya date olabilir." });

    var totalCount = await dbContext.Demands.CountAsync();
    // Aynı değere sahip talepler sayfalar arasında yer değiştirmesin diye id ile ikinci sıralama yapılır
    var items = await sortedDemands.ThenBy(d => d.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(d => ToDto(d))
        .ToListAsync();

    return Results.Ok(new Paged_Demands_DTO(items, totalCount, page, pageSize));
})
.WithSummary("Talepleri sayfa sayfa, sıralanmış olarak getirir.")
.WithDescription("pageSize: 20, 50 veya 100. sortBy: demandNo, title, description, createdBy, status, date (boşsa eklenme sırası). sortDirection: asc veya desc.");

//GET DEMANDS/TN121435
group.MapGet("/{DemandNo}", async (string DemandNo, DemandContext dbContext) =>
{
    var demand = await dbContext.Demands.FirstOrDefaultAsync(d => d.TalepNo == DemandNo);
   return demand is null
    ? Results.NotFound(new { mesaj = $"{DemandNo} numaralı talep bulunamadı." })
    : Results.Ok(ToDto(demand));
})
.WithName(EndpointName)
.WithSummary("Talep numarasına göre tek bir talebi getirir");

//GET DEMANDS/TN121435/status-history
group.MapGet("/{DemandNo}/status-history", async (string DemandNo, DemandContext dbContext) =>
{
    var demand = await dbContext.Demands.FirstOrDefaultAsync(d => d.TalepNo == DemandNo);
    if (demand is null)
        return Results.NotFound(new { mesaj = $"{DemandNo} numaralı talep bulunamadı." });

    var history = await dbContext.DemandStatusLogs
        .Where(l => l.DemandId == demand.Id)
        .OrderByDescending(l => l.Id)
        .Select(l => new Status_Log_DTO(l.OldStatus, l.NewStatus, l.ChangedAt))
        .ToListAsync();

    return Results.Ok(history);
})
.WithSummary("Talebin durum değişikliği geçmişini getirir")
.WithDescription("En yeni değişiklik en üstte. Talep bulunamazsa 404 döner.");


// GET /demands/report (Swagger'da görünmez)
group.MapGet("/report", async (IWebHostEnvironment env, ILogger<Program> logger) =>
{
    if (!await IsPythonAvailableAsync(logger))
        return Results.Problem(PythonNotFoundMessage, title: "Rapor oluşturulamadı");

    var api_root = env.ContentRootPath;
    var script_path = Path.Combine(api_root, "Python", "Report.py");

    var psi = new ProcessStartInfo
    {
        FileName = "py",
        ArgumentList = { script_path },
        WorkingDirectory = Directory.GetParent(api_root)!.FullName,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8,
    };
    psi.Environment["PYTHONIOENCODING"] = "utf-8";

    Process process;
    try
    {
        process = Process.Start(psi)!;
    }
    catch (System.ComponentModel.Win32Exception)
    {
        return Results.Problem(PythonNotFoundMessage, title: "Rapor oluşturulamadı");
    }

    using (process)
    {
        var ciktiTask = process.StandardOutput.ReadToEndAsync();
        var hataTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
            return Results.Text(await ciktiTask, "text/plain;");

        var hata = await hataTask;
        logger.LogError("Report.py {ExitCode} koduyla sonlandı:\n{Hata}", process.ExitCode, hata);
        return Results.Problem(RaporHatasiniAcikla(hata), title: "Rapor oluşturulamadı");
    }
}).WithSummary("Tüm taleplerin raporunu oluşturur")
.WithDescription("Rapor oluşturulamadıysa hata mesajını döner.");


// POST /DEMANDS
group.MapPost("/", async (Create_Demand_DTO created_demand, DemandContext dbContext) =>
{
    Demand newdemands = new()
    {
        Başlık = created_demand.Başlık,
        Açıklama = created_demand.Açıklama,
        Oluşturan_Kişi = created_demand.Oluşturan_Kişi,
        Durum = created_demand.Durum,
        Oluşturma_Tarihi = created_demand.Oluşturma_Tarihi
    };

    dbContext.Demands.Add(newdemands);

    // Aynı anda gelen iki istek aynı numarayı alırsa unique index hata verir, yeni numarayla tekrar denenir
    for (var attempt = 1; ; attempt++)
    {
        newdemands.TalepNo = await DemandNumberGenerator.GenerateAsync(dbContext);
        try
        {
            await dbContext.SaveChangesAsync();
            break;
        }
        catch (DbUpdateException) when (attempt < 3) { }
    }

    return Results.CreatedAtRoute(EndpointName, new { DemandNo = newdemands.TalepNo }, ToDto(newdemands));
})
.WithSummary("Yeni talep oluşturur")
.WithDescription("Durum: Yeni, Onaylandı, Reddedildi veya Revizyon Bekliyor. Talep No otomatik verilir (TN + gün + saat + dakika).");

//put demands
group.MapPut("/{DemandNo}", async (string DemandNo, Update_Demand_DTO update_demand, DemandContext dbContext) =>
{
    var demand = await dbContext.Demands.FirstOrDefaultAsync(d => d.TalepNo == DemandNo);
    if (demand is null)
    {
           return Results.NotFound(new { mesaj = $"{DemandNo} numaralı talep bulunamadı." });
    }

    // Durum değiştiyse geçmişe eklenir; güncellemeyle aynı SaveChanges içinde kaydedilir
    if (demand.Durum != update_demand.Durum)
    {
        dbContext.DemandStatusLogs.Add(new DemandStatusLog
        {
            DemandId = demand.Id,
            OldStatus = demand.Durum,
            NewStatus = update_demand.Durum,
            ChangedAt = DateTime.Now
        });
    }

    demand.Başlık = update_demand.Başlık;
    demand.Açıklama = update_demand.Açıklama;
    demand.Oluşturan_Kişi = update_demand.Oluşturan_Kişi;
    demand.Durum = update_demand.Durum;
    demand.Oluşturma_Tarihi = update_demand.Oluşturma_Tarihi;

    await dbContext.SaveChangesAsync();
    return Results.Ok(ToDto(demand));

})
.WithSummary("Talebin bilgilerini günceller")
.WithDescription("Tüm alanlar düzgün doldurulmalı. Talep bulunamazsa 404 döner.");

//delete demands
group.MapDelete("/{DemandNo}", async (string DemandNo, DemandContext dbContext) =>
{
    var deletedCount = await dbContext.Demands.Where(d => d.TalepNo == DemandNo).ExecuteDeleteAsync();
    return deletedCount == 0
    ? Results.NotFound(new { mesaj = $"{DemandNo} numaralı talep bulunamadı." })
    : Results.NoContent();

})
.WithSummary("Seçilen talebi siler")
.WithDescription("Talep bulunamazsa 404 döner.");

//toplu silme
group.MapPost("/bulk-delete", async (string[] demandNos, DemandContext dbContext) =>
{
    if (demandNos.Length == 0)
        return Results.BadRequest(new { mesaj = "Silinecek talep seçilmedi." });

    var deletedCount = await dbContext.Demands.Where(d => demandNos.Contains(d.TalepNo)).ExecuteDeleteAsync();
    return deletedCount == 0
    ? Results.NotFound(new { mesaj = "Seçilen taleplerin hiçbiri bulunamadı." })
    : Results.Ok(new { silinen = deletedCount });
})
.WithSummary("Seçilen talepleri toplu olarak siler")
.WithDescription("Gövde olarak talep numaralarının listesini alır (ör. [\"TN121435\", \"TN121436\"]). Silinen talep sayısını döner; hiçbiri bulunamazsa 404 döner.");
}
}   