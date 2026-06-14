using FastReport.DevComponents.DotNetBar.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using System.IO;

namespace personelizintakip
{
    public partial class Form1 : Form
    {
        private Dictionary<string, Form> openForms = new Dictionary<string, Form>();
        private System.Windows.Forms.ToolTip toolTip;
        public Form1()
        {
            InitializeComponent();
            // CheckButton olayını bağla
            checkButton1.CheckedChanged += checkButton1_CheckedChanged;
            checkButton1.Checked = false;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            //this.WindowState = FormWindowState.Maximized; // Formu tam ekran yapar
            this.FormClosing += Form1_FormClosing; // Form kapanırken çağrılacak olay
            this.Load += new EventHandler(Form1_Load); // Form yüklendiğinde çağrılacak olay
            // DataGridView'e çift tıklama olayı ekle
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            textBox1.TextChanged += TextBox1_TextChanged; // TextChanged olayına abone ol
            dataGridView3.CellDoubleClick += dataGridView3_CellDoubleClick;
            // ToolTip nesnesini oluştur ve ayarla
            toolTip = new System.Windows.Forms.ToolTip();
            toolTip.ToolTipTitle = "Buton Bilgisi";
            toolTip.IsBalloon = true; // Balon stili gösterimi
            toolTip.SetToolTip(button10, " Yeni Personel Ekle.");
            toolTip.SetToolTip(button11, " Personel İzin Belgesi Düzenle.");
            toolTip.SetToolTip(button12, " Resmi Tatiller.");
            toolTip.SetToolTip(button13, " Listeyi Yenile.");
            toolTip.SetToolTip(button14, " İzin Belgeleri Listesi");
        }
        private void UpdateRowCountLabel()
        {
            // DataGridView'deki satır sayısını al
            int rowCount = dataGridView1.Rows.Count;

            // Label üzerine mesaj yaz
            label2.Text = $"{rowCount} personel kaydı listelenmiştir.";
        }

