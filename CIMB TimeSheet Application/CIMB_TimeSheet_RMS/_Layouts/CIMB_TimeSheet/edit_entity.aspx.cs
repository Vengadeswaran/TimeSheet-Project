using System;
using Microsoft.SharePoint;

namespace CIMB_TimeSheet_RMS
{
    public partial class edit_entity : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string en_id = Request.QueryString["en_ID"].ToString();
                if (en_id != string.Empty)
                {
                    HiddenField1.Value = en_id;
                    using (SPSite site = new SPSite(MyConfiguration.GetSiteURL(SPContext.Current)))
                    {
                        SPWeb hrWeb = site.AllWebs["HRMV02"];
                        SPList entitylist = hrWeb.Lists["Entity"];
                        //SPQuery qry_edit_entity = new SPQuery();
                        //qry_edit_entity.Query = @"<Where><Eq><FieldRef Name='ID'/><Value Type='Counter'>"+en_id+@"</Value></Eq></Where>";
                        SPListItem edit_entitylist_item = entitylist.GetItemById(Convert.ToInt32(en_id.ToString()));
                        txtbox_edit_EntityName.Text = edit_entitylist_item["Title"].ToString();
                        rbtn_EntityStatus.Items.Clear();
                        SPFieldChoice rbt_entity_status = new SPFieldChoice(edit_entitylist_item.Fields, "Status");
                        for (int i = 0; i <= (Convert.ToInt16(rbt_entity_status.Choices.Count.ToString()) - 1); i++)
                        {
                            rbtn_EntityStatus.Items.Add(rbt_entity_status.Choices[i].ToString());
                            if (rbtn_EntityStatus.Items[i].Text == edit_entitylist_item["Status"].ToString())
                            {
                                rbtn_EntityStatus.Items[i].Selected = true;
                                //rbtn_EntityStatus.Items[i].Attributes["AutoPostBack"] = "false";
                            }
                        }
                    }
                }
            }
        }

        protected void btn_Save_Entity_update_Click(object sender, EventArgs e)
        {
            string cusiteurl = string.Empty;
            using (SPSite site = new SPSite(MyConfiguration.GetSiteURL(SPContext.Current)))
            {
                SPWeb hrWeb = site.AllWebs["HRMV02"];
                cusiteurl = hrWeb.Url.ToString();
                hrWeb.AllowUnsafeUpdates = true;
                SPList entitylist = hrWeb.Lists["Entity"];
                SPListItem edit_entitylist_item = entitylist.GetItemById(Convert.ToInt32(HiddenField1.Value.ToString()));
                edit_entitylist_item["Title"] = txtbox_edit_EntityName.Text;
                edit_entitylist_item["Status"] = rbtn_EntityStatus.SelectedValue.ToString();
                edit_entitylist_item.Update();
            }
            Response.Redirect(cusiteurl);
        }

        protected void btn_Cancel_Entity_update_Click(object sender, EventArgs e)
        {
            SPSite site = new SPSite(MyConfiguration.GetSiteURL(SPContext.Current));
            SPWeb hrweb = site.AllWebs["HRMV02"];
            Response.Redirect(hrweb.Url.ToString());
        }
    }
}