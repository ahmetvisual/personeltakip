using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Linq;
using ExcelDataReader;
using System.IO;

namespace personelizintakip
{
    public partial class izinbelgesi : Form
    {
        private int selectedPersonelID;
        private int? izinID; // Nullable int
        // Tatil günleri ve yarım gün tatiller için HashSet'ler
        private HashSet<DateTime> tatilGünleri;
        private HashSet<DateTime> yarimTatilGünleri;

        public izinbelgesi(int personelID, int? izinID = null)
        {
            InitializeComponent();
            selectedPersonelID = personelID;
            this.izinID = izinID;
            this.Load += new EventHandler(izinbelgesi_Load);
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox3.SelectedIndexChanged += comboBox3_SelectedIndexChanged;

        }
        // Genel başlangıç ve bitiş tarihleri
        private DateTime overallStartDate;
        private DateTime overallEndDate;

        // Gün işaretlemelerini saklamak için
        private Dictionary<DateTime, DayCellMarking> dayMarkings = new Dictionary<DateTime, DayCellMarking>();

        // Başlangıçta toplam izin gün sayısı (Pazar günleri hariç)
        private double totalAvailableDays = 0.0;

        // Kullanılan gün sayısı ve kalan gün sayısı
        private double usedDays = 0.0;
        private double remainingDays = 0.0;

        // DayCellMarking sınıfı
        public class DayCellMarking
        {
            public bool IsLeftMarked { get; set; }
            public bool IsRightMarked { get; set; }
        }

        private bool isInitializing = true; // Başlangıçta true

        private void LoadIzinYearsComboBox()
        {
            if (comboBox1.SelectedItem == null)
                return;

            int personelID = ((ComboBoxItem)comboBox1.SelectedItem).Value;
            comboBox3.Items.Clear();

            using (MySqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                string query = "SELECT YEAR(BaslangicTarihi) AS Yil FROM izintablolari WHERE PersonelID = @PersonelID GROUP BY Yil ORDER BY Yil";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PersonelID", personelID);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comboBox3.Items.Add(reader.GetInt32("Yil"));
                }
            }

            if (comboBox3.Items.Count > 0)
            {
                comboBox3.SelectedIndex = 0; // İsterseniz ilk yılı seçili yapabilirsiniz
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadIzinYearsComboBox();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedItem != null)
            {
                int selectedYear = (int)comboBox3.SelectedItem;
                LoadTatilGünleri(selectedYear);
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Sadece rakamlara ve kontrol tuşlarına izin ver
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void LoadTatilGünleri(int yil)
        {
            tatilGünleri = new HashSet<DateTime>();
            yarimTatilGünleri = new HashSet<DateTime>();

            using (MySqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                string query = "SELECT Tarih, TamGunMu FROM tatiller WHERE YEAR(Tarih) = @Yil";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Yil", yil);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime tarih = reader.GetDateTime("Tarih").Date;
                        bool tamGunMu = reader.GetInt32("TamGunMu") == 1;

                        if (tamGunMu)
                        {
                            tatilGünleri.Add(tarih);
                        }
                        else
                        {
                            yarimTatilGünleri.Add(tarih);
                        }
                    }
                }
            }
        }
        private void GenerateRandomIzinDates()
        {
            if (comboBox3.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir yıl seçiniz.");
                return;
            }

            if (!int.TryParse(textBox2.Text, out int izinGunSayisi) || izinGunSayisi <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir izin gün sayısı giriniz.");
                return;
            }

            int selectedYear = (int)comboBox3.SelectedItem;
            DateTime yearStart = new DateTime(selectedYear, 1, 1);
            DateTime yearEnd = new DateTime(selectedYear, 12, 31);

            // Tatil günlerini yükleyelim
            LoadTatilGünleri(selectedYear);

            Random rnd = new Random();
            bool izinBulundu = false;
            DateTime baslangicTarihi = DateTime.MinValue;
            DateTime bitisTarihi = DateTime.MinValue;

            int maxAttempts = 1000;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Rastgele bir gün seç
                int randomDayOffset = rnd.Next(0, (yearEnd - yearStart).Days + 1);
                DateTime startDate = yearStart.AddDays(randomDayOffset);

                // Pazar günü veya tam gün tatil ise devam et
                if (startDate.DayOfWeek == DayOfWeek.Sunday || tatilGünleri.Contains(startDate))
                    continue;

                // Bitiş tarihini hesapla
                DateTime endDate = CalculateEndDate(startDate, izinGunSayisi);

                // Bitiş tarihinin yıl sonunu aşmadığını kontrol edelim
                if (endDate > yearEnd)
                    continue;

                izinBulundu = true;
                baslangicTarihi = startDate;
                bitisTarihi = endDate;
                break;
            }

