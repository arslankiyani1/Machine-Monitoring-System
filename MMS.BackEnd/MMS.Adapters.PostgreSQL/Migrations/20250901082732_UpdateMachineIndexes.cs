using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMachineIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Machines_CustomerId",
                table: "Machines",
                newName: "idx_machine_customerid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("fed868ea-c0af-445e-8ad2-0ebe8309b98e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5e7ceff2-bc82-4d81-b159-3b59f54e4330"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("eca9bca6-fe3d-489e-b6fd-1d3cedf80c64"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("17c26217-cf65-4521-b462-224f74e4b890"));

          

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7889b369-da77-4643-9c66-a4638920c7ab"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7d88a0f8-0a62-4013-b106-94e1e00df148"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("97d38567-f261-444e-8a6a-e7c22d1b0c6f"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a59679ac-5bf9-4925-8084-80c16b27e663"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("73106772-1f08-4fa8-9b70-a7d42b16b390"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("14b27fad-642d-44b3-ac0a-b4a1b120876c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("865d7ea2-3319-4570-95e6-0cac05220504"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a22b114c-f53a-454b-be0a-92256f62d3fe"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("662d5e2d-94af-478e-8013-3379bf0665f9"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("8bd14d7a-b04b-44ad-8ff0-a4327a7e02dc"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("89adef32-28dd-4ca3-887f-5db10b5a0516"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5efe7961-ea3e-41b7-8454-b9745242816a"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("50a517f7-0f22-4da4-b2c6-c668ca31cc87"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("796b7846-1b28-4f60-9690-957c9ba1687c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("aafb4f07-c491-4460-a060-d5ad06ff08a2"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("c3732eba-9587-475f-8985-af3d27a7b4fd"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("492588a4-f2a2-4592-b3e7-ff794922d77d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4cdee32b-1ae1-40e1-b65a-7e8fa0735fcf"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("36c21f3c-5b5d-438c-b7d5-d6cc87dee614"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("870b3ea8-3bd3-4337-8838-79ad50dfba93"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("56bb779d-bbbc-4d47-b43f-07346322002e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4e48711e-4a68-4287-ac28-eb649f2149e5"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("150e6e50-49dc-4a2e-af40-0abbb3870190"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("83841984-6e32-4ab5-9ae6-fdedea1b41cf"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("073b69ca-ae32-4faa-9205-c69c3cb224a3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5b966335-c762-47a2-9822-56599f04df7d"));

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
                table: "MachineSettings",
                keyColumn: "Id",
                keyValue: new Guid("99999999-1111-1111-1111-111111111111"),
                columns: new[] { "Alerts", "DownTimeReasons" },
                values: new object[] { new List<Alert>(), new List<string> { "Power Failure" } });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "idx_machine_customerid",
                table: "Machines",
                newName: "IX_Machines_CustomerId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5e7ceff2-bc82-4d81-b159-3b59f54e4330"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("fed868ea-c0af-445e-8ad2-0ebe8309b98e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("17c26217-cf65-4521-b462-224f74e4b890"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("eca9bca6-fe3d-489e-b6fd-1d3cedf80c64"));

           

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7d88a0f8-0a62-4013-b106-94e1e00df148"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7889b369-da77-4643-9c66-a4638920c7ab"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a59679ac-5bf9-4925-8084-80c16b27e663"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("97d38567-f261-444e-8a6a-e7c22d1b0c6f"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("14b27fad-642d-44b3-ac0a-b4a1b120876c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("73106772-1f08-4fa8-9b70-a7d42b16b390"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a22b114c-f53a-454b-be0a-92256f62d3fe"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("865d7ea2-3319-4570-95e6-0cac05220504"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("8bd14d7a-b04b-44ad-8ff0-a4327a7e02dc"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("662d5e2d-94af-478e-8013-3379bf0665f9"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5efe7961-ea3e-41b7-8454-b9745242816a"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("89adef32-28dd-4ca3-887f-5db10b5a0516"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("796b7846-1b28-4f60-9690-957c9ba1687c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("50a517f7-0f22-4da4-b2c6-c668ca31cc87"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c3732eba-9587-475f-8985-af3d27a7b4fd"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("aafb4f07-c491-4460-a060-d5ad06ff08a2"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4cdee32b-1ae1-40e1-b65a-7e8fa0735fcf"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("492588a4-f2a2-4592-b3e7-ff794922d77d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("870b3ea8-3bd3-4337-8838-79ad50dfba93"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("36c21f3c-5b5d-438c-b7d5-d6cc87dee614"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4e48711e-4a68-4287-ac28-eb649f2149e5"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("56bb779d-bbbc-4d47-b43f-07346322002e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("83841984-6e32-4ab5-9ae6-fdedea1b41cf"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("150e6e50-49dc-4a2e-af40-0abbb3870190"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5b966335-c762-47a2-9822-56599f04df7d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("073b69ca-ae32-4faa-9205-c69c3cb224a3"));

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
                table: "MachineSettings",
                keyColumn: "Id",
                keyValue: new Guid("99999999-1111-1111-1111-111111111111"),
                columns: new[] { "Alerts", "DownTimeReasons" },
                values: new object[] { new List<Alert>(), new List<string> { "Power Failure" } });

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
        }
    }
}
