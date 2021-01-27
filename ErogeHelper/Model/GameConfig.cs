using Caliburn.Micro;
using Serilog;
using System;
using System.Xml.Linq;

namespace ErogeHelper.Model
{
    class GameConfig
    {
        public static string Path = string.Empty;

        public static string MD5 = string.Empty;
        public static bool IsUserHook = false;
        public static string HookCode = string.Empty;
        public static long ThreadContext;
        public static long SubThreadContext;
        public static string RegExp = string.Empty;
        public static bool NoFocus;

        public static void Load(string path)
        {
            Path = path;

            MD5 = GetString(EHNode.MD5);
            IsUserHook = GetBool(EHNode.IsUserHook);
            HookCode = GetString(EHNode.HookCode);
            ThreadContext = GetLong(EHNode.ThreadContext);
            SubThreadContext = GetLong(EHNode.SubThreadContext);
            RegExp = GetString(EHNode.RegExp);
            NoFocus = GetBool(EHNode.NoFocus);
        }

        public static void CreateConfig(string path)
        {
            Path = path;

            var baseNode = new XElement("Profile",
                new XAttribute("Name", value: DataRepository.MainProcess!.ProcessName + ".eh.config"),
                new XElement("MD5", content: MD5),
                new XElement("IsUserHook", content: IsUserHook),
                new XElement("HookCode", content: HookCode),
                new XElement("ThreadContext", content: ThreadContext),
                new XElement("SubThreadContext", content: SubThreadContext),
                new XElement("RegExp", content: RegExp),
                new XElement("NoFocus", content: NoFocus)
            );

            try
            {
                var tree = new XElement("EHConfig", baseNode);
                tree.Save(Path);
                Log.Info("Write config file succeed");
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(ex.Message);
                ModernWpf.MessageBox.Show($"{ex.Message}\nEroge Helper has no permission to access the path, " +
                    $"will save text temporally...", "Eroge Helper");
            }
        }

        private static string GetString(EHNode node, string defValue = "")
        {
            var doc = XElement.Load(Path);
            var profile = doc.Element(EHNode.Profile.Name);

            XElement? tarNode = profile!.Element(node.Name);

            if (tarNode == null)
            {
                profile.Add(new XElement(node.Name)
                {
                    Value = defValue
                });
                doc.Save(Path);
                return defValue;
            }
            else
            {
                return tarNode.Value;
            }
        }

        private static bool GetBool(EHNode node, bool defValue = false)
        {
            var doc = XElement.Load(Path);
            var profile = doc.Element(EHNode.Profile.Name);

            XElement? tarNode = profile!.Element(node.Name);

            if (tarNode == null)
            {
                profile.Add(new XElement(node.Name)
                {
                    Value = defValue.ToString()
                });
                doc.Save(Path);
                return defValue;
            }
            else
            {
                return bool.Parse(tarNode.Value);
            }
        }

        private static long GetLong(EHNode node, long defValue = -1)
        {
            var doc = XElement.Load(Path);
            var profile = doc.Element(EHNode.Profile.Name);

            XElement? tarNode = profile!.Element(node.Name);

            if (tarNode == null)
            {
                profile.Add(new XElement(node.Name)
                {
                    Value = defValue.ToString()
                });
                doc.Save(Path);
                return defValue;
            }
            else
            {
                return long.Parse(tarNode.Value);
            }
        }

        public static void SetValue(EHNode node, string value)
        {
            var doc = XDocument.Load(Path);
            var profile = doc.Element(EHNode.EHConfig.Name)!.Element(EHNode.Profile.Name);

            XElement? oldNode = profile!.Element(node.Name);
            if (oldNode != null)
            {
                oldNode.Value = value;
            }
            else
            {
                profile.Add(new XElement(node.Name)
                {
                    Value = value
                });
            }

            doc.Save(Path);
        }
    }

    public class EHNode
    {
        private EHNode(string value) { Name = value; }

        public string Name { get; set; }

        public static EHNode EHConfig { get { return new EHNode("EHConfig"); } }
        public static EHNode Profile { get { return new EHNode("Profile"); } }

        public static EHNode MD5 { get { return new EHNode("MD5"); } }
        public static EHNode IsUserHook { get { return new EHNode("IsUserHook"); } }
        public static EHNode HookCode { get { return new EHNode("HookCode"); } }
        public static EHNode ThreadContext { get { return new EHNode("ThreadContext"); } }
        public static EHNode SubThreadContext { get { return new EHNode("SubThreadContext"); } }
        public static EHNode RegExp { get { return new EHNode("RegExp"); } }
        public static EHNode NoFocus { get { return new EHNode("NoFocus"); } }
    }
}
