using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Services;
using Microsoft.SharePoint;

namespace CIMB_TimeSheet_RMS._Layouts.CIMB_TimeSheet
{
    public partial class ApprovalPending_Data : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        [WebMethod]
        public static string GetDataTable(string resuid)
        {
            string OutputXMLValue = "<?xml version='1.0' encoding='utf-8' ?><rows>";
            try
            {
                var curesuid = new Guid(resuid);
                string getresqry = @" SELECT   ResourceUID
                                            FROM     MSP_EpmResource_UserView
                                            WHERE    (ResourceTimesheetManagerUID = '" + curesuid + @"')";
                string siteurl = HttpContext.Current.Request.UrlReferrer.ToString();
                WindowsImpersonationContext wik = WindowsIdentity.Impersonate(IntPtr.Zero);
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        var con = new SqlConnection(MyConfiguration.GetDataBaseConnectionString(siteurl));
                        con.Open();
                        var resoures_dt = new DataSet();
                        var adapter = new SqlDataAdapter(new SqlCommand(getresqry, con));
                        adapter.Fill(resoures_dt);
                        var res_uids = string.Empty;
                        if (resoures_dt.Tables.Count > 0)
                        {
                            var resources_table = resoures_dt.Tables[0];
                            if (resources_table.Rows.Count > 0)
                            {
                                foreach (DataRow ro in resources_table.Rows)
                                {
                                    res_uids = res_uids + "'" + ro["ResourceUID"] + "',";
                                }
                            }
                            else res_uids = "'" + curesuid + "',";
                        }
                        res_uids = "(" + res_uids.Substring(0, res_uids.Length - 1) + ")";
                        string gridqry = @"
SELECT     MSP_TimesheetResource.ResourceName, MSP_TimesheetPeriod.PeriodName, CASE WHEN (MSP_TimesheetProject.ProjectName = 'Administrative')
                      THEN MSP_TimesheetClass.ClassName ELSE MSP_TimesheetProject.ProjectName END AS ProjectName,
                      SUM(MSP_TimesheetActual.ActualWorkBillable + MSP_TimesheetActual.ActualWorkNonBillable + MSP_TimesheetActual.ActualOvertimeWorkBillable + MSP_TimesheetActual.ActualOvertimeWorkNonBillable)
                       AS Actual,  MSP_Timesheet.TimesheetUID
FROM         MSP_TimesheetPeriod INNER JOIN
                      MSP_Timesheet ON MSP_TimesheetPeriod.PeriodUID = MSP_Timesheet.PeriodUID INNER JOIN
                      MSP_TimesheetActual INNER JOIN
                      MSP_TimesheetResource ON MSP_TimesheetActual.LastChangedResourceNameUID = MSP_TimesheetResource.ResourceNameUID INNER JOIN
                      MSP_TimesheetLine ON MSP_TimesheetActual.TimesheetLineUID = MSP_TimesheetLine.TimesheetLineUID INNER JOIN
                      MSP_TimesheetProject ON MSP_TimesheetLine.ProjectNameUID = MSP_TimesheetProject.ProjectNameUID ON
                      MSP_Timesheet.OwnerResourceNameUID = MSP_TimesheetResource.ResourceNameUID AND
                      MSP_Timesheet.TimesheetUID = MSP_TimesheetLine.TimesheetUID INNER JOIN
                      MSP_TimesheetClass ON MSP_TimesheetLine.ClassUID = MSP_TimesheetClass.ClassUID
                      WHERE     (MSP_Timesheet.TimesheetStatusID = 1)
GROUP BY MSP_TimesheetPeriod.PeriodName, MSP_TimesheetResource.ResourceName, CASE WHEN (MSP_TimesheetProject.ProjectName = 'Administrative')
                      THEN MSP_TimesheetClass.ClassName ELSE MSP_TimesheetProject.ProjectName END, MSP_Timesheet.TimesheetUID
";
                        //AND (MSP_TimesheetResource.ResourceUID IN " + res_uids + @")
                        var dt = new DataSet();
                        adapter = new SqlDataAdapter(new SqlCommand(gridqry, con));
                        adapter.Fill(dt);
                        if (dt.Tables.Count > 0)
                        {
                            var maintbl = dt.Tables[0];
                            OutputXMLValue += "<page>1</page><total>" + maintbl.Rows.Count + "</total><records>1</records>";
                            try
                            {
                                int RowIndex = 1;
                                int resourceIndex;
                                int periodIndex;
                                //Tabel Column List -- ResourceName-0,PeriodName-1,ProjectName-2, Actual-3
                                // Get all the resource names first
                                var resourcenames =
                                    from c in maintbl.AsEnumerable()
                                    group c by c["ResourceName"];
                                // loop for all the resources
                                foreach (IGrouping<object, DataRow> grouping in resourcenames)
                                {
                                    OutputXMLValue += "<row><cell>" + grouping.Key + "</cell>";
                                    var periodnames =
                                        (from c in maintbl.AsEnumerable()
                                         where
                                             c.Field<string>("ResourceName").
                                             Equals(grouping.Key)
                                         select c);
                                    var sum = periodnames.Sum(row => row.Field<decimal>("Actual"));
                                    OutputXMLValue += "<cell>" + sum + "</cell><cell>false</cell><cell>" + grouping.First()["TimesheetUID"] + "</cell><cell>1</cell><cell>0</cell>";
                                    resourceIndex = RowIndex++;
                                    if (periodnames.Count() > 0)
                                        OutputXMLValue += "<cell>false</cell><cell>true</cell></row>";
                                    else
                                        OutputXMLValue += "<cell>true</cell><cell>true</cell></row>";

                                    var groupedperiodnames = from c in periodnames.AsEnumerable()
                                                             group c by c["PeriodName"];
                                    // loop for all the Periodnames
                                    foreach (IGrouping<object, DataRow> row in groupedperiodnames)
                                    {
                                        if (row.Count() > 0)
                                        {
                                            OutputXMLValue += "<row><cell>" + row.First()["PeriodName"] + "</cell>";
                                            var projectnames =
                                                (from c in
                                                     periodnames.AsEnumerable()
                                                 where
                                                     c.Field<string>(
                                                     "PeriodName").Equals(
                                                     row.First()["PeriodName"])
                                                 select c);
                                            sum =
                                                projectnames.Sum(
                                                    r =>
                                                    r.Field<decimal>("Actual"));
                                            OutputXMLValue += "<cell>" + sum +
                                                            "</cell><cell>false</cell><cell>" + row.First()["TimesheetUID"] + "</cell><cell>2</cell><cell>" +
                                                            resourceIndex +
                                                            "</cell>";
                                            periodIndex = RowIndex++;
                                            if (projectnames.Count() > 0)
                                                OutputXMLValue +=
                                                    "<cell>false</cell><cell>true</cell></row>";
                                            else
                                                OutputXMLValue +=
                                                    "<cell>true</cell><cell>true</cell></row>";
                                            var groupedprojectnames =
                                                from c in
                                                    projectnames.AsEnumerable()
                                                group c by c["ProjectName"];
                                            // loop for all projectnames
                                            foreach (
                                                IGrouping<object, DataRow>
                                                    dataRows in
                                                    groupedprojectnames)
                                            {
                                                if (dataRows.Count() > 0)
                                                {
                                                    OutputXMLValue +=
                                                        "<row><cell>" +
                                                        dataRows.First()["ProjectName"] +
                                                        "</cell>";
                                                    OutputXMLValue +=
                                                        "<cell>" +
                                                        dataRows.First()["Actual"] + "</cell><cell>false</cell><cell>" + dataRows.First()["TimesheetUID"] + "</cell><cell>3</cell><cell>" + periodIndex + "</cell>";
                                                    OutputXMLValue +=
                                                        "<cell>true</cell><cell>true</cell></row>";
                                                    RowIndex++;
                                                }
                                            }
                                        }
                                    }
                                }
                                //select c;
                            }
                            catch (Exception ex)
                            {
                                MyConfiguration.ErrorLog("Error At Manipulating XML Data due to " + ex.Message, EventLogEntryType.Error);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    MyConfiguration.ErrorLog("Error at SPSecurity Delegate :" + ex.Message, EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                MyConfiguration.ErrorLog("Error at web method due to " + ex.Message, EventLogEntryType.Error);
            }
            return OutputXMLValue + "</rows>";
        }
    }
}