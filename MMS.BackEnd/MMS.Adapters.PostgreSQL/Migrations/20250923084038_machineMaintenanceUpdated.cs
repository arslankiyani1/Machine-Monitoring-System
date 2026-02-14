using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class machineMaintenanceUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineMaintenances_Machines_MachineId",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MachineMaintenances");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("33dc1d3e-2cdc-4f55-83c7-d8136db1dadd"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("6d9bbaeb-9d01-402b-b3cf-f47a3e77385d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a083923b-7fb9-441e-bf78-f44ecbcba9ed"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("2c6507b7-7e76-466c-b262-b8eedc2e3ce8"));

      
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a717f6f3-2ceb-4935-ae3a-4e29a3a8ed9f"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("f4105b75-39a1-4bdb-8253-325a95eec887"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("3981e1ba-72f4-4b6e-8ff6-bb54803116d6"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("334e8ae4-d6a8-472b-b69c-9b2529b50d84"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("59fb6a00-c7b9-40c4-8267-0a99e9dd8992"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4db228ac-5f19-4a81-9981-34b976733428"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("badbd203-af96-4eaf-84f5-331083021f72"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7fca1128-13fb-45fa-9662-2180f98aa57b"));

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "MachineMaintenances",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MachineMaintenances",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaintenanceTaskName",
                table: "MachineMaintenances",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "MachineMaintenances",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("20cb502f-7a71-40a7-a4aa-e1028bdf012c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5690b3c2-4752-454e-a4b3-6f7f4a9b3e47"));

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToUserId",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserName",
                table: "MachineMaintenances",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Attachments",
                table: "MachineMaintenances",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "MachineMaintenances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "MachineMaintenances",
                type: "text",
                nullable: false,
                defaultValue: "Medium");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "MachineMaintenances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("214ffe12-3fb8-46cd-9965-ba3e67dabe23"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("9ca15d65-2ea4-4d2d-9a46-87b4627ab88d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0983c345-7fa5-4b39-ba18-b415fd81b73e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("ed9f9839-126e-4c4e-97e3-bfe2509c3aa8"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c09d3780-807d-4c20-87b6-fd2652b82c07"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("71e3c5aa-dbbf-4a7b-a778-7fd2fa7e2235"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e2941570-5d5d-434e-9954-44b6042d140a"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0eb2fab4-38d4-463b-bfec-11406a0e40cd"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("92978adc-ce54-4969-8d1b-495bb6ccbf86"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("ec95d4e7-56af-436c-884c-8a84d51452af"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e47e4654-a4b7-4b24-b05d-2781c1fa5914"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("c4f9d980-dea6-477d-a91e-d99204239358"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("3f27cdfa-e4fb-43b3-a141-2666ea562e7b"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("72559ad8-e70a-4d1a-9357-fb362aa8ee5c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4ecc6d5e-d83a-4e42-9e43-36fcd16a63d5"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5127f4be-5893-4803-ac29-b089050d6fb7"));

            migrationBuilder.UpdateData(
                table: "CustomerDashboards",
                keyColumn: "Id",
                keyValue: new Guid("c1277139-895e-4efa-a9c0-c544406a2ce1"),
                column: "Layout",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "CustomerDashboards",
                keyColumn: "Id",
                keyValue: new Guid("c1277139-895e-4efa-a9c0-c544406a2ce2"),
                column: "Layout",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Shifts",
                value: new List<Shift>());

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"),
                column: "Shifts",
                value: new List<Shift>());

            migrationBuilder.UpdateData(
                table: "MachineMaintenances",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa1-1111-1111-1111-111111111111"),
                columns: new[] { "AssignedToUserId", "AssignedToUserName", "Attachments", "Category", "CustomerId", "DueDate", "JobId", "Priority", "ScheduledDate" },
                values: new object[] { null, null, new List<string>(), "Emergency", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "Medium", null });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("45b429a4-f21b-4be4-981b-a4ecee7244b1"),
                column: "Features",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("45b429a4-f21b-4be4-981b-a4ecee7244b2"),
                column: "Features",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "Widgets",
                keyColumn: "Id",
                keyValue: new Guid("ddddddd1-1111-1111-1111-111111111111"),
                column: "Config",
                value: new Dictionary<string, object>());

            migrationBuilder.CreateIndex(
                name: "IX_MachineMaintenances_CustomerId",
                table: "MachineMaintenances",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineMaintenances_Customers_CustomerId",
                table: "MachineMaintenances",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineMaintenances_Machines_MachineId",
                table: "MachineMaintenances",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineMaintenances_Customers_CustomerId",
                table: "MachineMaintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineMaintenances_Machines_MachineId",
                table: "MachineMaintenances");

            migrationBuilder.DropIndex(
                name: "IX_MachineMaintenances_CustomerId",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "AssignedToUserName",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "Attachments",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MachineMaintenances");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "MachineMaintenances");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("6d9bbaeb-9d01-402b-b3cf-f47a3e77385d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("33dc1d3e-2cdc-4f55-83c7-d8136db1dadd"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("2c6507b7-7e76-466c-b262-b8eedc2e3ce8"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a083923b-7fb9-441e-bf78-f44ecbcba9ed"));


            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("f4105b75-39a1-4bdb-8253-325a95eec887"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a717f6f3-2ceb-4935-ae3a-4e29a3a8ed9f"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("334e8ae4-d6a8-472b-b69c-9b2529b50d84"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("3981e1ba-72f4-4b6e-8ff6-bb54803116d6"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4db228ac-5f19-4a81-9981-34b976733428"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("59fb6a00-c7b9-40c4-8267-0a99e9dd8992"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7fca1128-13fb-45fa-9662-2180f98aa57b"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("badbd203-af96-4eaf-84f5-331083021f72"));

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "MachineMaintenances",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MachineMaintenances",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaintenanceTaskName",
                table: "MachineMaintenances",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "MachineMaintenances",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5690b3c2-4752-454e-a4b3-6f7f4a9b3e47"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("20cb502f-7a71-40a7-a4aa-e1028bdf012c"));

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MachineMaintenances",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("9ca15d65-2ea4-4d2d-9a46-87b4627ab88d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("214ffe12-3fb8-46cd-9965-ba3e67dabe23"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ed9f9839-126e-4c4e-97e3-bfe2509c3aa8"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0983c345-7fa5-4b39-ba18-b415fd81b73e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("71e3c5aa-dbbf-4a7b-a778-7fd2fa7e2235"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("c09d3780-807d-4c20-87b6-fd2652b82c07"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0eb2fab4-38d4-463b-bfec-11406a0e40cd"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e2941570-5d5d-434e-9954-44b6042d140a"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ec95d4e7-56af-436c-884c-8a84d51452af"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("92978adc-ce54-4969-8d1b-495bb6ccbf86"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c4f9d980-dea6-477d-a91e-d99204239358"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e47e4654-a4b7-4b24-b05d-2781c1fa5914"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("72559ad8-e70a-4d1a-9357-fb362aa8ee5c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("3f27cdfa-e4fb-43b3-a141-2666ea562e7b"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5127f4be-5893-4803-ac29-b089050d6fb7"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4ecc6d5e-d83a-4e42-9e43-36fcd16a63d5"));

            migrationBuilder.UpdateData(
                table: "CustomerDashboards",
                keyColumn: "Id",
                keyValue: new Guid("c1277139-895e-4efa-a9c0-c544406a2ce1"),
                column: "Layout",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "CustomerDashboards",
                keyColumn: "Id",
                keyValue: new Guid("c1277139-895e-4efa-a9c0-c544406a2ce2"),
                column: "Layout",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Shifts",
                value: new List<Shift>());

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"),
                column: "Shifts",
                value: new List<Shift>());

            migrationBuilder.UpdateData(
                table: "MachineMaintenances",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa1-1111-1111-1111-111111111111"),
                columns: new[] { "Category", "ImageUrl" },
                values: new object[] { 6, null });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("45b429a4-f21b-4be4-981b-a4ecee7244b1"),
                column: "Features",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("45b429a4-f21b-4be4-981b-a4ecee7244b2"),
                column: "Features",
                value: new Dictionary<string, object>());

            migrationBuilder.UpdateData(
                table: "Widgets",
                keyColumn: "Id",
                keyValue: new Guid("ddddddd1-1111-1111-1111-111111111111"),
                column: "Config",
                value: new Dictionary<string, object>());

            migrationBuilder.AddForeignKey(
                name: "FK_MachineMaintenances_Machines_MachineId",
                table: "MachineMaintenances",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
