using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

using ClassLibrary;

namespace LINQtoXML
{
    class ProgramMain
    {
        public static string filePath = @"C:\ProgramData\LINQtoXML\xmlDoc.xml";
        public static string shemaFilePath = @"C:\ProgramData\LINQtoXML\xmlDoc.xsd";
        static void Main(string[] args)
        {
            Student student1 = new Student();
            Student student2 = new Student();
            Student student3 = new Student();
            Student student4 = new Student();

            student1.SetStudent(1, "Petar", "Petrovic", "101", "FTN");
            student2.SetStudent(2, "Marko", "Markovic", "102", "FTN");
            student3.SetStudent(3, "Ivan", "Ivanovic", "103", "ETF");
            student4.SetStudent(4, "Jovan", "Jovanovic", "104", "FTN");

            List<Student> studentList = new List<Student>
            {
                student1,
                student2,
                student3
            };

            CreateXml(studentList);
            ReadDataFromXml();
            ConvertXmlToCsv();
            AddXmlElement(student4);
            ReadDataFromXml();
            XmlToHtml();
            TransformXml();
            XmlValidation();
            UpdateXmlElement("1", student3);
            ReadDataFromXml();
        }

        /// <summary>
        /// Creates XML file from List of Students elements
        /// </summary>
        private static void CreateXml(List<Student> sl)
        {
            Console.WriteLine("Create XML");

            XDocument xmlDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("Here is comment"),
                new XElement("Students",
                    from s in sl
                    select new XElement("Student", new XAttribute("Id", s.Id),
                        new XElement("Name", s.Name),
                        new XElement("LastName", s.LastName),
                        new XElement("IndexNo", s.IndexNo),
                        new XElement("Faculty", s.Faculty)
                        )
                    )
                );
            xmlDocument.Save(filePath);

            Console.WriteLine($"XML document is created on location {filePath}");
        }

        /// <summary>
        /// Reads data from XML file
        /// </summary>
        private static void ReadDataFromXml()
        {
            if (File.Exists(filePath))
            {
                XDocument xDoc = XDocument.Load(filePath);

                IEnumerable<string> studentNames = from s in xDoc.Descendants("Student")
                                                   orderby s.Element("Name").Value descending
                                                   select $"{s.Attribute("Id").Value} {s.Element("Name").Value} {s.Element("LastName").Value} {s.Element("IndexNo").Value} {s.Element("Faculty").Value}";

                Console.WriteLine("---------------------");
                foreach (string name in studentNames)
                {
                    Console.WriteLine(name);
                }
                Console.WriteLine("---------------------");
            }
        }

        /// <summary>
        /// Reads data from XML and write to CSV file
        /// </summary>
        /// <param name="filePath">Path of XML File</param>
        private static void ConvertXmlToCsv()
        {
            if (File.Exists(filePath))
            {
                StringBuilder stringBuilder = new StringBuilder();
                string delimator = ", ";

                XDocument.Load(filePath).Descendants("Student").ToList().ForEach(
                    element => stringBuilder.Append(element.Attribute("Id").Value + delimator +
                                                    element.Element("Name").Value + delimator +
                                                    element.Element("LastName").Value + delimator +
                                                    element.Element("IndexNo").Value + delimator +
                                                    element.Element("Faculty").Value + "\r\n"));

                StreamWriter streamWriter = new StreamWriter(Path.ChangeExtension(filePath, ".csv"));
                streamWriter.WriteLine(stringBuilder.ToString());
                streamWriter.Close();

                Console.WriteLine($"File is created on location {Path.ChangeExtension(filePath, ".csv")}");
            }
            else
            {
                Console.WriteLine($"File {filePath} does not exists");
            }
        }

