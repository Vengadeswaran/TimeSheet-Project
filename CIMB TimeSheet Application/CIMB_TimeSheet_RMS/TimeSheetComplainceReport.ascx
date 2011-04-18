<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeSheetComplainceReport.ascx.cs"
    Inherits="CIMB_TimeSheet_RMS.TimeSheetComplainceReport" %>
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
        /*      $('#_go').button();*/
        $('input[type=button]').button();
        var _hiddenstdate = $("[id$='_hiddenstdate']");
        var _hiddenenddate = $("[id$='_hiddenenddate']");
        $("#_stdate").datepicker({ dateFormat: 'dd-mm-yy', maxDate: '-2d', defaultDate: '-2wk', altField: _hiddenstdate, altFormat: 'dd-M-yy' });
        $("#_enddate").datepicker({ dateFormat: 'dd-mm-yy', maxDate: '-1d', defaultDate: '-1d', altField: _hiddenenddate, altFormat: 'dd-M-yy' });
        var counter = 1;
        $('#_go').click(function () {
            if (counter > 1) {
                $("#Grid1").jqGrid('GridUnload');
            }
            if (_hiddenstdate[0].value != null && _hiddenenddate[0].value != null) {
                counter++;
                creategrid();
            }
        });
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
    });
    function creategrid() {
        $("#Grid1").jqGrid
            ({
                url: '/_layouts/CIMB_TimeSheet/TimeSheetComplainceData.aspx/GetDataTable?_stdate=' + $("[id$='_hiddenstdate']")[0].value + '&_enddate=' + $("[id$='_hiddenenddate']")[0].value,
                datatype: "json",
                colNames: ['TimeSheet Manager', 'Name', 'Period Name', 'TimeSheet Status'],
                colModel: [{ name: 'TS_Manager', index: 'TS_Manager', searchoptions: { searchhidden: true} },
                                    { name: 'Name', index: 'Name', resizable: false, width: '250' },
                                    { name: 'PeriodName', index: 'PeriodName', resizable: false, width: '150' },
                                    { name: 'TimeSheetStatus', index: 'TimeSheetStatus', resizable: false, width: '200'}],
                rowNum: 50,
                rowList: [50, 100, 200],
                rowTotal: 10000,
                height: 'auto',
                width: '600',
                shrinkToFit: false,
                mtype: "POST",
                pager: '#pager',
                sortname: 'TS_Manager',
                viewrecords: true,
                sortorder: "desc",
                loadonce: true,
                caption: "TimeSheet Compliance",
                imgpath: '/_Layouts/CIMB_TimeSheet/css/smoothness/images',
                grouping: true,
                ignoreCase: true,
                groupingView: {
                    groupField: ['TS_Manager'],
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
    <div>
        From Date:
        <input id="_stdate" type="text" />To Date:
        <input id="_enddate" type="text" />
        <input id="_hiddenstdate" type="text" style="display: none;" runat="server" />
        <input id="_hiddenenddate" type="text" style="display: none;" runat="server" />
        <input id="_go" type="button" value="Go" />
        <%--        <asp:Button ID="exportcsv" runat="server" Text="Export to CSV" UseSubmitBehavior="false"
            OnClick="exportcsv_Click" />
        --%>
    </div>
    <br />
    <div id="pager" class="scroll" style="text-align: center;">
    </div>
    <table id="Grid1" class="scroll" align="center" width="100%">
    </table>
</div>