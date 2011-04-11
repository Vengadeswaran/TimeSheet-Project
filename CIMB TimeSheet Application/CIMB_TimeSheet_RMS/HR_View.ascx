<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HR_View.ascx.cs" Inherits="CIMB_TimeSheet_RMS.HR_View" %>
<script src="Scripts/jquery-1.4.1-vsdoc.js" type="text/javascript"></script>
<script src="/_Layouts/CIMB_TimeSheet/js/jquery-1.4.4.min.js" type="text/javascript"></script>
<link href="/_Layouts/CIMB_TimeSheet/css/cimb_timesheet.css" rel="stylesheet" type="text/css" />
<link href="/_Layouts/CIMB_TimeSheet/css/basic.css" rel="stylesheet" type="text/css" />
<link href="/_Layouts/CIMB_TimeSheet/css/basic_ie.css" rel="stylesheet" type="text/css" />
<link href="/_Layouts/CIMB_TimeSheet/css/jumppcs.css" rel="stylesheet" type="text/css" />
<link href="/_Layouts/CIMB_TimeSheet/css/StdGrid.css" rel="stylesheet" type="text/css" />
<script src="/_Layouts/CIMB_TimeSheet/js/jquery.simplemodal.js" type="text/javascript"></script>
<!-- IE6 "fix" for the close png image -->
<!--[if lt IE 7]>
<link type='text/css' href='css/basic_ie.css' rel='stylesheet' media='screen' />
<![endif]-->
<div id="div_edit_entity">
</div>
<div style="display: none;">
    <img src='/_layouts/cimb_timesheet/img/basic/x.png' alt='' />
</div>
<div>
    <label>
        Select the Entity</label>
    <br />
    <asp:DropDownList ID="EntiyList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="filterDept">
    </asp:DropDownList>
    <asp:Button ID="Edit_Entity_btn" runat="server" Enabled="false" OnClick="Edit_Entity_btn_Click"
        Text="Edit Entity" />
    <%--<asp:Button ID="Add_Entity_btn" runat="server" Text="Add New Entity" OnClick="Add_Entity_btn_Click" />--%>
    <br />
    <label>
        Select Department</label>
    <br />
    <asp:DropDownList ID="DeptList" runat="server" AutoPostBack="true" Enabled="false"
        OnSelectedIndexChanged="filterRes">
        <asp:ListItem>
                        Select the Department
        </asp:ListItem>
    </asp:DropDownList>
    <br />
    <label>
        Select Resource to see the Secondment Details</label>
    <br />
    <asp:GridView ID="ResDetails" runat="server" AllowPaging="True" AutoGenerateColumns="false"
        AutoGenerateSelectButton="True" DataKeyNames="ID" EmptyDataText="No resource to the selected department"
        Enabled="false" OnSelectedIndexChanged="filterSecondment" SelectedIndex="1" RowStyle-CssClass="grid_Detail_Rows"
        AlternatingRowStyle-CssClass="grid_Detail_Rows01">
        <HeaderStyle CssClass="grid_Detail_Header" />
        <Columns>
            <asp:BoundField DataField="Resource Name" HeaderText="Resource Name" />
            <asp:BoundField DataField="HRIS ID" HeaderText="HRIS ID" />
            <asp:BoundField DataField="Old Emp Number" HeaderText="Old Emp Num" />
        </Columns>
    </asp:GridView>
    <label>
        Resource Secondment Details</label>
    <br />
    <asp:GridView ID="ResSecondment" runat="server" AllowPaging="True" AutoGenerateColumns="false"
        EmptyDataText="No resource secondment happend" Enabled="false" RowStyle-CssClass="grid_Detail_Rows"
        AlternatingRowStyle-CssClass="grid_Detail_Rows01">
        <HeaderStyle CssClass="grid_Detail_Header" />
        <Columns>
            <asp:BoundField DataField="Department" HeaderText="Department" />
            <asp:BoundField DataField="From Date" HeaderText="Start Date" />
            <asp:BoundField DataField="To Date" HeaderText="End Date" />
        </Columns>
    </asp:GridView>
</div>
<script type="text/javascript">
    $(function () {
        $("[id$='Edit_Entity_btn']")
			.click(function () {
			    $('#div_edit_entity').load("/_layouts/CIMB_TimeSheet/edit_entity.aspx?en_ID=" + $("[id$='EntiyList']").val() + "&rnd=" + Math.random(), function () {
			        $("[id$='btn_Save_Entity_update']")
            .click(function () {
                $("#div_edit_entity").hide();
            });
			        $("[id$='btn_Cancel_Entity_update']")
            .click(function () {
                $("#div_edit_entity").hide();
            });
			    });
			    $('#div_edit_entity').modal({ containerCss: { height: 200, width: 350} });
			    return false;
			});
    });

</script>