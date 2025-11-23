using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ujikompbohadi
{
    public partial class Home : Form
    {
        private SqlCommand sqlCommand;
        private DataSet ds;
        private SqlDataAdapter adapter;
        private Connection connection;
        private string username;
        private int idUser;

        public Home(string nama, int idUser)
        {
            InitializeComponent();
            connection = new Connection();
            username = nama;
            this.idUser = idUser;
        }

        private void Home_Load(object sender, EventArgs e)
        {
            label1.Text = "Selamat Datang " + username;
            // Ambil koneksi dari class Connection
            SqlConnection sqlConnection = connection.GetConnection();

            try
            {
                sqlConnection.Open(); // Membuka koneksi ke database

                // Membuat perintah SQL untuk mengambil data dari tabel "items"
                string query = "SELECT p.id_produk, p.nama_produk, p.harga, p.stok, p.deskripsi, p.gambar_produk, p.tanggal_upload, u.nama AS pemilik FROM products p LEFT JOIN users u ON p.id_user = u.id_user";
                sqlCommand = new SqlCommand(query, sqlConnection);

                // Membuat DataSet dan DataAdapter untuk menampung dan mengisi data
                ds = new DataSet();
                adapter = new SqlDataAdapter(sqlCommand);

                // Menjalankan query dan mengisi DataSet
                adapter.Fill(ds, "products");

                // Tampilkan data ke dalam DataGridView
                dataGridView.DataSource = ds.Tables["products"];

                // Mengatur tampilan kolom DataGridView
                dataGridView.Columns["id_produk"].HeaderText = "ID";
                dataGridView.Columns["nama_produk"].HeaderText = "Nama Produk";
                dataGridView.Columns["harga"].HeaderText = "Harga";
                dataGridView.Columns["stok"].HeaderText = "Stok";
                dataGridView.Columns["deskripsi"].HeaderText = "Deskripsi";
                dataGridView.Columns["tanggal_upload"].HeaderText = "Tanggal Upload";
                dataGridView.Columns["pemilik"].HeaderText = "Pemilik";
                dataGridView.Columns["gambar_produk"].Visible = false; // Sembunyikan kolom gambar
            }
            catch (Exception ex)
            {
                // Tampilkan pesan jika terjadi error
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                sqlConnection.Close(); // Tutup koneksi di bagian finally agar selalu tertutup
            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Pastikan row dipilih
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Pilih item yang ingin diupdate.", "Peringatan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var row = dataGridView.SelectedRows[0];

                // Ambil id_produk dari row
                object idVal = row.Cells["id_produk"].Value;
                if (idVal == null || !int.TryParse(idVal.ToString(), out int idProduk) || idProduk <= 0)
                {
                    MessageBox.Show("ID tidak valid.", "Peringatan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Ambil input dari form dan validasi sederhana
                string nama = txbNamaProduk.Text.Trim();
                string hargaTxt = txbHarga.Text.Trim();
                string stokTxt = txbStok.Text.Trim();
                string deskripsi = txbDeskripsi.Text.Trim();
                string gambarInput = txbDirektori.Text.Trim(); // bisa nama file atau full path

                if (string.IsNullOrWhiteSpace(nama))
                {
                    MessageBox.Show("Nama produk harus diisi.", "Peringatan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(hargaTxt, out int harga) || harga < 0)
                {
                    MessageBox.Show("Harga harus berupa angka non-negatif.", "Peringatan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(stokTxt, out int stok) || stok < 0)
                {
                    MessageBox.Show("Stok harus berupa angka non-negatif.", "Peringatan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Jika user memberi path lengkap ke file gambar, salin ke folder Images aplikasi
                string gambarFileName = "";
                if (!string.IsNullOrEmpty(gambarInput))
                {
                    try
                    {
                        if (Path.IsPathRooted(gambarInput) && File.Exists(gambarInput))
                        {
                            string imagesFolder = Path.Combine(Application.StartupPath, "Images");
                            if (!Directory.Exists(imagesFolder))
                                Directory.CreateDirectory(imagesFolder);

                            gambarFileName = Path.GetFileName(gambarInput);
                            string destPath = Path.Combine(imagesFolder, gambarFileName);

                            // Jika file tujuan sudah ada, timpa atau bisa tambahkan unique name sesuai kebutuhan
                            File.Copy(gambarInput, destPath, true);
                        }
                        else
                        {
                            // Jika input bukan path penuh, anggap user sudah memasukkan nama file saja
                            gambarFileName = gambarInput;
                        }
                    }
                    catch (Exception exCopy)
                    {
                        MessageBox.Show("Gagal menyalin gambar: " + exCopy.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // Lanjutkan saja: masih bisa update field lain
                    }
                }

                // Query update — update gambar_produk dengan value (boleh kosong)
                using (SqlConnection sqlConnection = connection.GetConnection())
                {
                    sqlConnection.Open();

                    string updateQuery = @"
                UPDATE products
                SET nama_produk = @nama,
                    harga = @harga,
                    stok = @stok,
                    deskripsi = @deskripsi,
                    gambar_produk = @gambar
                WHERE id_produk = @id";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, sqlConnection))
                    {
                        cmd.Parameters.Add("@nama", SqlDbType.NVarChar, 200).Value = nama;
                        cmd.Parameters.Add("@harga", SqlDbType.Int).Value = harga;
                        cmd.Parameters.Add("@stok", SqlDbType.Int).Value = stok;
                        cmd.Parameters.Add("@deskripsi", SqlDbType.NVarChar, 1000).Value = deskripsi;
                        cmd.Parameters.Add("@gambar", SqlDbType.NVarChar, 500).Value = (object)gambarFileName ?? DBNull.Value;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduk;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("Data tidak ditemukan atau tidak ada perubahan.", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Data berhasil diupdate.", "Informasi",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

                // Refresh data di UI
                Home_Load(sender, e);

                // (Opsional) Kosongkan/refresh input
                txbNamaProduk.Clear();
                txbHarga.Clear();
                txbStok.Clear();
                txbDirektori.Clear();
                txbDeskripsi.Clear();
                pictureBox1.Image = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat update: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnCreate_Click(object sender, EventArgs e)
        {
            // Mengambil input dari form
            string namaproduk = txbNamaProduk.Text;
            string harga = txbHarga.Text;
            string stok = txbStok.Text;
            string deskripsi = txbDeskripsi.Text;
            string imagePath = txbDirektori.Text;

            // Validasi nama produk
            if (string.IsNullOrWhiteSpace(namaproduk))
            {
                MessageBox.Show("Nama Produk tidak boleh kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            // Validasi deskripsi
            if (string.IsNullOrWhiteSpace(deskripsi))
            {
                MessageBox.Show("Deskripsi tidak boleh kosong.", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            // Validasi harga
            if (string.IsNullOrWhiteSpace(harga) || !int.TryParse(harga, out int hargaValue) || hargaValue <= 0)
            {
                MessageBox.Show("Harga harus berupa angka yang valid dan lebih besar dari 0.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validasi stok
            if (string.IsNullOrWhiteSpace(stok) || !int.TryParse(stok, out int stokValue) || stokValue < 0)
            {
                MessageBox.Show("Stok harus berupa angka yang valid dan tidak boleh kurang dari 0.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validasi input Gambar
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                MessageBox.Show("Path gambar tidak valid atau gambar belum dipilih.", "Peringatan",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Penentuan folder tujuan untuk gambar
            string imageFolder = Path.Combine(Application.StartupPath, "gambar_produk");

            // Membuat folder Images jika belum ada
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }

            // Menyalin gambar ke folder Images
            string fileName = Path.GetFileName(imagePath);
            string targetFilePath = Path.Combine(imageFolder, fileName);

            File.Copy(imagePath, targetFilePath, true);

            try
            {
                SqlConnection sqlConnection = connection.GetConnection();
                sqlConnection.Open();

                string insertQuery = @"
                    INSERT INTO products 
                        (id_user, nama_produk, harga, stok, deskripsi, gambar_produk, tanggal_upload)
                    VALUES 
                        (@id_user, @nama_produk, @harga, @stok, @deskripsi, @gambar_produk, GETDATE())";

                SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection);

                insertCommand.Parameters.AddWithValue("@id_user", idUser);
                insertCommand.Parameters.AddWithValue("@nama_produk", namaproduk);
                insertCommand.Parameters.AddWithValue("@harga", hargaValue);
                insertCommand.Parameters.AddWithValue("@stok", stokValue);
                insertCommand.Parameters.AddWithValue("@deskripsi", deskripsi);
                insertCommand.Parameters.AddWithValue("@gambar_produk", fileName);

                insertCommand.ExecuteNonQuery();

                MessageBox.Show("Data berhasil ditambahkan!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                sqlConnection.Close();

                Home_Load(sender, e);
                txbNamaProduk.Clear();
                txbHarga.Clear();
                txbStok.Clear();
                txbDeskripsi.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        private void txbHarga_TextChanged(object sender, EventArgs e)
        {

        }

        private void txbDeskripsi_TextChanged(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            // Membuka dialog untuk memilih file gambar
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Simpan path file ke TextBox
                txbDirektori.Text = openFileDialog.FileName;

                // Tampilkan gambar ke PictureBox
                pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
            }

        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Pastikan baris yang diklik valid (bukan header)
            if (e.RowIndex >= 0)
            {
                // Ambil baris yang diklik
                DataGridViewRow selectedRow = dataGridView.Rows[e.RowIndex];

                // Tampilkan data ke TextBox
                txbNamaProduk.Text = selectedRow.Cells["nama_produk"].Value.ToString();
                txbHarga.Text = selectedRow.Cells["harga"].Value.ToString();
                txbStok.Text = selectedRow.Cells["stok"].Value.ToString();
                txbDeskripsi.Text = selectedRow.Cells["deskripsi"].Value.ToString();



                // Ambil nama file gambar
                string imageFileName = selectedRow.Cells["gambar_produk"].Value.ToString();
                string imagesFolder = Path.Combine(Application.StartupPath, "gambar_produk");
                string imagePath = Path.Combine(imagesFolder, imageFileName);

                // Tampilkan nama file di TextBox
                txbDirektori.Text = imageFileName;

                // Tampilkan gambar jika file ada
                if (File.Exists(imagePath))
                {
                    pictureBox1.Image = Image.FromFile(imagePath);
                }
                else
                {
                    pictureBox1.Image = null; // Kosongkan PictureBox jika gambar tidak ditemukan
                }
            }

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Pilih item yang ingin dihapus.", "Peringatan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var row = dataGridView.SelectedRows[0];

                // ================= FIX PALING PENTING =================
                object val = row.Cells["id_produk"].Value;
                if (val == null || !int.TryParse(val.ToString(), out int itemId))
                {
                    MessageBox.Show("ID tidak valid.", "Peringatan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // ======================================================

                if (MessageBox.Show("Apakah Anda yakin ingin menghapus item ini?", "Konfirmasi",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                using (SqlConnection sqlConnection = connection.GetConnection())
                {
                    sqlConnection.Open();

                    string deleteQuery = "DELETE FROM products WHERE id_produk = @id";

                    using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, sqlConnection))
                    {
                        deleteCommand.Parameters.Add("@id", SqlDbType.Int).Value = itemId;
                        deleteCommand.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Item berhasil dihapus.", "Informasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh UI
                Home_Load(sender, e);
                txbNamaProduk.Clear();
                txbHarga.Clear();
                txbStok.Clear();
                txbDirektori.Clear();
                txbDeskripsi.Clear();
                pictureBox1.Image = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            
        }

        private void txbSearch_TextChanged(object sender, EventArgs e)
        {

        }
    }
    }
}
