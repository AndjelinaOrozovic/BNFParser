using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;

namespace FormalneProjektni
{
    class XMLtree
    {

        public static void WriteXML(XmlTextWriter xmlWriter, string token, string line)
        {
            xmlWriter.Formatting = Formatting.Indented;

            xmlWriter.WriteStartElement(token.Substring(1, token.Length - 2)); // 1 i -2 zbog < i >
            string tokensOnTheRight = BNFReader.GetDefinitionForToken(token);

            string[] parts = tokensOnTheRight.Split(' '); // parts su tokeni desne strane

            parts = Cleaning(parts); // cisti od tokena koji imaju samo "" ili "|"

            for (int i = 0; i < parts.Count(); i++)
            {
                if (IsTerminal(parts[i]))
                    xmlWriter.WriteString(FindTheValueInFile(parts[i], line)); //ako je token terminalni, nadji njegovu vrijednost, inace ga upisi, tj idi na sljedeci
                else
                    WriteXML(xmlWriter, parts[i], line);
            }

            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Funkcija za ciscenje tokena, tj. uklanja sve tokene koji su oblika "" ili "|"
        /// </summary>
        /// <param name="dijelovi"></param>
        /// <returns></returns>
        private static string[] Cleaning(string[] parts)
        {
            List<string> pom = new List<string>();
            foreach (var part in parts)
                if (part != "" && part != "|")
                    pom.Add(part);
            return pom.ToArray();
        }

        /// <summary>
        /// Funkcija koja pronalazi vrijednost tokena iz config.bin, tj. liste u kojoj su mjenjane definicije
        /// </summary>
        /// <param name="terminalniToken"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string FindTheValueInFile(string terminalToken, string line) //line su rijeci iz ulaznog fajla
        {
            string[] matches = line.Split(' ');
            string token = "";
            foreach (var element in BNFReader.list)  // pronadji token iz liste(lista samo tokena)
                if (element.definition.Contains(terminalToken))
                    token = element.token;

            string patern = "";
            foreach (var bnfelement in BNFReader.BnfList)
                if (bnfelement.token == token) // na osnovu pronadjenog tokena iz liste, pronadji njegovu definiciju iz bnf liste(imamo regex) 
                    patern = bnfelement.definition;

            Regex regeks = new Regex(patern); 
            foreach (var word in matches) // ako se rijec poklapa sa pronadjenom definicijom vrati je
                if (word == regeks.Match(word).Value)
                    return word;

            return " ";
        }

        /// <summary>
        /// Funkcija koja provjerava da li je token terminalni, posto je BNF forma trenutno tako ogranicena, imamo samo navedene slucajeve
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool IsTerminal(string token)
        {
            bool flag = false;

            if (token.StartsWith("\"") | token.StartsWith("regex") | token.StartsWith("brojevna_konstanta") | token.StartsWith("mejl_adresa") | token.StartsWith("web_link") | token.StartsWith("broj_telefona") | token.StartsWith("veliki_grad"))
                flag = true;

                return flag;
        }
    }
}