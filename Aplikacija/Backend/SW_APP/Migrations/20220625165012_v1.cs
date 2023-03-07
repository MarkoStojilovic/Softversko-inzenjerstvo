using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SW_APP.Migrations
{
    public partial class v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Korisnik",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lozinka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    salt_value = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    korisnicko_ime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    tip = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    grad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    imgPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnik", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikID = table.Column<int>(type: "int", nullable: true),
                    ime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    prezime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Admin_Korisnik_KorisnikID",
                        column: x => x.KorisnikID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Oglasi",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    naziv = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    tehnologija = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lokacija = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    plata = table.Column<int>(type: "int", nullable: false),
                    remote_work = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    vreme = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KorisnikID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oglasi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Oglasi_Korisnik_KorisnikID",
                        column: x => x.KorisnikID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Poslodavac",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikID = table.Column<int>(type: "int", nullable: true),
                    tip_poslodavca = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    kontakt = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    prezime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    lokacija_firme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    naziv_firme = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poslodavac", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Poslodavac_Korisnik_KorisnikID",
                        column: x => x.KorisnikID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Private_Message",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    poruka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    naslov = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    vreme_pristizanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ID_PosiljaocaID = table.Column<int>(type: "int", nullable: true),
                    ID_PrimaocaID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Private_Message", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Private_Message_Korisnik_ID_PosiljaocaID",
                        column: x => x.ID_PosiljaocaID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Private_Message_Korisnik_ID_PrimaocaID",
                        column: x => x.ID_PrimaocaID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Radnik",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikID = table.Column<int>(type: "int", nullable: true),
                    ime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    prezime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Radnik", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Radnik_Korisnik_KorisnikID",
                        column: x => x.KorisnikID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Zahtevi",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status = table.Column<int>(type: "int", nullable: false),
                    ID_PosiljaocaID = table.Column<int>(type: "int", nullable: true),
                    ID_PrimaocaID = table.Column<int>(type: "int", nullable: true),
                    OglasiID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zahtevi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Zahtevi_Korisnik_ID_PosiljaocaID",
                        column: x => x.ID_PosiljaocaID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Zahtevi_Korisnik_ID_PrimaocaID",
                        column: x => x.ID_PrimaocaID,
                        principalTable: "Korisnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Zahtevi_Oglasi_OglasiID",
                        column: x => x.OglasiID,
                        principalTable: "Oglasi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CV",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    obrazovanje = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    godina_iskustva = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    adresa = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    licni_opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    jezici = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tehnologije = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    work_experience = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadnikID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CV", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CV_Radnik_RadnikID",
                        column: x => x.RadnikID,
                        principalTable: "Radnik",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_KorisnikID",
                table: "Admin",
                column: "KorisnikID");

            migrationBuilder.CreateIndex(
                name: "IX_CV_RadnikID",
                table: "CV",
                column: "RadnikID");

            migrationBuilder.CreateIndex(
                name: "IX_Oglasi_KorisnikID",
                table: "Oglasi",
                column: "KorisnikID");

            migrationBuilder.CreateIndex(
                name: "IX_Poslodavac_KorisnikID",
                table: "Poslodavac",
                column: "KorisnikID");

            migrationBuilder.CreateIndex(
                name: "IX_Private_Message_ID_PosiljaocaID",
                table: "Private_Message",
                column: "ID_PosiljaocaID");

            migrationBuilder.CreateIndex(
                name: "IX_Private_Message_ID_PrimaocaID",
                table: "Private_Message",
                column: "ID_PrimaocaID");

            migrationBuilder.CreateIndex(
                name: "IX_Radnik_KorisnikID",
                table: "Radnik",
                column: "KorisnikID");

            migrationBuilder.CreateIndex(
                name: "IX_Zahtevi_ID_PosiljaocaID",
                table: "Zahtevi",
                column: "ID_PosiljaocaID");

            migrationBuilder.CreateIndex(
                name: "IX_Zahtevi_ID_PrimaocaID",
                table: "Zahtevi",
                column: "ID_PrimaocaID");

            migrationBuilder.CreateIndex(
                name: "IX_Zahtevi_OglasiID",
                table: "Zahtevi",
                column: "OglasiID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "CV");

            migrationBuilder.DropTable(
                name: "Poslodavac");

            migrationBuilder.DropTable(
                name: "Private_Message");

            migrationBuilder.DropTable(
                name: "Zahtevi");

            migrationBuilder.DropTable(
                name: "Radnik");

            migrationBuilder.DropTable(
                name: "Oglasi");

            migrationBuilder.DropTable(
                name: "Korisnik");
        }
    }
}
