using System;
using System.Diagnostics;
using System.Linq;
using ITXProjectsLibrary.WebSvcResource;
using ITXProjectsLibrary.WebSvcTimeSheet;
using Microsoft.SharePoint;

namespace CIMB_TimeSheet_RMS
{
    public partial class ApprovalPending : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string siteurl = MyConfiguration.GetSiteURL(SPContext.Current);
            var Resource_Svc = new Resource();
            Resource_Svc.AllowAutoRedirect = true;
            Resource_Svc.UseDefaultCredentials = true;
            Resource_Svc.Url = siteurl + "/_vti_bin/psi/resource.asmx";
            LblCurrentResUID.Text = Resource_Svc.GetCurrentUserUid().ToString();
            //LblCurrentResUID.Text = "6FF0A657-63BC-4390-8AAF-7EE5CE033088";
        }

        protected void Approve_Click(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                string siteurl = MyConfiguration.GetSiteURL(SPContext.Current);

                var Resource_Svc = new Resource();
                Resource_Svc.AllowAutoRedirect = true;
                Resource_Svc.UseDefaultCredentials = true;
                Resource_Svc.Url = siteurl + "/_vti_bin/psi/resource.asmx";

                var Timesheet_Svc = new TimeSheet();
                Timesheet_Svc.AllowAutoRedirect = true;
                Timesheet_Svc.UseDefaultCredentials = true;
                Timesheet_Svc.Url = siteurl + "/_vti_bin/psi/timesheet.asmx";

                string[] selectedtsuids = Request.Form["LstSelectedtsuids"].Split('#');
                if (selectedtsuids.Length > 0)
                {
                    var grouped_tsuids = (from c in selectedtsuids
                                          group c by c).ToList();
                    foreach (IGrouping<string, string> tsuid_str in grouped_tsuids)
                    {
                        if (tsuid_str.Key != string.Empty)
                        {
                            try
                            {
                                var tsuid = new Guid(tsuid_str.Key);
                                var jobUID = Guid.NewGuid();
                                var nextapprover = Resource_Svc.GetCurrentUserUid();
                                Timesheet_Svc.QueueReviewTimesheet(jobUID, tsuid, nextapprover,
                                                                   "Approving using utility",
                                                                   ITXProjectsLibrary.WebSvcTimeSheet.Action.Approve);
                            }
                            catch (Exception ex)
                            {
                                MyConfiguration.ErrorLog("Error at approving timesheet due to " + ex.Message,
                                                         EventLogEntryType.Error);
                            }
                        }
                    }
                }
                LblStatus.Text = "Timesheet approval process completed successfully.";
            }
        }
    }
}