using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimViecLam.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorySlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "tblJobType",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "tblBenefit",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            // Cập nhật Slug ban đầu cho các bản ghi có sẵn trước khi tạo Unique Index
            migrationBuilder.Sql("UPDATE [tblJobType] SET [Slug] = LOWER(REPLACE([Code], ' ', '-')) WHERE [Slug] = '' OR [Slug] IS NULL;");
            migrationBuilder.Sql("UPDATE [tblBenefit] SET [Slug] = 'phuc-loi-' + CAST([BenefitId] AS nvarchar(20)) WHERE [Slug] = '' OR [Slug] IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_tblJobType_Slug",
                table: "tblJobType",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblBenefit_Slug",
                table: "tblBenefit",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tblJobType_Slug",
                table: "tblJobType");

            migrationBuilder.DropIndex(
                name: "IX_tblBenefit_Slug",
                table: "tblBenefit");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "tblJobType");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "tblBenefit");
        }
    }
}
