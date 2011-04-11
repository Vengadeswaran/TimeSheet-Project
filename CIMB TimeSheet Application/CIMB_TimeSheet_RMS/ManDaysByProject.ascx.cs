using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using Microsoft.SharePoint;
using PivotTools;

namespace CIMB_TimeSheet_RMS
{
    public partial class ManDaysByProject : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void exportcsv_Click(object sender, EventArgs e)
        {
            string _stdate = _hiddenstdate.Value;
            string _enddate = _hiddenenddate.Value;
            DateTime _enddateformatted = Convert.ToDateTime(_enddate);
            string siteurl = HttpContext.Current.Request.UrlReferrer.ToString();

            #region sqlqry

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

            #endregion sqlqry

            WindowsImpersonationContext wik = null;
            wik = WindowsIdentity.Impersonate(IntPtr.Zero);
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SqlConnection con = new SqlConnection(MyConfiguration.GetDataBaseConnectionString(siteurl));
                con.Open();
                DataSet dt = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand(gridqry, con));
                adapter.Fill(dt);
                DataTable maintbl = dt.Tables[0];
                maintbl.Columns.Add("HRISID");
                maintbl.Columns.Add("OLDEMPID");
                maintbl.Columns.Add("PayRolStDate");
                maintbl.Columns.Add("PayRolEntity");
                maintbl.Columns.Add("staftype");
                string pctcolumntype = maintbl.Columns["pct_total_time"].DataType.ToString();
                DataView view = new DataView(dt.Tables[0]);
                DataTable resourcelist = view.ToTable(true, "ResourceName", "res_email");
                string resname = string.Empty;
                string resemail = string.Empty;
                try
                {
                    using (SPSite site = new SPSite(MyConfiguration.frm_siteurl_GetSiteURL(siteurl)))
                    {
                        foreach (DataRow row in resourcelist.Rows)
                        {
                            SPWeb hrWeb = site.AllWebs["HRMV02"];
                            SPList res_detail = hrWeb.Lists["Resource Details"];
                            SPList seconded = hrWeb.Lists["Resource Secondment"];
                            SPQuery qry = new SPQuery();
                            qry.Query = @"<Where>
                                        <Eq>
                                            <FieldRef Name='EMail'/>
                                            <Value Type='Text'>" + row[1].ToString() + @"</Value>
                                        </Eq>
                                    </Where>";
                            SPListItemCollection items = res_detail.GetItems(qry);

                            #region secondment and resource data from HR Manipulation

                            if (items.Count > 0)
                            {
                                foreach (SPListItem itm in items)
                                {
                                    #region secondment query

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

                                    #endregion secondment query

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

                            #endregion secondment and resource data from HR Manipulation
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyConfiguration.ErrorLog("Error in accessing SPSite" + ex.Message, EventLogEntryType.Error);
                }
                maintbl.DefaultView.Sort = "staftype DESC";
                DataView pivotview = new DataView(maintbl);
                DataTable pivotviewtable = pivotview.ToTable(false, "ResUID", "costcode", "total_time");
                var qyery = (from c in maintbl.AsEnumerable()
                             group c by new { cost_code = c.Field<string>("costcode"), res_uid = c.Field<Guid>("ResUID") } into grp
                             orderby grp.Key.res_uid, grp.Key.cost_code
                             select new
                             {
                                 ResUID = grp.Key.res_uid,
                                 costcode = grp.Key.cost_code,
                                 total_time = grp.Sum(r => r.Field<decimal>("total_time")) / 8
                             });
                DataTable pvtable = qyery.CopyToDataTable();
                DataTable resulttbl = new DataTable();
                resulttbl = Pivot(pvtable, "ResUID", "costcode", "total_time");
                resulttbl.Columns.Add("ResourceName");
                resulttbl.Columns.Add("HRISID");
                resulttbl.Columns.Add("OLDEMPID");
                resulttbl.Columns.Add("PayRolStDate");
                resulttbl.Columns.Add("PayRolEntity");
                resulttbl.Columns.Add("staftype");

                resulttbl.Columns["ResourceName"].SetOrdinal(1);
                resulttbl.Columns["HRISID"].SetOrdinal(2);
                resulttbl.Columns["OLDEMPID"].SetOrdinal(3);
                resulttbl.Columns["PayRolStDate"].SetOrdinal(4);
                resulttbl.Columns["PayRolEntity"].SetOrdinal(5);
                resulttbl.Columns["staftype"].SetOrdinal(6);

                DataView resdetailview = new DataView(maintbl);
                DataTable resdetailtable = resdetailview.ToTable(true, "ResUID", "ResourceName", "HRISID", "OLDEMPID", "PayRolStDate", "PayRolEntity", "staftype");

                foreach (DataRow ro in resulttbl.Rows)
                {
                    foreach (DataRow mtlrow in resdetailtable.Select("ResUID = '" + ro["ResUID"] + "'"))
                    {
                        ro["ResourceName"] = mtlrow["ResourceName"];
                        ro["HRISID"] = mtlrow["HRISID"].ToString();
                        ro["OLDEMPID"] = mtlrow["OLDEMPID"];
                        if (mtlrow["PayRolStDate"].ToString() != "00-00-000")
                        {
                            ro["PayRolStDate"] = Convert.ToDateTime(mtlrow["PayRolStDate"]).ToString("dd-MM-yyyy");
                        }
                        else ro["PayRolStDate"] = mtlrow["PayRolStDate"];
                        ro["PayRolEntity"] = mtlrow["PayRolEntity"];
                        ro["staftype"] = mtlrow["staftype"];
                    }
                }

                #region export function

                WindowsImpersonationContext wic = null;
                try
                {
                    try
                    {
                        wic = WindowsIdentity.Impersonate(IntPtr.Zero);
                    }
                    catch (Exception)
                    {
                    }
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

                    for (int i = 1; i < resulttbl.Columns.Count; i++)
                    {
                        if (i > 6)
                        {
                            columnheaderline = columnheaderline + resulttbl.Columns[i].ColumnName + "(days),";
                        }
                        else
                        {
                            columnheaderline = columnheaderline + resulttbl.Columns[i].ColumnName + ",";
                        }
                    }
                    writer.WriteLine(columnheaderline);
                    // Writing row values
                    foreach (DataRow row in resulttbl.Rows)
                    {
                        string columnvalue = string.Empty;
                        for (int i = 1; i < resulttbl.Columns.Count; i++)
                        {
                            columnvalue = columnvalue + row[i] + ",";
                        }
                        writer.WriteLine(columnvalue);
                    }

                    writer.Flush();
                    writer.Close();
                    writer.Dispose();

                    // Sending files here
                    Response.ContentType = "application/CSV";
                    Response.AddHeader("content-disposition", "attachment; filename=ManDayByProject" + clientfilename + ".csv");
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

        public static PivotedTable Pivot(DataTable dataValues, string keyColumn, string pivotNameColumn, string pivotValueColumn)
        {
            return Pivot(dataValues.CreateDataReader(), keyColumn, pivotNameColumn, pivotValueColumn);
        }

        private static PivotedTable Pivot(IDataReader dataValues, string keyColumn, string pivotNameColumn, string pivotValueColumn)
        {
            PivotedTable tableResults = new PivotedTable();
            int pValIndex, pNameIndex;
            string sColumnName;
            bool isFirstRow = true;
            tableResults.IndexOfLastNonPivotColumn.Equals(dataValues.FieldCount - 3);
            // Add non-pivot columns to the data table:
            pValIndex = dataValues.GetOrdinal(pivotValueColumn);
            pNameIndex = dataValues.GetOrdinal(pivotNameColumn);
            for (int colIndex = 0; colIndex <= dataValues.FieldCount - 1; colIndex++)
                if (colIndex != pValIndex && colIndex != pNameIndex)
                    tableResults.Columns.Add(dataValues.GetName(colIndex), dataValues.GetFieldType(colIndex));

            // Now, fill up the table with the data:
            DataRow r = null;
            string LastKey = "//dummy//";
            while (dataValues.Read())
            {
                // see if we need to start a new row
                if (dataValues[keyColumn].ToString() != LastKey)
                {
                    // if this isn't the very first row, we need to add the last one to the table
                    if (!isFirstRow)
                        tableResults.Rows.Add(r);
                    r = tableResults.NewRow();
                    isFirstRow = false;

                    // Add all non-pivot column values to the new row:
                    for (int i = 0; i <= tableResults.IndexOfLastNonPivotColumn; i++)
                        r[i] = dataValues[tableResults.Columns[i].ColumnName];
                    LastKey = dataValues[keyColumn].ToString();
                }

                // assign the pivot values to the proper column; add new columns if needed:
                sColumnName = dataValues[pNameIndex].ToString();
                if (sColumnName != "")
                {
                    if (!tableResults.Columns.Contains(sColumnName))
                    {
                        DataColumn c = tableResults.Columns.Add(sColumnName, dataValues.GetFieldType(pValIndex));
                        // set the index so that it is sorted properly:
                        int newOrdinal = c.Ordinal;
                        for (int i = newOrdinal - 1; i >= dataValues.FieldCount - 2; i--)
                            if (c.ColumnName.CompareTo(tableResults.Columns[i].ColumnName) < 0)
                                newOrdinal = i;
                        c.SetOrdinal(newOrdinal);
                    }

                    r[sColumnName] = dataValues[pValIndex];
                }
            }

            // add that final row to the datatable:
            if (r != null)
                tableResults.Rows.Add(r);

            // Add in zeroes
            for (int row = 0; row < tableResults.Rows.Count; row++)
                for (int col = Math.Max(0, tableResults.IndexOfLastNonPivotColumn - 1); col < tableResults.Columns.Count; col++)
                    if (tableResults.Rows[row][col].ToString() == "")
                        tableResults.Rows[row][col] = 0;

            dataValues.NextResult();
            return tableResults;
        }
    }
}

public static class DataSetLinqOperators
{
    public static DataTable CopyToDataTable<T>(this IEnumerable<T> source)
    {
        return new ObjectShredder<T>().Shred(source, null, null);
    }

    public static DataTable CopyToDataTable<T>(this IEnumerable<T> source,
                                                DataTable table, LoadOption? options)
    {
        return new ObjectShredder<T>().Shred(source, table, options);
    }
}

public class ObjectShredder<T>
{
    private FieldInfo[] _fi;
    private PropertyInfo[] _pi;
    private Dictionary<string, int> _ordinalMap;
    private Type _type;

    public ObjectShredder()
    {
        _type = typeof(T);
        _fi = _type.GetFields();
        _pi = _type.GetProperties();
        _ordinalMap = new Dictionary<string, int>();
    }

    public DataTable Shred(IEnumerable<T> source, DataTable table, LoadOption? options)
    {
        if (typeof(T).IsPrimitive)
        {
            return ShredPrimitive(source, table, options);
        }

        if (table == null)
        {
            table = new DataTable(typeof(T).Name);
        }

        // now see if need to extend datatable base on the type T + build ordinal map
        table = ExtendTable(table, typeof(T));

        table.BeginLoadData();
        using (IEnumerator<T> e = source.GetEnumerator())
        {
            while (e.MoveNext())
            {
                if (options != null)
                {
                    table.LoadDataRow(ShredObject(table, e.Current), (LoadOption)options);
                }
                else
                {
                    table.LoadDataRow(ShredObject(table, e.Current), true);
                }
            }
        }
        table.EndLoadData();
        return table;
    }

    public DataTable ShredPrimitive(IEnumerable<T> source, DataTable table, LoadOption? options)
    {
        if (table == null)
        {
            table = new DataTable(typeof(T).Name);
        }

        if (!table.Columns.Contains("Value"))
        {
            table.Columns.Add("Value", typeof(T));
        }

        table.BeginLoadData();
        using (IEnumerator<T> e = source.GetEnumerator())
        {
            Object[] values = new object[table.Columns.Count];
            while (e.MoveNext())
            {
                values[table.Columns["Value"].Ordinal] = e.Current;

                if (options != null)
                {
                    table.LoadDataRow(values, (LoadOption)options);
                }
                else
                {
                    table.LoadDataRow(values, true);
                }
            }
        }
        table.EndLoadData();
        return table;
    }

    public DataTable ExtendTable(DataTable table, Type type)
    {
        // value is type derived from T, may need to extend table.
        foreach (FieldInfo f in type.GetFields())
        {
            if (!_ordinalMap.ContainsKey(f.Name))
            {
                DataColumn dc = table.Columns.Contains(f.Name) ? table.Columns[f.Name]
                    : table.Columns.Add(f.Name, f.FieldType);
                _ordinalMap.Add(f.Name, dc.Ordinal);
            }
        }
        foreach (PropertyInfo p in type.GetProperties())
        {
            if (!_ordinalMap.ContainsKey(p.Name))
            {
                DataColumn dc = table.Columns.Contains(p.Name) ? table.Columns[p.Name]
                    : table.Columns.Add(p.Name, p.PropertyType);
                _ordinalMap.Add(p.Name, dc.Ordinal);
            }
        }
        return table;
    }

    public object[] ShredObject(DataTable table, T instance)
    {
        FieldInfo[] fi = _fi;
        PropertyInfo[] pi = _pi;

        if (instance.GetType() != typeof(T))
        {
            ExtendTable(table, instance.GetType());
            fi = instance.GetType().GetFields();
            pi = instance.GetType().GetProperties();
        }

        Object[] values = new object[table.Columns.Count];
        foreach (FieldInfo f in fi)
        {
            values[_ordinalMap[f.Name]] = f.GetValue(instance);
        }

        foreach (PropertyInfo p in pi)
        {
            values[_ordinalMap[p.Name]] = p.GetValue(instance, null);
        }
        return values;
    }
}