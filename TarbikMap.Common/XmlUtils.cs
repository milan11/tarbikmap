namespace TarbikMap.Common
{
    using System.Xml;

    public static class XmlUtils
    {
        public static string XmlToString(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.XmlResolver = null;
            xmlDoc.LoadXml("<div>" + xml + "</div>");
            return xmlDoc.InnerText;
        }
    }
}