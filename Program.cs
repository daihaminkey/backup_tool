using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Backup_Khakhanov
{
    //Переписать логгер как класс, для повторного использования
    //Убрать ReadLine, чтобы выпоолнять скриптом
    class Program
    {
        /// <summary>
        /// Счетчик скопированных файлов
        /// </summary>
        static int fileCount;

        /// <summary>
        /// Счетчик скопированных директорий
        /// </summary>
        static int dirCount;

        /// <summary>
        /// Вывод в файл журналирования
        /// </summary>
        static StreamWriter logWriter = null;

        /// <summary>
        /// Флаг, определяющий, доступно ли журналирование
        /// </summary>
        static bool isLogAvailable = false;

        /// <summary>
        /// Журналирование в консоль и файл (если есть доступ)
        /// </summary>
        /// <param name="message">Событие</param>
        /// <param name="lvl">Уровень события</param>
        /// <param name="offset">Сдвиг, показывающий уровень вложенности</param>
        static void Log(string message, LogLevel lvl, int offset = 0)
        {
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
            if(isLogAvailable)
            {
                try
                {
                    logWriter.WriteLine(prefix+'\t'+output);
                }
                catch
                {
                    isLogAvailable = false;
                    Log("Потерян доступ к файлу журнала. Вывод ведется только в консоль.", LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Создает главный каталог для резервного копирования и файл журнала выполнения в нем
        /// Все выбрасываемые исключения возникают при создании каталога и считаются критическими
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns>Путь к созданной папке</returns>
        static string CreateBackupDir()
        {
            string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss");
            

            string dir = @"C:\Dev\Backup\To\" + timestamp;

            try
            {
                Directory.CreateDirectory(dir);
            }
            catch
            {
                //Невозможность создать эту директорию - критическая ошибка, которая должна приводить к завершению программы
                throw;
            }

            try
            {
                string logPath = dir + "\\log.txt";
                logWriter = File.AppendText(logPath);
                logWriter.AutoFlush = true;
                isLogAvailable = true;
                Log("Журнал выполнения создан успешно: "+logPath, LogLevel.Debug);
            }
            catch(Exception e)
            {
                //Программа продолжит работать, если запись в файл невозможна
                Log("Не удалось создать журнал выполнения: "+e.Message+"\nВывод ведется только в консоль", LogLevel.Error);
            }

            Log($"Штамп: {timestamp}", LogLevel.Debug);
            return dir;
        }

        /// <summary>
        /// Рекурсивно копирует внутрь каталога <see cref="toDir"/> каталог <see cref="fromDir"/> со всем его содержимым 
        /// </summary>
        /// <param name="fromDir">Каталог, содержимое которого копируется</param>
        /// <param name="toDir">Каталог, в котором создается копия <see cref="fromDir"/></param>
        /// <param name="offset">Глубина вложенности для оформления вывода</param>
        static void CopyDir(string fromDir, string toDir, int offset = 1)
        {
            string[] filesInDir = null;
            string[] foldersInDir = null;

            try
            {
                filesInDir = Directory.GetFiles(fromDir);
                foldersInDir = Directory.GetDirectories(fromDir);
                Log("Содержимое каталога получено", LogLevel.Debug, offset);
            }
            catch (Exception e)
            {
                Log($"При чтении данных каталога [{ fromDir }] произошла ошибка: { e.Message }", LogLevel.Error, offset);
                return;
            }

            try
            {
                toDir = toDir + "\\" + Path.GetFileName(fromDir);
                Directory.CreateDirectory(toDir);
                ++dirCount;
            }
            catch (Exception e)
            {
                Log($"Не удалось создать целевую директорию: {e.Message}", LogLevel.Error, offset);
                return;
            }


            foreach (string fileSource in filesInDir)
            {
                //Не выкидывает исключение, потому что filesInDir содержит только корректные пути
                string fileDest = toDir + "\\" + Path.GetFileName(fileSource);

                Log($"Копирование файла [{ fileSource }] в [{ fileDest }]...", LogLevel.Info, offset);

                try
                {
                    File.Copy(fileSource, fileDest);
                    ++fileCount;
                    Log("Файл успешно скопирован", LogLevel.Debug, offset + 1);
                }
                catch (Exception e)
                {
                    Log($"При копировании файла произошла ошибка: { e.Message }", LogLevel.Error, offset + 1);
                }
            }

            foreach (string folderSource in foldersInDir)
            {
                Log($"Копирование каталога [{ folderSource }] в [{ toDir }]...", LogLevel.Info, offset);

                CopyDir(folderSource, toDir, offset + 1);
            }

        }

        static void Main(string[] args)
        {
            string[] fromDirs = { @"C:\Dev\Backup\From1", @"C:\Dev\Backup\From2", @"C:\Dev\Backup\Fr:om3", "" };
            string toDir;

            try
            {
                toDir = CreateBackupDir();
            }
            catch (Exception e)
            {
                Log("Критическая ошибка: не удалось создать каталог для резервного копирования: "+e.Message+ "\nВыполнение программы прервано", LogLevel.Error);
                Console.ReadLine();
                return;
            }

            if (fromDirs?.Length > 0)
            {
                Log($"Каталогов для резервного копирования: {fromDirs.Length}", LogLevel.Debug);

                foreach (string fromDir in fromDirs)
                {
                    Log($"Резервное копирование каталога [{ fromDir }]...", LogLevel.Info);
                    CopyDir(fromDir, toDir);
                }


                Log($"Резервное копирование завершено\nКаталогов скопировано: { dirCount }\nФайлов скопировано: { fileCount}", LogLevel.Info);
            }
            else
            {
                Log("Отсутствуют каталоги для резервного копирования", LogLevel.Error);
            }

            Console.ReadLine();
        }
    }

    public enum LogLevel { Debug, Info, Error }
}
