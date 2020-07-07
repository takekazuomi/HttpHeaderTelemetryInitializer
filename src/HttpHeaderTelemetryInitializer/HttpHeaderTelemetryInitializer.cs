using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

// https://apmtips.com/posts/2014-12-01-telemetry-initializers/
// https://github.com/microsoft/ApplicationInsights-dotnet/issues/820

namespace AI.TelemetryInitializer
{
    public class HttpHeaderTelemetryInitializer : ITelemetryInitializer
    {
        private static void Quote(StringBuilder sb, string data)
        {
            sb.Append("\"");
            sb.Append(data);
            sb.Append("\"");
        }

        private static string ToJson(NameValueCollection source)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            foreach (string s in source.Keys)
            {
                Quote(sb, s);
                sb.Append(": ");
                Quote(sb, source[s]);
                sb.Append(",");
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            sb.Append("}");
            return sb.ToString();
        }

        public static void CopyHeaders(string name, NameValueCollection source, IDictionary<string, string> target)
        {
            if (source == null || !source.HasKeys())
                return;

            if (!target.ContainsKey(name))
            {
                target[name] = ToJson(source);
            }
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (!(telemetry is ISupportProperties telemetryWithProperties)) return;
            if (HttpContext.Current == null) return;

            CopyHeaders("request.headers", HttpContext.Current.Request.Headers, telemetryWithProperties.Properties);
            CopyHeaders("response.headers", HttpContext.Current.Response.Headers, telemetryWithProperties.Properties);
        }

    }
}
