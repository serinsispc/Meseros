using System;
using System.Web.UI;
using WebApplication.Helpers;

namespace WebApplication
{
    public partial class SiteMaster : MasterPage
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
