using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CB.Model
{
    public class Logs
    {
        private List<string> logEntries;

        public Logs()
        {
            LoadLogsFromFile(); // Dodano wczytywanie logów przy tworzeniu instancji
        }

        public void LogLogin(string username)
        {
            string logEntry = $"{DateTime.Now}: {username} logged in.";
            logEntries.Add(logEntry);
            Console.WriteLine(logEntry); // Opcjonalne: wyświetlenie logu w konsoli

            SaveLogsToFile();
        }
        public void LogLoginFiled(string username)
        {
            string logEntry = $"{DateTime.Now}: {username} login filed.";
            logEntries.Add(logEntry);
            Console.WriteLine(logEntry); // Opcjonalne: wyświetlenie logu w konsoli

            SaveLogsToFile();
        }
        public void LogChangePassword(string username)
        {
            string logEntry = $"{DateTime.Now}: {username} changed password.";
            logEntries.Add(logEntry);
            Console.WriteLine(logEntry); // Opcjonalne: wyświetlenie logu w konsoli

            SaveLogsToFile();
        }

        public void LogUserDelete(string username)
        {
            string logEntry = $"{DateTime.Now}: {username} was deleted.";
            logEntries.Add(logEntry);
            Console.WriteLine(logEntry); // Opcjonalne: wyświetlenie logu w konsoli

            SaveLogsToFile();
        }
        public void LogUserCreate(string username)
        {
            string logEntry = $"{DateTime.Now}: {username} was created.";
            logEntries.Add(logEntry);
            Console.WriteLine(logEntry); // Opcjonalne: wyświetlenie logu w konsoli

            SaveLogsToFile();
        }

        private void SaveLogsToFile()
        {
            string logsJson = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
            File.WriteAllText("logs.json", logsJson);
        }

        private void LoadLogsFromFile()
        {
            if (File.Exists("logs.json"))
            {
                string logsJson = File.ReadAllText("logs.json");
                logEntries = JsonConvert.DeserializeObject<List<string>>(logsJson) ?? new List<string>();
            }
            else
            {
                logEntries = new List<string>();
            }
        }
    }
}
