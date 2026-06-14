using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace personelizintakip
{
    public partial class izinbelgelist : Form
    {
        public izinbelgelist()
        {
            InitializeComponent();
            this.Load += new EventHandler(izinbelgelist_Load);
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged; // Yıl seçimi değişikliği olayı
            textBox1.TextChanged += TextBox1_TextChanged; // Arama yapıldığında tetiklenir
        }

        private void izinbelgelist_Load(object sender, EventArgs e)
        {
            LoadYearsToComboBox(); // Son 25 yılı combobox'a ekle
            LoadIzinListesi(); // İzin listesini yükle
            CustomizeDataGridView(dataGridView2); // DataGridView'i özelleştir
        }

        private void LoadYearsToComboBox()
        {
            int currentYear = DateTime.Now.Year;
            comboBox1.Items.Add("Yıl Seçebilirsiniz"); // Başlangıçta seçilmemiş
            comboBox1.SelectedIndex = 0;

            // Son 25 yılı ekle
            for (int i = 0; i < 25; i++)
            {
                comboBox1.Items.Add(currentYear - i);
            }
        }

        private void LoadIzinListesi()
        {
            string yearFilter = comboBox1.SelectedItem?.ToString() != "Yıl Seçebilirsiniz" ? comboBox1.SelectedItem.ToString() : null;
            string nameFilter = textBox1.Text.Trim();

            // SQL sorgusu
            string query = @"
                SELECT 
                    ki.ID,
                    CONCAT(pb.Adi, ' ', pb.Soyadi) AS Personel,
                    ki.BaslangicTarihi AS `Başlangıç Tarihi`,
                    ki.BitisTarihi AS `Bitiş Tarihi`,
                    ki.BelgeNo AS `Belge NO`,
                    ki.Aciklama AS İzahat,
                    ki.izinturu AS `İzin Türü`
                FROM kullanilanizinler ki
                INNER JOIN personelbilgileri pb ON ki.PersonelID = pb.ID
                WHERE (@nameFilter = '' OR pb.Adi LIKE CONCAT(@nameFilter, '%') 
                OR pb.Soyadi LIKE CONCAT('% ', @nameFilter))
                AND (@yearFilter IS NULL OR YEAR(ki.BaslangicTarihi) = @yearFilter OR YEAR(ki.BitisTarihi) = @yearFilter)";

            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@nameFilter", nameFilter);
                    cmd.Parameters.AddWithValue("@yearFilter", yearFilter);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        dataGridView2.DataSource = dt;
                        dataGridView2.AutoGenerateColumns = true;
                        dataGridView2.AllowUserToAddRows = false;
                        dataGridView2.ReadOnly = true;

                        dataGridView2.Columns["Personel"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    else
                    {
                        dataGridView2.DataSource = null;
                        MessageBox.Show("Veri bulunamadı.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadIzinListesi(); // Yıl seçildiğinde listeyi yenile
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            LoadIzinListesi(); // İsim araması yapıldığında listeyi yenile
        }

        private void CustomizeDataGridView(DataGridView gridView)
        {
            gridView.BorderStyle = BorderStyle.None;
            gridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridView.RowHeadersVisible = false;

            gridView.Font = new Font("Arial", 11, FontStyle.Regular);
            gridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 176, 255);
            gridView.DefaultCellStyle.SelectionForeColor = Color.White;

            gridView.RowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            gridView.AlternatingRowsDefaultCellStyle.BackColor = Color.White;

            gridView.RowTemplate.Height = 30;

            gridView.EnableHeadersVisualStyles = false;
            gridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(192, 192, 192);
            gridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            gridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            gridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            gridView.GridColor = Color.WhiteSmoke;
            gridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridView.MultiSelect = false;

            gridView.CellPainting += dataGridView_CellPainting;
        }

        private void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                e.PaintBackground(e.ClipBounds, true);

                using (Brush topBrush = new SolidBrush(Color.FromArgb(192, 192, 192)))
                {
                    e.Graphics.FillRectangle(topBrush, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width, e.CellBounds.Height / 2);
                }

                using (Brush bottomBrush = new SolidBrush(Color.FromArgb(160, 160, 160)))
                {
                    e.Graphics.FillRectangle(bottomBrush, e.CellBounds.Left, e.CellBounds.Top + e.CellBounds.Height / 2, e.CellBounds.Width, e.CellBounds.Height / 2);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                using (Pen pen = new Pen(Color.FromArgb(242, 242, 242), 0.5f))
                {
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Top);
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Left, e.CellBounds.Bottom);
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
                }

                e.Handled = true;
            }
        }
    }
}
