using System.Data.Common;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DemandTrack.Api.Data
{
    // SQLite varsayılan olarak Ç, Ş, İ gibi harfleri Z'den sonraya, küçük harfleri büyüklerden sonraya dizer.
    // Her bağlantı açıldığında Türkçe alfabe sırasına göre, büyük/küçük harf ayırmadan karşılaştıran bir collation ekler.
    public class TurkishCollationInterceptor : DbConnectionInterceptor
    {
        public const string CollationName = "TURKISH";

        static readonly CompareInfo TurkishCompareInfo = new CultureInfo("tr-TR").CompareInfo;

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData) =>
            AddCollation(connection);

        public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            AddCollation(connection);
            return Task.CompletedTask;
        }

        static void AddCollation(DbConnection connection) =>
            ((SqliteConnection)connection).CreateCollation(CollationName,
                (x, y) => TurkishCompareInfo.Compare(x, y, CompareOptions.IgnoreCase));
    }
}
