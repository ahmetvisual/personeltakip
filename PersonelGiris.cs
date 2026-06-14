using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient; // MySQL bağlantısı için gerekli
using ExcelDataReader;


namespace personelizintakip
{
    public partial class PersonelGiris : Form
    {
        private string selectedImagePath; // Seçilen resmin yolu

        public PersonelGiris()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.PersonelGiris_Load);
        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEdit1.Checked)
            {
                // O günün tarihini dateTimePicker5'e yazdır
                dateTimePicker5.Value = DateTime.Now;

                // Akit alanını 1 olarak kaydet
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "UPDATE personelbilgileri SET Akit = 1 WHERE ID = @PersonelID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PersonelID", PersonelID);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Akit alanını 0 olarak kaydet
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "UPDATE personelbilgileri SET Akit = 0 WHERE ID = @PersonelID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PersonelID", PersonelID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Formu kapatma işlemi
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Personel bilgilerini kaydetme işlemi
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // Zorunlu alanlar kontrolü
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    MessageBox.Show("Personelin adı girilmeli!");
                    return;
                }

                if (string.IsNullOrEmpty(textBox3.Text))
                {
                    MessageBox.Show("Personelin soyadı girilmeli!");
                    return;
                }

                if (string.IsNullOrEmpty(textBox2.Text))
                {
                    MessageBox.Show("TC numarası girilmeli!");
                    return;
                }

                if (string.IsNullOrEmpty(textBox6.Text))
                {
                    MessageBox.Show("Personelin Sicil Numarasını giriniz!");
                    return;
                }

                if (comboBox12.Text == null)
                {
                    MessageBox.Show("İş Akitlerinde işçi türünü seçiniz!");
                    return;
                }

                // İşe Giriş Tarihi kontrolü
                if (dateTimePicker6.Value == DateTime.MinValue)
                {
                    MessageBox.Show("İşe giriş Tarihi girilmelidir!");
                    return;
                }

                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    // Personel bilgilerini kaydetmek için sorgu
                    string query = @"INSERT INTO personelbilgileri (Adi, TCNo, Soyadi, SicilNo, Cinsiyet, KanGrubu, DogumYeri, 
                            DogumTarihi, AnaAdi, BabaAdi, CalistigiBirim, MedeniHaliKodu, Departman, 
                            KadroDerecesi, Derecesi, KadroMeclisKararSayisi, DereceIlerlemeTarihi, KadroMeclisKararTarihi, 
                            EkGosterge, Sendika, EmekliSandigiSicilNo, SigortaSicilNo, EvTel, CepTel, email, Adres, 
                            TahsilDurumu, MezunOlduguOkul, CocukSayisi, AskerlikDurumu, Resim, IsyeriID, KayitTarihi, 
                            AcilisIzinTarihi, KapanisIzinTarihi, BaslangicKidemi, EkAlan1, Akit) 
                            VALUES (@Adi, @TCNo, @Soyadi, @SicilNo, @Cinsiyet, @KanGrubu, @DogumYeri, @DogumTarihi, 
                            @AnaAdi, @BabaAdi, @CalistigiBirim, @MedeniHaliKodu, @Departman, 
                            @KadroDerecesi, @Derecesi, @KadroMeclisKararSayisi, @DereceIlerlemeTarihi, 
                            @KadroMeclisKararTarihi, @EkGosterge, @Sendika, @EmekliSandigiSicilNo, 
                            @SigortaSicilNo, @EvTel, @CepTel, @email, @Adres, @TahsilDurumu, @MezunOlduguOkul, 
                            @CocukSayisi, @AskerlikDurumu, @Resim, @IsyeriID, @KayitTarihi, @AcilisIzinTarihi, 
                            @KapanisIzinTarihi, @BaslangicKidemi, @EkAlan1, @Akit)";

                    MySqlCommand cmd = new MySqlCommand(query, con);

                    // Personel bilgilerini parametreler ile ekle
                    cmd.Parameters.AddWithValue("@Adi", string.IsNullOrEmpty(textBox1.Text) ? DBNull.Value : textBox1.Text);
                    cmd.Parameters.AddWithValue("@TCNo", string.IsNullOrEmpty(textBox2.Text) ? DBNull.Value : textBox2.Text);
                    cmd.Parameters.AddWithValue("@Soyadi", string.IsNullOrEmpty(textBox3.Text) ? DBNull.Value : textBox3.Text);
                    cmd.Parameters.AddWithValue("@SicilNo", string.IsNullOrEmpty(textBox6.Text) ? DBNull.Value : textBox6.Text);
                    cmd.Parameters.AddWithValue("@KanGrubu", string.IsNullOrEmpty(textBox7.Text) ? DBNull.Value : textBox7.Text);
                    cmd.Parameters.AddWithValue("@DogumYeri", string.IsNullOrEmpty(textBox4.Text) ? DBNull.Value : textBox4.Text);
                    cmd.Parameters.AddWithValue("@KayitTarihi", dateTimePicker3.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@DogumTarihi", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@AnaAdi", string.IsNullOrEmpty(textBox5.Text) ? DBNull.Value : textBox5.Text);
                    cmd.Parameters.AddWithValue("@BabaAdi", string.IsNullOrEmpty(textBox9.Text) ? DBNull.Value : textBox9.Text);
                    cmd.Parameters.AddWithValue("@Akit", checkEdit1.Checked ? 1 : 0);

                    // Cinsiyet: Erkek -> 0, Kadın -> 1
                    string cinsiyet = comboBox1.SelectedItem?.ToString() == "Erkek" ? "0" : "1";
                    cmd.Parameters.AddWithValue("@Cinsiyet", cinsiyet);
                    cmd.Parameters.AddWithValue("@CalistigiBirim", comboBox2.SelectedItem == null ? DBNull.Value : comboBox2.SelectedItem.ToString());

                    // Medeni Hali Kodu: Bekar -> 0, Evli -> 1
                    cmd.Parameters.AddWithValue("@MedeniHaliKodu", comboBox5.SelectedIndex == 0 ? 0 : 1);
                    cmd.Parameters.AddWithValue("@Departman", comboBox3.SelectedItem == null ? DBNull.Value : comboBox3.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@KadroDerecesi", string.IsNullOrEmpty(textBox11.Text) ? DBNull.Value : textBox11.Text);
                    cmd.Parameters.AddWithValue("@Derecesi", string.IsNullOrEmpty(textBox16.Text) ? DBNull.Value : textBox16.Text);
                    cmd.Parameters.AddWithValue("@KadroMeclisKararSayisi", string.IsNullOrEmpty(textBox12.Text) ? DBNull.Value : textBox12.Text);
                    cmd.Parameters.AddWithValue("@DereceIlerlemeTarihi", comboBox8.SelectedItem == null ? DBNull.Value : comboBox8.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@KadroMeclisKararTarihi", dateTimePicker2.Value);
                    cmd.Parameters.AddWithValue("@EkGosterge", string.IsNullOrEmpty(textBox17.Text) ? DBNull.Value : textBox17.Text);
                    cmd.Parameters.AddWithValue("@Sendika", string.IsNullOrEmpty(textBox14.Text) ? DBNull.Value : textBox14.Text);
                    cmd.Parameters.AddWithValue("@EmekliSandigiSicilNo", string.IsNullOrEmpty(textBox15.Text) ? DBNull.Value : textBox15.Text);
                    cmd.Parameters.AddWithValue("@SigortaSicilNo", string.IsNullOrEmpty(textBox18.Text) ? DBNull.Value : textBox18.Text);
                    cmd.Parameters.AddWithValue("@EvTel", string.IsNullOrEmpty(textBox13.Text) ? DBNull.Value : textBox13.Text);
                    cmd.Parameters.AddWithValue("@CepTel", string.IsNullOrEmpty(textBox21.Text) ? DBNull.Value : textBox21.Text);
                    cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(textBox19.Text) ? DBNull.Value : textBox19.Text);
                    cmd.Parameters.AddWithValue("@Adres", string.IsNullOrEmpty(richTextBox1.Text) ? DBNull.Value : richTextBox1.Text);
                    cmd.Parameters.AddWithValue("@TahsilDurumu", comboBox9.SelectedItem == null ? DBNull.Value : comboBox9.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@MezunOlduguOkul", string.IsNullOrEmpty(textBox22.Text) ? DBNull.Value : textBox22.Text);

                    // Çocuk sayısını varsayılan olarak 0 yapıyoruz
                    int cocukSayisi = string.IsNullOrEmpty(textBox23.Text) ? 0 : Convert.ToInt32(textBox23.Text);
                    cmd.Parameters.AddWithValue("@CocukSayisi", cocukSayisi);

                    // Askerlik Durumu: Muaf -> 0, Tecilli -> 1, Yaptı -> 2
                    cmd.Parameters.AddWithValue("@AskerlikDurumu", comboBox10.SelectedIndex);

                    // Resim ekleme işlemi (longblob)
                    if (pictureBox1.Image != null && !string.IsNullOrEmpty(selectedImagePath))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                            cmd.Parameters.AddWithValue("@Resim", ms.ToArray());
                        }
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Resim", DBNull.Value);
                    }

                    // Firma ID'sini comboBox11'den alıp IsyeriID olarak kaydetme
                    if (comboBox11.SelectedItem != null)
                    {
                        int isyeriID = ((KeyValuePair<int, string>)comboBox11.SelectedItem).Key;
                        cmd.Parameters.AddWithValue("@IsyeriID", isyeriID);
                    }
                    else
                    {
                        MessageBox.Show("Lütfen bir firma seçin!");
                        return;
                    }

                    // Tarih kontrolleri (AcilisIzinTarihi ve KapanisIzinTarihi boş olabilir)
                    if (dateTimePicker6.Value == null || dateTimePicker6.Checked == false)
                    {
                        cmd.Parameters.AddWithValue("@AcilisIzinTarihi", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@AcilisIzinTarihi", dateTimePicker6.Value);
                    }

                    if (dateTimePicker5.Value == null || dateTimePicker5.Checked == false)
                    {
                        cmd.Parameters.AddWithValue("@KapanisIzinTarihi", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@KapanisIzinTarihi", dateTimePicker5.Value);
                    }

                    // Başlangıç Kıdemi kontrolü
                    if (string.IsNullOrEmpty(textBox8.Text) || textBox8.Text.Length > 2)
                    {
                        MessageBox.Show("Başlangıç Kıdemi iki karakterli bir sayı olmalıdır!");
                        return;
                    }
                    cmd.Parameters.AddWithValue("@BaslangicKidemi", textBox8.Text);

                    // Çalışan Türü (EkAlan1)
                    if (string.IsNullOrEmpty(comboBox12.Text))
                    {
                        MessageBox.Show("Lütfen çalışan türünü seçiniz.");
                        return;
                    }
                    cmd.Parameters.AddWithValue("@EkAlan1", comboBox12.Text);


                    // Personel bilgilerini kaydet
                    cmd.ExecuteNonQuery();

                    // Son eklenen personel ID'sini al
                    long personelID = cmd.LastInsertedId;

                    // Çalışan türünü alın (örneğin: ComboBox12'den)
                    string calisanTuru = comboBox12.Text?.ToString();

                    if (string.IsNullOrEmpty(calisanTuru))
                    {
                        throw new Exception("Çalışan türü belirlenmedi!");
                    }

                    DateTime today = DateTime.Today;
                    DateTime dogumTarihi = dateTimePicker1.Value;
                    int age = today.Year - dogumTarihi.Year;
                    if (dogumTarihi.Date > today.AddYears(-age)) age--;

                    // İzin tablosunu oluştur ve verileri ekle
                    DateTime baslangicTarihi = dateTimePicker6.Value; // İşe giriş tarihi
                    decimal devredenIzin = 0;
                    DateTime bugun = DateTime.Now;
                    int kidemYili = 1;

                    DateTime iseGirisTarihi = dateTimePicker6.Value;
                    int toplamIzinHakki = 0;

                    while (baslangicTarihi <= bugun)
                    {
                        DateTime bitisTarihi = baslangicTarihi.AddYears(1);
                        int yillikHak = 0;

                        if (bitisTarihi <= bugun)
                        {
                            // Yıl tamamlanmış, izin hakkını hesapla
                            yillikHak = HesaplaIzinHakki(calisanTuru, dateTimePicker1.Value, bitisTarihi, kidemYili, iseGirisTarihi);
                            toplamIzinHakki += yillikHak;
                        }
                        else if (baslangicTarihi > DateTime.Today)
                        {
                            // Bugünden sonraki yıl, izin hakkı 0
                            yillikHak = 0;
                        }
                        else
                        {
                            // Yıl tamamlanmamış, izin hakkı 0
                            yillikHak = 0;
                        }

                        decimal kulHak = 0;
                        decimal devredilen = yillikHak - kulHak + devredenIzin;

                        string izinQuery = @"INSERT INTO izintablolari (PersonelID, BaslangicTarihi, BitisTarihi,
            DevirAlinan, YillikHak, KulHak, Devreden, Kidem) 
            VALUES (@PersonelID, @BaslangicTarihi, @BitisTarihi, @DevirAlinan,
            @YillikHak, @KulHak, @Devreden, @Kidem)";

                        MySqlCommand izinCmd = new MySqlCommand(izinQuery, con);
                        izinCmd.Parameters.AddWithValue("@PersonelID", personelID);
                        izinCmd.Parameters.AddWithValue("@BaslangicTarihi", baslangicTarihi);
                        izinCmd.Parameters.AddWithValue("@BitisTarihi", bitisTarihi);
                        izinCmd.Parameters.AddWithValue("@DevirAlinan", devredenIzin);
                        izinCmd.Parameters.AddWithValue("@YillikHak", yillikHak);
                        izinCmd.Parameters.AddWithValue("@KulHak", kulHak);
                        izinCmd.Parameters.AddWithValue("@Devreden", devredilen);
                        izinCmd.Parameters.AddWithValue("@Kidem", kidemYili);

                        izinCmd.ExecuteNonQuery();

                        baslangicTarihi = bitisTarihi;
                        devredenIzin = devredilen;
                        kidemYili++;
                    }

                    MessageBox.Show("Personel ve izin bilgileri başarıyla kaydedildi.");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("SQL Hatası: " + ex.Message + "\nDetay: " + ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Genel Hata: " + ex.Message + "\nDetay: " + ex.ToString());
            }
        }

        public void LoadPersonelData(int personelID)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    // Personel bilgilerini almak için sorgu
                    string query = @"SELECT Adi, TCNo, Soyadi, SicilNo, Cinsiyet, KanGrubu, DogumYeri, 
                             DogumTarihi, AnaAdi, BabaAdi, CalistigiBirim, MedeniHaliKodu, Departman, 
                             KadroDerecesi, Derecesi, KadroMeclisKararSayisi, 
                             DereceIlerlemeTarihi, KadroMeclisKararTarihi, EkGosterge, Sendika, 
                             EmekliSandigiSicilNo, SigortaSicilNo, EvTel, CepTel, email, Adres, 
                             TahsilDurumu, MezunOlduguOkul, CocukSayisi, AskerlikDurumu, Resim, 
                             IsyeriID, KayitTarihi, AcilisIzinTarihi, KapanisIzinTarihi, BaslangicKidemi, EkAlan1 
                             FROM personelbilgileri WHERE ID = @PersonelID";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PersonelID", personelID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // TextBox'ları doldur
                            textBox1.Text = reader["Adi"] as string ?? string.Empty;
                            textBox2.Text = reader["TCNo"] as string ?? string.Empty;
                            textBox3.Text = reader["Soyadi"] as string ?? string.Empty;
                            textBox6.Text = reader["SicilNo"] as string ?? string.Empty;
                            textBox7.Text = reader["KanGrubu"] as string ?? string.Empty;
                            textBox4.Text = reader["DogumYeri"] as string ?? string.Empty;
                            textBox5.Text = reader["AnaAdi"] as string ?? string.Empty;
                            textBox9.Text = reader["BabaAdi"] as string ?? string.Empty;
                            textBox11.Text = reader["KadroDerecesi"] as string ?? string.Empty;
                            textBox16.Text = reader["Derecesi"] as string ?? string.Empty;
                            textBox12.Text = reader["KadroMeclisKararSayisi"] as string ?? string.Empty;
                            textBox17.Text = reader["EkGosterge"] as string ?? string.Empty;
                            textBox14.Text = reader["Sendika"] as string ?? string.Empty;
                            textBox15.Text = reader["EmekliSandigiSicilNo"] as string ?? string.Empty;
                            textBox18.Text = reader["SigortaSicilNo"] as string ?? string.Empty;
                            textBox13.Text = reader["EvTel"] as string ?? string.Empty;
                            textBox21.Text = reader["CepTel"] as string ?? string.Empty;
                            textBox19.Text = reader["email"] as string ?? string.Empty;
                            richTextBox1.Text = reader["Adres"] as string ?? string.Empty;
                            textBox22.Text = reader["MezunOlduguOkul"] as string ?? string.Empty;

                            // Çocuk sayısı varsayılan olarak "0" atanır
                            textBox23.Text = reader["CocukSayisi"] != DBNull.Value ? reader["CocukSayisi"].ToString() : "0";

                            // BaslangicKidemi kontrolü
                            textBox8.Text = reader["BaslangicKidemi"] != DBNull.Value ? reader["BaslangicKidemi"].ToString() : string.Empty;

                            // ComboBox'ları doldur
                            comboBox1.SelectedItem = reader["Cinsiyet"] != DBNull.Value && Convert.ToInt32(reader["Cinsiyet"]) == 0 ? "Erkek" : "Kadın";
                            comboBox2.SelectedItem = reader["CalistigiBirim"] as string ?? string.Empty;
                            comboBox3.SelectedItem = reader["Departman"] as string ?? string.Empty;
                            comboBox5.SelectedIndex = reader["MedeniHaliKodu"] != DBNull.Value ? Convert.ToInt32(reader["MedeniHaliKodu"]) : -1;
                            comboBox8.SelectedItem = reader["DereceIlerlemeTarihi"] as string ?? string.Empty;
                            comboBox9.SelectedItem = reader["TahsilDurumu"] as string ?? string.Empty;
                            comboBox10.SelectedIndex = reader["AskerlikDurumu"] != DBNull.Value ? Convert.ToInt32(reader["AskerlikDurumu"]) : -1;

                            // Çalışan Türü (comboBox12)
                            comboBox12.Text = reader["EkAlan1"] as string ?? string.Empty;

                            // DateTimePicker'ları doldur
                            dateTimePicker1.Value = reader["DogumTarihi"] != DBNull.Value && DateTime.TryParse(reader["DogumTarihi"].ToString(), out var dogumTarihi)
                                ? dogumTarihi
                                : DateTime.Now;

                            dateTimePicker2.Value = reader["KadroMeclisKararTarihi"] != DBNull.Value && DateTime.TryParse(reader["KadroMeclisKararTarihi"].ToString(), out var kararTarihi)
                                ? kararTarihi
                                : DateTime.Now;

                            dateTimePicker3.Value = reader["KayitTarihi"] != DBNull.Value && DateTime.TryParse(reader["KayitTarihi"].ToString(), out var kayitTarihi)
                                ? kayitTarihi
                                : DateTime.Now;

                            // AcilisIzinTarihi kontrolü
                            if (reader["AcilisIzinTarihi"] != DBNull.Value && DateTime.TryParse(reader["AcilisIzinTarihi"].ToString(), out var acilisTarihi))
                            {
                                dateTimePicker4.Value = acilisTarihi;
                                dateTimePicker4.Format = DateTimePickerFormat.Short;
                            }
                            else
                            {
                                dateTimePicker4.CustomFormat = " ";
                                dateTimePicker4.Format = DateTimePickerFormat.Custom;
                            }

                            if (reader["KapanisIzinTarihi"] != DBNull.Value && DateTime.TryParse(reader["KapanisIzinTarihi"].ToString(), out var kapanisTarihi))
                            {
                                dateTimePicker5.Value = kapanisTarihi;
                                dateTimePicker5.Format = DateTimePickerFormat.Short; // Tarihi göster
                                dateTimePicker5.Checked = true; // Seçili olsun
                            }
                            else
                            {
                                dateTimePicker5.Format = DateTimePickerFormat.Custom;
                                dateTimePicker5.CustomFormat = " "; // Boş görünüm
                                dateTimePicker5.Checked = false; // Seçili olmasın
                            }

                            // Resim alanı (longblob)
                            if (!reader.IsDBNull(reader.GetOrdinal("Resim")))
                            {
                                byte[] resim = (byte[])reader["Resim"];
                                using (MemoryStream ms = new MemoryStream(resim))
                                {
                                    pictureBox1.Image = Image.FromStream(ms);
                                }
                            }
                            else
                            {
                                pictureBox1.Image = null; // Resim yoksa temizle
                            }

                            // Firma (comboBox11)
                            if (!reader.IsDBNull(reader.GetOrdinal("IsyeriID")))
                            {
                                int isyeriID = Convert.ToInt32(reader["IsyeriID"]);
                                comboBox11.SelectedValue = isyeriID;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Personel bilgileri yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void TextBoxToUpper(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox; // Gönderen TextBox nesnesi
            if (textBox != null)
            {
                textBox.Text = textBox.Text.ToUpper();
                textBox.SelectionStart = textBox.Text.Length; // İmleci sonuna getir
                textBox.SelectionLength = 0; // İmleci doğru konuma yerleştir
            }
        }

        // Bilgisayardan resim seç ve pictureBox1'de göster
        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = openFileDialog.FileName;
                pictureBox1.Image = Image.FromFile(selectedImagePath);
            }
        }

        // Resmi sil ve pictureBox1'ı temizle
        private void button2_Click_1(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            selectedImagePath = null;
        }
        public void DisableDatePickers()
        {
            dateTimePicker4.Enabled = false;
           // dateTimePicker5.Enabled = false;
        }
        private void dateTimePicker5_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker5.Checked)
            {
                dateTimePicker5.Format = DateTimePickerFormat.Short; // Tarihi göster
            }
            else
            {
                dateTimePicker5.Format = DateTimePickerFormat.Custom;
                dateTimePicker5.CustomFormat = " "; // Boş görünüm
            }
        }

        // Form yüklendiğinde işlemler
        private void PersonelGiris_Load(object sender, EventArgs e)
        {
            // textBox1 ve textBox3'ü büyük harfe çeviren olayları ekle
            textBox1.TextChanged += new EventHandler(TextBoxToUpper);
            textBox3.TextChanged += new EventHandler(TextBoxToUpper);
            // checkEdit1'in CheckedChanged olayını bağla
            checkEdit1.CheckedChanged += new EventHandler(checkEdit1_CheckedChanged);
            // dateTimePicker6'yı varsayılan ayarlarla başlat
            dateTimePicker6.Format = DateTimePickerFormat.Short;
            dateTimePicker6.Value = DateTime.Today;
            dateTimePicker6.ShowCheckBox = false;
            dateTimePicker6.ValueChanged += new EventHandler(dateTimePicker6_ValueChanged);
            // dateTimePicker5 başlangıçta boş görünmesi için ayar
            dateTimePicker5.Format = DateTimePickerFormat.Custom;
            dateTimePicker5.CustomFormat = " "; // Boş görünüm
            dateTimePicker5.Checked = false; // Seçili olmasın
            dateTimePicker5.ShowCheckBox = true; // Kullanıcının seçim yapması için checkbox göster
            dateTimePicker5.ValueChanged += new EventHandler(dateTimePicker5_ValueChanged);

            if (PersonelID > 0) // Güncelleme modunda
            {
                button4.Visible = false; // Kaydet butonunu gizle
            }
            // dateTimePicker4 ve dateTimePicker5'i kontrol et
            if (dateTimePicker4.Value == null || dateTimePicker4.Value == DateTime.MinValue)
            {
                dateTimePicker4.Format = DateTimePickerFormat.Custom;
                dateTimePicker4.CustomFormat = " ";
            }

            if (dateTimePicker5.Value == null || dateTimePicker5.Value == DateTime.MinValue)
            {
                dateTimePicker5.Format = DateTimePickerFormat.Custom;
                dateTimePicker5.CustomFormat = " ";
            }

            // comboBox12'ye çalışan türlerini ekle
            comboBox12.Items.AddRange(new string[] { "İşçi", "Memur", "Yeraltı" });

            // comboBox12'nin SelectedIndexChanged olayına event handler ekleyin
            comboBox12.SelectedIndexChanged += new EventHandler(comboBox12_SelectedIndexChanged);

            // Firmaları comboBox11'e yükleme işlemi
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query = "SELECT ID, firma FROM firmalar";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, string> firmalar = new Dictionary<int, string>();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32("ID");
                        string firmaAdi = reader.GetString("firma");
                        firmalar.Add(id, firmaAdi);
                    }

                    comboBox11.DataSource = new BindingSource(firmalar, null);
                    comboBox11.DisplayMember = "Value"; // Görünecek olan firma isimleri
                    comboBox11.ValueMember = "Key"; // Firma ID'leri
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firmalar yüklenirken hata oluştu: " + ex.Message);
            }
        }

        // comboBox12 seçim değiştiğinde izin hesaplamalarını güncelle
        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            HesaplaKidemVeIzin();
        }

        // Personelin kıdemini ve izin haklarını hesaplayıp gösteren metot
        private void HesaplaKidemVeIzin()
        {
            // Başlangıç ve bitiş tarihlerini al
            DateTime baslangicTarihi = dateTimePicker6.Value;
            DateTime? bitisTarihi = dateTimePicker5.Checked ? dateTimePicker5.Value : DateTime.Now;

            string calisanTuru = comboBox12.Text?.ToString();
            DateTime dogumTarihi = dateTimePicker1.Value;
            DateTime iseGirisTarihi = dateTimePicker6.Value;

            listBox1.Items.Clear();

            DateTime yilBasi = baslangicTarihi;
            int toplamIzinHakki = 0;
            int kidemYili = 1;

            // Yıllara göre izin haklarını hesapla
            while (yilBasi <= bitisTarihi.Value)
            {
                DateTime yilSonu = yilBasi.AddYears(1);
                int izinHakki = 0;
                if (yilSonu <= bitisTarihi.Value)
                {
                    // Yıl tamamlanmış, izin hakkını hesapla
                    izinHakki = HesaplaIzinHakki(calisanTuru, dateTimePicker1.Value, yilSonu, kidemYili, iseGirisTarihi);
                    toplamIzinHakki += izinHakki;
                }
                else if (yilBasi > DateTime.Today)
                {
                    // Bugünden sonraki yıl, izin hakkı 0
                    izinHakki = 0;
                }
                else
                {
                    // Yıl tamamlanmamış, izin hakkı 0
                    izinHakki = 0;
                }
                // Sonuçları liste kutusuna ekle
                listBox1.Items.Add($"{yilBasi:dd.MM.yyyy} - {yilSonu:dd.MM.yyyy}    " +
                    $"{kidemYili}. yıl   izin hakkı {izinHakki} gün.");
                yilBasi = yilSonu;
                kidemYili++;
            }

            // Toplam izin hakkını listeye ekle
            listBox1.Items.Add("----------------------------------------------------------");
            listBox1.Items.Add($"Toplam izin hakkı  :   {toplamIzinHakki} gündür.");

            // Kıdem yılını hesapla ve göster
            int toplamKidemYili = kidemYili - 1;
            textBox8.Text = toplamKidemYili.ToString();
            textBox8.ReadOnly = true;
        }

        private int HesaplaIzinHakki(string calisanTuru, DateTime dogumTarihi, DateTime bitisTarihi, int kidemYili, DateTime iseGirisTarihi)
        {
            int izinHakki = 0;

            // Yaşı hesapla
            int ageAtYear = bitisTarihi.Year - dogumTarihi.Year;
            if (dogumTarihi.Date > bitisTarihi.AddYears(-ageAtYear)) ageAtYear--;

            // 4857 Sayılı Kanun (2003 sonrası)
            if (bitisTarihi >= new DateTime(2003, 6, 10))
            {
                if (ageAtYear < 18)
                {
                    izinHakki = 20; // 18 yaşından küçükler için 20 gün
                }
                else if (kidemYili >= 15)
                {
                    izinHakki = 26; // 15 yıl ve üzeri kıdem için 26 gün
                }
                else if (kidemYili >= 6)
                {
                    izinHakki = 20; // 6-14 yıl arası kıdem için 20 gün
                }
                else
                {
                    izinHakki = 14; // 1-5 yıl arası kıdem için 14 gün
                }

                // Eğer kıdem 15 yıldan az ise ve yaş 50 ve üzeri ise, izin hakkını 20 güne tamamla
                if (kidemYili < 15 && ageAtYear >= 50 && izinHakki < 20)
                {
                    izinHakki = 20;
                }
            }
            else
            {
                // 1475 Sayılı Kanun (2003 öncesi)
                if (ageAtYear < 18)
                {
                    izinHakki = 18; // 18 yaşından küçükler için 18 gün
                }
                else if (kidemYili >= 15)
                {
                    izinHakki = 26; // 15 yıl ve üzeri kıdem için 26 gün
                }
                else if (kidemYili >= 6)
                {
                    izinHakki = 18; // 6-14 yıl arası kıdem için 18 gün
                }
                else
                {
                    izinHakki = 12; // 1-5 yıl arası kıdem için 12 gün
                }

                // Eğer kıdem 15 yıldan az ise ve yaş 50 ve üzeri ise, izin hakkını 20 güne tamamla
                if (kidemYili < 15 && ageAtYear >= 50 && izinHakki < 20)
                {
                    izinHakki = 20;
                }
            }

            return izinHakki;
        }

        private void dateTimePicker6_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker6.Format = DateTimePickerFormat.Short; // Tarih formatına geri döner

            HesaplaKidemVeIzin(); // Kıdem hesaplamayı yeniden yap
        }

        public int PersonelID { get; set; } // Güncellenecek personelin ID'si

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    string query = @"UPDATE personelbilgileri SET 
                Adi = @Adi, TCNo = @TCNo, Soyadi = @Soyadi, SicilNo = @SicilNo, Cinsiyet = @Cinsiyet, 
                KanGrubu = @KanGrubu, DogumYeri = @DogumYeri, DogumTarihi = @DogumTarihi, 
                AnaAdi = @AnaAdi, BabaAdi = @BabaAdi, Departman = @Departman, 
                MedeniHaliKodu = @MedeniHaliKodu, KadroDerecesi = @KadroDerecesi, Derecesi = @Derecesi, 
                KadroMeclisKararSayisi = @KadroMeclisKararSayisi, DereceIlerlemeTarihi = @DereceIlerlemeTarihi, 
                KadroMeclisKararTarihi = @KadroMeclisKararTarihi, EkGosterge = @EkGosterge, Sendika = @Sendika, 
                EmekliSandigiSicilNo = @EmekliSandigiSicilNo, SigortaSicilNo = @SigortaSicilNo, EvTel = @EvTel, 
                CepTel = @CepTel, email = @Email, Adres = @Adres, TahsilDurumu = @TahsilDurumu, 
                MezunOlduguOkul = @MezunOlduguOkul, CocukSayisi = @CocukSayisi, AskerlikDurumu = @AskerlikDurumu, 
                Resim = @Resim, IsyeriID = @IsyeriID, AcilisIzinTarihi = @AcilisIzinTarihi, 
                KapanisIzinTarihi = @KapanisIzinTarihi, BaslangicKidemi = @BaslangicKidemi 
                WHERE ID = @PersonelID";

                    MySqlCommand cmd = new MySqlCommand(query, con);

                    // Parametreleri ekliyoruz
                    cmd.Parameters.AddWithValue("@PersonelID", PersonelID);
                    cmd.Parameters.AddWithValue("@Adi", textBox1.Text);
                    cmd.Parameters.AddWithValue("@TCNo", textBox2.Text);
                    cmd.Parameters.AddWithValue("@Soyadi", textBox3.Text);
                    cmd.Parameters.AddWithValue("@SicilNo", textBox6.Text);
                    cmd.Parameters.AddWithValue("@Cinsiyet", comboBox1.SelectedIndex);
                    cmd.Parameters.AddWithValue("@KanGrubu", textBox7.Text);
                    cmd.Parameters.AddWithValue("@DogumYeri", textBox4.Text);
                    cmd.Parameters.AddWithValue("@DogumTarihi", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@AnaAdi", textBox5.Text);
                    cmd.Parameters.AddWithValue("@BabaAdi", textBox9.Text);
                    cmd.Parameters.AddWithValue("@Departman", comboBox2.SelectedItem?.ToString()); // CalistigiBirim yerine Departman
                    cmd.Parameters.AddWithValue("@MedeniHaliKodu", comboBox5.SelectedIndex);
                    cmd.Parameters.AddWithValue("@KadroDerecesi", textBox11.Text);
                    cmd.Parameters.AddWithValue("@Derecesi", textBox16.Text);
                    cmd.Parameters.AddWithValue("@KadroMeclisKararSayisi", textBox12.Text);
                    cmd.Parameters.AddWithValue("@DereceIlerlemeTarihi", comboBox8.SelectedItem?.ToString());
                    cmd.Parameters.AddWithValue("@KadroMeclisKararTarihi", dateTimePicker2.Value);
                    cmd.Parameters.AddWithValue("@EkGosterge", textBox17.Text);
                    cmd.Parameters.AddWithValue("@Sendika", textBox14.Text);
                    cmd.Parameters.AddWithValue("@EmekliSandigiSicilNo", textBox15.Text);
                    cmd.Parameters.AddWithValue("@SigortaSicilNo", textBox18.Text);
                    cmd.Parameters.AddWithValue("@EvTel", textBox13.Text);
                    cmd.Parameters.AddWithValue("@CepTel", textBox21.Text);
                    cmd.Parameters.AddWithValue("@Email", textBox19.Text);
                    cmd.Parameters.AddWithValue("@Adres", richTextBox1.Text);
                    cmd.Parameters.AddWithValue("@TahsilDurumu", comboBox9.SelectedItem?.ToString());
                    cmd.Parameters.AddWithValue("@MezunOlduguOkul", textBox22.Text);
                    cmd.Parameters.AddWithValue("@CocukSayisi", textBox23.Text);
                    cmd.Parameters.AddWithValue("@AskerlikDurumu", comboBox10.SelectedIndex);

                    // Resim ekleme işlemi (null kontrolü)
                    if (pictureBox1.Image != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                            cmd.Parameters.AddWithValue("@Resim", ms.ToArray());
                        }
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Resim", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@IsyeriID", ((KeyValuePair<int, string>)comboBox11.SelectedItem).Key);
                    cmd.Parameters.AddWithValue("@AcilisIzinTarihi", dateTimePicker6.Checked ? dateTimePicker6.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KapanisIzinTarihi", dateTimePicker5.Checked ? dateTimePicker5.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BaslangicKidemi", textBox8.Text);

                    // Komutu çalıştır
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Personel bilgileri başarıyla güncellendi.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme sırasında hata oluştu: " + ex.Message);
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xls;*.xlsx";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = false // Başlık satırını kullanma
                                }
                            });

                            var dataTable = result.Tables[0];

                            if (dataTable.Rows.Count <= 2)
                            {
                                MessageBox.Show("Excel dosyası yeterli veri içermiyor.");
                                return;
                            }

                            foreach (DataRow row in dataTable.Rows)
                            {
                                if (dataTable.Rows.IndexOf(row) < 2) // İlk iki satırı atlıyoruz
                                    continue;

                                string sicilNo = row[0]?.ToString();
                                string tcNo = row[1]?.ToString();
                                string adi = row[2]?.ToString();
                                string soyadi = row[3]?.ToString();
                                string departman = row[5]?.ToString();
                                DateTime? dogumTarihi = DateTime.TryParse(row[6]?.ToString(), out var parsedDogumTarihi)
                                    ? parsedDogumTarihi
                                    : null;
                                DateTime? acilisIzinTarihi = DateTime.TryParse(row[7]?.ToString(), out var parsedAcilisTarihi)
                                    ? parsedAcilisTarihi
                                    : DateTime.Now;

                                // Yeni eklenen kod: KapanisIzinTarihi ve Akit alanları
                                string kapanisIzinTarihiStr = row[8]?.ToString();
                                DateTime? kapanisIzinTarihi = DateTime.TryParse(kapanisIzinTarihiStr, out var parsedKapanisIzinTarihi)
                                    ? parsedKapanisIzinTarihi
                                    : (DateTime?)null;

                                int akit = kapanisIzinTarihi.HasValue ? 1 : 0;

                                // Eksik veya hatalı alan kontrolü
                                if (string.IsNullOrEmpty(sicilNo) || string.IsNullOrEmpty(tcNo) || string.IsNullOrEmpty(adi) || string.IsNullOrEmpty(soyadi))
                                {
                                    MessageBox.Show($"Eksik zorunlu alanlar: SicilNo, TCNo, Adi veya Soyadi boş olamaz. Satır: {dataTable.Rows.IndexOf(row) + 1}");
                                    continue;
                                }

                                // Doğum tarihi kontrolü
                                if (!dogumTarihi.HasValue)
                                {
                                    MessageBox.Show($"Eksik veya hatalı doğum tarihi. Satır: {dataTable.Rows.IndexOf(row) + 1}");
                                    continue;
                                }

                                using (MySqlConnection con = DatabaseHelper.GetConnection())
                                {
                                    con.Open();
                                    MySqlTransaction transaction = con.BeginTransaction();
                                    try
                                    {
                                        // Personel bilgilerini kaydet
                                        string personelQuery = @"INSERT INTO personelbilgileri 
                                (SicilNo, TCNo, Adi, Soyadi, Departman, DogumTarihi, AcilisIzinTarihi, KapanisIzinTarihi, Akit, IsyeriID, CocukSayisi, EkAlan1, BaslangicKidemi) 
                                VALUES (@SicilNo, @TCNo, @Adi, @Soyadi, @Departman, @DogumTarihi, @AcilisIzinTarihi, @KapanisIzinTarihi, @Akit, @IsyeriID, @CocukSayisi, @EkAlan1, @BaslangicKidemi)";
                                        MySqlCommand personelCmd = new MySqlCommand(personelQuery, con, transaction);

                                        // Başlangıç tarihine göre kıdem hesaplanıyor
                                        int baslangicKidemi = acilisIzinTarihi.HasValue
                                            ? DateTime.Now.Year - acilisIzinTarihi.Value.Year
                                            : 0;

                                        personelCmd.Parameters.AddWithValue("@SicilNo", sicilNo);
                                        personelCmd.Parameters.AddWithValue("@TCNo", tcNo);
                                        personelCmd.Parameters.AddWithValue("@Adi", adi);
                                        personelCmd.Parameters.AddWithValue("@Soyadi", soyadi);
                                        personelCmd.Parameters.AddWithValue("@Departman", (object)departman ?? DBNull.Value);
                                        personelCmd.Parameters.AddWithValue("@DogumTarihi", dogumTarihi.Value);
                                        personelCmd.Parameters.AddWithValue("@AcilisIzinTarihi", acilisIzinTarihi.Value);
                                        personelCmd.Parameters.AddWithValue("@KapanisIzinTarihi", kapanisIzinTarihi.HasValue ? (object)kapanisIzinTarihi.Value : DBNull.Value);
                                        personelCmd.Parameters.AddWithValue("@Akit", akit);
                                        personelCmd.Parameters.AddWithValue("@IsyeriID", 6);
                                        personelCmd.Parameters.AddWithValue("@CocukSayisi", 0);
                                        personelCmd.Parameters.AddWithValue("@EkAlan1", "İşçi");
                                        personelCmd.Parameters.AddWithValue("@BaslangicKidemi", baslangicKidemi);

                                        personelCmd.ExecuteNonQuery();

                                        // Yeni eklenen personel ID'sini al
                                        long personelID = personelCmd.LastInsertedId;

                                        // İzin Tablosu için hesaplamalar
                                        DateTime baslangicTarihi = acilisIzinTarihi.Value;
                                        DateTime bitisTarihi = baslangicTarihi.AddYears(1);
                                        decimal devredenIzin = 0;
                                        int kidemYili = 1;

                                        while (baslangicTarihi.Year <= DateTime.Now.Year)
                                        {
                                            int yillikHak = 0;

                                            // Yıllık Hak sadece tamamlanmış yıllar için ekleniyor
                                            if (bitisTarihi <= DateTime.Now)
                                            {
                                                // Yıllık izin hakkını hesaplayın
                                                yillikHak = HesaplaIzinHakki("İşçi", dogumTarihi.Value, bitisTarihi, kidemYili, acilisIzinTarihi.Value);
                                            }
                                            else if (baslangicTarihi > DateTime.Today)
                                            {
                                                // Bugünden sonraki yıl, izin hakkı 0
                                                yillikHak = 0;
                                            }
                                            else
                                            {
                                                // Yıl tamamlanmamış, izin hakkı 0
                                                yillikHak = 0;
                                            }

                                            decimal kulHak = 0; // Kullanılan izin
                                            decimal devirAlinan = devredenIzin; // Devir alınan önceki yıldan gelen hak
                                            devredenIzin += yillikHak - kulHak; // Devreden izin hesaplama

                                            string izinQuery = @"INSERT INTO izintablolari 
                                        (PersonelID, BaslangicTarihi, BitisTarihi, DevirAlinan, YillikHak, KulHak, Devreden, Kidem) 
                                        VALUES (@PersonelID, @BaslangicTarihi, @BitisTarihi, @DevirAlinan, @YillikHak, @KulHak, @Devreden, @Kidem)";
                                            MySqlCommand izinCmd = new MySqlCommand(izinQuery, con, transaction);

                                            izinCmd.Parameters.AddWithValue("@PersonelID", personelID);
                                            izinCmd.Parameters.AddWithValue("@BaslangicTarihi", baslangicTarihi);
                                            izinCmd.Parameters.AddWithValue("@BitisTarihi", bitisTarihi);
                                            izinCmd.Parameters.AddWithValue("@DevirAlinan", devirAlinan);
                                            izinCmd.Parameters.AddWithValue("@YillikHak", yillikHak);
                                            izinCmd.Parameters.AddWithValue("@KulHak", kulHak);
                                            izinCmd.Parameters.AddWithValue("@Devreden", devredenIzin);
                                            izinCmd.Parameters.AddWithValue("@Kidem", kidemYili);

                                            izinCmd.ExecuteNonQuery();

                                            baslangicTarihi = bitisTarihi;
                                            bitisTarihi = baslangicTarihi.AddYears(1);
                                            kidemYili++;
                                        }

                                        transaction.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        MessageBox.Show("Hata oluştu: " + ex.Message);
                                    }
                                }
                            }

                            MessageBox.Show("Excel verileri başarıyla eklendi ve izin tablosu oluşturuldu.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
        }

    }
}
