namespace DemandTrack.Api.Endpoints;
using System.Diagnostics;
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
    // GET DEMANDS
group.MapGet("/", async (DemandContext dbContext) =>
    await dbContext.Demands.Select(d => ToDto(d)).ToListAsync()).WithSummary("Tüm talepleri yazdırır.");

//GET DEMANDS/1
group.MapGet("/{TalepNos:int}", async (int TalepNos, DemandContext dbContext) =>
{
    var demand = await dbContext.Demands.FindAsync(TalepNos);
   return demand is null
    ? Results.NotFound(new { mesaj = $"{TalepNos} numaralı talep bulunamadı." })
    : Results.Ok(ToDto(demand));
})
.WithName(EndpointName)
.WithSummary("Talep numarasına göre tek bir talebi getirir");


// GET /demands/report (Swagger'da görünmez)
group.MapGet("/report", async (IWebHostEnvironment env, ILogger<Program> logger) =>
{
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
        return Results.Problem("Python bulunamadı. Python'un kurulu olduğundan ve 'py' komutunun çalıştığından emin olun.",
            title: "Rapor oluşturulamadı");
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
    await dbContext.SaveChangesAsync();

    return Results.CreatedAtRoute(EndpointName, new { TalepNos = newdemands.TalepNo }, ToDto(newdemands));
})
.WithSummary("Yeni talep oluşturur")
.WithDescription("Durum: Yeni, Onaylandı, Reddedildi veya Revizyon Bekliyor. Talep No otomatik verilir.");

//put demands
group.MapPut("/{TalepNos:int}", async (int TalepNos, Update_Demand_DTO update_demand, DemandContext dbContext) =>
{
    var demand = await dbContext.Demands.FindAsync(TalepNos);
    if (demand is null)
    {
           return Results.NotFound(new { mesaj = $"{TalepNos} numaralı talep bulunamadı." });
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
group.MapDelete("/{TalepNos:int}", async (int TalepNos, DemandContext dbContext) =>
{
    var silinenSayisi = await dbContext.Demands.Where(d => d.TalepNo == TalepNos).ExecuteDeleteAsync();
    return silinenSayisi == 0
    ? Results.NotFound(new { mesaj = $"{TalepNos} numaralı talep bulunamadı." })
    : Results.NoContent();

})
.WithSummary("Seçilen talebi siler")
.WithDescription("Talep bulunamazsa 404 döner.");
}   
}   