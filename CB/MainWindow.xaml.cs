using CB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;

namespace CB
{
    public partial class MainWindow : Window
    {
        private const string UsersFilePath = "users.json";
        private const string AdminsFilePath = "admin.json";
        private PasswordBox specialPasswordBox;  // Dodaj to pole w klasie



        int loginAttempts = 0;
		private DateTime startTime;
		private DateTime endTime;
		private DispatcherTimer timer;
		public MainWindow()
        {
            InitializeComponent();
            Timers();
            GenerateRandomNumber();

        }
        private void GenerateRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 101);
            randomNumberTextBlock.Text = $"x: {randomNumber}";
        }


        private void Timers()
        {
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += Timer_Tick;
		}
		private void Timer_Tick(object sender, EventArgs e)
		{
			if (DateTime.Now < endTime)
			{
				TimeSpan remainingTime = endTime - DateTime.Now;
				remainingTimeLabel.Content = $"Pozostało {remainingTime.TotalSeconds:F0} sekund.";
			}
			else
			{
				remainingTimeLabel.Content = "Blokada zdjęta. Można ponownie się logować";
				timer.Stop();
				LoginButton.IsEnabled = true;
                loginAttempts = 0;
			}
		}

        

       
		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			int maxLoginAttempts = 5;

			string attemptsToSave = "falseLogin.txt";
			string fileContentLogin = File.ReadAllText(attemptsToSave);
			int.TryParse(fileContentLogin, out int loginValue);
			maxLoginAttempts = loginValue;
			string timerToSave = "falseLoginTimer.txt";
			string fileContenttimer = File.ReadAllText(timerToSave);
			int.TryParse(fileContenttimer, out int timerValue);
			int czas = timerValue;
            string oneTimePassword = txtOneTimePassword.Text;


            string username = txtUsername.Text;
            string password = txtPassword.Password;

            var user = LoadUsers().FirstOrDefault(u => u.Username == username);

            if (loginAttempts==maxLoginAttempts)
            {
                
				MessageBox.Show($"Przekroczono liczbę dostępnych prób logowania poczekaj {czas} sekund");
				startTime = DateTime.Now;
				endTime = startTime.AddSeconds(czas); // Dodaj 10 sekund do początkowej daty i godziny
				timer.Start();
				LoginButton.IsEnabled = false;

			}
			if (user != null && user.IsFirstLogin)
            {
                Data.LoginName = username;

                ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(user);
                changePasswordWindow.Show();
                Close();
            }
            else
            if (username=="admin")
            {
                if (AuthenticateUser(username, password))
                {
                    MessageBox.Show("Logowanie zakończone sukcesem!");

                    // Open the appropriate window based on the user role
                    OpenAppropriateWindow(username);
                    Data.LoginName = username;
                    LogSuccessfulLogin(username);
                    loginAttempts = 0;
                    Close();
                }
            }
            else
            if (AuthenticateUser(username, password) && ValidateOneTimePassword(oneTimePassword))
            {
                MessageBox.Show("Logowanie zakończone sukcesem!");
				
				// Open the appropriate window based on the user role
				OpenAppropriateWindow(username);
                Data.LoginName = username;
                LogSuccessfulLogin(username);
                loginAttempts = 0;
                Close();
            }
            else
            {
                MessageBox.Show("Niepoprawny login lub hasło");
                loginAttempts++;
				remainingattemptLabel.Content = "Liczba prób " + loginAttempts.ToString();
                LogFailedLogin(username);

            }


        }

        private bool ValidateOneTimePassword(string oneTimePassword)
        {
            

            string validOneTimePassword = "123456"; 

            return oneTimePassword == validOneTimePassword;
        }

        private void LogSuccessfulLogin(string username)
        {
            Logs logs = new Logs();
            logs.LogLogin(username);
        }

        private void LogFailedLogin(string username)
        {
            Logs logs = new Logs();
            logs.LogLoginFiled(username);
        }


        private bool AuthenticateUser(string username, string password)
        {
            List<User> users = LoadUsers();
            List<Admin> admins = LoadAdmins();


            // Find the user by username
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {

                if (user.IsFirstLogin)
                {
                    // Prompt the user to change the password
                    
                    return false; // Returning false for now; the user will try to log in again with the new password
                }
                // Check if the account is locked
                if (user.IsLocked)
                {
                    MessageBox.Show("Twoje konto jest zablokowane. Skontaktuj się z administratorem.");
                    return false;
                }

                // Validate the password
                byte[] salt = Convert.FromBase64String(user.Salt);
                string hashedPassword = HashPassword(password, salt);

                return hashedPassword == user.PasswordHash;
            }

            var admin = admins.FirstOrDefault(a => a.Username == username);

            if (admin != null)
            {
                // Validate the password for admin
                byte[] salt = Convert.FromBase64String(admin.Salt);
                string hashedPassword = HashPassword(password, salt);

                return hashedPassword == admin.PasswordHash;
            }


            //return (username == "admin" && password == "admin");

            return false;
        }

       


        private List<Admin> LoadAdmins()
        {
            try
            {
                string json = File.ReadAllText(AdminsFilePath);
                return JsonConvert.DeserializeObject<List<Admin>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading admins: {ex.Message}");
                return new List<Admin>();
            }
        }

        private List<User> LoadUsers()
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

        private void OpenAppropriateWindow(string username)
        {
            Data.LoginName = username;
            if (username == "admin")
            {
                // Open the admin window
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show();
            }
            else
            {
                // Open the user window
                UserWindow userWindow = new UserWindow();
                userWindow.Show();
            }
        }



        private string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = new SHA256Managed())
            {
                // Convert the password string and salt to byte arrays
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                // Compute the hash value of the salted byte array
                byte[] hashBytes = sha256.ComputeHash(saltedPassword);

                // Convert the hash byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
