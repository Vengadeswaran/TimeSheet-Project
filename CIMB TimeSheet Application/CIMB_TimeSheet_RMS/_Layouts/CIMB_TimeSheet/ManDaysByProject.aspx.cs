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
    public partial class ManDaysByProject : System.Web.UI.Page
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
                string _stdate = HttpContext.Current.Request.QueryString["_stdate"].ToString();
                string _enddate = HttpContext.Current.Request.QueryString["_enddate"].ToString();
                DateTime _enddateformatted = Convert.ToDateTime(_enddate);
                //MyConfiguration.ErrorLog("Start Date : " + _stdate + "   End Date : " + _enddate, EventLogEntryType.SuccessAudit);
                string siteurl = HttpContext.Current.Request.UrlReferrer.ToString();
                int startindex = (page - 1);
                int endindex = page;
                string gridqry = @"
SELECT     t_res.ResourceUID AS ResUID, t_proj.ProjectUID AS ProjUID, t_res.ResourceName AS ResName,
CASE WHEN t_class.Type <> 0 THEN t_class.ClassNAME ELSE t_proj.ProjectName END AS ProjName, t_act.TimeByDay AS t_date,
t_act.TimeByDay_DayOfWeek AS t_wkday, t_class.Type AS t_type, CASE WHEN ((t_act.TimeByDay_DayOfWeek = 1 OR
t_act.TimeByDay_DayOfWeek = 7) AND (t_class.Type = 1)) THEN '0' WHEN (t_class.Type = 1 AND
(t_act.ActualWorkBillable + t_act.ActualWorkNonBillable + t_act.ActualOvertimeWorkBillable + t_act.ActualOvertimeWorkNonBillable <= 4))
THEN '4' WHEN (t_class.Type = 1 AND
(t_act.ActualWorkBillable + t_act.ActualWorkNonBillable + t_act.ActualOvertimeWorkBillable + t_act.ActualOvertimeWorkNonBillable > 4))
THEN '8' ELSE t_act.ActualWorkBillable + t_act.ActualWorkNonBillable + t_act.ActualOvertimeWorkBillable + t_act.ActualOvertimeWorkNonBillable END
AS totaltime
INTO            [#t1]
FROM         MSP_TimesheetActual AS t_act INNER JOIN
                      MSP_TimesheetResource AS t_res ON t_act.LastChangedResourceNameUID = t_res.ResourceNameUID INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_act.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID INNER JOIN
                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID
WHERE     (t_act.TimeByDay BETWEEN '" + _stdate + @"' AND '" + _enddate + @"')
ORDER BY ResUID, t_date, t_type

SELECT #t1.ResUID, #t1.ProjUID, #t1.ResName, #t1.ProjName, #t1.t_date, #t1.t_wkday, #t1.t_type, #t1.totaltime,
		ISNULL(dummyt1_nkwking.nonwktotal,0) AS nwktotal, ISNULL(dummyt1.daytotal,0) AS daytotal,
		CASE WHEN #t1.t_type <> 1 AND ((dummyt1_nkwking.nonwktotal = 4 AND dummyt1.daytotal >4)or (ISNULL(dummyt1_nkwking.nonwktotal,0) =8) or
					(ISNULL(dummyt1_nkwking.nonwktotal,0) = 0 AND dummyt1.daytotal > 8))
			THEN (8-ISNULL(dummyt1_nkwking.nonwktotal,0))*#t1.totaltime/ISNULL(dummyt1.daytotal,1)
		ELSE #t1.totaltime END AS normalizedtime into #t2
