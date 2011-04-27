using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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

        public struct s_GridResult
        {
            public int page;
            public int total;
            public int record;
            public s_RowData[] rows;
        }
        public struct s_RowData
        {
            public int id;
            public string[] cell;
        }

        [WebMethod]

        public static s_GridResult GetDataTable(string nd, int rows, int page, string sidx, string sord)
        {
            try
            {
                string siteurl = HttpContext.Current.Request.UrlReferrer.ToString();

                string gridqry = @"
SELECT     MSP_TimesheetResource.ResourceName, MSP_TimesheetPeriod.PeriodName, CASE WHEN (MSP_TimesheetProject.ProjectName = 'Administrative')
                      THEN MSP_TimesheetClass.ClassName ELSE MSP_TimesheetProject.ProjectName END AS ProjectName,
                      SUM(MSP_TimesheetActual.ActualWorkBillable + MSP_TimesheetActual.ActualWorkNonBillable + MSP_TimesheetActual.ActualOvertimeWorkBillable + MSP_TimesheetActual.ActualOvertimeWorkNonBillable)
                       AS Actual
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
                      THEN MSP_TimesheetClass.ClassName ELSE MSP_TimesheetProject.ProjectName END
";
                WindowsImpersonationContext wik = null;
                wik = WindowsIdentity.Impersonate(IntPtr.Zero);
                s_GridResult result = new s_GridResult();
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SqlConnection con = new SqlConnection(MyConfiguration.GetDataBaseConnectionString(siteurl));
                        con.Open();
                        DataSet dt = new DataSet();
                        SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand(gridqry, con));
                        adapter.Fill(dt);
                        DataTable maintbl = dt.Tables[0];
                        List<s_RowData> rowsadded = new List<s_RowData>();
                        int idx = 1;
                        try
                        {
                            foreach (DataRow row in maintbl.Rows)
                            {
                                s_RowData newrow = new s_RowData();
                                newrow.id = idx++;
                                //Tabel Column List -- ResourceName - 0,TimeSheet Period - 1,Project Name - 2, Time Clocked - 3
                                newrow.cell = new string[4];  //total number of columns
                                newrow.cell[0] = row[0].ToString(); //Resource Name
                                newrow.cell[1] = row[1].ToString(); //TimeSheet Period
                                newrow.cell[2] = row[2].ToString(); //Project Name
                                newrow.cell[3] = row[3].ToString(); //Time Clocked
                                rowsadded.Add(newrow);
                            }
                        }
                        catch (Exception ex)
                        {
                            MyConfiguration.ErrorLog("Error At Manipulating Json Data" + ex.Message, EventLogEntryType.Error);
                        }
                        result.rows = rowsadded.ToArray();
                        result.page = page;
                        result.total = dt.Tables[0].Rows.Count;
                        result.record = rowsadded.Count;
                    });
                }
                catch (Exception ex)
                {
                    MyConfiguration.ErrorLog("Error at SPSecurity Delegate :" + ex.Message, EventLogEntryType.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                MyConfiguration.ErrorLog("Error at web method due to " + ex.Message, EventLogEntryType.Error);
            }
            return new s_GridResult();
        }
    }
}