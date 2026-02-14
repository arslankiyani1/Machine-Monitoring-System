using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class IndexesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_machine_id",
                table: "Machines");

            migrationBuilder.RenameIndex(
                name: "idx_machine_customerid",
                table: "Machines",
                newName: "IX_Machine_CustomerId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("190a3a2f-c402-4c7a-879d-028a75b98b48"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("8dd2f908-5399-42a4-926e-1f84e77bd88e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7db6eb39-e7be-4a4e-be62-9851202a2f0e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("0d9a9e09-e6b2-40dc-9aa2-644d38c042f5"));

            

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("aba1c732-e051-4829-9739-bd6c89b720c1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7940345d-6d5b-428d-a596-edf19fb4664b"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("2b30e3c7-d098-4748-a073-bcb39887929a"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("849a38bd-ec2f-4e4f-82ae-d59cf7ee27fe"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("637558d3-efd1-4bf7-a190-9db3d1c03606"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("8e0430f7-3551-424f-8d85-579eec71db7e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("56939e7e-4743-4bc4-b3e2-1d8a0a1f6fbe"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("fcc74b6b-b44b-4c40-8b9f-76b6521750c2"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7480377a-4ef8-4646-b3da-6e672ea42514"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4c2074c3-66e5-4d34-b836-293bda9258a1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("6dd6dde4-43fc-4cbf-be59-afa53c89ef2e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("744b5b8b-323d-4bc3-a685-1c941bfac762"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("13c35ca0-577a-4011-934e-538664362050"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("76a977e3-e72e-4288-b599-21b9ad85ca55"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("519b55dd-8901-4657-87b3-c8e66c850f84"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("8f7163f4-e8ce-4121-8e92-d8a65ae4049b"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("284a1bea-50b6-4c3f-bf2d-dd33c1770b00"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("4d18cfac-c66b-45ce-82bd-e88dcca4aa62"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ad648dad-1d52-4d26-99fa-1ad421bc7aed"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("48b9c9bf-6454-4f53-92cd-9ef07f5f33d1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("602e5b96-04d7-4a6d-a046-2b58bce0e1c3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("15ef835b-a105-421d-828c-89256b72b4a4"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("92543910-ba3f-4942-b47c-62b68a6c4238"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("3898e58c-15da-45d7-aed6-56f3704911a3"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("e39b2ce1-26ec-4b83-bbda-9e4435a47ef3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("dc161d26-2570-4e4a-bd92-023135ae5911"));

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
                name: "IX_Machine_CustomerId_Deleted_MachineName",
                table: "Machines",
                columns: new[] { "CustomerId", "Deleted", "MachineName" });

            migrationBuilder.CreateIndex(
                name: "IX_Machine_MachineModel",
                table: "Machines",
                column: "MachineModel")
                .Annotation("Npgsql:IndexMethod", "btree")
                .Annotation("Npgsql:IndexOperators", new[] { "varchar_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Machine_SerialNumber",
                table: "Machines",
                column: "SerialNumber")
                .Annotation("Npgsql:IndexMethod", "btree")
                .Annotation("Npgsql:IndexOperators", new[] { "varchar_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Deleted",
                table: "Customers",
                column: "Deleted");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Email",
                table: "Customers",
                column: "Email")
                .Annotation("Npgsql:IndexMethod", "btree")
                .Annotation("Npgsql:IndexOperators", new[] { "varchar_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Name",
                table: "Customers",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "btree")
                .Annotation("Npgsql:IndexOperators", new[] { "varchar_pattern_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Machine_CustomerId_Deleted_MachineName",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machine_MachineModel",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machine_SerialNumber",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Customer_Deleted",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customer_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customer_Name",
                table: "Customers");

            migrationBuilder.RenameIndex(
                name: "IX_Machine_CustomerId",
                table: "Machines",
                newName: "idx_machine_customerid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Widgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("8dd2f908-5399-42a4-926e-1f84e77bd88e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("190a3a2f-c402-4c7a-879d-028a75b98b48"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserMachines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("0d9a9e09-e6b2-40dc-9aa2-644d38c042f5"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7db6eb39-e7be-4a4e-be62-9851202a2f0e"));

           

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("7940345d-6d5b-428d-a596-edf19fb4664b"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("aba1c732-e051-4829-9739-bd6c89b720c1"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("849a38bd-ec2f-4e4f-82ae-d59cf7ee27fe"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("2b30e3c7-d098-4748-a073-bcb39887929a"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("8e0430f7-3551-424f-8d85-579eec71db7e"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("637558d3-efd1-4bf7-a190-9db3d1c03606"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("fcc74b6b-b44b-4c40-8b9f-76b6521750c2"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("56939e7e-4743-4bc4-b3e2-1d8a0a1f6fbe"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MachineMaintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4c2074c3-66e5-4d34-b836-293bda9258a1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("7480377a-4ef8-4646-b3da-6e672ea42514"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("744b5b8b-323d-4bc3-a685-1c941bfac762"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("6dd6dde4-43fc-4cbf-be59-afa53c89ef2e"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("76a977e3-e72e-4288-b599-21b9ad85ca55"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("13c35ca0-577a-4011-934e-538664362050"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("8f7163f4-e8ce-4121-8e92-d8a65ae4049b"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("519b55dd-8901-4657-87b3-c8e66c850f84"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReportSetting",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("4d18cfac-c66b-45ce-82bd-e88dcca4aa62"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("284a1bea-50b6-4c3f-bf2d-dd33c1770b00"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("48b9c9bf-6454-4f53-92cd-9ef07f5f33d1"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("ad648dad-1d52-4d26-99fa-1ad421bc7aed"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerDashboards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("15ef835b-a105-421d-828c-89256b72b4a4"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("602e5b96-04d7-4a6d-a046-2b58bce0e1c3"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CustomerBillingAddresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("3898e58c-15da-45d7-aed6-56f3704911a3"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("92543910-ba3f-4942-b47c-62b68a6c4238"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("dc161d26-2570-4e4a-bd92-023135ae5911"),
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
    }
}
