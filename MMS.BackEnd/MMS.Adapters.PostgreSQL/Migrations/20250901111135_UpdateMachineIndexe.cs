using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMachineIndexe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e16b7663-74d0-4247-b870-f98c0dae56d2"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("fed868ea-c0af-445e-8ad2-0ebe8309b98e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("9ae6f68c-5489-472e-a0a2-3a54072b770f"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("eca9bca6-fe3d-489e-b6fd-1d3cedf80c64"));

          

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e540db55-67a8-4640-8e14-84e067e0e534"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7889b369-da77-4643-9c66-a4638920c7ab"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("44ed3c08-996d-4dbc-92f8-8d019d39068e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("97d38567-f261-444e-8a6a-e7c22d1b0c6f"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4bf10851-6a53-4760-acaa-c2c6443218dc"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("73106772-1f08-4fa8-9b70-a7d42b16b390"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("677bf3db-26d0-4fa6-9280-e79e892398d3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("865d7ea2-3319-4570-95e6-0cac05220504"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("74fd1459-6dac-4d70-b2ee-a0e7994cb2e8"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("662d5e2d-94af-478e-8013-3379bf0665f9"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("b6deb981-48da-43ce-ad1a-7cc1f999c3d9"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("89adef32-28dd-4ca3-887f-5db10b5a0516"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("8bd0bae0-c32e-4ba2-9e04-b5883144ca32"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("50a517f7-0f22-4da4-b2c6-c668ca31cc87"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("28e95ce3-2cc2-4247-82be-52fae3d9f394"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("aafb4f07-c491-4460-a060-d5ad06ff08a2"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("84d10093-5889-4fde-8c70-7abedb91eea9"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("492588a4-f2a2-4592-b3e7-ff794922d77d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("35e40e74-ffe2-4abf-aa63-3b1903a8f2bc"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("36c21f3c-5b5d-438c-b7d5-d6cc87dee614"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c9434249-083e-45ef-b3ce-4fc337dca0bf"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("56bb779d-bbbc-4d47-b43f-07346322002e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5b1bda0a-f247-4015-8759-1f0f955027f2"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("150e6e50-49dc-4a2e-af40-0abbb3870190"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("80546439-88e4-4fd4-8bcd-23abb332455d"),
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

            migrationBuilder.CreateIndex(
                name: "idx_machine_id",
                table: "Machines",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_machine_id",
                table: "Machines");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("fed868ea-c0af-445e-8ad2-0ebe8309b98e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e16b7663-74d0-4247-b870-f98c0dae56d2"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("eca9bca6-fe3d-489e-b6fd-1d3cedf80c64"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("9ae6f68c-5489-472e-a0a2-3a54072b770f"));

          

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7889b369-da77-4643-9c66-a4638920c7ab"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e540db55-67a8-4640-8e14-84e067e0e534"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("97d38567-f261-444e-8a6a-e7c22d1b0c6f"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("44ed3c08-996d-4dbc-92f8-8d019d39068e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("73106772-1f08-4fa8-9b70-a7d42b16b390"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4bf10851-6a53-4760-acaa-c2c6443218dc"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("865d7ea2-3319-4570-95e6-0cac05220504"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("677bf3db-26d0-4fa6-9280-e79e892398d3"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("662d5e2d-94af-478e-8013-3379bf0665f9"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("74fd1459-6dac-4d70-b2ee-a0e7994cb2e8"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("89adef32-28dd-4ca3-887f-5db10b5a0516"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("b6deb981-48da-43ce-ad1a-7cc1f999c3d9"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("50a517f7-0f22-4da4-b2c6-c668ca31cc87"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("8bd0bae0-c32e-4ba2-9e04-b5883144ca32"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("aafb4f07-c491-4460-a060-d5ad06ff08a2"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("28e95ce3-2cc2-4247-82be-52fae3d9f394"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("492588a4-f2a2-4592-b3e7-ff794922d77d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("84d10093-5889-4fde-8c70-7abedb91eea9"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("36c21f3c-5b5d-438c-b7d5-d6cc87dee614"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("35e40e74-ffe2-4abf-aa63-3b1903a8f2bc"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("56bb779d-bbbc-4d47-b43f-07346322002e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("c9434249-083e-45ef-b3ce-4fc337dca0bf"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("150e6e50-49dc-4a2e-af40-0abbb3870190"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5b1bda0a-f247-4015-8759-1f0f955027f2"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("073b69ca-ae32-4faa-9205-c69c3cb224a3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("80546439-88e4-4fd4-8bcd-23abb332455d"));

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
