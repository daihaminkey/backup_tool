using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Backup_Khakhanov
{

    class Program
    {

        static void Log(string message, LogLevel lvl, int offset = 0)
        {
            if (offset < 0)
                offset = 0;

            string tabs = new String(' ', offset * 2);
            ConsoleColor color;

            switch (lvl)
            {
                case LogLevel.Info:
                    color = ConsoleColor.Gray;
                    break;

                case LogLevel.Warning:
                    color = ConsoleColor.DarkYellow;
                    break;
                case LogLevel.Error:
                    color = ConsoleColor.DarkRed;
                    break;

                default:
                    color = ConsoleColor.DarkGray;
                    break;
            }

            Console.ForegroundColor = color;

            Console.WriteLine($"{tabs}{message}");
        }

        static void CopyDir(string fromDir, string toDir, int offset = 1)
        {
            string[] filesInDir = Directory.GetFiles(fromDir);
            string[] foldersInDir = Directory.GetDirectories(fromDir);
            Log("Содержимое каталога получено", LogLevel.Debug, offset);

            Directory.CreateDirectory(toDir);

            foreach (string fileSource in filesInDir)
            {
                //Не выкидывает исключение, потому что filesInDir содержит только корректные пути
                string fileDest = toDir + "\\" + Path.GetFileName(fileSource);

                Log($"Копирование файла [{ fileSource }] в [{ fileDest }]...", LogLevel.Info, offset);
                File.Copy(fileSource, fileDest);

                Log("Файл успешно скопирован", LogLevel.Debug, offset + 1);

            }

            foreach (string folderSource in foldersInDir)
            {
                //Не выкидывает исключение, потому что foldersInDir содержит только корректные пути
                string folderDest = toDir + "\\" + Path.GetFileName(folderSource);

                Log($"Копирование каталога [{ folderSource }] в [{ folderDest }]...", LogLevel.Info, offset);

                //Все исключения обрабатываются внутри
                CopyDir(folderSource, folderDest, offset + 1);
            }

        }

        static void Main(string[] args)
        {
            string[] fromDirs = { @"C:\Dev\Backup\From1", @"C:\Dev\Backup\From2", @"C:\Dev\Backup\Fr:om3", "" };

            Log($"Каталогов для резервного копирования: {fromDirs.Length}", LogLevel.Debug);
            string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss");
            Log($"Штамп: {timestamp}", LogLevel.Debug);


            foreach (string fromDir in fromDirs)
            {
                Log($"Резервное копирование каталога [{ fromDir }]...", LogLevel.Info);

                string toDir = $@"C:\Dev\Backup\To2\{ timestamp }\{ Path.GetFileName(fromDir) }";
                CopyDir(fromDir, toDir);
            }


            Log($"\nРезервное копирование { timestamp } завершено", LogLevel.Info);

            Console.ReadLine();
        }
    }

    public enum LogLevel { Debug, Info, Warning, Error}
}
