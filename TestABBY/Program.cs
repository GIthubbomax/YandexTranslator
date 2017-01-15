using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestABBY
{
    public class Langs
    {
        //        public string af { get; set; }
        //        public string am { get; set; }
        //        public string ar { get; set; }
        //        public string az { get; set; }
        //        public string ba { get; set; }
        //        public string be { get; set; }
        //        public string bg { get; set; }
        //        public string bn { get; set; }
        //        public string bs { get; set; }
        //        public string ca { get; set; }
        //        public string ceb { get; set; }
        //        public string cs { get; set; }
        //        public string cy { get; set; }
        //        public string da { get; set; }
        //        public string de { get; set; }
        //        public string el { get; set; }
        //        public string en { get; set; }
        //        public string eo { get; set; }
        //        public string es { get; set; }
        //        public string et { get; set; }
        //        public string eu { get; set; }
        //        public string fa { get; set; }
        //        public string fi { get; set; }
        //        public string fr { get; set; }
        //        public string ga { get; set; }
        //        public string gd { get; set; }
        //        public string gl { get; set; }
        //        public string gu { get; set; }
        //        public string he { get; set; }
        //        public string hi { get; set; }
        //        public string hr { get; set; }
        //        public string ht { get; set; }
        //        public string hu { get; set; }
        //        public string hy { get; set; }
        //        public string id { get; set; }
        //        public string @is { get; set; }
        //        public string it { get; set; }
        //        public string ja { get; set; }
        //        public string jv { get; set; }
        //        public string ka { get; set; }
        //        public string kk { get; set; }
        //        public string kn { get; set; }
        //        public string ko { get; set; }
        //        public string ky { get; set; }
        //        public string la { get; set; }
        //        public string lb { get; set; }
        //        public string lt { get; set; }
        //        public string lv { get; set; }
        //        public string mg { get; set; }
        //        public string mhr { get; set; }
        //        public string mi { get; set; }
        //        public string mk { get; set; }
        //        public string ml { get; set; }
        //        public string mn { get; set; }
        //        public string mr { get; set; }
        //        public string mrj { get; set; }
        //        public string ms { get; set; }
        //        public string mt { get; set; }
        //        public string ne { get; set; }
        //        public string nl { get; set; }
        //        public string no { get; set; }
        //        public string pa { get; set; }
        //        public string pap { get; set; }
        //        public string pl { get; set; }
        //        public string pt { get; set; }
        //        public string ro { get; set; }
        //        public string ru { get; set; }
        //        public string si { get; set; }
        //        public string sk { get; set; }
        //        public string sl { get; set; }
        //        public string sq { get; set; }
        //        public string sr { get; set; }
        //        public string su { get; set; }
        //        public string sv { get; set; }
        //        public string sw { get; set; }
        //        public string ta { get; set; }
        //        public string te { get; set; }
        //        public string tg { get; set; }
        //        public string th { get; set; }
        //        public string tl { get; set; }
        //        public string tr { get; set; }
        //        public string tt { get; set; }
        //        public string udm { get; set; }
        //        public string uk { get; set; }
        //        public string ur { get; set; }
        //        public string uz { get; set; }
        //        public string vi { get; set; }
        //        public string xh { get; set; }
        //        public string yi { get; set; }
        //        public string zh { get; set; }
    }

    public class RootObjectlangs
    {
        public List<string> dirs { get; set; }
        public Langs langs { get; set; }
    }


    public class RootObjectDefineLang
    {
        public int code { get; set; }
        public string lang { get; set; }
    }


    class Yandextranslator
    {
        const string key = "trnsl.1.1.20170113T143509Z.b4a732df995ebdfe.7d79b996374e63d3c3d5409435fa4c0f9ffa3065";

        static List<string> ReadFileNamesFromDirectory(string directory)
        {
            List<string> namefileList = new List<string>();
            try
            {
                string[] dirs = Directory.GetFiles(directory, "*.txt");
                namefileList = dirs.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            return namefileList;
        }

        static List<string> GetListLanguages()
        {
            List<string> langliList = new List<string>();
            WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/getLangs?"+
                                                   "key="+ key+
                                                   "&ui=ru"
            );

            WebResponse response = request.GetResponse();
            RootObjectlangs data;
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
                data = JsonConvert.DeserializeObject<RootObjectlangs>(responseString);
            }
            return data.dirs;
        }

        static string DetectLanguage(string text)
        {
            WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/detect?"+
                                                   "key="+ key+
                                                   "&text" +text
            );

            WebResponse response = request.GetResponse();
            RootObjectDefineLang data;
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
                data = JsonConvert.DeserializeObject<RootObjectDefineLang>(responseString);
            }
            if (data.code != 200)
            {
                string err = "ошибка";
                return err;
            }
            return data.lang;
        }

        static bool Translate(string text, string langfrom, string langto)
        {
            return false;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("введите путь к деректории, содержащей txt файлы");
            string directory = @"C:\Users\Бочков\Desktop\txt"; //Console.ReadLine();
            Console.WriteLine("введите исходный язык и язык перевода в формате **-**");
            foreach (var lang in GetListLanguages())
            {
                Console.WriteLine(lang);
            }
            string language = Console.ReadLine();


            foreach (var namefile in ReadFileNamesFromDirectory(directory))
            {
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();

                string text = File.ReadAllText(namefile);
                Console.WriteLine(DetectLanguage(text));
                swatch.Stop();
                Console.WriteLine(swatch.Elapsed);
            }
            Console.ReadLine();
        }
    }
}
