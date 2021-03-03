using System;
using System.IO;
using System.Text;
using log4net.Core;
using log4net.Layout;

namespace VEDrivers.Common
{
    public class LogLayout : ILayout
    {
        public string AdminCode { get; set; }
        public string ContentType => "text/plain";
        public string Header { get; set; }
        public string Footer { get; set; }
        public bool IgnoresException { get; set; } = true;
       
        public void Format(TextWriter writer, LoggingEvent l)
        {
            var builder = new StringBuilder();
            builder.Append(l.TimeStampUtc.ToString("dd.MM.yyyy HH:mm:ss.fff"));
            
            string level = l.Level.ToString();
            string padding = new string(' ', 6 - Math.Min(level.Length, 6));
            builder.Append(" [").Append(l.ThreadName).Append("] ").Append(level).Append(padding).Append(l.LoggerName);

            if (!String.IsNullOrWhiteSpace(AdminCode))
            {
                builder.Append($" >>> adminCode: ").Append(AdminCode);
            }
            if (!String.IsNullOrWhiteSpace(l.RenderedMessage))
            {
                builder.Append($" >>> messsage: ").Append(l.RenderedMessage);
            }
            if (IgnoresException && l.ExceptionObject != null)
            {
                builder.Append(" >>> exception: ").Append(l.ExceptionObject);
            }
            writer.WriteLine(builder.ToString());
        }
    }
}

