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
using System.Security.Cryptography;
using System.Windows.Threading;

namespace CB
{
    /// <summary>
    /// Logika interakcji dla klasy AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
		private DispatcherTimer inactivityTimer;
		private int inactivityDuration = 15; // 15 sekund 

		public AdminWindow()
        {
            InitializeComponent();
            LoadUserComboBox();
            DisplayAllUsers();

			inactivityTimer = new DispatcherTimer();
			inactivityTimer.Interval = TimeSpan.FromSeconds(inactivityDuration);
			inactivityTimer.Tick += (sender, args) => OpenMainWindowOnInactivity();

			MouseMove += (sender, args) => ResetInactivityTimer();

			Loaded += (sender, args) => StartInactivityTimer();
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

		private void DisplayAllUsers()
        {
            // Load all users
            var users = LoadUserData();

            // Bind the users to the ListBox
            lstAllUsers.ItemsSource = users.Select(user => user.Username).ToList();
        }

        private void LoadUserComboBox()
        {
            cmbUsers.Items.Clear();
            // Load users from users.json
            var users = LoadUserData();

            // Populate ComboBox with user names
            foreach (var user in users)
            {
                cmbUsers.Items.Add(user.Username);
            }
        }

        private void SaveUserData(string username, string password, bool validatePassword)
        {
            // In a real application, you would need more sophisticated logic to determine
            // whether the user is an admin or a regular user. For simplicity, we assume
            // that any user whose name starts with "admin" is an admin.

            string json;

            if (username.StartsWith("admin"))
            {
                // Admin user, load existing data from admin.json
                var existingAdmins = LoadAdminData();

                // Generate a random salt for each admin
                byte[] salt = GenerateSalt();

                // Add the new admin with hashed password and salt
                existingAdmins.Add(new Admin
                {
                    Username = username,
                    PasswordHash = HashPassword(password, salt),
                    Salt = Convert.ToBase64String(salt),
                    Role = "admin"
                });

                // Save the updated list back to admin.json
                json = JsonConvert.SerializeObject(existingAdmins);
                File.WriteAllText("admin.json", json);
            }
            else
            {
                // Regular user, load existing data from users.json
                var existingUsers = LoadUserData();

                var existingUser = existingUsers.FirstOrDefault(u => u.Username == username);

                if (existingUser != null)
                {
                    if (existingUser.IsLocked)
                    {
                        MessageBox.Show("Account is locked. Cannot add or update password.");
                        return;
                    }
                }

                if (validatePassword)
                {
                    // Your password validation logic here
                    // For example, check if the password meets certain criteria
                    

                    // Check if there are at least two digits in the password
                    if (password.Count(char.IsDigit) < 2)
                    {
                        MessageBox.Show("User password must contain at least two digits.");
                        return;
                    }

                    // Check if there is at least one uppercase letter in the password
                    if (!password.Any(char.IsUpper))
                    {
                        MessageBox.Show("User password must contain at least one uppercase letter.");
                        return;
                    }
                }


                // Generate a random salt for each user
                byte[] salt = GenerateSalt();

                // Add the new user with hashed password and salt
                existingUsers.Add(new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password, salt),
                    Salt = Convert.ToBase64String(salt),
                    Role = "user"
                });

                // Save the updated list back to users.json
                json = JsonConvert.SerializeObject(existingUsers);
                File.WriteAllText("users.json", json);


            }

            
            LoadUserComboBox();
            txtNewUsername.Clear();
            txtNewPassword.Clear();
        }

        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[32]; // You can adjust the size of the salt as needed
                rng.GetBytes(salt);
                return salt;
            }
        }
        private List<Admin> LoadAdminData()
        {
            if (File.Exists("admin.json"))
            {
                // Read existing data from admin.json
                string json = File.ReadAllText("admin.json");
                return JsonConvert.DeserializeObject<List<Admin>>(json) ?? new List<Admin>();
            }
            else
            {
                // Return an empty list if the file doesn't exist
                return new List<Admin>();
            }
        }

        private List<User> LoadUserData()
        {
            if (File.Exists("users.json"))
            {
                // Read existing data from users.json
                string json = File.ReadAllText("users.json");
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            else
            {
                // Return an empty list if the file doesn't exist
                return new List<User>();
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

        private void btnRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUsers.SelectedItem != null)
            {
                string selectedUser = cmbUsers.SelectedItem.ToString();

                // Confirm user deletion
                MessageBoxResult result = MessageBox.Show($"Czy napewno chcesz usunąć użytkownika '{selectedUser}'?", "Potwierdź", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Remove user from the list and update the file
                    RemoveUser(selectedUser);
                    MessageBox.Show($"User '{selectedUser}' removed successfully!");
                    LoadUserComboBox(); // Refresh ComboBox
                }
            }
            else
            {
                MessageBox.Show("Please select a user to remove.");
            }
        }

        private void RemoveUser(string username)
        {
            // Load existing users
            var existingUsers = LoadUserData();

            // Remove the selected user
            var userToRemove = existingUsers.FirstOrDefault(u => u.Username == username);
            if (userToRemove != null)
            {
                existingUsers.Remove(userToRemove);

                // Save the updated list back to users.json
                string json = JsonConvert.SerializeObject(existingUsers);
                File.WriteAllText("users.json", json);
            }
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = txtNewUsername.Text;
            string password = txtNewPassword.Password;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Check if password validation is requested
            bool validatePassword = chkPasswordValidation.IsChecked ?? false;

            // Save user data
            SaveUserData(username, password, validatePassword);
            MessageBox.Show("User added successfully!");
        }

        private void btnEditUserNick_Click(object sender, RoutedEventArgs e)
        {
            string oldUsername = cmbUsers.SelectedItem.ToString();
            string newUsername = txtEditUserNick.Text;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(oldUsername) || string.IsNullOrWhiteSpace(newUsername))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Load existing data
            List<User> existingUsers = LoadUserData();

            // Find the user to edit
            User userToEdit = existingUsers.FirstOrDefault(u => u.Username == oldUsername);

            if (userToEdit != null)
            {
                // Change user's nick
                userToEdit.Username = newUsername;

                // Save the updated list back to users.json
                string json = JsonConvert.SerializeObject(existingUsers);
                File.WriteAllText("users.json", json);

                // Load updated users to ComboBox
                LoadUserComboBox();

                MessageBox.Show("User nick edited successfully!");
            }
            else
            {
                MessageBox.Show("User not found.");
            }

            txtEditUserNick.Clear();

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Reinitialize the component to refresh the entire form
            InitializeComponent();

            // Load the user combo box
            LoadUserComboBox();
            DisplayAllUsers();
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Get selected user from ComboBox
            string selectedUser = cmbUsers.SelectedItem?.ToString();

            // Validate if user is selected
            if (string.IsNullOrWhiteSpace(selectedUser))
            {
                MessageBox.Show("Please select a user to change password.");
                return;
            }

            // Get new password
            string newPassword = txtNewPasswordForChange.Password;

            // Validate if the new password is provided
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Please provide a new password.");
                return;
            }

            // Change user's password
            ChangeUserPassword(selectedUser, newPassword);

            // Clear input fields
            txtNewPasswordForChange.Clear();
        }

        private void ChangeUserPassword(string username, string newPassword)
        {
            // Load existing data
            List<User> existingUsers = LoadUserData();

            // Find the user to edit
            User userToEdit = existingUsers.FirstOrDefault(u => u.Username == username);

            if (userToEdit != null)
            {
                // Generate a new random salt
                byte[] salt = GenerateSalt();

                // Update the user's password and salt
                userToEdit.PasswordHash = HashPassword(newPassword, salt);
                userToEdit.Salt = Convert.ToBase64String(salt);

                // Save the updated list back to users.json
                string json = JsonConvert.SerializeObject(existingUsers);
                File.WriteAllText("users.json", json);

                MessageBox.Show("User password changed successfully!");
            }
            else
            {
                MessageBox.Show("User not found.");
            }
        }

        private void btnLockUser_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUsers.SelectedItem != null)
            {
                var selectedUser = cmbUsers.SelectedItem.ToString();
                var existingUsers = LoadUserData();
                var userToLock = existingUsers.FirstOrDefault(u => u.Username == selectedUser);

                if (userToLock != null)
                {
                    userToLock.IsLocked = true;
                    string json = JsonConvert.SerializeObject(existingUsers);
                    File.WriteAllText("users.json", json);

                    MessageBox.Show($"User '{selectedUser}' locked successfully!");
                    LoadUserComboBox();
                }
            }
            else
            {
                MessageBox.Show("Please select a user to lock.");
            }
        }


        private void btnUnlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUsers.SelectedItem != null)
            {
                var selectedUser = cmbUsers.SelectedItem.ToString();
                var existingUsers = LoadUserData();
                var userToUnlock = existingUsers.FirstOrDefault(u => u.Username == selectedUser);

                if (userToUnlock != null)
                {
                    userToUnlock.IsLocked = false;
                    string json = JsonConvert.SerializeObject(existingUsers);
                    File.WriteAllText("users.json", json);

                    MessageBox.Show($"User '{selectedUser}' unlocked successfully!");
                    LoadUserComboBox();
                }
            }
            else
            {
                MessageBox.Show("Please select a user to unlock.");
            }
        }


        private void btnChangeAdminPassword_Click(object sender, RoutedEventArgs e)
        {
            // Get the current admin username (assuming it starts with "admin")
            string adminUsername = txtAdminUsername.Text;

            // Validate if admin is selected
            if (string.IsNullOrWhiteSpace(adminUsername) || !adminUsername.StartsWith("admin"))
            {
                MessageBox.Show("Invalid admin username.");
                return;
            }

            // Get the current admin password
            string currentPassword = txtCurrentAdminPassword.Password;

            // Validate if the current password is provided
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                MessageBox.Show("Please provide the current admin password.");
                return;
            }

            // Authenticate the admin based on the current password
            if (AuthenticateAdmin(adminUsername, currentPassword))
            {
                // Get the new admin password
                string newPassword = txtNewAdminPassword.Password;

                // Validate if the new password is provided
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("Please provide a new admin password.");
                    return;
                }

                // Change admin's password
                ChangeAdminPassword(adminUsername, newPassword);

                // Clear input fields
                txtCurrentAdminPassword.Clear();
                txtNewAdminPassword.Clear();

                MessageBox.Show("Admin password changed successfully!");
            }
            else
            {
                MessageBox.Show("Invalid current admin password.");
            }
        }

        private bool AuthenticateAdmin(string username, string password)
        {
            List<Admin> admins = LoadAdminData();

            // Find the admin by username
            var admin = admins.FirstOrDefault(a => a.Username == username);

            if (admin != null)
            {
                // Validate the password
                byte[] salt = Convert.FromBase64String(admin.Salt);
                string hashedPassword = HashPassword(password, salt);

                return hashedPassword == admin.PasswordHash;
            }

            return false;
        }

        private void ChangeAdminPassword(string username, string newPassword)
        {
            // Load existing admins
            var existingAdmins = LoadAdminData();

            // Find the admin to edit
            Admin adminToEdit = existingAdmins.FirstOrDefault(a => a.Username == username);

            if (adminToEdit != null)
            {
                // Generate a new random salt
                byte[] salt = GenerateSalt();

                // Update the admin's password and salt
                adminToEdit.PasswordHash = HashPassword(newPassword, salt);
                adminToEdit.Salt = Convert.ToBase64String(salt);

                // Save the updated list back to admin.json
                string json = JsonConvert.SerializeObject(existingAdmins);
                File.WriteAllText("admin.json", json);
            }
        }

        
        //-----------------------------------------------------------//
    

	}
}
