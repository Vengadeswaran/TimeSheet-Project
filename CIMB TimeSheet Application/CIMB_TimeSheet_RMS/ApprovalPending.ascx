<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ApprovalPending.ascx.cs"
    Inherits="CIMB_TimeSheet_RMS.ApprovalPending" %>
<link href="/_Layouts/CIMB_TimeSheet/css/smoothness/jquery-ui-1.8.9.custom.css" rel="stylesheet"
    type="text/css" />
<link href="/_Layouts/CIMB_TimeSheet/css/ui.jqgrid.css" rel="stylesheet" type="text/css" />

<script src="/_Layouts/CIMB_TimeSheet/js/jquery-1.4.4.min.js" type="text/javascript"></script>

<script src="_Layouts/CIMB_TimeSheet/js/jquery-ui-1.8.9.custom.min.js" type="text/javascript"></script>

<script src="/_Layouts/CIMB_TimeSheet/js/jquery.json-2.2.min.js" type="text/javascript"></script>

<script src="/_Layouts/CIMB_TimeSheet/js/grid.locale-en.js" type="text/javascript"></script>

<script src="/_Layouts/CIMB_TimeSheet/js/jquery.jqGrid.min.js" type="text/javascript"></script>

<script type="text/javascript">
    $(function() {
        $.ajax({
            type: "POST",
            url: "/_layouts/CIMB_TimeSheet/ApprovalPending_Data.aspx/GetDataTable",
            data: "{resuid:'" + $('.CurrentUserID').html() + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(msg) {
                $("#Grid1").jqGrid({
                    datastr: msg.d,
                    datatype: 'xmlstring',
                    height: 'auto',
                    rowNum: 50,
                    rowList: [10, 20, 30],
                    colNames: ['name', 'amount'],
                    colModel: [{ name: 'name', index: 'name', width: 420 },
                                { name: 'amount', index: 'amount', width: 80}],
                    pager: "#pager",
                    viewrecords: true,
                    sortname: 'name',
                    sortorder: "desc",
                    width: '510',
                    treeGridModel: 'adjacency',
                    treeGrid: true,
                    ExpandColumn: 'name',
                    ExpandColClick: true,
                    loadonce: true,
                    mtype: "POST",
                    shrinkToFit: false,
                    ignoreCase: true,
                    imgpath: '/_Layouts/CIMB_TimeSheet/css/smoothness/images',
                    caption: "Approval Pending",
                    serializeGridData: function(data) {
                    },
                    gridComplete: function() {
                    }
                });
            }
        });
    });
</script>

<style type="text/css">
    body
    {
        font-size: 75%;
    }
</style>
<div style="padding-left: 10px; padding-top: 10px; padding-top: 10px;">
    <div id="pager" class="scroll" style="text-align: center;">
    </div>
    <table id="Grid1" class="scroll" align="center" width="100%">
    </table>
        <asp:Label runat="server" CssClass="CurrentUserID" ID="LblCurrentResUID" style="display:none;" />
</div>