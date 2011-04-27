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
    $(document).ready(function () {
        $.extend($.jgrid.defaults,
                  { datatype: 'json' },
                  { ajaxGridOptions: { contentType: "application/json",
                      success: function (data, textStatus) {
                          if (textStatus == "success") {
                              var thegrid = $("#Grid1")[0];
                              thegrid.addJSONData(data.d);
                              thegrid.grid.hDiv.loading = false;
                              switch (thegrid.p.loadui) {
                                  case "disable":
                                      break;
                                  case "enable":
                                      $("#load_" + thegrid.p.id).hide();
                                      break;
                                  case "block":
                                      $("#lui_" + thegrid.p.id).hide();
                                      $("#load_" + thegrid.p.id).hide();
                                      break;
                              }
                          }
                      }
                  }

                  });
        creategrid();
    });
    function creategrid() {
        $("#Grid1").jqGrid
            ({
                url: '/_layouts/CIMB_TimeSheet/ApprovalPending_Data.aspx/GetDataTable',
                datatype: "json",
                colNames: ['Name', 'Period', 'Project', 'Time'],
                colModel: [{ name: 'Name', index: 'Name', resizable: false, width: '200' },
                                    { name: 'Period', index: 'Period', resizable: false, width: '150' },
                                    { name: 'Project', index: 'Project', resizable: false, width: '250' },
                                    { name: 'Time', index: 'Time', resizable: false, width: '75'}],
                rowNum: 50,
                rowList: [50, 100, 200],
                rowTotal: 2000,
                height: 'auto',
                width: '675',
                shrinkToFit: false,
                mtype: "POST",
                pager: '#pager',
                sortname: 'Name',
                viewrecords: true,
                sortorder: "desc",
                loadonce: true,
                caption: "Approval Pending",
                imgpath: '/_Layouts/CIMB_TimeSheet/css/smoothness/images',
                grouping: false,
                ignoreCase: true,
                groupingView: {
                    groupField: ['Period'],
                    groupColumnShow: [false],
                    groupText: ['<b>{0}</b>'],
                    groupSummary: [true],
                    showSummaryOnHide: true,
                    groupCollapse: false
                },
                serializeGridData: function (data) {
                    return $.toJSON(data);
                },
                gridComplete: function () { $("#Grid1").setGridParam({ datatype: 'local' }); }
            });
        jQuery("#Grid1").jqGrid('navGrid', '#pager', { add: false, edit: false, del: false }, {}, {}, {}, { autosearch: true });
    }

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
</div>