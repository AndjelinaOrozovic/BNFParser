using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 

namespace FormalneProjektni
{
    class BNFReader
    {
        public static List<Info> list = new List<Info>(); // lista bnf-a, samo tokeni
        public class Info // ime i definicija svakog tokena, objekat Info
        {
            public string token;
            public string definition;

            public Info(string token, string definiton)
            {
                this.token = token;
                this.definition = definiton;
            }
        }

        public static List<Info> BnfList = new List<Info>(); //neka nova lista u koju smjestamo sve tokene ali zamjenjene njihovim definicijama

        /// <summary>
        /// Citanje iz Config.bin, razdvoji ime tokena i definiciju 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void ReadFromConfigFile(string name)
        {
            
            try
            {
                StreamReader reader = new StreamReader(new FileStream("config.bnf", FileMode.Open));
                string text = reader.ReadLine();
                do
                {
                    Match token = Regex.Match(text, @"<[\w\d\s_-]+>");
                    Match Definition = Regex.Match(text, @"::=(.+)");
                    string definition = Definition.Value;
                    definition = definition.Substring(definition.IndexOf('=') + 1);
                    if (token.Success && Definition.Success)
                    {
                        Info info = new Info(token.Value, definition);
                        list.Add(info);
                    }
                    text = reader.ReadLine();
                }
                while (text != null);
                reader.Close();

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error. config.bin is not found.");
            }
        }
        
        /// <summary>
        /// Uklanja bjelinu ispred definicije tokena, ako ima
        /// </summary>
        /// <param name="BnfList"></param>
        public Info RemoveWhiteSpace(Info part)
        {
             Match j = Regex.Match(part.definition, @"[\w\<\>]");
             string definition = part.definition.Remove(0, j.Index);
             part = new Info(part.token, definition);
             return part;
        }

        /// <summary>
        /// Petlja koja mjenja definiciju liste(ako imamo cvorove koji sadrze regularni izraz), uklanja bjeline i suvisne znakove
        /// </summary>
        public void ListDefinition()
        {
            List<string> productionList = new List<string>();

            for (int i = 0; i < BnfList.Count(); ++i)
            {
                BnfList[i] = RemoveWhiteSpace(BnfList[i]);

                if (BnfList[i].definition.StartsWith("regex("))
                {
                    BnfList[i].definition = BnfList[i].definition.Remove(0, 6);
                    BnfList[i].definition = BnfList[i].definition.Remove(BnfList[i].definition.Length - 1);
                }
                else if(BnfList[i].definition.Contains('|'))
                {
                    string[] productions = BnfList[i].definition.Split('|');
                    for (int j = 0; j < productions.Count(); j++)
                       productionList.Add("(" + productions[j].Trim() + ")");
                    string result = "";
                    foreach (var prod in productionList)
                        result += prod + "|";
                    BnfList[i].definition = BnfList[i].definition.Replace(BnfList[i].definition, result.Substring(0, result.Length - 1 ));
                }
                else if (BnfList[i].definition.Contains("broj_telefona"))
                {
                    string phone = "((00387 *)|(\\+387 *)|(0 *))\\d{2}((\\/)|(-)|( ))?\\d{3}((\\/)|(-)|( ))?\\d{3,4}";
                    BnfList[i].definition = BnfList[i].definition.Replace("broj_telefona", phone);
                }
                else if (BnfList[i].definition.Contains("brojevna_konstanta"))
                {
                    string constNumber = @"\d+([\.\,]{1}\d+)?";
                    BnfList[i].definition = BnfList[i].definition.Replace("brojevna_konstanta", constNumber);
                }
                else if (BnfList[i].definition.Contains("mejl_adresa"))
                {
                    string mailAddress = @"([\w\d\.\-]+)@([\w\d\-]+)((\.(\w){2,3})+)";
                    BnfList[i].definition = BnfList[i].definition.Replace("mejl_adresa", mailAddress);
                }
                else if (BnfList[i].definition.Contains("web_link"))
                {
                    string webLink = @"(https|http|ftp){1}(\:){1}((\/\/)?([\d\w\.]*\/)+[\d\w\.]*)(\?[\d\w\.\&]+)?(\#[\d\w\.\&]+)?";
                    BnfList[i].definition = BnfList[i].definition.Replace("web_link", webLink);
                }
                else if(BnfList[i].definition.Contains("veliki_grad"))
                {
                    string grad = GetCities();
                    BnfList[i].definition = BnfList[i].definition.Replace("veliki_grad", grad);
                }
                else if(BnfList[i].definition.Contains("\""))
                {
                    BnfList[i].definition = BnfList[i].definition.Replace("\"", "");
                }

            } // for petlja koja uklanja bjeline i mijenja regularne izraze

            for(int i = 0; i < BnfList.Count(); ++i)
            {
                string definition = Change(BnfList[i].definition);
                BnfList[i] = new Info(BnfList[i].token, definition);
            } // for petlja koja mjenja tokene sa njihovim definicijama

            StreamWriter writer = new StreamWriter(new FileStream("Root.txt", FileMode.Open)); //
            foreach (var x in BnfList)                                                         // Upis u neki text fajl da vidim root regex
                writer.Write(x.token + x.definition + "\n");                                   //

            writer.Close();

        }

        /// <summary>
        ///Zamjena tokena sa njegovom definicijom 
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public string Change(string definition)
        {
            string def = definition;
            Match token = Regex.Match(def, @"<[\w\d\s_-]+>");
            if(token.Success)
            {
                while(token.Success)
                {
                    string newDef = GetDefinition(token.Value);
                    def = def.Replace(token.Value, "(" + newDef + ")");
                    token = token.NextMatch();
                }
                def = Change(def);
            }
            return def;
        }

        /// <summary>
        /// Geter za definiciju tokena, vraca regex
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetDefinition(string token)
        {
            string def = "";
            foreach(Info i in BnfList)
            {
                if (token == i.token)
                    def = i.definition;
            }
            return def;
        }

        public BNFReader(string name) /// konstruktor
        {
            ReadFromConfigFile(name);
            foreach (var element in list)
                BnfList.Add(element);
            ListDefinition();
        } 

        /// <summary>
        /// Funkcija koja cita gradove iz datoteke gradovi.txt
        /// </summary>
        /// <returns></returns>
        private string GetCities()
        {
            StreamReader reader = new StreamReader(new FileStream("gradovi.txt", FileMode.Open));
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Geter za definiciju tokena, vraca token, (kao u bnf-u)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetDefinitionForToken(string token)
        {
            string def = "";
            foreach (Info i in list)
            {
                if (token == i.token)
                {
                    def = i.definition;
                    break;
                }
            }
            return def;
        }
    }
    
}
