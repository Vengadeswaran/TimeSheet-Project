<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CIMB_TimeSheet_RMS._Default" %>

<%@ Register Src="HR_View.ascx" TagName="HR_View" TagPrefix="uc1" %>
<%@ Register Src="Ajax_Test.ascx" TagName="Ajax_Test" TagPrefix="uc2" %>
<%@ Register Src="Res_Captilization.ascx" TagName="Res_Captilization" TagPrefix="uc3" %>
<%@ Register Src="ManDaysByProject.ascx" TagName="ManDaysByProject" TagPrefix="uc4" %>
<%@ Register Src="TimeSheetComplainceReport.ascx" TagName="TimeSheetComplainceReport"
    TagPrefix="uc5" %>
<%@ Register Src="ApprovalPending.ascx" TagName="ApprovalPending" TagPrefix="uc6" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%--<uc1:HR_View ID="HR_View1" runat="server" />--%>
        <%--<uc2:Ajax_Test ID="Ajax_Test1" runat="server" />--%>
        <%--<uc3:Res_Captilization ID="Res_Captilization1" runat="server" />--%>
        <%--<uc4:ManDaysByProject ID="ManDaysByProject1" runat="server" />--%>
        <uc5:TimeSheetComplainceReport ID="TimeSheetComplainceReport1" runat="server" />
        <%--<uc6:ApprovalPending ID="ApprovalPending1" runat="server" />--%>
    </div>
    </form>
</body>
</html>