FROM #t1 LEFT OUTER JOIN(
SELECT     dummyt1_nwking.ResUID, dummyt1_nwking.t_date, SUM(dummyt1_nwking.totaltime) AS nonwktotal
FROM         [#t1] AS dummyt1_nwking
WHERE     (dummyt1_nwking.t_type = 1)
GROUP BY dummyt1_nwking.ResUID, dummyt1_nwking.t_date
--ORDER BY dummyt1_nwking.ResUID, dummyt1_nwking.t_date
) AS dummyt1_nkwking ON #t1.ResUID = dummyt1_nkwking.ResUID AND #t1.t_date = dummyt1_nkwking.t_date LEFT OUTER JOIN
(
SELECT     dummyt1.ResUID, dummyt1.t_date, SUM(dummyt1.totaltime) AS daytotal
FROM         [#t1] AS dummyt1
WHERE     (dummyt1.t_type <> 1)
GROUP BY dummyt1.ResUID, dummyt1.t_date
--ORDER BY dummyt1.ResUID, dummyt1.t_date
) AS dummyt1 ON #t1.ResUID = dummyt1.ResUID AND #t1.t_date = dummyt1.t_date
ORDER BY #t1.ResUID, #t1.t_date, #t1.t_type
drop table #t1

SELECT distinct     TimeByDay AS missing_date,TimeDayOfTheWeek AS missing_wkday, t_Resources.ResUID AS missing_ResUID, t_Resources.ResName INTO #t3
FROM         MSP_TimeByDay CROSS JOIN (SELECT distinct [#t2].ResUID, #t2.ResName FROM [#t2]) AS t_Resources
WHERE     (TimeByDay BETWEEN '" + _stdate + @"' AND '" + _enddate + @"') AND
(NOT EXISTS
(SELECT     *
FROM         [#t2] WHERE MSP_TimeByDay.TimeByDay = [#t2].t_date AND t_Resources.ResUID = [#t2].ResUID))

INSERT INTO #t2
SELECT #t3.missing_ResUID, 'E38038FA-F8CA-47D1-BFD4-6B45B8462972',#t3.ResName,'NotClocked',#t3.missing_date, #t3.missing_wkday, 1, 8, 8,0,8
FROM #t3 WHERE #t3.missing_wkday <> '1' AND #t3.missing_wkday <> '7'

drop table #t3

SELECT #t2.ResUID, SUM(#t2.normalizedtime) AS total_paid_leave INTO #t4
FROM #t2
WHERE #t2.t_type = 1 AND #t2.ProjName = 'Paid Leave-Public Holiday'
GROUP By #t2.ResUID
SELECT #t2.ResUID,#t2.ProjUID,
SUM(
	(CONVERT(NUMERIC(18,14),#t2.normalizedtime) /(CASE WHEN nortotal.total_Proj = 0 THEN 1.0 ELSE CONVERT(NUMERIC(18,14),nortotal.total_Proj) END)
													)*CONVERT(NUMERIC(18,14),ISNULL(#t4.total_paid_leave,0)) + CONVERT(NUMERIC(18,14),#t2.normalizedtime)
) AS normalized_proj_time
INTO #t5
FROM #t2 INNER JOIN
(select #t2.ResUID, SUM(#t2.normalizedtime) AS total_Proj FROM #t2 WHERE t_type = 0 GROUP BY #t2.ResUID) AS nortotal ON #t2.ResUID = nortotal.ResUID
LEFT OUTER JOIN #t4 ON #t2.ResUID = #t4.ResUID
WHERE #t2.t_type = 0
GROUP BY #t2.ResUID, #t2.ProjUID, #t2.ProjName, #t2.t_type

--select * from #t2
drop table #t4

SELECT #t2.ResUID, #t2.ProjUID, #t2.ProjName, #t2.t_type, SUM (convert(numeric(18,14),#t2.normalizedtime)) AS nor_time
INTO #t6
FROM #t2
GROUP By #t2.ResUID, #t2.ProjUID, #t2.ProjName,#t2.t_type

drop table #t2

select #t6.ResUID, #t6.ProjUID, #t6.ProjName,
CASE WHEN #t6.t_type = 0 THEN #t5.normalized_proj_time ELSE #t6.nor_time END AS total_time,
CASE WHEN #t6.t_type = 0 THEN (convert (float,#t5.normalized_proj_time)/res_total.res_total_time)*100
ELSE (convert(float,#t6.nor_time) /res_total.res_total_time)*100 END
AS pct_total_time, #t6.t_type INTO #t7
from #t6 LEFT OUTER JOIN #t5 on #t6.ResUID = #t5.ResUID and #t6.ProjUID = #t5.ProjUID LEFT OUTER JOIN
(select #t6.ResUID, SUM (#t6.nor_time) AS res_total_time FROM #t6 GROUP BY #t6.ResUID) AS res_total ON #t6.ResUID = res_total.ResUID
where #t6.ProjName <> 'Paid Leave-Public Holiday'
order by #t6.ResUID
drop table #t5
drop table #t6
select #t7.ResUID, #t7.ProjUID, #t7.ProjName, #t7.total_time, #t7.pct_total_time,uv_res.ResourceName,
ISNULL (uv_proj.Project_RC_Code, #t7.ProjName) AS costcode, ISNULL(uv_res.ResourceEmailAddress,'NoEmail') AS res_email
FROM #t7 left outer join dbo.MSP_EpmResource_UserView as uv_res on #t7.ResUID = uv_res.ResourceUID
left outer join dbo.MSP_EpmProject_UserView as uv_proj on #t7.ProjUID = uv_proj.ProjectUID
order by #t7.resuid, #t7.t_type

drop table #t7
";
                string projwkspaceurlqry = @"SELECT     ProjectUID, ProjectWorkspaceInternalHRef
                                            FROM         MSP_EpmProject_UserView";
                WindowsImpersonationContext wik = null;
                wik = WindowsIdentity.Impersonate(IntPtr.Zero);
                s_GridResult result = new s_GridResult();
                //string siteurl = "http://epmdr/2011";  //right now i am hard coding it in the myconfiguration itself
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        SqlConnection con = new SqlConnection(MyConfiguration.GetDataBaseConnectionString(siteurl));
                        con.Open();
                        DataSet dt = new DataSet();
                        DataSet pjwkspacerecord = new DataSet();
                        SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand(gridqry, con));
                        adapter.Fill(dt);
                        adapter = new SqlDataAdapter(new SqlCommand(projwkspaceurlqry, con));
                        adapter.Fill(pjwkspacerecord);
                        DataTable maintbl = dt.Tables[0];
                        maintbl.Columns.Add("HRISID");
                        maintbl.Columns.Add("OLDEMPID");
                        maintbl.Columns.Add("PayRolStDate");
                        maintbl.Columns.Add("PayRolEntity");
                        maintbl.Columns.Add("staftype");
                        //maintbl.Columns.Add("ProjRole");
                        DataView view = new DataView(dt.Tables[0]);
                        DataTable resourcelist = view.ToTable(true, "ResourceName", "res_email");
                        string resname = string.Empty;
                        string resemail = string.Empty;
                        //DataTable projlist = view.ToTable(true, "ProjUID", "ResourceName", "res_email");

                        try
                        {
                            using (SPSite site = new SPSite(MyConfiguration.frm_siteurl_GetSiteURL(siteurl)))
                            {
                                #region Project Role Manipulation

                                /*
                                foreach (DataRow row in projlist.Rows)
                                {
                                    foreach (DataRow siteurlrow in pjwkspacerecord.Tables[0].Select("ProjectUID ='" + row["ProjUID"] + "'"))
                                    {
                                        string pjwkspaceurl = siteurlrow["ProjectWorkspaceInternalHRef"].ToString();
                                        try
                                        {
                                            SPWeb pjwkspaceWeb = site.AllWebs["pjwkspaceurl"];
                                            try
                                            {
                                                SPList pjwkspace_list = pjwkspaceWeb.Lists["Project Resource Details"];
                                                SPQuery resdetailqry = new SPQuery();
                                                resdetailqry.Query = @"
                                                <Where>
                                                    <Eq>
                                                        <FieldRef Name='Resource_x0020_Name' />
                                                        <Value Type='User'>" + row["ResourceName"] + @"</Value>
                                                    </Eq>
                                                </Where>
                                                <OrderBy>
                                                    <FieldRef Name='ID' Ascending='False' />
                                                </OrderBy>
                                                <RowLimit>1</RowLimit>";
                                                SPListItemCollection resdetailitems = pjwkspace_list.GetItems(resdetailqry);
                                                if (resdetailitems.Count > 0)
                                                {
                                                    foreach (SPListItem itm in resdetailitems)
                                                    {
                                                        if ((itm.Fields.ContainsField("Project_x0020_Role")) && (itm["Project_x0020_Role"] != null))
                                                        {
                                                            foreach (DataRow maintblrow in maintbl.Select("ProjectUID ='" + row["ProjUID"] + "'"))
                                                            {
                                                                maintblrow["ProjRole"] = itm["Project_x0020_Role"].ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            MyConfiguration.ErrorLog(row["ResourceName"].ToString() + " don't have any Role in this Project", EventLogEntryType.Error);

                                                            foreach (DataRow maintblrow in maintbl.Select("ProjUID ='" + row["ProjUID"] + "' AND ResourceName ='" + row["ResourceName"] + "'"))
                                                            {
                                                                maintblrow["ProjRole"] = "NA";
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    MyConfiguration.ErrorLog(row["ProjUID"].ToString() + " Don't have any Resource Detail Record", EventLogEntryType.Error);

                                                    foreach (DataRow maintblrow in maintbl.Select("ProjUID ='" + row["ProjUID"] + "' AND ResourceName ='" + row["ResourceName"] + "'"))
                                                    {
                                                        maintblrow["ProjRole"] = "NA";
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                MyConfiguration.ErrorLog(row["ProjUID"].ToString() + " Don't have Resource Detail List", EventLogEntryType.Error);
                                                foreach (DataRow maintblrow in maintbl.Select("ProjUID ='" + row["ProjUID"] + "'"))
                                                {
                                                    maintblrow["ProjRole"] = "NA";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MyConfiguration.ErrorLog(row["ProjUID"].ToString() + " Don't have Workspace", EventLogEntryType.Error);
                                            foreach (DataRow maintblrow in maintbl.Select("ProjUID ='" + row["ProjUID"] + "'"))
                                            {
                                                maintblrow["ProjRole"] = "NA";
                                            }
                                        }
                                    }
                                }
                                */

                                #endregion Project Role Manipulation

                                foreach (DataRow row in resourcelist.Rows)
                                {
                                    SPWeb hrWeb = site.AllWebs["HRMV02"];
                                    //MyConfiguration.ErrorLog("SPWeb URL : " + hrWeb.Url.ToString(), EventLogEntryType.SuccessAudit);
                                    //SPDataSource resdetds = new SPDataSource();
                                    SPList res_detail = hrWeb.Lists["Resource Details"];
                                    SPList seconded = hrWeb.Lists["Resource Secondment"];
                                    SPQuery qry = new SPQuery();
                                    qry.Query = @"<Where>
                                        <Eq>
                                            <FieldRef Name='EMail'/>
                                            <Value Type='Text'>" + row[1].ToString() + @"</Value>
                                        </Eq>
                                    </Where>";
                                    //resdetds.List = hrWeb.Lists["Resource Details"];
                                    SPListItemCollection items = res_detail.GetItems(qry);
                                    //MyConfiguration.ErrorLog("Resource Details Return Rows : " + items.Count, EventLogEntryType.SuccessAudit);
                                    if (items.Count > 0)
                                    {
                                        foreach (SPListItem itm in items)
                                        {
                                            #region Secondment Data manipulation

                                            SPQuery secondedqry = new SPQuery();
                                            secondedqry.Query = @"
<ViewFields>
    <FieldRef Name='Department' />
    <FieldRef Name='From_x0020_Date' />
    <FieldRef Name='To_x0020_Date' />
</ViewFields>
<Where>
    <And>
        <Eq>
            <FieldRef Name='Resource_x0020_Name' />
            <Value Type='Lookup'>" + itm["Resource Name"].ToString() + @"</Value>
        </Eq>
        <Leq>
            <FieldRef Name='From_x0020_Date' />
            <Value Type='DateTime'>" + string.Format("{0:yyyy-MM-dd}", _enddateformatted) + @"</Value>
        </Leq>
    </And>
</Where>
<OrderBy>
    <FieldRef Name='ID' Ascending='False' />
</OrderBy>
<RowLimit>1</RowLimit>
                                            ";
                                            //MyConfiguration.ErrorLog("List Resource Name : " + itm["Resource Name"].ToString(), EventLogEntryType.SuccessAudit);
                                            //MyConfiguration.ErrorLog("List Department Name : " + itm["Department"].ToString(), EventLogEntryType.SuccessAudit);
                                            foreach (DataRow ro in maintbl.Select("res_email = '" + row[1].ToString() + "'"))
                                            {
                                                ro["ResourceName"] = itm["Resource Name"].ToString();
                                                if ((itm.Fields.ContainsField("HRIS_x0020_ID")) && (itm["HRIS_x0020_ID"] != null))
                                                { ro["HRISID"] = itm["HRIS_x0020_ID"].ToString(); }
                                                else { ro["HRISID"] = "-"; }
                                                if ((itm.Fields.ContainsField("Old_x0020_Emp_x0020_Num")) && (itm["Old_x0020_Emp_x0020_Num"] != null))
                                                { ro["OLDEMPID"] = itm["Old_x0020_Emp_x0020_Num"].ToString(); }
                                                else { ro["OLDEMPID"] = "-"; }
                                                if ((itm.Fields.ContainsField("Date_x0020_On_x0020_Board")) && (itm["Date_x0020_On_x0020_Board"] != null))
                                                { ro["PayRolStDate"] = itm["Date_x0020_On_x0020_Board"].ToString(); }
                                                else { ro["PayRolStDate"] = "01-01-1900"; }
                                                if ((itm.Fields.ContainsField("Department")) && (itm["Department"] != null))
                                                {
                                                    string[] deptname = itm["Department"].ToString().Split(new char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                                    SPList deptlist = hrWeb.Lists["Department"];
                                                    SPListItem deptitem = deptlist.GetItemById(Convert.ToInt32(deptname[0].ToString()));
                                                    string[] entityname = deptitem["Entity"].ToString().Split(new char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                                    ro["PayRolEntity"] = entityname[1].ToString();
                                                    if (ro["ProjName"].ToString() != string.Empty && ro["ProjName"].ToString() == "Orignal Department")
                                                    {
                                                        ro["ProjName"] = deptname[1].ToString();
                                                        if ((deptitem.Fields.ContainsField("Cost_x0020_Code")) && (deptitem["Cost_x0020_Code"] != null))
                                                        { ro["costcode"] = deptitem["Cost_x0020_Code"].ToString(); }
                                                        else { ro["costcode"] = deptname[1].ToString(); }
                                                    }
                                                }
                                                else
                                                {
                                                    ro["PayRolEntity"] = "-";
                                                }
                                                if ((itm.Fields.ContainsField("Resource_x0020_Type")) && (itm["Resource_x0020_Type"] != null))
                                                { ro["staftype"] = itm["Resource_x0020_Type"].ToString(); }
                                                else { ro["staftype"] = "-"; }
                                                SPListItemCollection secondeditems = seconded.GetItems(secondedqry);
                                                if (secondeditems.Count > 0)
                                                {
                                                    foreach (SPListItem secondeditem in secondeditems)
                                                    {
                                                        if ((secondeditem.Fields.ContainsField("Department")) && (secondeditem["Department"] != null))
                                                        {
                                                            string[] deptname = secondeditem["Department"].ToString().Split(new char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                                            SPList deptlist = hrWeb.Lists["Department"];
                                                            SPListItem deptitem = deptlist.GetItemById(Convert.ToInt32(deptname[0].ToString()));
                                                            string[] entityname = deptitem["Entity"].ToString().Split(new char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                                            ro["PayRolEntity"] = entityname[1].ToString();
                                                            if (ro["ProjName"].ToString() != string.Empty && ro["ProjName"].ToString() == "TO BAU")
                                                            {
                                                                ro["ProjName"] = deptname[1].ToString();
                                                                if ((deptitem.Fields.ContainsField("Cost_x0020_Code")) && (deptitem["Cost_x0020_Code"] != null))
                                                                { ro["costcode"] = deptitem["Cost_x0020_Code"].ToString(); }
                                                                else { ro["costcode"] = deptname[1].ToString(); }
                                                            }
                                                            //MyConfiguration.ErrorLog("Resource Name: " + ro["ResourceName"].ToString(), EventLogEntryType.SuccessAudit);
                                                            //MyConfiguration.ErrorLog("Deptment: " + ro["ProjName"].ToString(), EventLogEntryType.SuccessAudit);
                                                            //MyConfiguration.ErrorLog("Code: " + ro["costcode"].ToString(), EventLogEntryType.SuccessAudit);
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion Secondment Data manipulation
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataRow ro in maintbl.Select("res_email = '" + row[1].ToString() + "'"))
                                        {
                                            ro["HRISID"] = "NA in HRM";
                                            ro["OLDEMPID"] = "NA in HRM";
                                            ro["PayRolStDate"] = "01-01-1900";
                                            ro["PayRolEntity"] = "NA in HRM";
                                            ro["staftype"] = "Internal";
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MyConfiguration.ErrorLog("Error in accessing SPSite" + ex.Message, EventLogEntryType.Error);
                        }
                        List<s_RowData> rowsadded = new List<s_RowData>();
                        int idx = 1;
                        maintbl.DefaultView.Sort = "staftype DESC";
                        try
                        {
                            foreach (DataRow row in maintbl.Rows)
                            {
                                s_RowData newrow = new s_RowData();
                                newrow.id = idx++;
                                //Tabel Column List -- ResUID - 0,ProjUID - 1,ProjName - 2,total_time - 3,pct_total_time - 4,ResourceName - 5,
                                //costcode - 6,res_email - 7,HRISID - 8,OLDEMPID - 9,PayRolStDate - 10,PayRolEntity - 11,staftype - 12
                                newrow.cell = new string[8];  //total number of columns
                                newrow.cell[0] = row[5].ToString(); //resource name from project server
                                newrow.cell[1] = row[8].ToString(); //HRIS Number
                                newrow.cell[2] = row[9].ToString(); //OldEmpNo
                                newrow.cell[3] = row[11].ToString(); //Pay Role Entity
                                newrow.cell[4] = row[10].ToString(); //Pay Role Start Date
                                newrow.cell[5] = row[2].ToString(); //project name from server
                                newrow.cell[6] = row[6].ToString(); //cost code
                                newrow.cell[7] = Convert.ToString(Convert.ToDouble(row[3].ToString()) / 8); //total time clocked
                                rowsadded.Add(newrow);
                            }
                        }
                        catch (Exception ex)
                        {
                            MyConfiguration.ErrorLog("Error At Manipulating Joson Data" + ex.Message, EventLogEntryType.Error);
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