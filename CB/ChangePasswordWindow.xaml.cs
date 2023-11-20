using CB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ReCaptcha.Desktop.WPF.Client;
using ReCaptcha.Desktop.WPF.Client.Interfaces;
using ReCaptcha.Desktop.WPF.Configuration;
using System.Threading;

namespace CB
{
    public partial class ChangePasswordWindow : Window
    {
        private User _user;
        private const string UsersFilePath = "users.json";
        private const string AdminsFilePath = "admin.json";
        public ChangePasswordWindow(User user)
        {
            InitializeComponent();
            _user = user;
            _user.IsFirstLogin = false;
        }




        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Pobierz wprowadzone dane
            string currentPassword = currentPasswordBox.Password;
            string newPassword = passwordBox.Password;
            string confirmNewPassword = confirmPasswordBox.Password;

            if (!robotCheckBox.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show("Proszę potwierdzić, że nie jesteś robotem.");
                return;
            }

            AuthenticateUser(currentPassword);

            // Sprawdź, czy nowe hasło i jego potwierdzenie są takie same
            if (newPassword != confirmNewPassword)
            {
                MessageBox.Show("Nowe hasło i jego potwierdzenie są różne.");
            }


            if (AuthenticateUser(currentPassword))
            {
                ChangeUserPassword(newPassword);
                MessageBox.Show("Hasło zostało pomyślnie zmienione.");
                LogChangePass(Data.LoginName);
            }
            else
            {
                MessageBox.Show("Hasło aktualne nie jest poprawne");
            }
            
        }
        private void LogChangePass(string username)
        {
            Logs logs = new Logs();
            logs.LogChangePassword(username);
        }
        private bool AuthenticateUser(string password)
        {
            List<User> users = LoadUsers();
            List<Admin> admins = LoadAdmins();


            // Find the user by username
            var user = users.FirstOrDefault(u => u.Username == Data.LoginName);

            if (user != null)
            {
                if (user.IsFirstLogin)
                {
                    // Prompt the user to change the password

                    return false;

                    // Returning false for now; the user will try to log in again with the new password
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

        private void ChangeUserPassword(string newPassword)
        {
            // Wygeneruj nowy losowy salt
            byte[] salt = GenerateSalt();

            // Zaktualizuj hasło i salt użytkownika
            _user.PasswordHash = HashPassword(newPassword, salt);
            _user.Salt = Convert.ToBase64String(salt);

            // Wczytaj istniejące dane
            List<User> existingUsers = LoadUserData();

            // Znajdź użytkownika do edycji
            User userToEdit = existingUsers.Find(u => u.Username == _user.Username);

            if (userToEdit != null)
            {
                // Zaktualizuj hasło, salt i isFirstLogin
                userToEdit.PasswordHash = _user.PasswordHash;
                userToEdit.Salt = _user.Salt;
                userToEdit.IsFirstLogin = _user.IsFirstLogin;

                // Zapisz zaktualizowaną listę z powrotem do users.json
                string json = JsonConvert.SerializeObject(existingUsers);
                File.WriteAllText("users.json", json);
            }
        }

        private List<User> LoadUserData()
        {
            if (File.Exists("users.json"))
            {
                // Wczytaj istniejące dane z users.json
                string json = File.ReadAllText("users.json");
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            else
            {
                // Zwróć pustą listę, jeśli plik nie istnieje
                return new List<User>();
            }
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = new SHA256Managed())
            {
                // Konwertuj ciąg znaków hasła i salt do tablicy bajtów
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                // Oblicz wartość skrótu z tablicy bajtów zasoloną
                byte[] hashBytes = sha256.ComputeHash(saltedPassword);

                // Konwertuj tablicę bajtów skrótu na szesnastkowy ciąg znaków
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[32]; // Możesz dostosować rozmiar soli według potrzeb
                rng.GetBytes(salt);
                return salt;
            }
        }

        private async void captcha_Click(object sender, RoutedEventArgs e)
        {
            WindowConfig uiConfig = new("WINDOW_TITLE"); // WPF
            ReCaptchaConfig config = new("6Lf2VhYpAAAAAGL0yJt93LkcYjCogV7BjES2xY6T", "test.com");
            IReCaptchaClient reCaptcha = new ReCaptchaClient(config, uiConfig);
            reCaptcha.VerificationRecieved += (s, e) =>
            {
                MessageBox.Show("Verification recieved");
                robotCheckBox.IsChecked = true;
            };

            reCaptcha.VerificationCancelled += (s, e) =>
                MessageBox.Show($"Occurred At: {e.OccurredAt}", "Verification cancelled");
            CancellationTokenSource cts = new(TimeSpan.FromMinutes(1));
            string token = await reCaptcha.VerifyAsync(cts.Token);
        }
    }
}