        private void KontrolEt()
        {
            var dataTable = dataGridView1.DataSource as DataTable;

            if (dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    MessageBox.Show($"ID: {row["ID"]}, Akit: {row["Akit"]}", "Debug");
                }
            }
        }
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Columns.Contains("Akit"))
            {
                int akitValue = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["Akit"].Value);

                if (akitValue == 1)
                {
                    e.CellStyle.BackColor = ColorTranslator.FromHtml("#ffb8b8");
                }
                else
                {
                    e.CellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                }
            }
        }
        private void checkButton1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var dataTable = dataGridView1.DataSource as DataTable;

                if (dataTable != null)
                {
                    if (checkButton1.Checked)
                    {
                        // Akit değeri 1 ve 0 olanları listele
                        dataTable.DefaultView.RowFilter = "Akit = 1 OR Akit = 0";
                    }
                    else
                    {
                        // Yalnızca Akit = 0 olanları listele
                        dataTable.DefaultView.RowFilter = "Akit = 0";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Filtreleme sırasında bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Seçilen izin ID'sini al
                int izinID = Convert.ToInt32(dataGridView3.Rows[e.RowIndex].Cells["ID"].Value);

                // Seçili personel ID'sini al
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    int personelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                    // izinbelgesi formunu güncelleme modu için aç
                    izinbelgesi izinForm = new izinbelgesi(personelID, izinID);
                    izinForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Lütfen bir personel seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = textBox1.Text.Trim();

            // DataTable kaynak nesneyi al
            var dataTable = dataGridView1.DataSource as DataTable;
            if (dataTable == null) return; // null check

            // checkButton1 durumuna göre Akit filtresi ayarla
            string akitFilter;
            if (checkButton1.Checked)
            {
                // checkButton1 seçiliyse Akit = 0 veya Akit = 1
                akitFilter = "(Akit = 0 OR Akit = 1)";
            }
            else
            {
                // checkButton1 seçili değilse yalnızca Akit = 0
                akitFilter = "Akit = 0";
            }

            // Metin dolu ise hem AdiSoyadi hem Akit filtresi
            // Boş ise sadece Akit filtresi
            if (!string.IsNullOrEmpty(filterText))
            {
                dataTable.DefaultView.RowFilter = $"AdiSoyadi LIKE '{filterText}%' AND {akitFilter}";
            }
            else
            {
                dataTable.DefaultView.RowFilter = akitFilter;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // DataGridView'i özelleştir
            CustomizeDataGridView(dataGridView1);
            CustomizeDataGridView(dataGridView2);
            CustomizeDataGridView(dataGridView3);
            dataGridView1.SelectionChanged += new EventHandler(dataGridView1_SelectionChanged);
            // Form yüklendiğinde personel bilgilerini getir ve DataGridView'e yükle
            dataGridView1.MouseDown += dataGridView1_MouseDown;
            checkButton1.Checked = false;
            LoadPersonelData();
            // Chart verilerini yükle
            UpdateRowCountLabel();
            CustomizePastadilimSeries(); // "pastadilim" serisini özelleştirir
        }
        private void CustomizePastadilimSeries()
        {
            try
            {
                // "pastadilim" adlı seriyi ChartControl'den al
                var series = chartControl1.Series["pastadilim"] as DevExpress.XtraCharts.Series;

                if (series != null)
                {
                    // Veritabanından veri kaynağını al
                    using (MySqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        string query = @"
                    SELECT Departman, COUNT(*) AS PersonelSayisi
                    FROM personelbilgileri
                    WHERE Akit = 0
                    GROUP BY Departman";

                        MySqlCommand cmd = new MySqlCommand(query, con);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Veri kaynağını seriye bağla
                        chartControl1.DataSource = dt;
                        series.ArgumentDataMember = "Departman"; // Dilimler için departman isimleri
                        series.ValueDataMembers.AddRange(new string[] { "PersonelSayisi" }); // Değerler için personel sayısı
                    }

                    // Seriyi Doughnut görünümüne dönüştür
                    var doughnutView = series.View as DevExpress.XtraCharts.DoughnutSeriesView;
                    if (doughnutView != null)
                    {
                        // Doughnut görünümü ayarları
                        doughnutView.HoleRadiusPercent = 60; // Halka ortası için boşluk oranı
                        doughnutView.ExplodeMode = DevExpress.XtraCharts.PieExplodeMode.All; // Tüm dilimleri ayırabilir
                        doughnutView.ExplodedDistancePercentage = 5; // Ayrılma mesafesi
                    }
                    else
                    {
                        // Eğer Doughnut değilse Pie görünümüne geç
                        var pieView = series.View as DevExpress.XtraCharts.PieSeriesView;
                        if (pieView != null)
                        {
                            pieView.ExplodeMode = DevExpress.XtraCharts.PieExplodeMode.All; // Tüm dilimleri ayırabilir
                            pieView.ExplodedDistancePercentage = 5; // Ayrılma mesafesi
                        }
                    }

                    // Etiket ve yüzdeleri göster
                    var pieSeriesLabel = series.Label as DevExpress.XtraCharts.PieSeriesLabel;
                    if (pieSeriesLabel != null)
                    {
                        pieSeriesLabel.TextPattern = "{A}: {V} ({VP:0.0%})"; // {A}=Departman, {V}=Değer, {VP}=Yüzde
                    }

                    // Lejantı özelleştir
                    chartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True; // Lejantı göster
                    chartControl1.Legend.AlignmentHorizontal = DevExpress.XtraCharts.LegendAlignmentHorizontal.Center; // Ortala
                    chartControl1.Legend.AlignmentVertical = DevExpress.XtraCharts.LegendAlignmentVertical.BottomOutside; // Alt kısımda hizala
                    chartControl1.Legend.Direction = DevExpress.XtraCharts.LegendDirection.LeftToRight; // Soldan sağa göster
                }
                else
                {
                    MessageBox.Show("Seri bulunamadı: pastadilim", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CustomizeDataGridView(DataGridView gridView)
        {
            // Kenarlıkları kaldır
            gridView.BorderStyle = BorderStyle.None;
            gridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridView.RowHeadersVisible = false;

            // Yazı tipi ve arka plan ayarları
            gridView.Font = new Font("Arial", 11, FontStyle.Regular);
            gridView.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#58B19F"); // Seçim rengi
            gridView.DefaultCellStyle.SelectionForeColor = Color.White; // Seçili hücrelerde yazı rengi beyaz

            //// Zebra stilinde satır renkleri
            //gridView.RowsDefaultCellStyle.BackColor = Color.FromArgb(224, 240, 240); // Daha açık teal
            //gridView.AlternatingRowsDefaultCellStyle.BackColor = Color.White; // Zebra stili

            // Satır yüksekliği ve sütun genişliği ayarlamaları
            gridView.RowTemplate.Height = 30;

            // Başlık stili (beyaza yakın nötr tonlar)
            gridView.EnableHeadersVisualStyles = false;
            gridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 250, 245); // Beyaza yakın vizon
            gridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black; // Başlık yazı rengi
            gridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Regular); // Normal yazı tipi
            gridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; // Başlıkları sola hizala

            // Hücre çizgileri (hafif gri)
            gridView.GridColor = Color.WhiteSmoke;
            // Seçim modunu ayarla (tüm satır seçimi)
            gridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridView.MultiSelect = false;

            // CellPainting olayına abone ol
            gridView.CellPainting += dataGridView_CellPainting;
        }
        private void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Sadece sütun başlıkları için özel çizim yap
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                // Arka planı tek renk ile boyama (mavi-yeşil tonları)
                using (Brush backgroundBrush = new SolidBrush(ColorTranslator.FromHtml("#EAB543")))
                {
                    e.Graphics.FillRectangle(backgroundBrush, e.CellBounds);
                }

                // Asıl başlık hücresinin içeriğini çiz
                e.PaintContent(e.CellBounds);

                e.Handled = true; // Olayı işledik, varsayılan çizimi devre dışı bırak
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Normal hücre çizimi: sadece gerekli kısımlar boyanır
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                using (Pen pen = new Pen(Color.FromArgb(242, 242, 242), 0.5f)) // Beyazın açık tonunu kullanarak çizgileri ayarla
                {
                    // Üst çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Top);
                    // Sol çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Left, e.CellBounds.Bottom);
                    // Alt çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    // Sağ çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
                }

                e.Handled = true; // Olayı işledik, varsayılan çizimi devre dışı bırak
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Geçerli bir satıra çift tıklandıysa
            {
                int personelID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value);

                // PersonelGiris formunu aç ve verileri yükle
                PersonelGiris personelForm = new PersonelGiris();
                personelForm.PersonelID = personelID; // ID'yi aktar
                personelForm.LoadPersonelData(personelID);
                // DateTimePicker'ları pasif yap
                personelForm.DisableDatePickers();
                personelForm.ShowDialog(); // Formu modal olarak aç
            }
        }
        private void LoadKullanilanIzinler(int personelID)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = @"SELECT ID, BaslangicTarihi, BitisTarihi, BelgeNo, izinturu, Toplam, Aciklama 
                             FROM kullanilanizinler 
                             WHERE PersonelID = @PersonelID
                             ORDER BY BaslangicTarihi ASC"; // Sıralama eklendi

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PersonelID", personelID);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridView3.DataSource = dt;

                    dataGridView3.Columns["ID"].Visible = false;

                    // Diğer sütunları ayarla
                    for (int i = 0; i < dataGridView3.Columns.Count - 1; i++)
                    {
                        dataGridView3.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }

                    // En sağdaki sütunu Fill yap
                    dataGridView3.Columns[dataGridView3.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanılan izinler yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Uygulamanın tamamen kapanmasını sağlar
            Application.Exit();
        }

        // Veritabanından personel bilgilerini çekip dataGridView1'e yükleyen metot
        private void LoadPersonelData()
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = @"
        SELECT ID, SicilNo, CONCAT(Adi, ' ', Soyadi) AS AdiSoyadi, Akit 
        FROM personelbilgileri
        ORDER BY Adi, Soyadi";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // **Burada RowFilter'ı ayarlıyoruz**
                    dt.DefaultView.RowFilter = "Akit = 0";

                    dataGridView1.DataSource = dt;

                    // ID sütununu gizle
                    dataGridView1.Columns["ID"].Visible = false;

                    // Akit sütununu gizle (opsiyonel)
                    dataGridView1.Columns["Akit"].Visible = false;

                    // Sütun başlıklarını ayarla
                    dataGridView1.Columns["SicilNo"].HeaderText = "Sicil No";
                    dataGridView1.Columns["AdiSoyadi"].HeaderText = "Adı Soyadı";
                    dataGridView1.Columns["AdiSoyadi"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Personel bilgileri yüklenirken hata oluştu: " + ex.Message);
            }
        }

        // Seçilen personelin izin tablolarını çekip dataGridView2'ye yükleyen metot
        private void LoadIzinTablolar(int personelID)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = @"SELECT BaslangicTarihi, BitisTarihi, DevirAlinan, YillikHak, KulHak, Devreden, Kidem 
                             FROM izintablolari WHERE PersonelID = @PersonelID
                             ORDER BY BaslangicTarihi ASC"; // Sıralamayı başlangıç tarihine göre yapın

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PersonelID", personelID);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Bugünden sonraki yıllar için YıllıkHak 0 değilse güncelle
                    foreach (DataRow row in dt.Rows)
                    {
                        DateTime baslangic = Convert.ToDateTime(row["BaslangicTarihi"]);
                        if (baslangic > DateTime.Today && Convert.ToInt32(row["YillikHak"]) != 0)
                        {
                            // Veritabanında güncelle
                            string updateQuery = "UPDATE izintablolari SET YillikHak = 0 WHERE PersonelID = @PersonelID AND BaslangicTarihi = @BaslangicTarihi";
                            MySqlCommand updateCmd = new MySqlCommand(updateQuery, con);
                            updateCmd.Parameters.AddWithValue("@PersonelID", personelID);
                            updateCmd.Parameters.AddWithValue("@BaslangicTarihi", baslangic);
                            updateCmd.ExecuteNonQuery();
                            // Gridde de güncelle
                            row["YillikHak"] = 0;
                        }
                    }

                    dataGridView2.DataSource = dt;

                    for (int i = 0; i < dataGridView2.Columns.Count - 1; i++)
                    {
                        dataGridView2.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                    dataGridView2.Columns[dataGridView2.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                // Veriler yüklendikten sonra devreden değerlerini yeniden hesaplayın
                RecalculateDevreden(dataGridView2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzin tabloları yüklenirken hata oluştu: " + ex.Message);
            }
        }
        private void RecalculateDevreden(DataGridView grid)
        {
            if (!(grid.DataSource is DataTable dt)) return;

            decimal oncekiYilinDevreden = 0m; // Önceki satırın devredenini tutacak değişken

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];

                // Değerleri uygun tipte çek
                decimal devirAlinan = Convert.ToDecimal(row["DevirAlinan"]);
                int yillikHak = Convert.ToInt32(row["YillikHak"]);
                decimal kulHak = Convert.ToDecimal(row["KulHak"]);
                if (i > 0)
                {
                    devirAlinan = oncekiYilinDevreden;
                    row["DevirAlinan"] = devirAlinan;
                }

                // Yeni devreden = DevirAlinan + YillikHak - KulHak
                decimal yeniDevreden = devirAlinan + yillikHak - kulHak;
                row["Devreden"] = yeniDevreden;

                // Bir sonraki satırda kullanmak için sakla
                oncekiYilinDevreden = yeniDevreden;
            }

            // Değişiklikleri grid'e yansıt
            grid.Refresh();
        }

        //dataGridView1'de satır seçildiğinde izintablolarını yükle
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Seçilen personel ID'sini al
                int selectedPersonelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                // İzin tablolarını kontrol et ve güncelle
                KontrolVeGuncelleIzinTablosu(selectedPersonelID);

                // Personel bilgilerini al ve label'lara yazdır
                LoadPersonelDetails(selectedPersonelID);

                // dataGridView2 ve dataGridView3'ü güncelle
                LoadIzinTablolar(selectedPersonelID);
                LoadKullanilanIzinler(selectedPersonelID);
            }
        }
        /// <summary>
        /// Seçili personelin izintablolari tablosunu kontrol eder; dönemi kapanış tarihi geldiğinde yeni yılı ekler.
        /// </summary>
        private void KontrolVeGuncelleIzinTablosu(int personelID)
        {
            try
            {
                using (var con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    // 1) Personelden gerekli bilgileri al
                    DateTime dogumTarihi, acilisIzinTarihi;
                    string calisanTuru;
                    using (var personelCmd = new MySqlCommand(
                        "SELECT DogumTarihi, AcilisIzinTarihi, EkAlan1 " +
                        "FROM personelbilgileri " +
                        "WHERE ID = @PersonelID", con))
                    {
                        personelCmd.Parameters.AddWithValue("@PersonelID", personelID);
                        using (var rdr = personelCmd.ExecuteReader())
                        {
                            if (!rdr.Read())
                                return; // Personel bulunamadı
                            dogumTarihi = rdr.GetDateTime("DogumTarihi");
                            acilisIzinTarihi = rdr.GetDateTime("AcilisIzinTarihi");
                            calisanTuru = rdr.GetString("EkAlan1");
                        }
                    }

                    // 2) En son bitiş tarihi ve devreden miktarını al
                    DateTime sonBitisTarihi = DateTime.MinValue;
                    decimal devirAlinan = 0m;
                    int sonKidem = 0;
                    bool satirVar = false;

                    using (var izinCmd = new MySqlCommand(
                        "SELECT BitisTarihi, Devreden, Kidem " +
                        "FROM izintablolari " +
                        "WHERE PersonelID = @PersonelID " +
                        "ORDER BY BitisTarihi DESC LIMIT 1", con))
                    {
                        izinCmd.Parameters.AddWithValue("@PersonelID", personelID);
                        using (var rdr = izinCmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                satirVar = true;
                                sonBitisTarihi = rdr.GetDateTime("BitisTarihi").Date;
                                devirAlinan = rdr.GetDecimal("Devreden");
                                sonKidem = rdr.GetInt32("Kidem");
                            }
                        }
                    }

                    if (!satirVar)
                        return; // Henüz hiç kayıt yoksa ekleme yok

                    // 3) Yeni dönemin başlangıç ve bitiş tarihlerini belirle
                    DateTime yeniBaslangic = sonBitisTarihi;
                    DateTime yeniBitis = yeniBaslangic.AddYears(1);

                    // 4) Sadece kapanış tarihi (yeniBitis) veya sonrası geldiğinde ekle
                    if (DateTime.Today.Date >= yeniBitis.Date)
                    {
                        int yeniKidem = sonKidem + 1;
                        // İzin hakkını hesapla
                        int yillikHak = IzinHelper.HesaplaIzinHakki(
                            calisanTuru,
                            dogumTarihi,
                            yeniBitis,
                            yeniKidem,
                            acilisIzinTarihi
                        );

                        // 5) Yeni satırı ekle
                        using (var insertCmd = new MySqlCommand(
                            @"INSERT INTO izintablolari
                      (PersonelID, BaslangicTarihi, BitisTarihi,
                       DevirAlinan, YillikHak, KulHak, Devreden, Kidem)
                      VALUES
                      (@PersonelID, @BaslangicTarihi, @BitisTarihi,
                       @DevirAlinan,   @YillikHak,   0,      @Devreden, @Kidem)", con))
                        {
                            insertCmd.Parameters.AddWithValue("@PersonelID", personelID);
                            insertCmd.Parameters.AddWithValue("@BaslangicTarihi", yeniBaslangic);
                            insertCmd.Parameters.AddWithValue("@BitisTarihi", yeniBitis);
                            insertCmd.Parameters.AddWithValue("@DevirAlinan", devirAlinan);
                            insertCmd.Parameters.AddWithValue("@YillikHak", yillikHak);
                            insertCmd.Parameters.AddWithValue("@Devreden", devirAlinan + yillikHak);
                            insertCmd.Parameters.AddWithValue("@Kidem", yeniKidem);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzin tablosu güncellenirken hata oluştu: " + ex.Message,
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int HesaplaIzinHakki(string calisanTuru, int ageAtYear, int kidemYili)
        {
            int izinHakki = 0;

            if (calisanTuru == "İşçi")
            {
                if (ageAtYear >= 50)
                {
                    izinHakki = kidemYili <= 15 ? 20 : 26;
                }
                else
                {
                    if (kidemYili <= 5)
                        izinHakki = 14;
                    else if (kidemYili > 5 && kidemYili < 15)
                        izinHakki = 20;
                    else
                        izinHakki = 26;
                }
            }
            else if (calisanTuru == "Memur")
            {
                izinHakki = kidemYili <= 10 ? 20 : 30;
            }
            else if (calisanTuru == "Yeraltı")
            {
                if (kidemYili <= 5)
                    izinHakki = 18;
                else if (kidemYili > 5 && kidemYili < 15)
                    izinHakki = 24;
                else
                    izinHakki = 30;
            }

            return izinHakki;
        }
        private void LoadPersonelDetails(int personelID)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT Adi, Soyadi, SicilNo, Departman FROM personelbilgileri WHERE ID = @PersonelID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PersonelID", personelID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string adi = reader["Adi"].ToString().ToUpper();
                            string soyadi = reader["Soyadi"].ToString().ToUpper();
                            string sicilNo = reader["SicilNo"].ToString().ToUpper();
                            string departman = reader["Departman"].ToString().ToUpper();

                            // RichTextBox'ları formatlayarak yazdır
                            SetRichTextBoxText(richTextBox4, "Adı:   ", adi);
                            SetRichTextBoxText(richTextBox5, "Soyadı:  ", soyadi);
                            SetRichTextBoxText(richTextBox6, "Sicil No:  ", sicilNo);
                            SetRichTextBoxText(richTextBox7, "Departman:   ", departman);
                        }
                    }
                    // richTextBox5, 6 ve 7 için veritabanı sorgusunu ekle
                    LoadIzinBilgileri(personelID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Personel bilgileri yüklenirken hata oluştu: " + ex.Message);
            }
        }
        // İzin Hakkı, Kullanılan ve Kalan İzinleri hesaplayan ve yazdıran metot
        private void LoadIzinBilgileri(int personelID)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    // Toplamları hesaplamak için SQL sorgusu
                    string query = @"SELECT 
                                SUM(YillikHak) AS ToplamYillikHak,
                                SUM(KulHak) AS ToplamKulHak
                             FROM izintablolari 
                             WHERE PersonelID = @PersonelID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PersonelID", personelID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal toplamYillikHak = reader.IsDBNull(0) ? 0 : reader.GetDecimal("ToplamYillikHak");
                            decimal toplamKulHak = reader.IsDBNull(1) ? 0 : reader.GetDecimal("ToplamKulHak");

                            // Kalan günleri hesapla (YillikHak - KulHak)
                            decimal kalanIzin = toplamYillikHak - toplamKulHak;

                            // Label5: İzin Hakkı (bold) + YillikHak toplamı
                            SetRichTextBoxText(richTextBox1, "İzin Hakkı: ", toplamYillikHak.ToString("0.0"));

                            // Label6: Kullanılan (bold) + KulHak toplamı
                            SetRichTextBoxText(richTextBox2, "Kullanılan: ", toplamKulHak.ToString("0.0"));

                            // Label7: Kalan (bold) + Kalan izin (YillikHak - KulHak)
                            SetRichTextBoxText(richTextBox3, "Kalan: ", kalanIzin.ToString("0.0"));

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İzin bilgileri yüklenirken hata oluştu: " + ex.Message);
            }
        }
        // RichTextBox'u bold ve normal stil ile yazdırmak için metot
        private void SetRichTextBoxText(RichTextBox richTextBox, string boldText, string normalText)
        {
            richTextBox.Clear(); // Mevcut metni temizle
            // "Bold" kısmı yazdır
            richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Bold);
            richTextBox.AppendText(boldText);

            // "Normal" kısmı yazdır
            richTextBox.SelectionFont = new Font(richTextBox.Font, FontStyle.Regular);
            richTextBox.AppendText(normalText);

            // RichTextBox'ı sadece okunabilir yap
            richTextBox.ReadOnly = true;
            richTextBox.BorderStyle = BorderStyle.None; // Kenarlıkları kaldır
            richTextBox.BackColor = this.BackColor; // Arka planı form arka planıyla aynı yap
        }
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = dataGridView1.HitTest(e.X, e.Y);
                if (hitTestInfo.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hitTestInfo.RowIndex].Selected = true;
                }
            }
        }
        private void dataGridView3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTestInfo = dataGridView3.HitTest(e.X, e.Y);
                if (hitTestInfo.RowIndex >= 0)
                {
                    dataGridView3.ClearSelection();
                    dataGridView3.Rows[hitTestInfo.RowIndex].Selected = true;
                }
            }
        }
        private void PersonelSiltoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Seçili personelin ID'sini al
                int personelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                // Silme işlemini onayla
                DialogResult result = MessageBox.Show("Bu personeli silmek istediğinizden emin misiniz?",
                                                      "Personel Silme",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection con = DatabaseHelper.GetConnection())
                        {
                            con.Open();
                            string query = "DELETE FROM personelbilgileri WHERE ID = @PersonelID";
                            MySqlCommand cmd = new MySqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@PersonelID", personelID);
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Personel başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // DataGridView'i güncelle
                            LoadPersonelData();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Personel silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz personeli seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void izinsiltoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                int izinID = Convert.ToInt32(dataGridView3.SelectedRows[0].Cells["ID"].Value);
                int personelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                DialogResult result = MessageBox.Show("Bu izin kaydını silmek istediğinizden emin misiniz?",
                                                      "İzin Silme", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection con = DatabaseHelper.GetConnection())
                        {
                            con.Open();
                            using (MySqlTransaction transaction = con.BeginTransaction())
                            {
                                try
                                {
                                    // 1. Silinecek izin kaydının bilgilerini al
                                    string getIzinQuery = @"SELECT BaslangicTarihi, BitisTarihi, Toplam 
                                                  FROM kullanilanizinler 
                                                  WHERE ID = @IzinID";
                                    MySqlCommand getIzinCmd = new MySqlCommand(getIzinQuery, con, transaction);
                                    getIzinCmd.Parameters.AddWithValue("@IzinID", izinID);

                                    DateTime baslangicTarihi = DateTime.MinValue;
                                    DateTime bitisTarihi = DateTime.MinValue;
                                    decimal toplamGun = 0;

                                    using (MySqlDataReader reader = getIzinCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            baslangicTarihi = reader.GetDateTime("BaslangicTarihi");
                                            bitisTarihi = reader.GetDateTime("BitisTarihi");
                                            toplamGun = reader.GetDecimal("Toplam");
                                        }
                                    }

                                    // 2. İzin tablolarını güncelle (izni çıkar)
                                    IzinHelper.UpdateIzinTablolari(con, transaction, personelID,
                                                                 baslangicTarihi, bitisTarihi,
                                                                 toplamGun, false); // false = izin çıkarma işlemi

                                    // 3. Kullanilanizinler tablosundan kaydı sil
                                    string deleteIzinQuery = "DELETE FROM kullanilanizinler WHERE ID = @IzinID";
                                    MySqlCommand deleteIzinCmd = new MySqlCommand(deleteIzinQuery, con, transaction);
                                    deleteIzinCmd.Parameters.AddWithValue("@IzinID", izinID);
                                    deleteIzinCmd.ExecuteNonQuery();

                                    // 4. İzin günlerini sil
                                    string deleteGunlerQuery = "DELETE FROM izin_gunleri WHERE IzinID = @IzinID";
                                    MySqlCommand deleteGunlerCmd = new MySqlCommand(deleteGunlerQuery, con, transaction);
                                    deleteGunlerCmd.Parameters.AddWithValue("@IzinID", izinID);
                                    deleteGunlerCmd.ExecuteNonQuery();

                                    transaction.Commit();

                                    MessageBox.Show("İzin kaydı başarıyla silindi.", "Başarılı",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // DataGridView'leri güncelle
                                    LoadKullanilanIzinler(personelID);
                                    LoadIzinTablolar(personelID);
                                    LoadPersonelDetails(personelID);
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    MessageBox.Show("İzin silinirken hata oluştu: " + ex.Message,
                                                  "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Veritabanı bağlantı hatası: " + ex.Message,
                                      "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz izin kaydını seçiniz.",
                              "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedPersonelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                // Her iki tablo için veriyi alıyoruz
                DataTable izinData = GetIzinTablosuData(selectedPersonelID);
                DataTable kullanilanIzinlerData = GetKullanilanIzinlerData(selectedPersonelID);

                // Raporu başlatıyoruz
                PrintIzinReport(izinData, kullanilanIzinlerData);
            }
            else
            {
                MessageBox.Show("Lütfen bir personel seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable GetIzinTablosuData(int personelID)
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM izin_tablosu_view2 WHERE PersonelID = @PersonelID";

            using (MySqlConnection con = DatabaseHelper.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@PersonelID", personelID);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        private DataTable GetKullanilanIzinlerData(int personelID)
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM kullanilan_izinler_view WHERE PersonelID = @PersonelID ORDER BY KullanilanBaslangicTarihi ASC";

            using (MySqlConnection con = DatabaseHelper.GetConnection())
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@PersonelID", personelID);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }


        private void PrintIzinReport(DataTable izinData, DataTable kullanilanIzinlerData)
        {
            string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rapor", "izinraporu.repx");

            if (!File.Exists(reportPath))
            {
                MessageBox.Show("Rapor dosyası bulunamadı: " + reportPath);
                return;
            }

            XtraReport report = new XtraReport();
            report.LoadLayout(reportPath);

            // Ana veri kaynağını atıyoruz
            report.DataSource = new List<object> { izinData, kullanilanIzinlerData };

            // DetailReport1 için DataMember ayarlıyoruz
            var detailReport1 = report.Bands["DetailReport"] as DetailReportBand;
            if (detailReport1 != null)
            {
                detailReport1.DataSource = izinData;
                detailReport1.DataMember = ""; // DataMember boş olabilir.
            }

            // DetailReport2 için DataMember ayarlıyoruz
            var detailReport2 = report.Bands["DetailReport1"] as DetailReportBand;
            if (detailReport2 != null)
            {
                detailReport2.DataSource = kullanilanIzinlerData;
                detailReport2.DataMember = ""; // DataMember boş olabilir.
            }

            // Raporu gösteriyoruz
            ReportPrintTool printTool = new ReportPrintTool(report);
            printTool.ShowPreviewDialog();
        }
        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    // Seçili personel ID'sini al
                    int selectedPersonelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                    // DataGridView'i yenile
                    LoadPersonelData(); // dataGridView1'i yeniler

                    // Yenileme işleminden sonra seçili satırı geri yükle
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["ID"].Value) == selectedPersonelID)
                        {
                            row.Selected = true;
                            dataGridView1.FirstDisplayedScrollingRowIndex = row.Index; // Görünümde tutmak için
                            break;
                        }
                    }

                    // Diğer DataGridView'leri de yenile
                    LoadIzinTablolar(selectedPersonelID); // dataGridView2'yi yeniler
                    LoadKullanilanIzinler(selectedPersonelID); // dataGridView3'ü yeniler
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yenilenirken hata oluştu: " + ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------------
        private List<string> GetButtonNames()
        {
            List<string> buttonNames = new List<string>();
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is System.Windows.Forms.Button)
                {
                    buttonNames.Add(ctrl.Text);
                }
            }
            return buttonNames;
        }

        public void SetModulePermissions(Dictionary<string, bool> moduleAccess)
        {
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is System.Windows.Forms.Button button)
                {
                    // Eğer butonun metni moduleAccess'de varsa ve yetki false ise gizle
                    bool hasPermission = moduleAccess.ContainsKey(button.Text) ? moduleAccess[button.Text] : true;  // Default true olmalı
                    button.Visible = hasPermission;
                }
            }
        }
        private void OpenOrActivateForm(string formName, Form formInstance)
        {
            if (openForms.ContainsKey(formName))
            {
                // Form zaten açıksa, onu ön plana getirin
                openForms[formName].Activate();
            }
            else
            {
                // Form açık değilse, açın ve dictionary'e ekleyin
                formInstance.FormClosed += (s, args) => openForms.Remove(formName); // Form kapandığında dictionary'den çıkar
                openForms[formName] = formInstance;
                formInstance.Owner = this; // Form1'i owner olarak ayarla
                formInstance.Show();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //ModelGiris modelGirisForm = new ModelGiris();
            //OpenOrActivateForm("ModelGiris", modelGirisForm);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //SiparisGiris siparisGirisForm = new SiparisGiris();
            //OpenOrActivateForm("SiparisGiris", siparisGirisForm);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //UretimTipleri UretimTipleriForm = new UretimTipleri();
            //OpenOrActivateForm("UretimTipleri", UretimTipleriForm);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //SiparisList SiparisListForm = new SiparisList(LoginForm.LoggedInUser);
            //OpenOrActivateForm("SiparisList", SiparisListForm);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //CariKartListesi CariKartListesiForm = new CariKartListesi();
            //OpenOrActivateForm("CariKartListesi", CariKartListesiForm);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //GenelTanimlar GenelTanimlarForm = new GenelTanimlar();
            //OpenOrActivateForm("GenelTanimlar", GenelTanimlarForm);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<string> buttonNames = GetButtonNames();
            YonetimPaneli YonetimPaneliForm = new YonetimPaneli(buttonNames);
            OpenOrActivateForm("YonetimPaneli", YonetimPaneliForm);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            //PersonelReport PersonelReportForm = new PersonelReport();
            //OpenOrActivateForm("PersonelReport", PersonelReportForm);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //MusteriTalepList MusteriTalepListForm = new MusteriTalepList();
            //OpenOrActivateForm("MusteriTalepList", MusteriTalepListForm);
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            PersonelGiris PersonelGirisForm = new PersonelGiris();
            OpenOrActivateForm("PersonelGiris", PersonelGirisForm);
        }
        private void button11_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedPersonelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                izinbelgesi izinbelgesiForm = new izinbelgesi(selectedPersonelID);
                OpenOrActivateForm("izinbelgesi", izinbelgesiForm);
            }
            else
            {
                MessageBox.Show("Lütfen bir personel seçiniz.");
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        private void button12_Click_1(object sender, EventArgs e)
        {
            resmitatiller resmitatillerForm = new resmitatiller();
            OpenOrActivateForm("resmitatiller", resmitatillerForm);
        }


        private void button14_Click(object sender, EventArgs e)
        {
            izinbelgelist izinbelgelistForm = new izinbelgelist();
            OpenOrActivateForm("İzin Belgeleri Listesi", izinbelgelistForm);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            PersonelReport PersonelReportForm = new PersonelReport();
            OpenOrActivateForm("PersonelReport", PersonelReportForm);
        }

        // dataGridView2'de satıra tıklanınca bugünden sonraki yıl için YıllıkHak 0 değilse güncelle
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView2.Rows[e.RowIndex];
                DateTime baslangic = Convert.ToDateTime(row.Cells["BaslangicTarihi"].Value);
                int yillikHak = Convert.ToInt32(row.Cells["YillikHak"].Value);
                int personelID = 0;
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    personelID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                }
                if (baslangic > DateTime.Today && yillikHak != 0 && personelID != 0)
                {
                    using (MySqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        string updateQuery = "UPDATE izintablolari SET YillikHak = 0 WHERE PersonelID = @PersonelID AND BaslangicTarihi = @BaslangicTarihi";
                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, con);
                        updateCmd.Parameters.AddWithValue("@PersonelID", personelID);
                        updateCmd.Parameters.AddWithValue("@BaslangicTarihi", baslangic);
                        updateCmd.ExecuteNonQuery();
                    }
                    row.Cells["YillikHak"].Value = 0;
                }
            }
        }
    }
}