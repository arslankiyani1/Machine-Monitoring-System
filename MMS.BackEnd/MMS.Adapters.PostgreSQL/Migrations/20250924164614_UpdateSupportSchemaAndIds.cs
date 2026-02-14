using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupportSchemaAndIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("5c0fea26-ba0e-44ce-b706-88428bada830"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("ac597cc6-d082-4ecc-b2db-3a108b5bee5a"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("bde9cbff-b9c9-4464-9ea8-bd6f004c5049"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("f8fe13a1-f15e-4e20-a9da-4d235c930e44"));


            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("759efa60-f3db-4be3-bbc2-29bc313e11d5"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e92f6ed2-2f13-4f2f-96e1-2b465ffc19e9"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("9297ac99-bb12-42d8-a377-0bf792ccb041"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("56411734-7189-44a1-901a-b61be56bee67"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("9c088ccb-0e76-43e7-a9ee-199d06363fd0"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("c9fd428a-451d-4a08-a2d0-d3140840d385"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("d21ac4a7-a896-4e86-9946-f1174383a07b"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7feaed6e-c69d-4d94-bd7c-a3ef18a66e91"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0e5e56c7-0a1e-4892-b075-c0ba7fdecb8d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("56915525-9b5e-495a-98d5-305d5b116399"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0efc090a-899e-4014-a79b-87d907d65c35"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("efcfc0a2-7068-439d-b038-73cfbf5b24c1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("68962624-7bad-453b-bd00-ce6981ce9b94"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("77961b73-8a28-4f42-84af-2aa4afffee3c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("2a383218-a438-404b-a8fc-ddb41637567f"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0540fb2b-7e1a-450b-be1b-a43a639aab63"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e6a77f6e-c104-4bf1-855d-3dcd49b73ebb"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("fb22269f-ea94-4a5c-8db2-c471c0a8746f"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c6757ed8-a7ae-40fb-ab86-567c2283175c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("6ba99bf4-1eff-473e-9e09-81a2c65c585d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("62a36c8f-5d92-4ac2-ad05-698def977e3c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("2cb2e7d5-6537-475e-b01a-21a3c26b94c1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("94635e9c-9727-420f-956c-b84a106a3def"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("d4da973c-4aa4-41a6-9649-b607cb9f222b"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("2709cbc5-06d7-4512-94a7-27e16ee79436"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("babfa396-5c9f-4c8b-9549-8c1425f1aeff"));

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
                column: "Attachments",
                value: new List<string>());

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
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ac597cc6-d082-4ecc-b2db-3a108b5bee5a"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("5c0fea26-ba0e-44ce-b706-88428bada830"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("f8fe13a1-f15e-4e20-a9da-4d235c930e44"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("bde9cbff-b9c9-4464-9ea8-bd6f004c5049"));


            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e92f6ed2-2f13-4f2f-96e1-2b465ffc19e9"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("759efa60-f3db-4be3-bbc2-29bc313e11d5"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("56411734-7189-44a1-901a-b61be56bee67"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("9297ac99-bb12-42d8-a377-0bf792ccb041"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("c9fd428a-451d-4a08-a2d0-d3140840d385"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("9c088ccb-0e76-43e7-a9ee-199d06363fd0"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7feaed6e-c69d-4d94-bd7c-a3ef18a66e91"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("d21ac4a7-a896-4e86-9946-f1174383a07b"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("56915525-9b5e-495a-98d5-305d5b116399"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0e5e56c7-0a1e-4892-b075-c0ba7fdecb8d"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("efcfc0a2-7068-439d-b038-73cfbf5b24c1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0efc090a-899e-4014-a79b-87d907d65c35"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("77961b73-8a28-4f42-84af-2aa4afffee3c"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("68962624-7bad-453b-bd00-ce6981ce9b94"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0540fb2b-7e1a-450b-be1b-a43a639aab63"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("2a383218-a438-404b-a8fc-ddb41637567f"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("fb22269f-ea94-4a5c-8db2-c471c0a8746f"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("e6a77f6e-c104-4bf1-855d-3dcd49b73ebb"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("6ba99bf4-1eff-473e-9e09-81a2c65c585d"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("c6757ed8-a7ae-40fb-ab86-567c2283175c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("2cb2e7d5-6537-475e-b01a-21a3c26b94c1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("62a36c8f-5d92-4ac2-ad05-698def977e3c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("d4da973c-4aa4-41a6-9649-b607cb9f222b"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("94635e9c-9727-420f-956c-b84a106a3def"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("babfa396-5c9f-4c8b-9549-8c1425f1aeff"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("2709cbc5-06d7-4512-94a7-27e16ee79436"));

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
                column: "Attachments",
                value: new List<string>());

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
