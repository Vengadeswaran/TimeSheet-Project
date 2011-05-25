<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TimeSheetComplainceSubGridData.aspx.cs"
    Inherits="CIMB_TimeSheet_RMS._Layouts.CIMB_TimeSheet.TimeSheetComplainceSubGridData" %>

<%@ Register Src="../../TimeSheetComplainceReport.ascx" TagName="TimeSheetComplainceReport"
    TagPrefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <uc1:TimeSheetComplainceReport ID="TimeSheetComplainceReport1" runat="server" />
    </form>
</body>
</html>