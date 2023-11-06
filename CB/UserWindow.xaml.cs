using CB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CB
{
    /// <summary>
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
	{
		private DispatcherTimer inactivityTimer;

		private const string UsersFilePath = "users.json";

        private List<User> users;
        public UserWindow()
        {
            InitializeComponent();
            users = LoadUsers();
			LogoutAuto();
        }

        private void LogoutAuto()
        {
			string filePath = "automatLogout.txt";
			string fileContent = File.ReadAllText(filePath);
			int.TryParse(fileContent, out int intValue);
			inactivityTimer = new DispatcherTimer();
			inactivityTimer.Interval = TimeSpan.FromSeconds(intValue);

			inactivityTimer.Tick += (sender, args) => OpenMainWindowOnInactivity();

			MouseMove += (sender, args) => ResetInactivityTimer();

			Loaded += (sender, args) => StartInactivityTimer();
			
			MessageBox.Show($"czas do wylogowania{intValue}");
		}
		private void OpenMainWindowOnInactivity()
		{
			// Otwieraj okno MainWindow lub podejmuj odpowiednie działania po jednej minucie nieaktywności
			// Możesz użyć tej metody do otwarcia okna MainWindow.

			MainWindow mainWindow = new MainWindow();
			mainWindow.Show();
			Close();
			MessageBox.Show("wylogowano z powodu braku aktywności");
		}
		private void ResetInactivityTimer()
		{
			inactivityTimer.Stop();
			inactivityTimer.Start();
		}

		private void StartInactivityTimer()
		{
			inactivityTimer.Start();
		}



		public List<User> LoadUsers()
        {
            try
            {
                string json = File.ReadAllText(UsersFilePath);
                return JsonConvert.DeserializeObject<List<User>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }

       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            User newuser = users.FirstOrDefault(u => u.Username == Data.LoginName);



            ChangePasswordWindow change = new ChangePasswordWindow(newuser);
            change.Show();
        }
    }
}
