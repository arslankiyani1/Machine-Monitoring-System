using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class updateMAchineSeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MachineSettings",
                keyColumn: "Id",
                keyValue: new Guid("99999999-1111-1111-1111-111111111111"));

            migrationBuilder.DropColumn(
                name: "Alerts",
                table: "MachineSettings");

            migrationBuilder.DropColumn(
                name: "PartsPerCycle",
                table: "MachineSettings");

            migrationBuilder.RenameColumn(
                name: "StopInterval",
                table: "MachineSettings",
                newName: "StopTimelimit");

            migrationBuilder.RenameColumn(
                name: "SoundAlert",
                table: "MachineSettings",
                newName: "ReverseCSlockLogic");

            migrationBuilder.RenameColumn(
                name: "MaxCycleDuration",
                table: "MachineSettings",
                newName: "PlannedProdusctionTime");

            migrationBuilder.RenameColumn(
                name: "CycleStop",
                table: "MachineSettings",
                newName: "GuestLock");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("64faa25f-f502-452f-beb5-5ff4c2fe7ed7"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("190a3a2f-c402-4c7a-879d-028a75b98b48"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("aa74f9bc-2ccc-4d46-a8a8-ca2777b13394"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7db6eb39-e7be-4a4e-be62-9851202a2f0e"));

          

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("6d5a4f0c-7650-424a-8e9f-775c2694bbd1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("aba1c732-e051-4829-9739-bd6c89b720c1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("996d54ec-568d-48aa-8092-cb937ff2640d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("2b30e3c7-d098-4748-a073-bcb39887929a"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ee1afec9-80a3-4cfd-b5f2-dbf67ae7eb28"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("637558d3-efd1-4bf7-a190-9db3d1c03606"));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "MinElapsedCycleTime",
                table: "MachineSettings",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e50ce7ac-ca1c-4555-9e6f-74761de249bb"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("56939e7e-4743-4bc4-b3e2-1d8a0a1f6fbe"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("314354ac-4fc7-4925-8a32-aba991b138fb"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7480377a-4ef8-4646-b3da-6e672ea42514"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("3467462e-2828-45fc-a516-c3bee247378e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("6dd6dde4-43fc-4cbf-be59-afa53c89ef2e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("b79f5646-f1f0-43cb-9696-65f6671f19c1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("13c35ca0-577a-4011-934e-538664362050"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("b80cda70-de5b-4ec2-9987-1a15b97fd867"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("519b55dd-8901-4657-87b3-c8e66c850f84"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("6217bd49-b5fd-4466-9679-a16f4d451704"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("284a1bea-50b6-4c3f-bf2d-dd33c1770b00"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("476fd4ec-1cb9-484f-8dd3-cd9f95404255"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("ad648dad-1d52-4d26-99fa-1ad421bc7aed"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("3c3a2069-6079-448f-97cd-7cb57429c2a3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("602e5b96-04d7-4a6d-a046-2b58bce0e1c3"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ad78dab2-eca9-4a02-8595-97ff153298e1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("92543910-ba3f-4942-b47c-62b68a6c4238"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("abfeb68d-c264-4c97-81a0-070cfb1168b1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e39b2ce1-26ec-4b83-bbda-9e4435a47ef3"));

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
            migrationBuilder.DropColumn(
                name: "MinElapsedCycleTime",
                table: "MachineSettings");

            migrationBuilder.RenameColumn(
                name: "StopTimelimit",
                table: "MachineSettings",
                newName: "StopInterval");

            migrationBuilder.RenameColumn(
                name: "ReverseCSlockLogic",
                table: "MachineSettings",
                newName: "SoundAlert");

            migrationBuilder.RenameColumn(
                name: "PlannedProdusctionTime",
                table: "MachineSettings",
                newName: "MaxCycleDuration");

            migrationBuilder.RenameColumn(
                name: "GuestLock",
                table: "MachineSettings",
                newName: "CycleStop");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("190a3a2f-c402-4c7a-879d-028a75b98b48"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("64faa25f-f502-452f-beb5-5ff4c2fe7ed7"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7db6eb39-e7be-4a4e-be62-9851202a2f0e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("aa74f9bc-2ccc-4d46-a8a8-ca2777b13394"));


            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("aba1c732-e051-4829-9739-bd6c89b720c1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("6d5a4f0c-7650-424a-8e9f-775c2694bbd1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("2b30e3c7-d098-4748-a073-bcb39887929a"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("996d54ec-568d-48aa-8092-cb937ff2640d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("637558d3-efd1-4bf7-a190-9db3d1c03606"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("ee1afec9-80a3-4cfd-b5f2-dbf67ae7eb28"));

            migrationBuilder.AddColumn<List<Alert>>(
                name: "Alerts",
                table: "MachineSettings",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "PartsPerCycle",
                table: "MachineSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("56939e7e-4743-4bc4-b3e2-1d8a0a1f6fbe"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e50ce7ac-ca1c-4555-9e6f-74761de249bb"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7480377a-4ef8-4646-b3da-6e672ea42514"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("314354ac-4fc7-4925-8a32-aba991b138fb"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("6dd6dde4-43fc-4cbf-be59-afa53c89ef2e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("3467462e-2828-45fc-a516-c3bee247378e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("13c35ca0-577a-4011-934e-538664362050"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("b79f5646-f1f0-43cb-9696-65f6671f19c1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("519b55dd-8901-4657-87b3-c8e66c850f84"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("b80cda70-de5b-4ec2-9987-1a15b97fd867"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("284a1bea-50b6-4c3f-bf2d-dd33c1770b00"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("6217bd49-b5fd-4466-9679-a16f4d451704"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ad648dad-1d52-4d26-99fa-1ad421bc7aed"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("476fd4ec-1cb9-484f-8dd3-cd9f95404255"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("602e5b96-04d7-4a6d-a046-2b58bce0e1c3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("3c3a2069-6079-448f-97cd-7cb57429c2a3"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("92543910-ba3f-4942-b47c-62b68a6c4238"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("ad78dab2-eca9-4a02-8595-97ff153298e1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e39b2ce1-26ec-4b83-bbda-9e4435a47ef3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("abfeb68d-c264-4c97-81a0-070cfb1168b1"));

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

            migrationBuilder.InsertData(
                table: "MachineSettings",
                columns: new[] { "Id", "Alerts", "AutomaticPartsCounter", "CreatedAt", "CreatedBy", "CycleStartInterlock", "CycleStop", "Deleted", "DeletedBy", "DownTimeReasons", "MachineId", "MaxCycleDuration", "MaxFeedrate", "MaxSpindleSpeed", "PartsPerCycle", "SoundAlert", "Status", "StopInterval", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("99999999-1111-1111-1111-111111111111"), new List<Alert>(), false, null, null, false, false, null, null, new List<string> { "Power Failure" }, new Guid("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d91"), new TimeOnly(0, 0, 0), null, null, null, false, 0, new TimeOnly(0, 0, 0), null, null });

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
