<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Ajax_Test.ascx.cs" Inherits="CIMB_TimeSheet_RMS.Ajax_Test" %>
<div>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <asp:UpdatePanel runat="server" ID="UpdatePanel" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="UpdateButton1" EventName="Click" />
        </Triggers>
        <ContentTemplate>
            <asp:TextBox ID="DateTimeLabel1" runat="server"></asp:TextBox>
            <asp:Button runat="server" ID="UpdateButton1" OnClick="UpdateButton_Click" Text="Update" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel runat="server" ID="UpdatePanel1" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:TextBox ID="DateTimeLabel2" runat="server"></asp:TextBox>
            <asp:Button runat="server" ID="UpdateButton2" OnClick="UpdateButton_Click" Text="Update" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>