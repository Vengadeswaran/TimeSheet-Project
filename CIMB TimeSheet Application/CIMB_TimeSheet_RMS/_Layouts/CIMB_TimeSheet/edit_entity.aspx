<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="edit_entity.aspx.cs" Inherits="CIMB_TimeSheet_RMS.edit_entity" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server" action="/_layouts/CIMB_TimeSheet/edit_entity.aspx">
    <div>
        <h3>
            Edit Entity</h3>
        <label>
            Enity Name</label><br />
        <asp:TextBox ID="txtbox_edit_EntityName" runat="server" CssClass="txtbox_edit_entity"></asp:TextBox>
        <asp:RadioButtonList ID="rbtn_EntityStatus" runat="server">
        </asp:RadioButtonList>
        <asp:Button ID="btn_Save_Entity_update" runat="server" OnClick="btn_Save_Entity_update_Click"
            Text="Save Update" />
        <asp:Button ID="btn_Cancel_Entity_update" runat="server" Text="Cancel Update" OnClick="btn_Cancel_Entity_update_Click" />
        <asp:HiddenField ID="HiddenField1" runat="server" />
    </div>
    </form>
</body>
</html>