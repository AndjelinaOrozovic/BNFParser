using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace FormalneProjektni
{
    class Program
    {
        static void Main(string[] args)
        {
            BNFReader bnfReader = new BNFReader("config.bin"); //procitaj bnf

            StreamReader reader = new StreamReader(new FileStream(args[0], FileMode.Open)); // otvori ulazni fajl
            string line = reader.ReadLine();                                                // procitaj prvu liniju
            string[] mecevi = line.Split(' ');                                              // razdvoji liniju

            Match m = Regex.Match(line, "^(" + BNFReader.BnfList[0].definition + ")$"); // pokusaj mecirati liniju i root definiciju

            if (m.Success)
            {
                Console.WriteLine("Parsiranje uspjesno.");
                XmlTextWriter xmlWriter = new XmlTextWriter(args[1], System.Text.Encoding.UTF8); // pravi xml fajl
                
                XMLtree.WriteXML(xmlWriter, BNFReader.BnfList[0].token, line); // dodaj tokene
                xmlWriter.Close();
            }
            else
                Console.WriteLine("Parsiranje neuspjesno.");

            reader.Close();
            Console.Read();

        }
    }
}
