using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace WebApplication.Helpers
{
    public class ModalHelper
    {
        public static void Open(Page page, Control modal)
        {
            var script = $"bootstrap.Modal.getOrCreateInstance(document.getElementById('{modal.ClientID}')).show();";
            ScriptManager.RegisterStartupScript(page, page.GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}