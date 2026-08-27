using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FarmApp.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Sender = table.Column<string>(type: "text", nullable: false),
                    TypeUrlForRedirection = table.Column<string>(type: "text", nullable: false),
                    UrlForRedirection = table.Column<string>(type: "text", nullable: true),
                    TypeOfSend = table.Column<string>(type: "text", nullable: false),
                    DateTimeOfSend = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    TypeOfTargetUser = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    NotificationKind = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ownerships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ownerships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyNoteStatusEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TextColorHex = table.Column<string>(type: "text", nullable: false),
                    BGColorHex = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyNoteStatusEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Purposes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purposes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "push_device_registrations",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    DeviceToken = table.Column<string>(type: "text", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    TagsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_push_device_registrations", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "PushNotificationsQueue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PropertyNoteId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SendAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotificationsQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    ZipCode = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    EmailConfirmToken = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenLifeTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    EmailVerificationCode = table.Column<string>(type: "text", nullable: true),
                    VerificationSentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IconNumber = table.Column<int>(type: "integer", nullable: false),
                    SelectedLocationLatitude = table.Column<double>(type: "double precision", nullable: true),
                    SelectedLocationLongitude = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Steads",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CadNum = table.Column<string>(type: "text", nullable: false),
                    Area = table.Column<float>(type: "real", nullable: false),
                    AreaUnit = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    OwnershipId = table.Column<string>(type: "text", nullable: false),
                    PurposeId = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steads_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Steads_Ownerships_OwnershipId",
                        column: x => x.OwnershipId,
                        principalTable: "Ownerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Steads_Purposes_PurposeId",
                        column: x => x.PurposeId,
                        principalTable: "Purposes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    MultipolygonSerialized = table.Column<string>(type: "text", nullable: false),
                    Area = table.Column<float>(type: "real", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Properties_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_preferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NotificationsDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    SystemNotificationsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    WeatherAlertsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ActivityRemindersEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    InAppNotificationsOnly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notification_preferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_notification_preferences_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomSteads",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SteadId = table.Column<string>(type: "text", nullable: true),
                    Coordinates = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSteads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomSteads_Steads_SteadId",
                        column: x => x.SteadId,
                        principalTable: "Steads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomSteads_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyNotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Header = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PreviewMediaId = table.Column<string>(type: "text", nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: true),
                    PropertyId = table.Column<string>(type: "text", nullable: false),
                    NotificationsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyBeforeStart = table.Column<int>(type: "integer", nullable: true),
                    NotifyBeforeEnd = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyNotes_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyNotes_PropertyNoteStatusEntity_StatusId",
                        column: x => x.StatusId,
                        principalTable: "PropertyNoteStatusEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PropertyAndSteads",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SteadId = table.Column<string>(type: "text", nullable: true),
                    CustomSteadId = table.Column<string>(type: "text", nullable: true),
                    PropertyId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAndSteads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyAndSteads_CustomSteads_CustomSteadId",
                        column: x => x.CustomSteadId,
                        principalTable: "CustomSteads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyAndSteads_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyAndSteads_Steads_SteadId",
                        column: x => x.SteadId,
                        principalTable: "Steads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyNoteMedia",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PropertyNoteId = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    MediaType = table.Column<int>(type: "integer", nullable: false),
                    RelativePath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyNoteMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyNoteMedia_PropertyNotes_PropertyNoteId",
                        column: x => x.PropertyNoteId,
                        principalTable: "PropertyNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PropertyNoteStatusEntity",
                columns: new[] { "Id", "BGColorHex", "Code", "IsDefault", "Name", "TextColorHex", "UserId" },
                values: new object[,]
                {
                    { 1, "#DCFAE9", "FAILED", true, "Не виконано", "#1D8B41", null },
                    { 2, "#FDF2CA", "IN_PROGRESS", true, "В процессі", "#925C00", null },
                    { 3, "#C42921", "DONE", true, "Виконано", "#FFDED8", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomSteads_SteadId",
                table: "CustomSteads",
                column: "SteadId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomSteads_UserId",
                table: "CustomSteads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_UserId",
                table: "Properties",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAndSteads_CustomSteadId",
                table: "PropertyAndSteads",
                column: "CustomSteadId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAndSteads_PropertyId",
                table: "PropertyAndSteads",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAndSteads_SteadId",
                table: "PropertyAndSteads",
                column: "SteadId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyNoteMedia_PropertyNoteId",
                table: "PropertyNoteMedia",
                column: "PropertyNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyNotes_PropertyId",
                table: "PropertyNotes",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyNotes_StatusId",
                table: "PropertyNotes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Purposes_Name",
                table: "Purposes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationsQueue_PropertyNoteId",
                table: "PushNotificationsQueue",
                column: "PropertyNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationsQueue_Status_SendAt",
                table: "PushNotificationsQueue",
                columns: new[] { "Status", "SendAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Steads_CategoryId",
                table: "Steads",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Steads_OwnershipId",
                table: "Steads",
                column: "OwnershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Steads_PurposeId",
                table: "Steads",
                column: "PurposeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PropertyAndSteads");

            migrationBuilder.DropTable(
                name: "PropertyNoteMedia");

            migrationBuilder.DropTable(
                name: "push_device_registrations");

            migrationBuilder.DropTable(
                name: "PushNotificationsQueue");

            migrationBuilder.DropTable(
                name: "user_notification_preferences");

            migrationBuilder.DropTable(
                name: "CustomSteads");

            migrationBuilder.DropTable(
                name: "PropertyNotes");

            migrationBuilder.DropTable(
                name: "Steads");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "PropertyNoteStatusEntity");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Ownerships");

            migrationBuilder.DropTable(
                name: "Purposes");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
