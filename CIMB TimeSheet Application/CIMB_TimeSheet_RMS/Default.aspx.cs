using System;

namespace CIMB_TimeSheet_RMS
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        //        public struct s_GridResult
        //        {
        //            public int page;
        //            public int total;
        //            public int record;
        //            public s_RowData[] rows;
        //        }
        //        public struct s_RowData
        //        {
        //            public int id;
        //            public string[] cell;
        //        }
        //        [WebMethod]
        //        public static s_GridResult GetDataTable(string _search, string nd, string rows, string page, string sidx, string sord)
        //        {
        //            string gridqry = string.Empty;
        //            gridqry = @"SELECT      t_res.ResourceUID, t_proj.ProjectUID, DATEPART(yyyy, t_actual.TimeByDay) AS t_Year,
        //                                        DATEPART(MM, t_actual.TimeByDay) AS t_Month,
        //                                        t_class.Type, CASE WHEN (t_class.Type = 1) THEN 'Non-Wokring' ELSE t_class.ClassName END
        //                                            AS ClassName,
        //                                        SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable +
        //                                            t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable)
        //                                            AS [Clocked Time]
        //                            FROM        MSP_TimesheetResource AS t_res INNER JOIN
        //                      MSP_TimesheetActual AS t_actual ON t_res.ResourceNameUID = t_actual.LastChangedResourceNameUID INNER JOIN
        //                      MSP_TimesheetLine AS t_line ON t_actual.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
        //                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID INNER JOIN
        //                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID
        //                            GROUP BY t_res.ResourceUID, DATEPART(yyyy, t_actual.TimeByDay),
        //                            DATEPART(MM, t_actual.TimeByDay), t_class.Type, t_proj.ProjectUID,
        //                            CASE WHEN (t_class.Type = 1) THEN 'Non-Wokring' ELSE t_class.ClassName END";
        //            WindowsImpersonationContext wik = null;
        //            wik = WindowsIdentity.Impersonate(IntPtr.Zero);
        //            SqlConnection con = new SqlConnection(MyConfiguration.GetDataBaseConnectionString(SPContext.Current));
        //            con.Open();
        //            DataSet dt = new DataSet();
        //            SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand(gridqry, con));
        //            adapter.Fill(dt);

        //            s_GridResult result = new s_GridResult();
        //            List<s_RowData> rowsadded = new List<s_RowData>();
        //            int idx = 1;
        //            foreach (DataRow row in dt.Tables[0].Rows)
        //            {
        //                s_RowData newrow = new s_RowData();
        //                newrow.id = idx++;
        //                newrow.cell = new string[6];  //total number of columns
        //                newrow.cell[0] = row[0].ToString();
        //                newrow.cell[1] = row[1].ToString();
        //                newrow.cell[2] = row[2].ToString();
        //                newrow.cell[3] = row[3].ToString();
        //                newrow.cell[4] = row[4].ToString();
        //                newrow.cell[5] = row[5].ToString();
        //                rowsadded.Add(newrow);
        //            }
        //            result.rows = rowsadded.ToArray();
        //            result.page = 1;
        //            result.total = dt.Tables[0].Rows.Count;
        //            result.record = rowsadded.Count;
        //            return result;
        //        }
    }
}