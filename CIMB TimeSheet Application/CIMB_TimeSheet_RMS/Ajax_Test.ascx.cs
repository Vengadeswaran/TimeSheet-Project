using System;

namespace CIMB_TimeSheet_RMS
{
    public partial class Ajax_Test : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            string dt1 = DateTimeLabel1.Text.ToString();
            string dt2 = DateTimeLabel2.Text.ToString();
            DateTimeLabel1.Text = DateTime.Now.ToString();
            DateTimeLabel2.Text = DateTime.Now.ToString();
        }
    }
}