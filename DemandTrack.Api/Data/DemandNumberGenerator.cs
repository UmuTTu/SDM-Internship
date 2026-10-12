using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Api.Data
{
    public static class DemandNumberGenerator
    {
        // TN + gün + saat + dakika, ör. 11. gün 14:35 -> 111435
        public static int GetBaseNumber(DateTime time) => time.Day * 10000 + time.Hour * 100 + time.Minute;

        public static string Format(int number) => $"TN{number:D6}";

        // Şu anki zamandan başlar, numara kullanılıyorsa +1 ekleyip tekrar kontrol eder
        public static async Task<string> GenerateAsync(DemandContext dbContext)
        {
            var number = GetBaseNumber(DateTime.Now);
            var demandNo = Format(number);

            while (await dbContext.Demands.AnyAsync(d => d.TalepNo == demandNo))
            {
                number++;
                demandNo = Format(number);
            }

            return demandNo;
        }
    }
}
