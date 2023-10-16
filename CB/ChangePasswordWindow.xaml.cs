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

namespace CB
{
    public partial class ChangePasswordWindow : Window
    {
        private User _user;

        public ChangePasswordWindow(User user)
        {
            InitializeComponent();
            _user = user;
            _user.IsFirstLogin = false;
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Pobierz wprowadzone dane
            string newPassword = passwordBox.Password;
            string confirmNewPassword = confirmPasswordBox.Password;

            // Sprawdź, czy nowe hasło i jego potwierdzenie są takie same
            if (newPassword != confirmNewPassword)
            {
                MessageBox.Show("Nowe hasło i jego potwierdzenie są różne.");
                return;
            }

            // Zmień hasło
            ChangeUserPassword(newPassword);

            // Ustaw isFirstLogin na false po zmianie hasła
            

            MessageBox.Show("Hasło zostało pomyślnie zmienione.");
            Close(); // Zamknij okno po udanej zmianie hasła
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
    }
}