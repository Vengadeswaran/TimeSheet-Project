using System;
using ITXProjectsLibrary.WebSvcResource;
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
        }
    }
}