            if (izinBulundu)
            {
                // Tarih seçicilere ayarlıyoruz
                dateTimePicker4.Value = baslangicTarihi;
                dateTimePicker5.Value = bitisTarihi.AddDays(1); // Bitiş tarihini dahil etmiyoruz, o yüzden bir gün ekliyoruz

                // DateTimePicker_ValueChanged metodunu çağırıyoruz
                DateTimePicker_ValueChanged(null, null);

                // Kullanıcıya bilgi veriyoruz
                MessageBox.Show($"İzin tarihleri oluşturuldu: {baslangicTarihi.ToShortDateString()} - {bitisTarihi.ToShortDateString()}\nOnaylamak için 'Kaydet' butonuna basınız.");
            }
            else
            {
                MessageBox.Show("Belirtilen kriterlere uygun izin tarihi bulunamadı.");
            }
        }


        private DateTime CalculateEndDate(DateTime startDate, double izinGunSayisi)
        {
            DateTime currentDate = startDate;
            double workingDaysCount = 0.0;

            while (workingDaysCount < izinGunSayisi)
            {
                bool isSunday = currentDate.DayOfWeek == DayOfWeek.Sunday;
                bool isFullHoliday = tatilGünleri.Contains(currentDate);
                bool isHalfHoliday = yarimTatilGünleri.Contains(currentDate);

                // Çalışma günlerini sayarken, pazar günleri ve tam gün tatilleri atlıyoruz
                if (!isSunday && !isFullHoliday)
                {
                    if (isHalfHoliday)
                    {
                        workingDaysCount += 0.5;
                    }
                    else
                    {
                        workingDaysCount += 1.0;
                    }
                }

                // Sonraki güne geçiyoruz
                currentDate = currentDate.AddDays(1);
            }

            // Bitiş tarihi, izin günlerini tamamladığımız günün bir önceki günüdür (bitiş tarihini dahil etmiyoruz)
            return currentDate.AddDays(-1);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            GenerateRandomIzinDates();
        }
        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            if (isInitializing)
                return;
            // Eğer başlangıç tarihi bitiş tarihinden sonraysa bitiş tarihini ayarla
            if (dateTimePicker4.Value > dateTimePicker5.Value)
            {
                dateTimePicker5.Value = dateTimePicker4.Value;
                MessageBox.Show("Başlangıç tarihi, bitiş tarihinden önce olmalıdır!", "Geçersiz Tarih Seçimi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dateTimePicker5_ValueChanged(object sender, EventArgs e)
        {
            if (isInitializing)
                return;
            // Eğer bitiş tarihi başlangıç tarihinden önceyse başlangıç tarihini ayarla
            if (dateTimePicker5.Value < dateTimePicker4.Value)
            {
                dateTimePicker4.Value = dateTimePicker5.Value;
                MessageBox.Show("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır!", "Geçersiz Tarih Seçimi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Form yüklendiğinde
        private void izinbelgesi_Load(object sender, EventArgs e)
        {
            // Personel listesini yükleyelim
            LoadPersonelComboBox();
            SelectPersonelInComboBox();
            LoadIzinYearsComboBox();
            // Form yüklendiğinde seçili yıl varsa tatil günlerini yükleyin
            if (comboBox3.SelectedItem != null)
            {
                int selectedYear = (int)comboBox3.SelectedItem;
                LoadTatilGünleri(selectedYear);
            }
            // DataGridView'in sütunlarını ayarlayalım
            dataGridView1.Columns.Clear(); // Mevcut sütunları temizleyelim

            // Sütunları oluşturalım ve özelliklerini ayarlayalım
            DataGridViewColumn colHafta = new DataGridViewTextBoxColumn
            {
                Name = "Hafta",
                HeaderText = "Hafta",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font(dataGridView1.DefaultCellStyle.Font, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            };
            dataGridView1.Columns.Add(colHafta);

            DataGridViewColumn colBaslangic = new DataGridViewTextBoxColumn
            {
                Name = "HaftaBaşlangıcı",
                HeaderText = "Hafta Başlangıcı",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 50,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            };
            dataGridView1.Columns.Add(colBaslangic);

            DataGridViewColumn colSonu = new DataGridViewTextBoxColumn
            {
                Name = "HaftaSonu",
                HeaderText = "Hafta Sonu",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 50,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            };
            dataGridView1.Columns.Add(colSonu);

            // Kullanıcının yeni satır eklemesini engelle
            dataGridView1.AllowUserToAddRows = false;

            // DataGridView'in satır başlıklarını gösterelim
            dataGridView1.RowHeadersVisible = true;

            // DataGridView SelectionChanged olayını ekleyelim
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;

            // Tarih seçicilerin olaylarına abone ol
            dateTimePicker4.ValueChanged += dateTimePicker4_ValueChanged;
            dateTimePicker5.ValueChanged += dateTimePicker5_ValueChanged;

            // DateTimePicker olaylarını ekleyelim
            dateTimePicker4.ValueChanged += DateTimePicker_ValueChanged;
            dateTimePicker5.ValueChanged += DateTimePicker_ValueChanged;

            // Eğer izinID varsa, güncelleme moduna geç
            if (izinID.HasValue)
            {
                // İzin verilerini yükle
                LoadIzinData(izinID.Value);

                // button4'ü gizle, button16'yı göster
                button4.Visible = false;
                button16.Visible = true;
            }
            else
            {
                // Yeni izin ekleme modu
                button4.Visible = true;
                button16.Visible = false;

                // Başlangıç ve bitiş tarihlerini bugünün tarihi olarak ayarlayalım
                dateTimePicker4.Value = DateTime.Today;
                dateTimePicker5.Value = DateTime.Today;

                // Tarih aralığına göre haftaları dolduralım
                DateTimePicker_ValueChanged(null, null);
            }

            isInitializing = false; // Form yüklemesi tamamlandı
        }

        private void LoadIzinData(int izinID)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = @"SELECT * FROM kullanilanizinler WHERE ID = @IzinID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@IzinID", izinID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dateTimePicker4.Value = Convert.ToDateTime(reader["BaslangicTarihi"]);
                            dateTimePicker5.Value = Convert.ToDateTime(reader["BitisTarihi"]);
                            textBox1.Text = reader["BelgeNo"].ToString();
                            richTextBox1.Text = reader["Aciklama"].ToString();

                            // İzin türünü ayarla
                            string izinturu = reader["izinturu"].ToString();
                            if (!string.IsNullOrEmpty(izinturu))
                            {
                                comboBox2.SelectedItem = izinturu;
                            }
                        }
                    }

                    // İzin günlerini yükle
                    LoadDayMarkings(izinID);

                    // Haftaları ve günleri güncelle
                    DateTimePicker_ValueChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzin verileri yüklenirken hata oluştu: " + ex.Message);
            }
        }
        private void UpdateIzinTablolari(MySqlConnection con, MySqlTransaction transaction, int personelID,
                                  DateTime baslangicTarihi, DateTime bitisTarihi, decimal kalanGun, bool isAdding)
        {
            string izinTabloQuery = @"SELECT ID, BaslangicTarihi, BitisTarihi, KulHak, Devreden, DevirAlinan, YillikHak 
                              FROM izintablolari 
                              WHERE PersonelID = @PersonelID 
                              ORDER BY BaslangicTarihi";
            MySqlCommand izinTabloCmd = new MySqlCommand(izinTabloQuery, con, transaction);
            izinTabloCmd.Parameters.AddWithValue("@PersonelID", personelID);

            List<IzinTabloRecord> izinTabloList = new List<IzinTabloRecord>();

            using (MySqlDataReader reader = izinTabloCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    IzinTabloRecord record = new IzinTabloRecord
                    {
                        ID = reader.GetInt32("ID"),
                        BaslangicTarihi = reader.GetDateTime("BaslangicTarihi").Date,
                        BitisTarihi = reader.GetDateTime("BitisTarihi").Date,
                        KulHak = reader.GetDecimal("KulHak"),
                        Devreden = reader.GetDecimal("Devreden"),
                        DevirAlinan = reader.GetDecimal("DevirAlinan"),
                        YillikHak = reader.GetDecimal("YillikHak")
                    };
                    izinTabloList.Add(record);
                }
            }

            // Tüm yılların güncellenmesi
            for (int i = 0; i < izinTabloList.Count; i++)
            {
                var currentRecord = izinTabloList[i];

                // Eğer bugünden sonraki yıl ise YıllıkHak 0 olmalı
                if (currentRecord.BaslangicTarihi > DateTime.Today && currentRecord.YillikHak != 0)
                {
                    string updateYillikHakQuery = @"UPDATE izintablolari SET YillikHak = 0 WHERE ID = @ID";
                    MySqlCommand updateYillikHakCmd = new MySqlCommand(updateYillikHakQuery, con, transaction);
                    updateYillikHakCmd.Parameters.AddWithValue("@ID", currentRecord.ID);
                    updateYillikHakCmd.ExecuteNonQuery();
                    currentRecord.YillikHak = 0;
                }

                // Eğer Başlangıç Tarihi Kıdem'in tarih aralığı içindeyse, KulHak değerini güncelle
                if (baslangicTarihi >= currentRecord.BaslangicTarihi && baslangicTarihi < currentRecord.BitisTarihi)
                {
                    if (isAdding)
                    {
                        // Ekleme işlemi: Kullanılan izin günlerini ekle
                        currentRecord.KulHak += kalanGun;
                    }
                    else
                    {
                        // İptal işlemi: Kullanılan izin günlerini geri al
                        currentRecord.KulHak -= kalanGun;
                    }
                }

                // Devreden hesaplama: (DevirAlinan + YillikHak) - KulHak
                currentRecord.Devreden = (currentRecord.DevirAlinan + currentRecord.YillikHak) - currentRecord.KulHak;

                // Bir sonraki yılın DevirAlinan değerini güncelle
                if (i < izinTabloList.Count - 1)
                {
                    izinTabloList[i + 1].DevirAlinan = currentRecord.Devreden;
                }

                // Güncellenmiş veriyi SQL'e yaz
                string updateQuery = @"UPDATE izintablolari 
                               SET KulHak = @NewKulHak, Devreden = @NewDevreden, DevirAlinan = @NewDevirAlinan, YillikHak = @NewYillikHak 
                               WHERE ID = @ID";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, con, transaction);
                updateCmd.Parameters.AddWithValue("@NewKulHak", currentRecord.KulHak);
                updateCmd.Parameters.AddWithValue("@NewDevreden", currentRecord.Devreden);
                updateCmd.Parameters.AddWithValue("@NewDevirAlinan", currentRecord.DevirAlinan);
                updateCmd.Parameters.AddWithValue("@NewYillikHak", currentRecord.YillikHak);
                updateCmd.Parameters.AddWithValue("@ID", currentRecord.ID);

                updateCmd.ExecuteNonQuery();
            }
        }

        private void LoadDayMarkings(int izinID)
        {
            dayMarkings.Clear();

            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT Tarih, IsLeftMarked, IsRightMarked FROM izin_gunleri WHERE IzinID = @IzinID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@IzinID", izinID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime tarih = reader.GetDateTime("Tarih");
                            bool isLeftMarked = reader.GetBoolean("IsLeftMarked");
                            bool isRightMarked = reader.GetBoolean("IsRightMarked");

                            dayMarkings[tarih] = new DayCellMarking
                            {
                                IsLeftMarked = isLeftMarked,
                                IsRightMarked = isRightMarked
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzin günleri yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void SelectPersonelInComboBox()
        {
            foreach (ComboBoxItem item in comboBox1.Items)
            {
                if (item.Value == selectedPersonelID)
                {
                    comboBox1.SelectedItem = item;
                    break;
                }
            }
        }

        // ComboBoxItem sınıfı
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        // Personel listesini yükleme
        private void LoadPersonelComboBox()
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT ID, CONCAT(Adi, ' ', Soyadi, ' - ', SicilNo) AS PersonelInfo FROM personelbilgileri ORDER BY Adi ASC";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Personel listesini doldur
                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Text = reader["PersonelInfo"].ToString(),
                            Value = Convert.ToInt32(reader["ID"])
                        };
                        comboBox1.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Personel bilgileri yüklenirken hata oluştu: " + ex.Message);
            }
        }

        // DateTimePicker değerleri değiştiğinde
        private void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            overallStartDate = dateTimePicker4.Value.Date;
            overallEndDate = dateTimePicker5.Value.Date;

            // Başlangıç ve bitiş tarihleri farklı yıllardaysa, her yıl için tatil günlerini yükleyin
            int startYear = overallStartDate.Year;
            int endYear = overallEndDate.Year;
            for (int yil = startYear; yil <= endYear; yil++)
            {
                LoadTatilGünleri(yil);
            }

            // Haftaları DataGridView'e ekleyelim
            FillWeeksInDataGridView(overallStartDate, overallEndDate);

            // İlk haftayı seçelim
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.Rows[0].Selected = true;
            }

            // Başlangıçta toplam izin gün sayısını hesaplayalım
            totalAvailableDays = CalculateTotalAvailableDays();

            // Kalan izin süresini güncelle
            HesaplaIzinSuresi();
        }


        // Başlangıçta toplam izin gün sayısını hesaplama (Pazar günleri hariç)
        private double CalculateTotalAvailableDays()
        {
            double totalDays = 0.0;
            for (DateTime dt = overallStartDate; dt < overallEndDate; dt = dt.AddDays(1))
            {
                if (dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    // Pazar günleri hariç tutulur
                    continue;
                }

                bool isFullHoliday = tatilGünleri.Contains(dt);
                bool isHalfHoliday = yarimTatilGünleri.Contains(dt);

                // Tam gün tatiller hariç tutulur
                if (isFullHoliday)
                {
                    continue;
                }

                // Yarım gün tatiller için 0.5 gün eklenir
                if (isHalfHoliday)
                {
                    totalDays += 0.5;
                }
                else
                {
                    // Diğer günler için 1 gün eklenir
                    totalDays += 1.0;
                }
            }
            return totalDays;
        }

        // Haftaları DataGridView'e doldurma
        private void FillWeeksInDataGridView(DateTime startDate, DateTime endDate)
        {
            dataGridView1.SuspendLayout();
            dataGridView1.Rows.Clear();

            DateTime currentDate = startDate;
            int weekNumber = 1;

            while (currentDate < endDate)
            {
                DateTime weekStartDate = currentDate;
                DateTime weekEndDate = currentDate.AddDays(6);

                if (weekEndDate >= endDate)
                    weekEndDate = endDate.AddDays(-1); // Bitiş tarihini dahil etmiyoruz

                string weekLabel = $"{weekNumber}. Hafta";
                dataGridView1.Rows.Add(weekLabel, weekStartDate.ToString("dd.MM.yyyy"), weekEndDate.ToString("dd.MM.yyyy"));

                currentDate = currentDate.AddDays(7);
                weekNumber++;
            }
            dataGridView1.ResumeLayout();
        }

        // DataGridView'de haftalar değiştiğinde
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Seçilen haftanın başlangıç ve bitiş tarihlerini al
                string selectedStartDate = dataGridView1.SelectedRows[0].Cells["HaftaBaşlangıcı"].Value.ToString();
                string selectedEndDate = dataGridView1.SelectedRows[0].Cells["HaftaSonu"].Value.ToString();

                DateTime startDate = DateTime.ParseExact(selectedStartDate, "dd.MM.yyyy", null).Date;
                DateTime endDate = DateTime.ParseExact(selectedEndDate, "dd.MM.yyyy", null).Date.AddDays(1); // Bitiş tarihini dahil etmek için bir gün ekliyoruz

                // TableLayoutPanel'i seçilen haftaya göre güncelle
                FillTableLayoutPanel(startDate, endDate);
            }
        }

        // TableLayoutPanel'i doldurma
        private void FillTableLayoutPanel(DateTime startDate, DateTime endDate)
        {
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Padding = new Padding(0);
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel1.ColumnCount = 7;
            tableLayoutPanel1.RowCount = 2;

            for (int i = 0; i < 7; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));
            }

            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F));

            DateTime currentDate = startDate;
            for (int col = 0; col < 7; col++)
            {
                Label dayNameLabel = new Label
                {
                    Text = currentDate.ToString("dddd"),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };
                tableLayoutPanel1.Controls.Add(dayNameLabel, col, 0);

                DayCellControl dayCell = new DayCellControl
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    Date = currentDate
                };

                if (currentDate >= endDate || currentDate < overallStartDate)
                {
                    dayCell.Enabled = false;
                    dayCell.Visible = false;
                }
                else
                {
                    // Tatil kontrolünü bellekte yapıyoruz
                    bool isFullHoliday = tatilGünleri.Contains(currentDate);
                    bool isHalfHoliday = yarimTatilGünleri.Contains(currentDate);

                    // DayCellControl'ün tatil ve gün özelliklerini ayarla
                    dayCell.IsHoliday = isFullHoliday || isHalfHoliday;
                    dayCell.IsFullDayHoliday = isFullHoliday;
                    dayCell.IsHalfDayHoliday = isHalfHoliday;

                    // Pazar günlerini kontrol et
                    dayCell.IsSunday = currentDate.DayOfWeek == DayOfWeek.Sunday;

                    // İşaretlemeleri ayarla
                    if (dayMarkings.ContainsKey(currentDate))
                    {
                        dayCell.IsLeftMarked = dayMarkings[currentDate].IsLeftMarked;
                        dayCell.IsRightMarked = dayMarkings[currentDate].IsRightMarked;
                    }

                    dayCell.MarkingChanged += DayCell_MarkingChanged;
                }

                tableLayoutPanel1.Controls.Add(dayCell, col, 1);
                currentDate = currentDate.AddDays(1);
            }
            tableLayoutPanel1.ResumeLayout();
        }

        // DayCellControl'de işaretlemeler değiştiğinde
        private void DayCell_MarkingChanged(object sender, EventArgs e)
        {
            if (sender is DayCellControl dayCell)
            {
                DateTime date = dayCell.Date;

                if (dayMarkings.ContainsKey(date))
                {
                    dayMarkings[date].IsLeftMarked = dayCell.IsLeftMarked;
                    dayMarkings[date].IsRightMarked = dayCell.IsRightMarked;
                }
                else
                {
                    dayMarkings[date] = new DayCellMarking
                    {
                        IsLeftMarked = dayCell.IsLeftMarked,
                        IsRightMarked = dayCell.IsRightMarked
                    };
                }

                // Kalan izin süresini güncelle
                HesaplaIzinSuresi();
            }
        }

        // Kalan izin süresini hesaplama
        private void HesaplaIzinSuresi()
        {
            usedDays = 0.0;

            foreach (var entry in dayMarkings)
            {
                DateTime dt = entry.Key;
                if (dt < overallStartDate || dt >= overallEndDate)
                    continue;

                if (dt.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                DayCellMarking marking = entry.Value;

                int markedHalves = 0;
                if (marking.IsLeftMarked)
                    markedHalves++;
                if (marking.IsRightMarked)
                    markedHalves++;

                usedDays += 0.5 * markedHalves;
            }
            remainingDays = totalAvailableDays - usedDays;

            // Kalan gün sayısını label5'e yazdır
            label5.Text = $"Kalan : {remainingDays} gün";
        }

        private bool ValidateIzinKaydi()
        {
            if (dateTimePicker5.Value.Date <= dateTimePicker4.Value.Date)
            {
                MessageBox.Show("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.", "Geçersiz Tarih Seçimi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (remainingDays <= 0)
            {
                MessageBox.Show("Kaydedilecek izin süresi 0 gün olamaz.", "Geçersiz İzin Süresi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool CheckForOverlappingLeaves(int personelID, DateTime startDate, DateTime endDate, int? excludeIzinID = null)
        {
            using (MySqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                string query = @"SELECT COUNT(*) FROM kullanilanizinler 
                         WHERE PersonelID = @PersonelID 
                         AND ((BaslangicTarihi < @EndDate) AND (BitisTarihi > @StartDate))";
                if (excludeIzinID.HasValue)
                {
                    query += " AND ID != @IzinID";
                }
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PersonelID", personelID);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                if (excludeIzinID.HasValue)
                {
                    cmd.Parameters.AddWithValue("@IzinID", excludeIzinID.Value);
                }

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // Kaydet butonuna tıklama olayı (button4)
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // Seçilen personel bilgisi (ComboBox1'den)
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir personel seçiniz.");
                    return;
                }

                int personelID = ((dynamic)comboBox1.SelectedItem).Value;

                if (!ValidateIzinKaydi())
                {
                    return;
                }

                // Öncelikle tarihler arasında çakışan izin var mı kontrol edelim
                bool hasOverlap = CheckForOverlappingLeaves(personelID, dateTimePicker4.Value.Date, dateTimePicker5.Value.Date);
                if (hasOverlap)
                {
                    MessageBox.Show("Seçilen tarihler arasında bu personel için zaten bir izin kaydı bulunmaktadır.", "Çakışan İzin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    // Transaction başlatalım
                    using (MySqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // İzin kaydını kullanilanizinler tablosuna ekleyelim
                            string query = @"INSERT INTO kullanilanizinler (PersonelID, BaslangicTarihi, BitisTarihi, BelgeNo, izinturu, Toplam, Aciklama)
                             VALUES (@PersonelID, @BaslangicTarihi, @BitisTarihi, @BelgeNo, @izinturu, @Toplam, @Aciklama)";
                            MySqlCommand cmd = new MySqlCommand(query, con, transaction);

                            // Seçilen personel bilgisi (ComboBox1'den)
                            if (comboBox1.SelectedItem == null)
                            {
                                MessageBox.Show("Lütfen bir personel seçiniz.");
                                return;
                            }

                            cmd.Parameters.AddWithValue("@PersonelID", ((dynamic)comboBox1.SelectedItem).Value);
                            cmd.Parameters.AddWithValue("@BaslangicTarihi", dateTimePicker4.Value.Date);
                            cmd.Parameters.AddWithValue("@BitisTarihi", dateTimePicker5.Value.Date);
                            cmd.Parameters.AddWithValue("@BelgeNo", string.IsNullOrEmpty(textBox1.Text) ? DBNull.Value : textBox1.Text);
                            cmd.Parameters.AddWithValue("@izinturu", comboBox2.SelectedItem != null ? comboBox2.SelectedItem.ToString() : DBNull.Value);
                            cmd.Parameters.AddWithValue("@Aciklama", string.IsNullOrEmpty(richTextBox1.Text) ? DBNull.Value : richTextBox1.Text);

                            // Toplam alanına remainingDays değerini kaydediyoruz
                            cmd.Parameters.AddWithValue("@Toplam", Convert.ToDecimal(remainingDays));

                            cmd.ExecuteNonQuery();

                            // Son eklenen izin kaydının ID'sini alalım
                            long izinID = cmd.LastInsertedId;

                            // İzin günlerini izin_gunleri tablosuna ekleyelim
                            foreach (var entry in dayMarkings)
                            {
                                DateTime tarih = entry.Key;
                                bool isLeftMarked = entry.Value.IsLeftMarked;
                                bool isRightMarked = entry.Value.IsRightMarked;

                                string insertGunQuery = @"INSERT INTO izin_gunleri (IzinID, Tarih, IsLeftMarked, IsRightMarked)
                                                  VALUES (@IzinID, @Tarih, @IsLeftMarked, @IsRightMarked)";
                                MySqlCommand insertGunCmd = new MySqlCommand(insertGunQuery, con, transaction);
                                insertGunCmd.Parameters.AddWithValue("@IzinID", izinID);
                                insertGunCmd.Parameters.AddWithValue("@Tarih", tarih);
                                insertGunCmd.Parameters.AddWithValue("@IsLeftMarked", isLeftMarked);
                                insertGunCmd.Parameters.AddWithValue("@IsRightMarked", isRightMarked);

                                insertGunCmd.ExecuteNonQuery();
                            }
                            // --- Burada IzinHelper fonksiyonunu çağırıyoruz ---
                            IzinHelper.UpdateIzinTablolari(con, transaction, personelID,
                                dateTimePicker4.Value.Date, dateTimePicker5.Value.Date,
                                Convert.ToDecimal(remainingDays), true);
                            // --- ---
                            transaction.Commit();

                            MessageBox.Show("İzin ve yıllık hak güncellemeleri başarıyla tamamlandı.");
                        }
                        catch (Exception ex)
                        {
                            // Hata durumunda transaction'ı geri alalım
                            transaction.Rollback();
                            MessageBox.Show("İzin kaydederken hata oluştu: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası: " + ex.Message);
            }
        }

        public class IzinTabloRecord
        {
            public int ID { get; set; }
            public DateTime BaslangicTarihi { get; set; }
            public DateTime BitisTarihi { get; set; }
            public decimal KulHak { get; set; }
            public decimal Devreden { get; set; }
            public decimal DevirAlinan { get; set; }
            public decimal YillikHak { get; set; } // Yeni eklenen alan

        }

        // İptal butonuna tıklama olayı (button3)
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Güncelle butonuna tıklama olayı (button16)
        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                // Seçilen personel bilgisi (ComboBox1'den)
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir personel seçiniz.");
                    return;
                }

                int personelID = ((dynamic)comboBox1.SelectedItem).Value;

                if (!ValidateIzinKaydi())
                {
                    return;
                }

                // Öncelikle tarihler arasında çakışan izin var mı kontrol edelim (mevcut izin hariç)
                bool hasOverlap = CheckForOverlappingLeaves(personelID, dateTimePicker4.Value.Date, dateTimePicker5.Value.Date, izinID.Value);
                if (hasOverlap)
                {
                    MessageBox.Show("Seçilen tarihler arasında bu personel için zaten bir izin kaydı bulunmaktadır.", "Çakışan İzin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (MySqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Eski izin kaydını al
                            DateTime eskiBaslangic, eskiBitis;
                            double eskiremainingDays;

                            string eskiIzinQuery = "SELECT BaslangicTarihi, BitisTarihi, Toplam FROM kullanilanizinler WHERE ID = @IzinID";
                            MySqlCommand eskiIzinCmd = new MySqlCommand(eskiIzinQuery, con, transaction);
                            eskiIzinCmd.Parameters.AddWithValue("@IzinID", izinID.Value);

                            using (MySqlDataReader reader = eskiIzinCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    eskiBaslangic = reader.GetDateTime("BaslangicTarihi");
                                    eskiBitis = reader.GetDateTime("BitisTarihi");
                                    eskiremainingDays = Convert.ToDouble(reader.GetDecimal("Toplam"));
                                }
                                else
                                {
                                    MessageBox.Show("Eski izin kaydı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    transaction.Rollback();
                                    return;
                                }
                            }
                            // Eski izin günlerini izintablolari'ndan geri al
                            IzinHelper.UpdateIzinTablolari(con, transaction, personelID, eskiBaslangic, eskiBitis, Convert.ToDecimal(eskiremainingDays), false);
                            // İzin kaydını güncelle
                            string query = @"UPDATE kullanilanizinler 
                                     SET BaslangicTarihi = @BaslangicTarihi, 
                                         BitisTarihi = @BitisTarihi, 
                                         BelgeNo = @BelgeNo, 
                                         izinturu = @izinturu, 
                                         Toplam = @Toplam, 
                                         Aciklama = @Aciklama
                                     WHERE ID = @IzinID";
                            MySqlCommand cmd = new MySqlCommand(query, con, transaction);
                            cmd.Parameters.AddWithValue("@IzinID", izinID.Value);
                            cmd.Parameters.AddWithValue("@BaslangicTarihi", dateTimePicker4.Value.Date);
                            cmd.Parameters.AddWithValue("@BitisTarihi", dateTimePicker5.Value.Date);
                            cmd.Parameters.AddWithValue("@BelgeNo", string.IsNullOrEmpty(textBox1.Text) ? DBNull.Value : textBox1.Text);
                            cmd.Parameters.AddWithValue("@izinturu", comboBox2.SelectedItem != null ? comboBox2.SelectedItem.ToString() : DBNull.Value);
                            cmd.Parameters.AddWithValue("@Aciklama", string.IsNullOrEmpty(richTextBox1.Text) ? DBNull.Value : richTextBox1.Text);

                            // Toplam alanına usedDays değerini kaydediyoruz
                            cmd.Parameters.AddWithValue("@Toplam", Convert.ToDecimal(remainingDays));

                            cmd.ExecuteNonQuery();

                            // Mevcut izin günlerini sil
                            string deleteGunQuery = "DELETE FROM izin_gunleri WHERE IzinID = @IzinID";
                            MySqlCommand deleteGunCmd = new MySqlCommand(deleteGunQuery, con, transaction);
                            deleteGunCmd.Parameters.AddWithValue("@IzinID", izinID.Value);
                            deleteGunCmd.ExecuteNonQuery();

                            // Yeni izin günlerini ekle
                            foreach (var entry in dayMarkings)
                            {
                                DateTime tarih = entry.Key;
                                bool isLeftMarked = entry.Value.IsLeftMarked;
                                bool isRightMarked = entry.Value.IsRightMarked;

                                string insertGunQuery = @"INSERT INTO izin_gunleri (IzinID, Tarih, IsLeftMarked, IsRightMarked)
                                                  VALUES (@IzinID, @Tarih, @IsLeftMarked, @IsRightMarked)";
                                MySqlCommand insertGunCmd = new MySqlCommand(insertGunQuery, con, transaction);
                                insertGunCmd.Parameters.AddWithValue("@IzinID", izinID.Value);
                                insertGunCmd.Parameters.AddWithValue("@Tarih", tarih);
                                insertGunCmd.Parameters.AddWithValue("@IsLeftMarked", isLeftMarked);
                                insertGunCmd.Parameters.AddWithValue("@IsRightMarked", isRightMarked);
                                insertGunCmd.ExecuteNonQuery();
                            }

                            // Yeni izin günlerini izintablolari'na ekle
                            IzinHelper.UpdateIzinTablolari(con, transaction, personelID, dateTimePicker4.Value.Date, dateTimePicker5.Value.Date, Convert.ToDecimal(remainingDays), true);
                            transaction.Commit();
                            MessageBox.Show("İzin güncellemesi başarıyla tamamlandı.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("İzin güncellenirken hata oluştu: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Excel dosyasını seçmek için OpenFileDialog kullanıyoruz
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Dosyaları|*.xls;*.xlsx;*.xlsm";
                openFileDialog.Title = "Bir Excel Dosyası Seçin";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string excelFilePath = openFileDialog.FileName;

                    // Encoding sağlayıcısını kaydediyoruz
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    // Excel dosyasını açıyoruz
                    using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            int rowIndex = 0; // Sıfırdan başlıyoruz
                            List<string> errors = new List<string>();
                            List<string> successMessages = new List<string>();

                            do // Tüm sayfaları döngüye alıyoruz (gerekliyse)
                            {
                                while (reader.Read()) // Her bir satır için
                                {
                                    rowIndex++;

                                    if (rowIndex == 1)
                                    {
                                        // Başlık satırını atlıyoruz
                                        continue;
                                    }

                                    try
                                    {
                                        // Verileri ilgili sütunlardan okuyoruz
                                        string sicilNo = reader.GetValue(1)?.ToString(); // B sütunu (index 1)
                                        string baslangicTarihiStr = reader.GetValue(6)?.ToString(); // G sütunu (index 6)
                                        string bitisTarihiStr = reader.GetValue(9)?.ToString(); // J sütunu (index 9)
                                        string toplamStr = reader.GetValue(10)?.ToString(); // K sütunu (index 10)

                                        if (string.IsNullOrEmpty(sicilNo))
                                        {
                                            errors.Add($"Satır {rowIndex}: SicilNo boş.");
                                            continue;
                                        }

                                        // Tarihleri parse ediyoruz
                                        if (!DateTime.TryParse(baslangicTarihiStr, out DateTime baslangicTarihi))
                                        {
                                            errors.Add($"Satır {rowIndex}: Geçersiz Başlangıç Tarihi.");
                                            continue;
                                        }
                                        if (!DateTime.TryParse(bitisTarihiStr, out DateTime bitisTarihi))
                                        {
                                            errors.Add($"Satır {rowIndex}: Geçersiz Bitiş Tarihi.");
                                            continue;
                                        }

                                        // Toplam değerini parse ediyoruz
                                        if (!decimal.TryParse(toplamStr, out decimal toplam))
                                        {
                                            errors.Add($"Satır {rowIndex}: Geçersiz Toplam değeri.");
                                            continue;
                                        }

                                        // SicilNo'ya göre PersonelID'yi alıyoruz
                                        int personelID = GetPersonelIDBySicilNo(sicilNo);
                                        if (personelID == 0)
                                        {
                                            errors.Add($"Satır {rowIndex}: SicilNo {sicilNo} personelbilgileri tablosunda bulunamadı.");
                                            continue;
                                        }

                                        // İzin türü 'Ücretli İzin' olarak ayarlanıyor
                                        string izinturu = "Ücretli İzin";

                                        // İzin verilerini kullanilanizinler tablosuna ekliyoruz
                                        InsertIzin(personelID, baslangicTarihi, bitisTarihi, izinturu, toplam);

                                        // Başarılı işlemleri kaydediyoruz
                                        successMessages.Add($"Satır {rowIndex}: İzin başarıyla eklendi (PersonelID: {personelID}).");
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Satır {rowIndex}: Hata - {ex.Message}");
                                    }
                                }

                                rowIndex = 0; // Bir sonraki sayfa için satır indeksini sıfırlıyoruz
                            } while (reader.NextResult()); // Sonraki sayfaya geçiyoruz (varsa)

                            // İşlem tamamlandıktan sonra özet mesajı gösteriyoruz
                            string summaryMessage = "İşlem tamamlandı.\n";

                            if (successMessages.Count > 0)
                            {
                                summaryMessage += $"\nBaşarılı işlemler ({successMessages.Count} adet):\n";
                                summaryMessage += string.Join("\n", successMessages.Take(10)); // İlk 10 başarıyı gösteriyoruz
                                if (successMessages.Count > 10)
                                {
                                    summaryMessage += $"\n... ve diğer {successMessages.Count - 10} işlem.";
                                }
                            }
                            else
                            {
                                summaryMessage += "\nBaşarılı işlem yok.";
                            }

                            if (errors.Count > 0)
                            {
                                summaryMessage += $"\n\nHatalar ({errors.Count} adet):\n";
                                summaryMessage += string.Join("\n", errors.Take(10)); // İlk 10 hatayı gösteriyoruz
                                if (errors.Count > 10)
                                {
                                    summaryMessage += $"\n... ve diğer {errors.Count - 10} hata.";
                                }
                            }
                            else
                            {
                                summaryMessage += "\n\nHata yok.";
                            }

                            MessageBox.Show(summaryMessage, "İşlem Özeti", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beklenmeyen bir hata oluştu: " + ex.Message);
            }
        }

        // SicilNo'ya göre PersonelID'yi getiren metod
        private int GetPersonelIDBySicilNo(string sicilNo)
        {
            try
            {
                int personelID = 0;
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT ID FROM personelbilgileri WHERE SicilNo = @SicilNo";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SicilNo", sicilNo);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        personelID = Convert.ToInt32(result);
                    }
                    else
                    {
                        MessageBox.Show($"SicilNo {sicilNo} için personel bulunamadı.");
                    }
                }
                return personelID;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetPersonelIDBySicilNo Hatası: {ex.Message}");
                return 0;
            }
        }

        // İzin verilerini veritabanına ekleyen metod
        private void InsertIzin(int personelID, DateTime baslangicTarihi, DateTime bitisTarihi, string izinturu, decimal toplam)
        {
            using (MySqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                using (MySqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // Çakışan izin kontrolü
                        bool hasOverlap = CheckForOverlappingLeaves(personelID, baslangicTarihi.Date, bitisTarihi.Date);
                        if (hasOverlap)
                        {
                            throw new Exception("PersonelID " + personelID + " için çakışan izinler bulundu.");
                        }

                        // kullanilanizinler tablosuna ekleme
                        string query = @"INSERT INTO kullanilanizinler (PersonelID, BaslangicTarihi, BitisTarihi, izinturu, Toplam)
                                 VALUES (@PersonelID, @BaslangicTarihi, @BitisTarihi, @izinturu, @Toplam)";
                        MySqlCommand cmd = new MySqlCommand(query, con, transaction);
                        cmd.Parameters.AddWithValue("@PersonelID", personelID);
                        cmd.Parameters.AddWithValue("@BaslangicTarihi", baslangicTarihi.Date);
                        cmd.Parameters.AddWithValue("@BitisTarihi", bitisTarihi.Date);
                        cmd.Parameters.AddWithValue("@izinturu", izinturu);
                        cmd.Parameters.AddWithValue("@Toplam", toplam);
                        cmd.ExecuteNonQuery();

                        // Son eklenen izinID'yi alıyoruz
                        long izinID = cmd.LastInsertedId;

                        // Eğer toplam değeri buçukluysa, yarım gün izin için izin_gunleri tablosuna kayıt ekliyoruz
                        if (toplam % 1 != 0)
                        {
                            InsertYarimGunIzin(con, transaction, izinID, baslangicTarihi.Date, bitisTarihi.Date);
                        }

                        // izintablolari tablosunu güncelliyoruz
                        IzinHelper.UpdateIzinTablolari(con, transaction, personelID, baslangicTarihi.Date, bitisTarihi.Date, toplam, true);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("PersonelID " + personelID + " için izin eklenirken hata: " + ex.Message);
                    }
                }
            }
        }
        private void InsertYarimGunIzin(MySqlConnection con, MySqlTransaction transaction, long izinID, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            // Yarım gün iznin hangi güne denk geldiğini belirliyoruz (örneğin, son gün)
            DateTime yarimGunTarih = bitisTarihi.Date;

            // IsLeftMarked ve IsRightMarked değerlerini ayarlıyoruz
            // Yarım gün izni temsil etmek için bir alan 0, diğeri 1 olmalı
            bool isLeftMarked = false;  // 0
            bool isRightMarked = true;  // 1

            string insertGunQuery = @"INSERT INTO izin_gunleri (IzinID, Tarih, IsLeftMarked, IsRightMarked)
                              VALUES (@IzinID, @Tarih, @IsLeftMarked, @IsRightMarked)";
            MySqlCommand insertGunCmd = new MySqlCommand(insertGunQuery, con, transaction);
            insertGunCmd.Parameters.AddWithValue("@IzinID", izinID);
            insertGunCmd.Parameters.AddWithValue("@Tarih", yarimGunTarih);
            insertGunCmd.Parameters.AddWithValue("@IsLeftMarked", isLeftMarked ? 1 : 0);
            insertGunCmd.Parameters.AddWithValue("@IsRightMarked", isRightMarked ? 1 : 0);
            insertGunCmd.ExecuteNonQuery();
        }

    }
}
