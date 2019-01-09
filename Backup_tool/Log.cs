using System;
using System.IO;

namespace Backup_Khakhanov
{
    public class Log
    {
        /// <summary>
        /// Уровень логгирования
        /// </summary>
        public static LogLevel logLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Флаг, определяющий, доступно ли журналирование
        /// </summary>
        static bool isLogAvailable = false;

        /// <summary>
        /// Вывод в файл журналирования
        /// </summary>
        private static StreamWriter logWriter = null;

        /// <summary>
        /// Настраивает вывод логов не только в консоль, но и в текстовый файл
        /// </summary>
        /// <param name="path">Путь к файлу вывода</param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static void SetupOutput(string path)
        {
            logWriter = File.AppendText(path);
            logWriter.AutoFlush = true;
            isLogAvailable = true;
        }

        /// <summary>
        /// Журналирование в консоль и файл (если есть доступ)
        /// </summary>
        /// <param name="message">Событие</param>
        /// <param name="lvl">Уровень события</param>
        /// <param name="offset">Сдвиг, показывающий уровень вложенности</param>
        public static void Print(string message, LogLevel lvl, int offset = 0)
        {
            if (logLevel < lvl)
                return;

            if (offset < 0)
                offset = 0;

            string output = new String(' ', offset * 2) + message;
            string prefix;
            ConsoleColor color;

            switch (lvl)
            {
                case LogLevel.Info:
                    color = ConsoleColor.Gray;
                    prefix = "INF";
                    break;

                case LogLevel.Error:
                    color = ConsoleColor.DarkRed;
                    prefix = "ERR";
                    break;

                default:
                    color = ConsoleColor.DarkGray;
                    prefix = "DBG";
                    break;
            }

            Console.ForegroundColor = color;
            Console.WriteLine(output);
            Console.ForegroundColor = ConsoleColor.Gray;

            //Запись в файл журнала производится, только если он доступен
            if (isLogAvailable)
            {
                try
                {
                    logWriter.WriteLine(prefix + '\t' + output);
                }
                catch
                {
                    isLogAvailable = false;
                    Print("Потерян доступ к файлу журнала. Вывод ведется только в консоль.", LogLevel.Error);
                }
            }
        }
    }


    public enum LogLevel { Error, Info, Debug }
}
