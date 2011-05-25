<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ApprovalPending.ascx.cs"
    Inherits="CIMB_TimeSheet_RMS.ApprovalPending" %>
<link href="/_Layouts/CIMB_TimeSheet/css/smoothness/jquery-ui-1.8.9.custom.css" rel="stylesheet"
    type="text/css" />
<link href="/_Layouts/CIMB_TimeSheet/css/ui.jqgrid.css" rel="stylesheet" type="text/css" />
<script src="/_Layouts/CIMB_TimeSheet/js/jquery-1.5.2.min.js" type="text/javascript"></script>
<script src="_Layouts/CIMB_TimeSheet/js/jquery-ui-1.8.9.custom.min.js" type="text/javascript"></script>
<script src="/_Layouts/CIMB_TimeSheet/js/jquery.json-2.2.min.js" type="text/javascript"></script>
<script src="/_Layouts/CIMB_TimeSheet/js/grid.locale-en.js" type="text/javascript"></script>
<script src="/_Layouts/CIMB_TimeSheet/js/jquery.jqGrid.min.js" type="text/javascript"></script>
<script type="text/javascript">

    $(function () {
        $.ajax({
            type: "POST",
            url: "/_layouts/CIMB_TimeSheet/ApprovalPending_Data.aspx/GetDataTable?id=" + Math.random(),
            data: "{resuid:'" + $('.CurrentUserID').html() + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $("#Grid1").jqGrid({
                    datastr: msg.d,
                    datatype: 'xmlstring',
                    height: 'auto',
                    rowNum: 'auto',
                    /*
                    rowList: [10, 20, 30],
                    */
                    colNames: ['Name', 'Time', 'Approve', 'TSUID'],
                    colModel: [{ name: 'Name', index: 'Name', width: 420 },
                     { name: 'Time', index: 'Time', width: 80, formatter: 'number' },
                                { name: 'Approve', index: 'Approve', width: 60, align: 'center', formatter: disableCheckbox, editoptions: { value: '1:0' }, formatoptions: { disabled: true} },
                                { name: 'TSUID', index: 'TSUID', hidden: true },
                              ],
                    pager: "#pager",
                    viewrecords: true,
                    sortname: 'Name',
                    sortorder: "desc",
                    width: '575',
                    treeGridModel: 'adjacency',
                    treeGrid: true,
                    ExpandColumn: 'Name',
                    ExpandColClick: false,
                    loadonce: true,
                    //mtype: "POST",
                    shrinkToFit: true,
                    ignoreCase: true,
                    imgpath: '/_Layouts/CIMB_TimeSheet/css/smoothness/images',
                    caption: "Approval Pending",
                    serializeGridData: function (data) {
                    },
                    gridComplete: function () {
                        $('.Btnapprovets').show();
                    }
                });
            },
            error: function (e) {
                alert(e.message);
            }
        });
        function disableCheckbox(cellValue, opts, rowObject) {
            if (rowObject.childNodes[4].nodeTypedValue == 2) {
                return '<input type="checkbox" checked="checked" />';
            } return '<input type="checkbox" disabled="disabled" />';
        }
        $('.Btnapprovets').click(function () {
            var selectedtsuids = "";
            $("input:checked").each(function () {
                selectedtsuids += $(this).parent().next('td').attr('title') + "#";
            });
            $('.LstSelectedtsuids').val(selectedtsuids);
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
    <asp:Label runat="server" CssClass="CurrentUserID" ID="LblCurrentResUID" Style="display: none;" />
    <input type="hidden" name="LstSelectedtsuids" class="LstSelectedtsuids" />
    <asp:Button runat="server" ID="Approve" CssClass="Btnapprovets" Text="Approve" OnClick="Approve_Click"
        Style="display: none;" />
    <asp:Label runat="server" ID="LblStatus" />
</div>