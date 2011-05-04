using System;
using ITXProjectsLibrary.WebSvcTimeSheet;
using Microsoft.SharePoint;

namespace TestConsole_Timesheetapproval
{
    class Program
    {
        static void Main()
        {
            using (var Site = new SPSite("http://epm2007demo/pwa03"))
            {
                var Timesheet_Svc = new TimeSheet
                                        {
                                            UseDefaultCredentials = true,
                                            AllowAutoRedirect = true,
                                            Url = (Site.Url + "/_vti_bin/psi/timesheet.asmx")
                                        };

                var jobUID = Guid.NewGuid();
                var tsUID = Guid.NewGuid();
                var nextapprover = Guid.NewGuid();
                Timesheet_Svc.QueueReviewTimesheet(jobUID, tsUID, nextapprover, "Approving using utility", ITXProjectsLibrary.WebSvcTimeSheet.Action.Approve);
            }
        }
    }
}