using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Principal;
using System.Web;
using System.Web.Services;
using ITXProjectsLibrary;
using Microsoft.SharePoint;

namespace CIMB_TimeSheet_RMS._Layouts.CIMB_TimeSheet
{
    public partial class TimeSheetComplainceData : System.Web.UI.Page
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

        //public static s_GridResult GetDataTable(string _search, string nd, int rows, int page, string sidx, string sord)
        public static s_GridResult GetDataTable(string nd, int rows, int page, string sidx, string sord)
        {
            try
            {
                string _stdate = HttpContext.Current.Request.QueryString["_stdate"].ToString();
                string _enddate = HttpContext.Current.Request.QueryString["_enddate"].ToString();
                DateTime _enddateformatted = Convert.ToDateTime(_enddate);
                string siteurl = HttpContext.Current.Request.UrlReferrer.ToString();
                var url = new Uri(siteurl);
                string rbsurl = url.Scheme + "://" + url.Host + ":" + url.Port + url.Segments[0] + url.Segments[1];
                int startindex = (page - 1);
                int endindex = page;
                Guid adminguid;
                adminguid = new Guid("6FF0A657-63BC-4390-8AAF-7EE5CE033088");
                var rbs = new ReadRBSValues(System.Net.CredentialCache.DefaultNetworkCredentials, rbsurl);
                //var rbs = new ReadRBSValues(System.Net.CredentialCache.DefaultNetworkCredentials, "http://jump/cimb");
                var resuids = rbs.GetBottomLevelResouceUIDs();
                string filterresource = "(";
                foreach (var resuid in resuids)
                {
                    filterresource += "'" + resuid.ToString() + "',";
                }
                filterresource = filterresource.Substring(0, filterresource.Length - 1) + ")";
                string gridqry = @"
SELECT     res.ResourceUID, res.ResourceName, res.RBS, tperiod.PeriodUID, tperiod.PeriodStatusID, tperiod.StartDate, tperiod.EndDate, tperiod.PeriodName,
                      tperiod.LCID, ISNULL(TM_Name.ResourceName, 'Not Assigned') AS TM_Name
INTO            [#t1]
FROM         MSP_EpmResource_UserView AS TM_Name RIGHT OUTER JOIN
                      MSP_EpmResource_UserView AS res ON TM_Name.ResourceUID = res.ResourceTimesheetManagerUID CROSS JOIN
                      MSP_TimesheetPeriod AS tperiod
WHERE     (tperiod.StartDate BETWEEN DATEADD(d, - 7,'" + _stdate + @"') AND '" + _enddate + @"') AND (res.ResourceUID IN " + filterresource + @")
SELECT      [#t1].PeriodUID, [#t1].ResourceUID,[#t1].TM_Name, [#t1].RBS, [#t1].ResourceName, [#t1].PeriodName,
			ISNULl(tstatus.Description,'Not Created') AS [TimeSheet Status], [#t1].StartDate, [#t1].EndDate
INTO #t2
FROM        MSP_TimesheetStatus AS tstatus INNER JOIN
            MSP_Timesheet AS tsheet ON tstatus.TimesheetStatusID = tsheet.TimesheetStatusID INNER JOIN
            MSP_TimesheetResource AS tres ON tsheet.OwnerResourceNameUID = tres.ResourceNameUID RIGHT OUTER JOIN
            [#t1] ON [#t1].ResourceUID = tres.ResourceUID AND [#t1].PeriodUID = tsheet.PeriodUID
drop table #t1
SELECT TM_Name, ResourceName,PeriodName, [TimeSheet Status] FROM #t2 WHERE [TimeSheet Status] <> 'Approved' ORDER BY TM_Name, ResourceName, PeriodName
drop table #t2
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
                                //Tabel Column List -- ResourceName - 0,TimeSheet Period - 1,TimeSheet Period Status - 2
                                newrow.cell = new string[4];  //total number of columns
                                newrow.cell[0] = row[0].ToString(); //TimeSheet Manager name
                                newrow.cell[1] = row[1].ToString(); //resource name
                                newrow.cell[2] = row[2].ToString(); //TimeSheet Period Name
                                newrow.cell[3] = row[3].ToString(); //TimeSheet Period Status
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