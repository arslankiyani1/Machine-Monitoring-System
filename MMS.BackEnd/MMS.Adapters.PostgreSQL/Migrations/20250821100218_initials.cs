using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class initials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDashboards_Customers_CustomerId",
                table: "CustomerDashboards");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineMaintenances_Machines_MachineId",
                table: "MachineMaintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineSettings_Machines_MachineId",
                table: "MachineSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMachines_Machines_MachineId",
                table: "UserMachines");

            migrationBuilder.DropForeignKey(
                name: "FK_Widgets_CustomerDashboards_DashboardId",
                table: "Widgets");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5e7ceff2-bc82-4d81-b159-3b59f54e4330"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("fe15bb6a-cf12-47bd-8710-8616c6522380"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("17c26217-cf65-4521-b462-224f74e4b890"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("8622de74-9481-4cf8-9ce2-7dfda13bf17d"));

           

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7d88a0f8-0a62-4013-b106-94e1e00df148"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("747568f1-f304-4392-8f70-50434d1790b7"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a59679ac-5bf9-4925-8084-80c16b27e663"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("325819ed-9a79-455e-b325-94f5d714c9db"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("14b27fad-642d-44b3-ac0a-b4a1b120876c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a4b59edc-3d39-49ce-9c72-47ef9425b472"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a22b114c-f53a-454b-be0a-92256f62d3fe"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("b759b73f-1984-42a8-ba17-bc0dd7537593"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("8bd14d7a-b04b-44ad-8ff0-a4327a7e02dc"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0f9a811c-885f-4c51-862d-f8582c9812df"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5efe7961-ea3e-41b7-8454-b9745242816a"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5f6c6b7e-2afc-416a-bee6-4b352d12a553"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("796b7846-1b28-4f60-9690-957c9ba1687c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("347f8e76-9dd7-4706-9362-9fb374d7f5f0"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c3732eba-9587-475f-8985-af3d27a7b4fd"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0dc5df1e-96f2-4294-a060-cdf88144f58f"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4cdee32b-1ae1-40e1-b65a-7e8fa0735fcf"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("66a95568-0615-431d-927d-dcd0810b680d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("870b3ea8-3bd3-4337-8838-79ad50dfba93"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a2a2307c-54e0-416b-a879-32d6074d46e3"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4e48711e-4a68-4287-ac28-eb649f2149e5"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("13dd8e7c-ec9f-48d6-80bc-973da538aef3"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("83841984-6e32-4ab5-9ae6-fdedea1b41cf"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("3a2c8c27-653a-4ae7-ab30-33dfc6a24c08"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5b966335-c762-47a2-9822-56599f04df7d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("805052a9-e25a-4cab-9a2b-032ab18d432e"));

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

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerDashboards_Customers_CustomerId",
                table: "CustomerDashboards",
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
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineSettings_Machines_MachineId",
                table: "MachineSettings",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMachines_Machines_MachineId",
                table: "UserMachines",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Widgets_CustomerDashboards_DashboardId",
                table: "Widgets",
                column: "DashboardId",
                principalTable: "CustomerDashboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDashboards_Customers_CustomerId",
                table: "CustomerDashboards");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineMaintenances_Machines_MachineId",
                table: "MachineMaintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineSettings_Machines_MachineId",
                table: "MachineSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMachines_Machines_MachineId",
                table: "UserMachines");

            migrationBuilder.DropForeignKey(
                name: "FK_Widgets_CustomerDashboards_DashboardId",
                table: "Widgets");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("fe15bb6a-cf12-47bd-8710-8616c6522380"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5e7ceff2-bc82-4d81-b159-3b59f54e4330"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("8622de74-9481-4cf8-9ce2-7dfda13bf17d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("17c26217-cf65-4521-b462-224f74e4b890"));

          

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("747568f1-f304-4392-8f70-50434d1790b7"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7d88a0f8-0a62-4013-b106-94e1e00df148"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("325819ed-9a79-455e-b325-94f5d714c9db"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a59679ac-5bf9-4925-8084-80c16b27e663"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a4b59edc-3d39-49ce-9c72-47ef9425b472"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("14b27fad-642d-44b3-ac0a-b4a1b120876c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("b759b73f-1984-42a8-ba17-bc0dd7537593"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("a22b114c-f53a-454b-be0a-92256f62d3fe"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0f9a811c-885f-4c51-862d-f8582c9812df"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("8bd14d7a-b04b-44ad-8ff0-a4327a7e02dc"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5f6c6b7e-2afc-416a-bee6-4b352d12a553"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5efe7961-ea3e-41b7-8454-b9745242816a"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("347f8e76-9dd7-4706-9362-9fb374d7f5f0"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("796b7846-1b28-4f60-9690-957c9ba1687c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0dc5df1e-96f2-4294-a060-cdf88144f58f"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("c3732eba-9587-475f-8985-af3d27a7b4fd"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("66a95568-0615-431d-927d-dcd0810b680d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4cdee32b-1ae1-40e1-b65a-7e8fa0735fcf"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("a2a2307c-54e0-416b-a879-32d6074d46e3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("870b3ea8-3bd3-4337-8838-79ad50dfba93"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("13dd8e7c-ec9f-48d6-80bc-973da538aef3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4e48711e-4a68-4287-ac28-eb649f2149e5"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("3a2c8c27-653a-4ae7-ab30-33dfc6a24c08"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("83841984-6e32-4ab5-9ae6-fdedea1b41cf"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("805052a9-e25a-4cab-9a2b-032ab18d432e"),
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

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerDashboards_Customers_CustomerId",
                table: "CustomerDashboards",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineMaintenances_Machines_MachineId",
                table: "MachineMaintenances",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineSettings_Machines_MachineId",
                table: "MachineSettings",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMachines_Machines_MachineId",
                table: "UserMachines",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Widgets_CustomerDashboards_DashboardId",
                table: "Widgets",
                column: "DashboardId",
                principalTable: "CustomerDashboards",
                principalColumn: "Id");
        }
    }
}
