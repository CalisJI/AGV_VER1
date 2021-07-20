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
        static App_Config app_Config = new App_Config();

        public static string Create_MapFile(string FileName)
        {
            string MapFile = Application.StartupPath + @"\" + FileName + ".xml";
            return MapFile;
        }
        public static string Community_path(string FileName) 
        {
            app_Config = GetSystem_Config();
            string MapFile = app_Config.Path + @"\" + FileName + ".xml";
            return MapFile;
        }
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
                App_Config.Table = "data1";
                App_Config.Kp = "0.15";
                App_Config.Kd = "0";
                App_Config.Ki = "4";
                App_Config.rotate = "True";
                App_Config.current_angle = 0;
                App_Config.manual_speed = 300;
                App_Config.Path = "";
                XmlSerializer serializer = new XmlSerializer(typeof(App_Config));
                Stream stream = new FileStream(Xml_path, FileMode.Create);
                XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                serializer.Serialize(writer, App_Config);
                writer.Close();
                stream.Close();
                return App_Config;
            }
        }
        public static  Mapping GetMapping(string Mapping_path) 
        {
            if (File.Exists(Create_MapFile(Mapping_path)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Mapping));
                Stream stream = new FileStream(Create_MapFile(Mapping_path), FileMode.Open);
                Mapping Mapping = (Mapping)serializer.Deserialize(stream);
                stream.Close();
                return Mapping;
            }
            else 
            {
                App_Config App_Config = new App_Config();
                App_Config = Configxml.GetSystem_Config();
                Mapping Mapping = new Mapping();
                Mapping.Rectangles = new List<System.Drawing.Rectangle>();
                Mapping.Egde = new List<System.Drawing.Point>();
                Mapping.Route = new List<string>();
                XmlSerializer serializer = new XmlSerializer(typeof(Mapping));
                Stream stream = new FileStream(Create_MapFile(App_Config.Table), FileMode.Create);
                XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                serializer.Serialize(writer, Mapping);
                writer.Close();
                stream.Close();
                return Mapping;
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
        public static void Update_Mapping(Mapping mapping, string Mappfile_path) 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Mapping));
            using (TextWriter tw = new StreamWriter(Create_MapFile(Mappfile_path)))
            {
                serializer.Serialize(tw, mapping);
                tw.Close();
            }
        }
        public static Community Get_Order(string Mapping_path)
        {
            if (File.Exists(Community_path(Mapping_path)))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Community));
                Stream stream = new FileStream(Community_path(Mapping_path), FileMode.Open);
                Community Comunity = (Community)xmlSerializer.Deserialize(stream);
                stream.Close();
                return Comunity;
            }
            else
            {
                Community Comunity = new Community();

                Comunity.DataTable = new System.Data.DataTable { TableName = "Map_Data" };
                Comunity.DataTable.Columns.Add("ID", typeof(string));
                Comunity.DataTable.Columns.Add("X", typeof(string));
                Comunity.DataTable.Columns.Add("Y", typeof(string));
                Comunity.DataTable.Columns.Add("RUN", typeof(string));
                Comunity.DataTable.Columns.Add("LR", typeof(string));
                Comunity.DataTable.Columns.Add("SPEED", typeof(string));
                Comunity.DataTable.Columns.Add("DIR", typeof(string));
                Comunity.DataTable.Columns.Add("LIFT", typeof(string));
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Community));
                Stream stream = new FileStream(Community_path(Mapping_path), FileMode.Create);
                XmlWriter xmlwriter = new XmlTextWriter(stream, Encoding.UTF8);
                xmlSerializer.Serialize(xmlwriter, Comunity);
                xmlwriter.Close();
                stream.Close();
                return Comunity;
            }
        }
    }
}
