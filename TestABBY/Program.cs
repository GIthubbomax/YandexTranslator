using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace TestABBY
{


    public class RootObjectlangs
    {
        public List<string> dirs { get; set; }
    }


    public class RootObjectDefineLang
    {
        public int code { get; set; }
        public string lang { get; set; }
    }

    public class RootObjectTranslate
    {
        public int code { get; set; }
        public string lang { get; set; }
        public List<string> text { get; set; }
    }
    public class ErrorListlanguages
    {
        public int code { get; set; }
        public string message { get; set; }
    }



    public class Yandextranslator
    {
        const string key = "trnsl.1.1.20170113T143509Z.b4a732df995ebdfe.7d79b996374e63d3c3d5409435fa4c0f9ffa3065";//"trnsl.1.1.20170113T143509Z.b4a732df995ebdfe.7d79b996374e63d3c3d5409435fa4c0f9ffa3065";

        /// <summary>
        /// Метод получает список путей к txt файлам 
        /// </summary>
        /// <param name="directory">путь к директории, в которой лежат исходные файлы</param>
        /// <returns>список путей к файлам</returns>
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
                Console.WriteLine("Некоррекртный путь: {0}", directory);
            }
            return namefileList;
        }

        /// <summary>
        /// Выыод списка языков и формирование словаря переводов
        /// </summary>
        /// <returns>Словаря доступных переводов в формате: название языка сокращение_названия -сокращение_названия_языка перевода* или null в результате ошибок выполнения запроса</returns>
        static List<string> GetListLanguages()
        {
            Dictionary<string, string> languages = new Dictionary<string, string>();
            List<string> langliList = new List<string>();
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/getLangs?" +
                                                      "key=" + key +
                                                      "&ui=ru"
               );

                response = request.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine("Не удалось выполнить запрос на получение списка языков по причине: {0}. Программа будет завершена, нажмите Enter", e.Message);
                Console.ReadLine();
                return null;
            }
            RootObjectlangs data = null;
            if (response != null)
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string responseString = reader.ReadToEnd();
                    JObject res = JObject.Parse(responseString);
                    if (res["langs"] != null)
                    {
                        List<JToken> results = res["langs"].Children().ToList();

                        foreach (JProperty lang in results)
                        {
                            languages.Add(lang.Name, lang.Value.ToString());
                        }
                        data = JsonConvert.DeserializeObject<RootObjectlangs>(responseString);
                        string prefix = data.dirs[0].Substring(0, 2);
                        string line = languages[prefix] + " " + prefix + ":";
                        foreach (var langdirect in data.dirs)
                        {

                            if (prefix.Equals(langdirect.Substring(0, 2)))
                                line += langdirect.Substring(2, 3);
                            else
                            {
                                langliList.Add(line);
                                prefix = langdirect.Substring(0, 2);
                                line = languages[prefix] + " " + prefix + ":";
                            }

                        }
                        foreach (var lang in langliList)
                        {
                            Console.WriteLine(lang);
                        }
                    }
                    else
                    {
                        ErrorListlanguages dataerror = JsonConvert.DeserializeObject<ErrorListlanguages>(responseString);
                        Console.WriteLine("Список языков не получен по причине:" + dataerror.message+ "Программа будет завершена, нажмите Enter");
                        Console.ReadLine();
                        return null;
                    }
                }

            return data.dirs;
        }

        /// <summary>
        /// Метод делает запрос к яндексу с текстом, и возвращает его язык или код ошибки
        /// </summary>
        /// <param name="text">исходный текст файла</param>
        /// <param name="fileresult">путь к файлу, в который будет записан результат</param>
        /// <param name="namefile">имя файла, которое будет записано в результат</param>
        /// <returns>язык текста или пустую строку если ответ не получен или null при ошибке запроса</returns>
        static string DetectLanguage(string text,string fileresult,string namefile)
        {
            WebResponse response = null;
            System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
            swatch.Start();
            try
            {
                WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/detect?" +
                                                   "key=" + key +
                                                   "&text=" + text
            );

                response = request.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine("Не удалось выполнить определить язык в файле {1} по причине: {0}", e.Message, Path.GetFileName(namefile));
            }

            RootObjectDefineLang data;
            if (response != null)
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string responseString = reader.ReadToEnd();
                    data = JsonConvert.DeserializeObject<RootObjectDefineLang>(responseString);
                    swatch.Stop();
                    switch (data.code)
                    {
                        case 200:
                            try
                            {
                                File.AppendAllText(fileresult, Environment.NewLine + Path.GetFileName(namefile) + ";" + swatch.ElapsedMilliseconds + ";");
                            }
                            catch (Exception e) {

                            }
                            break;
                        case 401:
                            Console.WriteLine("Неправильный API-ключ");
                            break;
                        case 402:
                            Console.WriteLine("API - ключ заблокирован");
                            break;
                        case 404:
                            Console.WriteLine("Превышено суточное ограничение на объем переведенного текста");
                            break;
                    }
                    return data.lang;
                }
            return "";

        }

        /// <summary>
        /// Метод выполняет перевод текста в указанном направлении 
        /// </summary>
        /// <param name="text">исходный текст файла</param>
        /// <param name="langdirect">направлене перевода</param>
        /// <param name="fileresult">путь к файлу, в который будет записан результат</param>
        static void Translate(string text, string langdirect, string fileresult)
        {
            WebResponse response = null;
            System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
            swatch.Start();
            try
            {
                WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/translate?" +
                                                   "key=" + key +
                                                   "&text=" + text +
                                                   "&lang=" + langdirect
            );

                response = request.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine("Не удалось выполнить перевод по причине: {0}", e.Message);
            }
            RootObjectTranslate data;
            if (response != null)
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string responseString = reader.ReadToEnd();
                    data = JsonConvert.DeserializeObject<RootObjectTranslate>(responseString);
                    swatch.Stop();
                    switch (data.code)
                    {
                        case 200:
                            File.AppendAllText(fileresult, swatch.ElapsedMilliseconds + ";");
                            break;
                        case 401:
                            Console.WriteLine("Неправильный API-ключ");
                            break;
                        case 402:
                            Console.WriteLine("API - ключ заблокирован");
                            break;
                        case 404:
                            Console.WriteLine("Превышено суточное ограничение на объем переведенного текста");
                            break;
                        case 413:
                            Console.WriteLine("Превышен максимально допустимый размер текста");
                            break;
                        case 422:
                            Console.WriteLine("Текст не может быть переведен");
                            break;
                        case 501:
                            Console.WriteLine("Заданное направление перевода не поддерживается");
                            break;
                    }
                }


        }

        static void Main(string[] args)
        {
            Console.WriteLine("Введите путь к деректории, содержащей txt файлы");
            string directory = @"C:\Users\User\Desktop\txt"; 
            List<string> paths = ReadFileNamesFromDirectory(directory);
            if (paths.Count == 0)
            {
                Console.WriteLine("txt-файлов не обнаруженно по пути {0}, программа будет завершена по нажатии Enter", directory);
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Введите путь к деректории, в которую будут сохранены результаты в файле fileres.csv");
            string fileresult = @"C:\Users\User\Desktop\txt\";//Console.ReadLine();
            while (!Directory.Exists(fileresult))
            {
                Console.WriteLine("Введите допустимый путь к деректории, в которую будут сохранены результаты в файле fileres.csv");
                fileresult = Console.ReadLine();
            }
            fileresult += "fileresult.csv";
            try
            {
                ConsoleKeyInfo resquestion;
                if (File.Exists(fileresult))
                {
                    Console.WriteLine("Файл с результатами уже существует, если хотите дописать в него нажмите 'y'");
                    resquestion = Console.ReadKey();
                    Console.WriteLine();
                    if (resquestion.KeyChar != 'y')
                    {
                        File.WriteAllText(fileresult, "Имя файла;Время детектирования(мс);Время перевода(мс)");
                    }
                }
                else File.AppendAllText(fileresult, "Имя файла;Время детектирования;Время перевода");
            }
            catch (Exception e)
            {
                Console.WriteLine("Не удалось создать файл и добавить текст по причине {0}. Программа будет завершена, нажмите Enter", e.Message);
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Список доступных языков:");
            var dictionary = GetListLanguages();
            if (dictionary == null) return;
            Console.WriteLine("Введите исходный язык и язык перевода в формате **-**");
            //проверка формата ввода языка 
            string langdirect = Console.ReadLine().ToLower();
            while (!dictionary.Contains(langdirect))
            {
                Console.WriteLine("Введите исходный язык и язык перевода в формате **-**");
                langdirect = Console.ReadLine().ToLower();
            }

            foreach (var namefile in paths)
            {
               
                string text = null;
                try { text = File.ReadAllText(namefile); }
                catch (Exception e) { Console.WriteLine("Не удалось прочиать содержимое файла по причине " + e.Message); }
                if (text != null)
                {
                    if (text.Length > 10000)
                    {
                        Console.WriteLine("Текст в файле {0} слишком длинный для перевода", Path.GetFileName(namefile));
                        Console.ReadLine();
                    }
                    else if (text.Length == 0)
                    {
                        Console.WriteLine("Текста в файле {0} нет", Path.GetFileName(namefile));
                    }
                    else
                    {
                        string detectlang = DetectLanguage(text,fileresult,namefile);
                        if (detectlang == null) return;
                        if (detectlang.Equals(langdirect.Substring(0, 2))) Translate(text, langdirect,fileresult);
                       
                    }
                }
            }
            Console.ReadLine();
        }
    }
}
