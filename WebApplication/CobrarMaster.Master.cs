using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.Helpers;

namespace WebApplication
{
    public partial class CobrarMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var faviconUrl = BrandingImageHelper.ResolveFaviconUrl(Page);
            faviconLink.Visible = !string.IsNullOrWhiteSpace(faviconUrl);
            if (faviconLink.Visible)
            {
                faviconLink.Href = faviconUrl;
            }
        }
    }
}
