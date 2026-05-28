using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SocialListening.API.Data;

#nullable disable

namespace SocialListening.API.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260527210000_AddBrandMonitoring")]
    public partial class AddBrandMonitoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrandProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Aliases = table.Column<string>(type: "text", nullable: false),
                    Competitors = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BrandSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandProfileId = table.Column<int>(type: "integer", nullable: false),
                    Query = table.Column<string>(type: "text", nullable: false),
                    TotalMentions = table.Column<int>(type: "integer", nullable: false),
                    Engagement = table.Column<int>(type: "integer", nullable: false),
                    Positive = table.Column<int>(type: "integer", nullable: false),
                    Negative = table.Column<int>(type: "integer", nullable: false),
                    Neutral = table.Column<int>(type: "integer", nullable: false),
                    PlatformsJson = table.Column<string>(type: "text", nullable: false),
                    TopTermsJson = table.Column<string>(type: "text", nullable: false),
                    ItemsJson = table.Column<string>(type: "text", nullable: false),
                    RecommendationsJson = table.Column<string>(type: "text", nullable: false),
                    AiInsightsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandSnapshots_BrandProfiles_BrandProfileId",
                        column: x => x.BrandProfileId,
                        principalTable: "BrandProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrandProfiles_Name",
                table: "BrandProfiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BrandSnapshots_BrandProfileId",
                table: "BrandSnapshots",
                column: "BrandProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrandSnapshots");

            migrationBuilder.DropTable(
                name: "BrandProfiles");
        }
    }
}
