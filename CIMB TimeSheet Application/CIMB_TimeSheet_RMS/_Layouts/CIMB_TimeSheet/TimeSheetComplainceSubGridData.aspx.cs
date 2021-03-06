﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Principal;
using System.Web;
using System.Web.Services;
using ITXProjectsLibrary.WebSvcResource;
using Microsoft.SharePoint;

namespace CIMB_TimeSheet_RMS._Layouts.CIMB_TimeSheet
{
    public partial class TimeSheetComplainceSubGridData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public struct s_GridResult_subgrid
        {
            public int page;
            public int total;
            public int record;
            public s_RowData_subgrid[] rows;
        }
        public struct s_RowData_subgrid
        {
            public int id;
            public string[] cell;
        }

        [WebMethod]

        public static s_GridResult_subgrid GetSubGridData(string nd, int rows, int page, string sidx, string sord)
        {
            try
            {
                string _stdate = HttpContext.Current.Request.QueryString["_stdate"].ToString();
                string _enddate = HttpContext.Current.Request.QueryString["_enddate"].ToString();
                string _periodname = HttpContext.Current.Request.QueryString["_periodname"].ToString();
                DateTime _enddateformatted = Convert.ToDateTime(_enddate);
                string siteurl = HttpContext.Current.Request.UrlReferrer.ToString();
                var url = new Uri(siteurl);
                string rbsurl = url.Scheme + "://" + url.Host + ":" + url.Port + url.Segments[0] + url.Segments[1];
                int startindex = (page - 1);
                int endindex = page;
                Guid adminguid;
                var resource_svc = new Resource();
                adminguid = new Guid("6FF0A657-63BC-4390-8AAF-7EE5CE033088");
                resource_svc.Url = "http://jump/cimb/_vti_bin/psi/resource.asmx";

                if (url.Host.ToString() != "localhost")
                {
                    //var rbs = new ReadRBSValues(System.Net.CredentialCache.DefaultNetworkCredentials, "http://jump/cimb");
                    resource_svc.Url = rbsurl + "/_vti_bin/psi/resource.asmx";
                }
                //var rbs = new ReadRBSValues(System.Net.CredentialCache.DefaultNetworkCredentials, rbsurl);
                //var resuids = rbs.GetBottomLevelResouceUIDs();
                resource_svc.UseDefaultCredentials = true;
                resource_svc.AllowAutoRedirect = true;
                Guid currentuserid = resource_svc.GetCurrentUserUid();
                string res_under_curr_user = @"
                SELECT		ResourceUID
                FROM		dbo.MSP_EpmResource_UserView
                WHERE		(RBS Like (	(
						                SELECT	RBS
						                FROM	dbo.MSP_EpmResource_UserView
						                WHERE	ResourceUID = '" + currentuserid.ToString() + @"'
						                )
					                +'.%')
			                ) AND ResourceIsActive = 1
                ";
                WindowsImpersonationContext wik = null;
                wik = WindowsIdentity.Impersonate(IntPtr.Zero);
                SqlConnection con = new SqlConnection(MyConfiguration.GetDataBaseConnectionString(siteurl));
                con.Open();
                DataSet filterresourcelist = new DataSet();
                SqlDataAdapter filterresourceadapter = new SqlDataAdapter(res_under_curr_user, con);
                filterresourceadapter.Fill(filterresourcelist);
                string filterresource = "(";
                foreach (DataRow row in filterresourcelist.Tables[0].Rows)
                {
                    filterresource += "'" + row[0].ToString() + "',";
                }
                /*                foreach (var resuid in resuids)
                                {
                                    filterresource += "'" + resuid.ToString() + "',";
                                }
                */
                filterresource = filterresource.Substring(0, filterresource.Length - 1) + ")";
                MyConfiguration.ErrorLog("My Resource UIDs : " + filterresource, EventLogEntryType.Information);
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
SELECT		TM_Name, ResourceName, COUNT(CASE WHEN ([TimeSheet Status] = 'In Progress') THEN [TimeSheet Status] END)
            AS [In Progress], COUNT(CASE WHEN ([TimeSheet Status] = 'Not Created') THEN [TimeSheet Status] END) AS [Not Created],
            COUNT(CASE WHEN ([TimeSheet Status] = 'Submitted') THEN [TimeSheet Status] END) AS Submitted,
            COUNT(CASE WHEN ([TimeSheet Status] = 'Approved') THEN [TimeSheet Status] END) AS Approved
FROM        [#t2]
--WHERE		([TimeSheet Status] <> 'Approved') AND (PeriodName = '" + _periodname + @"')
WHERE		(PeriodName = '" + _periodname + @"')
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
                s_GridResult_subgrid result = new s_GridResult_subgrid();
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        DataSet dt = new DataSet();
                        SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand(gridqry, con));
                        adapter.Fill(dt);
                        DataTable maintbl = dt.Tables[0];
                        List<s_RowData_subgrid> rowsadded = new List<s_RowData_subgrid>();
                        int idx = 1;
                        try
                        {
                            foreach (DataRow row in maintbl.Rows)
                            {
                                s_RowData_subgrid newrow = new s_RowData_subgrid();
                                newrow.id = idx++;
                                //Tabel Column List -- TM Name - 0,Resource Name - 1,In Progress - 2,Not Created - 3,Submitted - 4
                                newrow.cell = new string[6];  //total number of columns
                                newrow.cell[0] = row[0].ToString(); //Timesheet Manager
                                newrow.cell[1] = row[1].ToString(); //Resource Name
                                newrow.cell[2] = row[2].ToString(); //In Progress
                                newrow.cell[3] = row[3].ToString(); //Not Created
                                newrow.cell[4] = row[4].ToString(); //Submitted
                                newrow.cell[5] = row[5].ToString(); //Approved
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
            return new s_GridResult_subgrid();
        }
    }
}