using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ErogeHelper.Common.Helper
{
    public class SakuraNoUtaHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SakuraNoUtaHelper));

        static readonly XDocument doc = XDocument.Load(@"C:\Users\k1mlka\Downloads\gamedicexport\sakura_no_uta.xml");

        static public string QueryText(string source)
        {
            string trans = string.Empty;

            doc.XPathSelectElements("//comment[@type='subtitle']")
                //.Where(p => p.Element("context")!.Value.Equals(source))
                .Where(p => 
                { 
                    if (p.Element("context")!.Value.IndexOf(source) != -1)
                    {
                        var pos = p.Element("context")!.Value.IndexOf(source) + source.Length;
                        if (pos == p.Element("context")!.Value.Length)
                            return true;
                    }
                    // （ 「」
                    return false;
                })
                .Select(p => trans = p.Element("text")!.Value)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(trans))
                log.Info(trans);
            return trans;
        }
    }
}
