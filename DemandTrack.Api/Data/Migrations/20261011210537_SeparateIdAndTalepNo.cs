using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemandTrack.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeparateIdAndTalepNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski TalepNo sütunu veritabanı id'si olur, mevcut kayıtların id'leri korunur
            migrationBuilder.RenameColumn(
                name: "TalepNo",
                table: "Demands",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "TalepNo",
                table: "Demands",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Mevcut kayıtların saati bilinmediği için 00:00 kabul edilir: TN + gün + 0000,
            // aynı güne ait her sonraki kayıt için +1 (ör. TN290000, TN290001)
            migrationBuilder.Sql(@"
                UPDATE Demands SET TalepNo = (
                    SELECT 'TN' || printf('%06d', numbered.Number)
                    FROM (
                        SELECT Id,
                               CAST(substr(Oluşturma_Tarihi, 9, 2) AS INTEGER) * 10000
                               + ROW_NUMBER() OVER (PARTITION BY substr(Oluşturma_Tarihi, 9, 2) ORDER BY Id) - 1 AS Number
                        FROM Demands
                    ) AS numbered
                    WHERE numbered.Id = Demands.Id
                );");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_TalepNo",
                table: "Demands",
                column: "TalepNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Demands_TalepNo",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "TalepNo",
                table: "Demands");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Demands",
                newName: "TalepNo");
        }
    }
}
