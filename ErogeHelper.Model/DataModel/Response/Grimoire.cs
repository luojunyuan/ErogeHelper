using System.Xml.Serialization;

namespace ErogeHelper.Model.DataModel.Response
{
    /// <summary>
    /// VNR Grimoire xml format, use for fetching code on Aniclan
    /// </summary>
    [XmlRoot(ElementName = "grimoire")]
    public class Grimoire
    {
        [XmlElement(ElementName = "games")]
        public GamesClass? Games { get; set; }

        [XmlRoot(ElementName = "games")]
        public class GamesClass
        {
            [XmlElement(ElementName = "game")]
            public GameClass? Game { get; set; }

            [XmlRoot(ElementName = "game")]
            public class GameClass
            {
                [XmlElement(ElementName = "md5")]
                public string Md5 { get; set; } = string.Empty;

                [XmlElement(ElementName = "encoding")]
                public string Encoding { get; set; } = string.Empty;

                [XmlElement(ElementName = "itemId")]
                public int ItemId { get; set; }

                [XmlElement(ElementName = "language")]
                public string Language { get; set; } = string.Empty;

                [XmlElement(ElementName = "names")]
                public NamesClass? Names { get; set; }

                [XmlElement(ElementName = "threads")]
                public ThreadsClass? Threads { get; set; }

                [XmlElement(ElementName = "hook")]
                public string Hook { get; set; } = string.Empty;

                [XmlAttribute(AttributeName = "id")]
                public int Id { get; set; }

                [XmlText]
                public string Text { get; set; } = string.Empty;

                [XmlRoot(ElementName = "names")]
                public class NamesClass
                {
                    [XmlElement(ElementName = "name")]
                    public NameClass? Name { get; set; }

                    [XmlRoot(ElementName = "name")]
                    public class NameClass
                    {
                        [XmlAttribute(AttributeName = "type")]
                        public string Type { get; set; } = string.Empty;

                        [XmlText]
                        public string Text { get; set; } = string.Empty;
                    }
                }

                [XmlRoot(ElementName = "threads")]
                public class ThreadsClass
                {
                    [XmlElement(ElementName = "thread")]
                    public ThreadClass? Thread { get; set; }

                    [XmlRoot(ElementName = "thread")]
                    public class ThreadClass
                    {
                        [XmlElement(ElementName = "name")]
                        public string Name { get; set; } = string.Empty;

                        [XmlElement(ElementName = "signature")]
                        public int Signature { get; set; }

                        [XmlAttribute(AttributeName = "type")]
                        public string Type { get; set; } = string.Empty;

                        [XmlText]
                        public string Text { get; set; } = string.Empty;
                    }
                }
            }
        }
    }
}
