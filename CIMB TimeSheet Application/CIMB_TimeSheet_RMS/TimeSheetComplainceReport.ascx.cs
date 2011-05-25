using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Web;
using ITXProjectsLibrary;
using Microsoft.SharePoint;

namespace CIMB_TimeSheet_RMS
{
    public partial class TimeSheetComplainceReport : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //var rbs = new ReadRBSValues("tm01", "http://jump/cimb",);
            //var resids = rbs.GetBottomLevelResouceUIDs();
        }

        protected void exportcsv_Click(object sender, EventArgs e)
        {
            string _stdate = _hiddenstdate.Value.ToString();
            string _enddate = _hiddenenddate.Value.ToString();
            DateTime _enddateformatted = Convert.ToDateTime(_enddate);
            string siteurl = HttpContext.Current.Request.UrlReferrer.ToString();
            var url = new Uri(siteurl);
            string rbsurl = url.Scheme + "://" + url.Host + ":" + url.Port + url.Segments[0] + url.Segments[1];
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
SELECT		res.ResourceUID, res.ResourceName, res.RBS, tperiod.PeriodUID, tperiod.PeriodStatusID, tperiod.StartDate, tperiod.EndDate, tperiod.PeriodName,
            tperiod.LCID, ISNULL(TM_Name.ResourceName, 'Not Assigned') AS TM_Name
INTO        [#t1]
FROM        MSP_EpmResource_UserView AS TM_Name RIGHT OUTER JOIN
            MSP_EpmResource_UserView AS res ON TM_Name.ResourceUID = res.ResourceTimesheetManagerUID CROSS JOIN
            MSP_TimesheetPeriod AS tperiod
WHERE		(tperiod.StartDate BETWEEN (
			(SELECT		CASE WHEN (TimeDayOfTheWeek = 2) THEN '" + _stdate + @"' WHEN (TimeDayOfTheWeek = 1) THEN DATEADD(d,1, '" + _stdate + @"' )
						ELSE DATEADD(d,(2-TimeDayofTheWeek), '" + _stdate + @"' ) END AS stdate
			FROM        MSP_TimeByDay
			WHERE		(TimeByDay = CONVERT(DATETIME, '" + _stdate + @"' , 102)))
			)
			AND '" + _enddate + @"' ) AND (res.ResourceUID IN " + filterresource + @")
SELECT      [#t1].PeriodUID, [#t1].ResourceUID,[#t1].TM_Name, [#t1].RBS, [#t1].ResourceName, [#t1].PeriodName,
			ISNULl(tstatus.Description,'Not Created') AS [TimeSheet Status], [#t1].StartDate, [#t1].EndDate
INTO #t2
FROM        MSP_TimesheetStatus AS tstatus INNER JOIN
            MSP_Timesheet AS tsheet ON tstatus.TimesheetStatusID = tsheet.TimesheetStatusID INNER JOIN
            MSP_TimesheetResource AS tres ON tsheet.OwnerResourceNameUID = tres.ResourceNameUID RIGHT OUTER JOIN
            [#t1] ON [#t1].ResourceUID = tres.ResourceUID AND [#t1].PeriodUID = tsheet.PeriodUID
drop table	#t1
SELECT		PeriodName, TM_Name, ResourceName, COUNT(CASE WHEN ([TimeSheet Status] = 'In Progress') THEN [TimeSheet Status] END)
            AS [In Progress], COUNT(CASE WHEN ([TimeSheet Status] = 'Not Created') THEN [TimeSheet Status] END) AS [Not Created],
            COUNT(CASE WHEN ([TimeSheet Status] = 'Submitted') THEN [TimeSheet Status] END) AS Submitted
FROM        [#t2]
WHERE		([TimeSheet Status] <> 'Approved')
GROUP BY	PeriodName, TM_Name, ResourceName
ORDER BY	PeriodName, TM_Name, ResourceName
/*
SELECT		PeriodName, COUNT(CASE WHEN ([TimeSheet Status] = 'In Progress') THEN [TimeSheet Status] END)
            AS [In Progress], COUNT(CASE WHEN ([TimeSheet Status] = 'Not Created') THEN [TimeSheet Status] END) AS [Not Created],
            COUNT(CASE WHEN ([TimeSheet Status] = 'Submitted') THEN [TimeSheet Status] END) AS Submitted
FROM        [#t2]
WHERE		([TimeSheet Status] <> 'Approved')
GROUP BY	PeriodName
ORDER BY	PeriodName
*/
drop table	#t2
";

            WindowsImpersonationContext wik = null;
            wik = WindowsIdentity.Impersonate(IntPtr.Zero);
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SqlConnection con = new SqlConnection(MyConfiguration.GetDataBaseConnectionString(siteurl));
                con.Open();
                DataSet dt = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand(gridqry, con));
                adapter.Fill(dt);

                #region export function

                try
                {
                    // You should be very carefull with sharepoint layout folder, we have to specify full control permission to everybody.
                    string FolderPath = Server.MapPath("/_layouts/CIMB_TimeSheet/CSVFiles/");
                    if (Directory.Exists(FolderPath))
                    {
                        foreach (string file in Directory.GetFiles(FolderPath))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(FolderPath);
                    }
                    string clientfilename = DateTime.Now.ToString("ddMMyyhhmmss") + DateTime.Now.Millisecond.ToString();
                    string filename = FolderPath + clientfilename + ".csv";
                    var writer = new StreamWriter(filename);

                    //Writing column name at first row
                    string columnheaderline = string.Empty;

                    for (int i = 0; i < dt.Tables[0].Columns.Count; i++)
                    {
                        columnheaderline = columnheaderline + dt.Tables[0].Columns[i].ColumnName + ",";
                    }
                    writer.WriteLine(columnheaderline);
                    // Writing row values
                    foreach (DataRow row in dt.Tables[0].Rows)
                    {
                        string columnvalue = string.Empty;
                        for (int i = 0; i < dt.Tables[0].Columns.Count; i++)
                        {
                            columnvalue = columnvalue + row[i] + ",";
                        }
                        writer.WriteLine(columnvalue);
                    }
                    try
                    {
                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }
                    catch (Exception ex) { MyConfiguration.ErrorLog("Error in writing CSV : " + ex.Message, EventLogEntryType.Error); }
                    // Sending files here
                    Response.ContentType = "application/CSV";
                    Response.AddHeader("content-disposition", "attachment; filename=TimeSheetNonCompliance" + clientfilename + ".csv");
                    Response.TransmitFile(filename);
                    Response.Flush();
                    Response.End();
                }
                catch (Exception)
                {
                }

                #endregion export function
            });
        }
    }
}