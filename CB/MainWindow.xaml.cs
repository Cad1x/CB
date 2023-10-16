using CB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

namespace CB
{
    public partial class MainWindow : Window
    {
        private const string UsersFilePath = "users.json";
        private const string AdminsFilePath = "admin.json";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            var user = LoadUsers().FirstOrDefault(u => u.Username == username);

            if (user != null && user.IsFirstLogin)
            {
                ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(user);
                changePasswordWindow.Show();
                Close();
            }
            else
            if (AuthenticateUser(username, password))
            {
                MessageBox.Show("Logowanie zakończone sukcesem!");

                // Open the appropriate window based on the user role
                OpenAppropriateWindow(username);

                Close();
            }
            else
            {
                MessageBox.Show("Niepoprawny login lub hasło");
            }


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