        /// <summary>
        /// Convert XML to HTML
        /// </summary>
        private static void XmlToHtml()
        {
            XDocument htmlDoc = new XDocument(
                new XElement("table", new XAttribute("border", 1),
                    new XElement("thead",
                        new XElement("tr",
                            new XElement("th", "Id"),
                            new XElement("th", "Name"),
                            new XElement("th", "LastName"),
                            new XElement("th", "Index"),
                            new XElement("th", "Faculty"))),
                    new XElement("tbody",
                        from student in XDocument.Load(filePath).Descendants("Student")
                        select new XElement("tr",
                            new XElement("td", student.Attribute("Id").Value),
                            new XElement("td", student.Element("Name").Value),
                            new XElement("td", student.Element("LastName").Value),
                            new XElement("td", student.Element("IndexNo").Value),
                            new XElement("td", student.Element("Faculty").Value)))));
            htmlDoc.Save(Path.ChangeExtension(filePath, ".html"));
        }

        /// <summary>
        /// Transform one XML format to another XML format
        /// </summary>
        /// <param name="filePath">XML file path</param>
        private static void TransformXml()
        {
            XDocument xmlDocument = XDocument.Load(filePath);

            XDocument result = new XDocument(
                new XElement("Students",
                    new XElement("FTN",
                        from s in xmlDocument.Descendants("Student")
                        where s.Element("Faculty").Value == "ETF"
                        select new XElement("Student",
                            new XElement("Id", s.Attribute("Id").Value),
                            new XElement("Name", s.Element("Name").Value),
                            new XElement("LastName", s.Element("LastName").Value),
                            new XElement("IndexNo", s.Element("IndexNo").Value))),
                    new XElement("ETF",
                        from s in xmlDocument.Descendants("Student")
                        where s.Element("Faculty").Value == "FTN"
                        select new XElement("Student",
                            new XElement("Id", s.Attribute("Id").Value),
                            new XElement("Name", s.Element("Name").Value),
                            new XElement("LastName", s.Element("LastName").Value),
                            new XElement("IndexNo", s.Element("IndexNo").Value)))));

            string file = Path.GetFileNameWithoutExtension(filePath);
            result.Save(filePath.Replace(file , "New"+file));
        }

        /// <summary>
        /// XML validation against XSD
        /// </summary>
        private static void XmlValidation()
        {
            string xsdFilePath = filePath.Replace(".xml", ".xsd");
            XmlSchemaSet xmlSchema = new XmlSchemaSet();
            xmlSchema.Add("", xsdFilePath);

            XDocument xmlDoc = XDocument.Load(filePath);
            xmlDoc.Validate(xmlSchema, (s,e)=>
            {
                Console.WriteLine($"XML validation error - {e.Message}");
                Console.WriteLine($"Please check schema file on location {xsdFilePath}");
            });
        }

        /// <summary>
        /// Adding student element
        /// </summary>
        /// <param name="student">Student object</param>
        private static void AddXmlElement(Student student)
        {
            if (File.Exists(filePath))
            {
                Console.WriteLine("Add Element");
                XDocument xDoc = XDocument.Load(filePath);

                xDoc.Element("Students").Add(
                new XElement("Student", new XAttribute("Id", student.Id),
                    new XElement("Name", student.Name),
                    new XElement("LastName", student.LastName),
                    new XElement("IndexNo", student.IndexNo),
                    new XElement("Faculty", student.Faculty)
                    )
                );

                Console.WriteLine("Element added");
                xDoc.Save(filePath);
            }
            else
            {
                Console.WriteLine($"File {filePath} does not exists!");
            }
        }

        /// <summary>
        /// Updating Student element with data from student object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="s"></param>
        private static void UpdateXmlElement(string id, Student s)
        {

            XDocument xmlDoc = XDocument.Load(filePath);
            xmlDoc.Element("Students")
                .Elements("Student").Where(x => x.Attribute("Id").Value == id).FirstOrDefault()
                .SetElementValue("Name", s.Name);
            xmlDoc.Element("Students")
                .Elements("Student").Where(x => x.Attribute("Id").Value == id).FirstOrDefault()
                .SetElementValue("LastName", s.LastName);
            xmlDoc.Element("Students")
                .Elements("Student").Where(x => x.Attribute("Id").Value == id).FirstOrDefault()
                .SetElementValue("IndexNo", s.IndexNo);
            xmlDoc.Element("Students")
                .Elements("Student").Where(x => x.Attribute("Id").Value == id).FirstOrDefault()
                .SetElementValue("Faculty", s.Faculty);

            xmlDoc.Save(filePath);
        }
    }
}
