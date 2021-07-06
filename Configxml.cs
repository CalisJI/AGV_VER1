using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Xml;


namespace READ_TEXT485
{
    
    class Configxml
    {
        private static string Xml_path = Application.StartupPath + @"\" + "System Configuration.xml";
        public static App_Config GetSystem_Config() 
        {
            if (File.Exists(Xml_path)) 
            {
                XmlSerializer serializer = new XmlSerializer(typeof(App_Config));
                Stream stream = new FileStream(Xml_path, FileMode.Open);
                App_Config App_Config = (App_Config)serializer.Deserialize(stream);
                stream.Close();
                return App_Config;
            }
            else
            {
                App_Config App_Config = new App_Config();

                App_Config.COM = "COM1";
                App_Config.Baud = "115200";
                App_Config.Database = "agv_data";
                App_Config.Table = "";
                App_Config.Kp = "0.1";
                App_Config.Kd = "0";
                App_Config.Ki = "1";
                App_Config.rotate = "True";
                XmlSerializer serializer = new XmlSerializer(typeof(App_Config));
                Stream stream = new FileStream(Xml_path, FileMode.Create);

                XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                serializer.Serialize(writer, App_Config);
                writer.Close();
                stream.Close();
                return App_Config;
            }
        }
        public static void UpdateSystem_Config(string nodeName, string value)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Xml_path);
            XmlElement xml_elm = xmlDoc.DocumentElement;
            foreach (XmlNode node in xml_elm.ChildNodes)
            {
                if (node.Name == nodeName) node.InnerText = value;
            }
            xmlDoc.Save(Xml_path);

        }
    }
}
