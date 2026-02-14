using Microsoft.AspNetCore.Authorization;

namespace MMS.Application.Common.Constants;

public static class AuthorizationPolicies
{

    public const string RequireSystemAdmin = "RequireSystemAdmin";
    public const string RequireCustomerAdmin = "RequireCustomerAdmin";
    public const string RequireOperator = "RequireOperator";
    public const string RequireTechnician = "RequireTechnician";
    public const string RequireViewer = "RequireViewer";
    public const string RequireMMSBridge= "RequireMMSBridge";
    public const string DenyAll = "DenyAll";


    public const string RequireSysAdminOrCustAdmin = "RequireSysAdminCustAdmin";
    public const string RequireSysAdminOrCustAdminOrViewer = "RequireSysAdminCustAdminViewer";
    public const string RequireSysAdminOrCustAdminOrTechnician = "RequireSysAdminOrCustAdminOrTechnician";
    public const string RequireSysAdminOrViewer = "RequireSysAdminOrViewer";
    public const string RequireTechnicianOrOperator = "RequireTechnicianOrOperator";
    public const string RequireSysAdminOrCustAdminOrTechnicianOrOperator = "RequireSysAdminOrCustAdminOrTechnicianOrOperator";
    public const string RequireSysAdminOrCustAdminOrOperator = "RequireSysAdminOrCustAdminOrOperator";
    public const string RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator = "RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperator";
    public const string RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge = "RequireSysAdminOrCustAdminOrViewerOrTechnicianOrOperatorOrMMSBridge";
    public const string RequireSysAdminOrCustAdminOrMMSBridge = "RequireSysAdminOrCustAdminOrMMSBridge";

}


//public static class AuthorizationPolicies
//{
//    public const string SysAdmin = "SysAdmin";
//    public const string CustAdmin = "CustAdmin";
//    public const string Operator = "Operator";
//    public const string Technician = "Technician";
//    public const string Viewer = "Viewer";
//    public const string MMSBridge = "MMSBridge";
//    public const string DenyAll = "DenyAll";

//    public const string SysOrCustAdmin = "SysOrCustAdmin";
//    public const string SysOrCustOrViewer = "SysOrCustOrViewer";
//    public const string SysOrCustOrTech = "SysOrCustOrTech";
//    public const string SysOrViewer = "SysOrViewer";
//    public const string TechOrOperator = "TechOrOperator";
//    public const string SysOrCustOrTechOrOp = "SysOrCustOrTechOrOp";
//    public const string SysOrCustOrOp = "SysOrCustOrOp";
//    public const string SysOrCustOrViewerOrTechOrOp = "SysOrCustOrViewerOrTechOrOp";
//    public const string SysOrCustOrViewerOrTechOrOpOrBridge = "SysOrCustOrViewerOrTechOrOpOrBridge";
//}