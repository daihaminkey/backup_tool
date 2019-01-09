using System;
using System.IO;
using Newtonsoft.Json;

namespace Backup_Khakhanov
{
    class Config
    {
        /// <summary>
        /// Стандартый путь файла конфигурации, рядом с исполняемым файлом
        /// </summary>
        public const string PATH = "config.js";

        /// <summary>
        /// Массив каталогов для резервного копирования
        /// </summary>
        public string[] copyFrom;

        /// <summary>
        /// Каталог, в который будет производится резервное копирование
        /// </summary>
        public string copyTo;

        /// <summary>
        /// Уровень журналирования
        /// </summary>
        public LogLevel logLevel;

        /// <summary>
        /// Пустой конструктор нужен библиотеке Json.NET
        /// </summary>
        private Config() { }

        /// <summary>
        /// Создает конфигурацию с указанными параметрами
        /// </summary>
        /// <param name="copyFrom">Массив каталогов для резервного копирования</param>
        /// <param name="copyTo">Каталог, в который будет производится резервное копирование</param>
        /// <param name="logLevel">Уровень журналирования</param>
        public Config(string[] copyFrom, string copyTo, LogLevel logLevel = LogLevel.Info)
        {
            this.copyFrom = copyFrom;
            this.copyTo = copyTo;
            this.logLevel = logLevel;
        }

        /// <summary>
        /// Загружает конфигурацию из json-файла
        /// </summary>
        /// <param name="path">Путь к json-файлу</param>
        /// 
        /// Исключения, возникающие при чтении файла:
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// 
        /// Исключение, возникающее в случае некорректного содержимого файла:
        /// <exception cref="InvalidCastException"></exception>
        /// <returns>Загруженная конфигурация</returns>
        public static Config Load(string path = PATH)
        {
            string json = File.ReadAllText(path);
            Log.Print("Содержимое конфигурации считано", LogLevel.Debug);

            // Нормальзация путей:  "C:\\Dev\\Backup\\To" и "C:\Dev\Backup\To" -> "C:/Dev/Backup/To" в строке
            if (json.Contains("\\\\"))
            {
                json = json.Replace("\\\\", "/");
                Log.Print("Содержимое конфигурации нормализовано", LogLevel.Debug);
            }
            else if(json.Contains("\\"))
            {
                json = json.Replace("\\", "/");
                Log.Print("Содержимое конфигурации нормализовано", LogLevel.Debug);
            }

            try
            {
                Config conf = JsonConvert.DeserializeObject<Config>(json);
                return conf;
            }
            catch
            {
                throw new InvalidCastException();
            }
        }

        /// <summary>
        /// Сохранить конфигурацию в формате json
        /// </summary>
        /// <param name="path">Путь, по которому сохраняется конфигурация</param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public void Save(string path = PATH)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (StreamWriter sw = File.CreateText(path))
                sw.Write(json);
        }

        /// <summary>
        /// Сохранить пустую конфигурацию в качестве шаблона
        /// </summary>
        /// <param name="path">Путь, по которому сохраняется конфигурация</param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static void SaveTemplate(string path = PATH)
        {
            Config template = new Config( new string[] { "", "" }, "", LogLevel.Debug );
            template.Save(path);
        }
    }
}
