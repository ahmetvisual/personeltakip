using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace personelizintakip
{
    public partial class resmitatiller : Form
    {
        public resmitatiller()
        {
            InitializeComponent();
            LoadYearsToComboBox();
            LoadMonthsAndDaysToDataGrid();
            dataGridView1.CellClick += dataGridView1_CellClick;
            // Form açıldığında listBox'ı güncelle
            this.Load += Resmitatiller_Load;

            // Satır seçiciyi kaldır
            dataGridView1.RowHeadersVisible = false;
            // Form açıldığında kaydedilen tatil günlerini işaretle
            MarkSavedHolidayDays();
            // Yıl seçimi değiştiğinde tetiklenecek olay
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMonthsAndDaysToDataGrid();
            MarkSavedHolidayDays();
            LoadTatiller();
        }
        // Form yüklendiğinde listBox'ı güncelleyen metod
        private void Resmitatiller_Load(object sender, EventArgs e)
        {
            LoadTatiller(); // ListBox'ı güncelle
        }

        public void ClearHolidayMark(DateTime date)
        {
            int ayIndex = date.Month - 1;  // Ayın satırdaki konumu (0 bazlı)
            int gunIndex = date.Day;       // Günün sütundaki konumu

            DataGridViewCell cell = dataGridView1.Rows[ayIndex].Cells[gunIndex];
            cell.Style.BackColor = Color.White;
            cell.Value = gunIndex.ToString();
        }

        public void MarkSavedHolidayDays()
        {
            // Seçili yılı alalım
            int selectedYear = Convert.ToInt32(comboBox1.SelectedItem);

            // Ayların gün sayılarını belirleyelim
            int[] daysInMonth = { 31, DateTime.IsLeapYear(selectedYear) ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            // Tüm hücrelerin arka plan rengini ve değerlerini sıfırlayalım
            for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
            {
                DataGridViewRow row = dataGridView1.Rows[rowIndex];
                int daysInThisMonth = daysInMonth[rowIndex];

                for (int colIndex = 1; colIndex <= 31; colIndex++)
                {
                    DataGridViewCell cell = row.Cells[colIndex];
                    if (colIndex <= daysInThisMonth)
                    {
                        cell.Style.BackColor = Color.White;
                        cell.Value = colIndex.ToString();
                    }
                    else
                    {
                        cell.Value = null;
                        cell.Style.BackColor = Color.Gray; // Ayda olmayan günler gri renkte olabilir
                    }
                }
            }

            // Tatil günlerini yeniden işaretleyelim
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT Tarih, TamGunMu FROM tatiller WHERE YEAR(Tarih) = @Year";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Year", selectedYear);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime tarih = reader.GetDateTime("Tarih");
                            bool tamGun = reader.GetBoolean("TamGunMu");

                            int ayIndex = tarih.Month - 1;
                            int gunIndex = tarih.Day;

                            if (tamGun)
                            {
                                dataGridView1.Rows[ayIndex].Cells[gunIndex].Style.BackColor = Color.LightSkyBlue;
                            }
                            else
                            {
                                dataGridView1.Rows[ayIndex].Cells[gunIndex].Style.BackColor = Color.LightYellow;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        // Son 25 yılı ComboBox'a yükleyen metod
        private void LoadYearsToComboBox()
        {
            comboBox1.Items.Clear();
            int currentYear = DateTime.Now.Year;
            for (int i = 0; i < 25; i++)
            {
                comboBox1.Items.Add(currentYear - i);
            }
            comboBox1.SelectedItem = 2024; // 2024 varsayılan seçili yıl
        }

        // Ayları ve günleri DataGridView'e yükleyelim
        private void LoadMonthsAndDaysToDataGrid()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            string[] months = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
            int[] daysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            int selectedYear = Convert.ToInt32(comboBox1.SelectedItem);
           
            if (DateTime.IsLeapYear(selectedYear))
            {
                daysInMonth[1] = 29;
            }

            // Eğer yıl artık yıl ise Şubat 29 gün olacak
            if (DateTime.IsLeapYear(Convert.ToInt32(comboBox1.SelectedItem)))
            {
                daysInMonth[1] = 29;
            }

            // Sütun ve satır boyutlarını ayarlayalım
            int cellSize = 50; // Hücrelerin kare boyutu
            dataGridView1.RowTemplate.Height = cellSize; // Satır yüksekliği ayarlaması

            // DataGridView sütunlarını günler için oluşturuyoruz
            dataGridView1.Columns.Add("Month", "Ay");
            dataGridView1.Columns[0].Width = 80; // Ay isimlerinin olduğu sütun genişliği

            // Her gün için sabit genişlikte sütunlar oluşturalım
            for (int day = 1; day <= 31; day++)
            {
                dataGridView1.Columns.Add($"Day{day}", day.ToString());
                dataGridView1.Columns[day].Width = cellSize; // Her sütunun genişliği sabit (tam kare olacak)
            }

            // DataGridView satırlarına ay isimleri ve günleri ekleyelim
            for (int i = 0; i < months.Length; i++)
            {
                int rowIndex = dataGridView1.Rows.Add(months[i]);

                // Ay hücresini ortalamak ve mor renkte yazdırmak
                dataGridView1.Rows[rowIndex].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Rows[rowIndex].Cells[0].Style.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                dataGridView1.Rows[rowIndex].Cells[0].Style.ForeColor = Color.Purple; // Ay yazıları mor renkte olacak

                // Her ayın hücrelerini renklendirmek ve gün numaralarını eklemek için
                for (int day = 1; day <= daysInMonth[i]; day++)
                {
                    dataGridView1.Rows[rowIndex].Cells[day].Value = day.ToString(); // Gün numaraları
                    dataGridView1.Rows[rowIndex].Cells[day].Style.BackColor = Color.White; // Hücre arka plan rengi
                    dataGridView1.Rows[rowIndex].Cells[day].Style.Alignment = DataGridViewContentAlignment.MiddleCenter; // Gün ortalama
                }

                // Ay isimlerine arka plan renkleri verelim
                Color monthColor = GetMonthColor(i);
                dataGridView1.Rows[rowIndex].Cells[0].Style.BackColor = monthColor; // Ay ismi hücresinin arka plan rengi
            }

            // Sütun ve satırların yeniden boyutlandırılmasını engellemek için:
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // "Ay" başlığını gizlemek için sütun başlıklarını gizleyelim
            dataGridView1.ColumnHeadersVisible = false;
        }

        // Ayların farklı renklerle görünmesi için bir metod
        private Color GetMonthColor(int monthIndex)
        {
            Color[] monthColors = {
        Color.LightPink, Color.LightCoral, Color.LightSkyBlue, Color.LightGreen, Color.LightYellow,
        Color.LightSalmon, Color.LightBlue, Color.LightGoldenrodYellow, Color.LightSteelBlue,
        Color.Plum, Color.PeachPuff, Color.MistyRose
    };
            return monthColors[monthIndex % monthColors.Length];
        }

        // Hücreye tıklandığında form açıp izin bilgisi giriyoruz
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex > 0) // İlk sütun olan "Ay" hariç
            {
                int day = e.ColumnIndex; // Günü alıyoruz
                DateTime selectedDate = new DateTime(Convert.ToInt32(comboBox1.SelectedItem), e.RowIndex + 1, day);

                // Tatil detaylarını kontrol edelim, veritabanından yükleyelim
                string aciklama = string.Empty;
                bool tamGunMu = false;
                bool tatilKayitliMi = GetTatillerForDate(selectedDate, out aciklama, out tamGunMu); // Veritabanından bilgileri çekelim

                // Yeni izin formunu açalım ve ana form referansını gönderelim
                using (tatildetayi izinForm = new tatildetayi(this))  // Ana form referansını ilettik
                {
                    // Eğer tatil kaydı varsa bilgileri forma gönderelim
                    if (tatilKayitliMi)
                    {
                        izinForm.SecilenTarih = selectedDate;
                        izinForm.Aciklama = aciklama;
                        izinForm.IzinDurumu = tamGunMu ? "Tam Gün" : "Yarım Gün";
                    }
                    else
                    {
                        izinForm.SecilenTarih = selectedDate;
                    }

                    // Formu göster ve sonucu kontrol et
                    var result = izinForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // Tatil eklendi veya güncellendi
                        // Kaydetme işlemi zaten tatildetayi formunda yapıldı, tekrar çağırmaya gerek yok

                        // Tatil işaretlemelerini güncelle
                        MarkSavedHolidayDays();
                        // Listeyi güncelle
                        LoadTatiller();
                    }
                    else if (result == DialogResult.Abort) // Silme işlemi için kullanılan DialogResult değeri
                    {
                        // Tatil silindi
                        // Tatil işaretlemelerini güncelle
                        MarkSavedHolidayDays();
                        // Listeyi güncelle
                        LoadTatiller();
                    }
                    // Eğer kullanıcı iptal ettiyse (DialogResult.Cancel), hiçbir şey yapmaya gerek yok
                }
            }
        }

        // Tatil bilgilerini veritabanından alıyoruz
        private bool GetTatillerForDate(DateTime date, out string aciklama, out bool tamGunMu)
        {
            bool kayitVar = false;
            aciklama = string.Empty;
            tamGunMu = false;

            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT Aciklama, TamGunMu FROM tatiller WHERE Tarih = @Tarih";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Tarih", date);
                        MySqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            aciklama = reader.GetString("Aciklama");
                            tamGunMu = reader.GetBoolean("TamGunMu");
                            kayitVar = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }

            return kayitVar;
        }

        // Tatil bilgilerini veritabanına kaydediyoruz
        private void SaveToDatabase(DateTime date, string aciklama, bool tamGun)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "INSERT INTO tatiller (Tarih, Aciklama, TamGunMu) VALUES (@Tarih, @Aciklama, @TamGunMu) " +
                                   "ON DUPLICATE KEY UPDATE Aciklama = @Aciklama, TamGunMu = @TamGunMu";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Tarih", date);
                    cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                    cmd.Parameters.AddWithValue("@TamGunMu", tamGun ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.Message);
            }
        }
        public void LoadTatiller()
        {
            listBox1.Items.Clear();

            int selectedYear = Convert.ToInt32(comboBox1.SelectedItem);

            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    string query = "SELECT Tarih, Aciklama, TamGunMu FROM tatiller WHERE YEAR(Tarih) = @Year ORDER BY Tarih ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Year", selectedYear);
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            DateTime tarih = reader.GetDateTime("Tarih");
                            string aciklama = reader.GetString("Aciklama");
                            bool tamGunMu = reader.GetBoolean("TamGunMu");

                            string izinDurumu = tamGunMu ? "Tam Gün" : "Yarım Gün";
                            string listItem = $"{tarih:dd MMMM yyyy dddd} - {aciklama} ({izinDurumu})";

                            listBox1.Items.Add(listItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

    }
}
