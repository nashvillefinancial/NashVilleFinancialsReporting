using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Error : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string errorMessage = ASPxWebControl.GetCallbackErrorMessage();
        //errorMessage +=  System.Web.HttpContext.Current.Server.GetLastError().Message;
        errorMessage += Request.QueryString["Error"];
        Response.Output.Write(errorMessage);
    }
}