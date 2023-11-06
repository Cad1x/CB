using System.Windows;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;

namespace CB
{
    public partial class LogsWindow : Window
    {
        private const string LogsFilePath = "logs.json";

        public LogsWindow()
        {
            InitializeComponent();
            LoadLogsFromFile();
        }

        private void btnBackToAdmin_Click(object sender, RoutedEventArgs e)
        {
            // Zamknij bieżące okno LogsWindow
            this.Close();

            // Otwórz ponownie okno AdminWindow
            AdminWindow adminWindow = new AdminWindow();
            adminWindow.Show();
        }

        private void LoadLogsFromFile()
        {
            try
            {
                if (File.Exists(LogsFilePath))
                {
                    // Wczytaj zawartość pliku logs.json
                    string json = File.ReadAllText(LogsFilePath);

                    // Deserializuj dane do odpowiedniej struktury, np. List<string>
                    List<string> logs = JsonConvert.DeserializeObject<List<string>>(json);

                    // Dodaj wczytane logi do kontrolki wyświetlającej logi (np. TextBox)
                    foreach (var log in logs)
                    {
                        // Dodaj log do kontrolki, np. txtLogs
                        txtLogs.AppendText(log + Environment.NewLine);
                    }
                }
                else
                {
                    MessageBox.Show("Plik logs.json nie istnieje.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wczytywania logów: {ex.Message}");
            }
        }
    }
}
