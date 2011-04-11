using System;
using System.Diagnostics;
using System.Security.Principal;
using ITXProjectsLibrary;
using Microsoft.SharePoint;

namespace CIMB_TimeSheet_RMS
{
    public class MyConfiguration
    {
        public static string GetSiteURL(SPContext Context)
        {
            if (Context != null)
            {
                return Utilities.GetDefaultZoneUri(SPContext.Current.Site);
            }
            else
            {
                return "http://jump/cimb";
            }
        }

        public static string frm_siteurl_GetSiteURL(string siteurl)
        {
            int dev = siteurl.IndexOf("http://localhost");

            if (siteurl != null && dev < 0)
            {
                return Utilities.GetDefaultZoneUri(new SPSite(siteurl));
            }
            else
            {
                return "http://jump/cimb";
            }
        }

        public static string GetDataBaseName(SPContext Context)
        {
            if (Context != null)
            {
                return Utilities.GetProjectServerSQLDatabaseName(Context.Site.Url, ITXProjectsLibrary.Utilities.DatabaseType.ReportingDatabase);
            }
            else
            {
                // For Development
                return "";
            }
        }

        //public static string GetDataBaseConnectionString(SPContext Context)
        //{
        //    if (Context != null)
        //    {
        //        return Utilities.GetProjectServerSQLDatabaseConnectionString(Context.Site.Url, ITXProjectsLibrary.Utilities.DatabaseType.ReportingDatabase);
        //    }
        //    else
        //    {
        //        // For Development
        //        return "Data Source=localhost; Initial Catalog=ProjectServer_Reporting; Integrated Security=true";
        //    }
        //}

        public static string GetDataBaseConnectionString(string siteurl)
        {
            int dev = siteurl.IndexOf("http://localhost");

            if (siteurl != null && dev < 0)
            {
                SPSite site = new SPSite(siteurl);
                return Utilities.GetProjectServerSQLDatabaseConnectionString(site.Url, ITXProjectsLibrary.Utilities.DatabaseType.ReportingDatabase);
            }
            else
            {
                SPSite site = new SPSite("http://jump/cimb");
                return Utilities.GetProjectServerSQLDatabaseConnectionString(site.Url, ITXProjectsLibrary.Utilities.DatabaseType.ReportingDatabase);
            }
        }

        public static void ErrorLog(string LogStr, EventLogEntryType Type)
        {
            try
            {
                WindowsImpersonationContext wic = WindowsIdentity.Impersonate(IntPtr.Zero);
                EventLog El = new EventLog();
                if (EventLog.SourceExists("CIMBTimeSheet") == false)
                    EventLog.CreateEventSource("CIMBTimeSheet", "CIMBTimeSheet");
                El.Source = "CIMBTimeSheet";
                El.WriteEntry(LogStr, Type);
                El.Close();
                wic.Undo();
            }
            catch (System.Exception Ex87)
            {
                WriteTextLog(Ex87.Message + "\r" + LogStr);
            }
        }

        private static void WriteTextLog(string LogStr)
        {
            try
            {
                System.IO.StreamWriter Writer = new System.IO.StreamWriter(@"c:\CIMBTimeSheet.txt", true);
                Writer.WriteLine(LogStr);
                Writer.Close();
                Writer.Dispose();
            }
            catch (System.Exception Ex) { }
        }
    }
}