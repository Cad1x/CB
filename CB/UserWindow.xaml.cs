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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CB
{
    /// <summary>
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private const string UsersFilePath = "users.json";

        private List<User> users;
        public UserWindow()
        {
            InitializeComponent();
            users = LoadUsers();
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
