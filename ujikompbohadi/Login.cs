using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ujikompbohadi
{
    public partial class Login : Form
    {
        private Connection connection;

        public Login()
        {
            InitializeComponent();
            connection = new Connection();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = tctbUsername.Text.Trim();
            string password = txbPassword.Text.Trim();
            SqlConnection sqlConnection = connection.GetConnection();


            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username dan Password tidak boleh kosong!");
                return;
            }

            using (sqlConnection)
            {
                sqlConnection.Open();
                string query = "SELECT * FROM users WHERE username = @nama AND password = @password";
                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@nama", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string nama = reader["username"].ToString();
                        int idUser = Convert.ToInt32(reader["id_user"]);
                        Home f2 = new Home(nama, idUser);
                        f2.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Username atau Password salah!");
                        tctbUsername.Clear();
                        txbPassword.Focus();
                    }
                }
            }
        }
    }
}
