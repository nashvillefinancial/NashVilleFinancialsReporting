using DevExpress.DashboardCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for Logger
/// </summary>
public static class TextLogger
{
    public static void AddToLog(Exception exception, string path)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(DateTime.Now.ToLocalTime().ToString("F"));
        sb.AppendLine("Source File: " + System.Web.HttpContext.Current.Request.RawUrl);
        GetExceptionInfo(exception, sb);
        sb.AppendLine("------------------------------------------------------------" + Environment.NewLine);
        HttpContext.Current.Response.Write(sb.ToString());
        File.AppendAllText(path, sb.ToString());
    }

    private static void GetExceptionInfo(Exception exception, StringBuilder sb)
    {
        sb.AppendLine(exception.GetType().ToString());
        sb.AppendLine(exception.Message);
        sb.AppendLine("Stack Trace: ");
        sb.AppendLine(exception.StackTrace);
        if (exception is DashboardDataLoadingException)
        {
            foreach (var dataLoadingError in ((DashboardDataLoadingException)exception).Errors)
            {
                sb.AppendLine("InnerException: ");
                GetExceptionInfo(dataLoadingError.InnerException, sb);
            }
        }
        if (exception.InnerException != null)
        {
            sb.AppendLine("InnerException: ");
            GetExceptionInfo(exception.InnerException, sb);
        }
    }
}