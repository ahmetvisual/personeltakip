using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace personelizintakip
{
    public partial class tatildetayi : Form
    {
        // Tatildetayi formunu açarken resmitatiller formundaki işaretleme metoduna erişim için bir referans alıyoruz
        private resmitatiller _parentForm;

        public tatildetayi(resmitatiller parentForm)
        {
            InitializeComponent();
            _parentForm = parentForm;
            this.Load += Tatildetayi_Load;
        }

        // Dışarıdan gelen tarih, izin durumu ve açıklama bilgilerini saklamak için
        public DateTime SecilenTarih { get; set; }
        public string Aciklama { get; set; }
        public string IzinDurumu { get; set; }

        private void Tatildetayi_Load(object sender, EventArgs e)
        {
            // ComboBox'a seçenekleri ekleyelim
            comboBox1.Items.Add("Tam Gün");
            comboBox1.Items.Add("Yarım Gün");

            // Eğer var olan izin bilgileri varsa dolduralım
            if (!string.IsNullOrEmpty(IzinDurumu))
            {
                comboBox1.SelectedItem = IzinDurumu; // Önceden seçilmiş izin durumu
                textBox1.Text = Aciklama; // Önceden kaydedilmiş açıklama
            }
            else
            {
                comboBox1.SelectedIndex = 0; // Varsayılan olarak Tam Gün seçili olsun
            }

            // Form başlığında seçilen tarihi gösterelim
            this.Text = $"{SecilenTarih:dd MMMM yyyy} İzin Detayı";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Formda girilen izin bilgilerini alalım
            DateTime secilenTarih = SecilenTarih;
            string izinDurumu = comboBox1.SelectedItem.ToString();
            string aciklama = textBox1.Text;

            // Tam gün mü, yarım gün mü bilgisini hazırlayalım
            bool tamGun = izinDurumu == "Tam Gün";

            try
            {
                using (MySqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    // Eğer tarih zaten kayıtlıysa güncelle, yoksa yeni ekleme yap
                    string query = "INSERT INTO tatiller (Tarih, Aciklama, TamGunMu) " +
                                   "VALUES (@Tarih, @Aciklama, @TamGunMu) " +
                                   "ON DUPLICATE KEY UPDATE Aciklama = @Aciklama, TamGunMu = @TamGunMu";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Tarih", secilenTarih);
                        cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                        cmd.Parameters.AddWithValue("@TamGunMu", tamGun ? 1 : 0);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Formu kapatmadan önce ana formda işaretlemeleri güncelleyelim
                _parentForm.MarkSavedHolidayDays();

                // Başarı mesajı
                MessageBox.Show("Tatil bilgisi başarıyla kaydedildi.");

                // Formu kapat ve DialogResult olarak OK döndür
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bu tatil bilgisini silmek istediğinize emin misiniz?",
                                         "Onay",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();

                        string query = "DELETE FROM tatiller WHERE Tarih = @Tarih";
                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Tarih", SecilenTarih);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Ana formdaki işaretlemeleri güncelle
                    _parentForm.MarkSavedHolidayDays();

                    // ListBox'ı güncelle
                    _parentForm.LoadTatiller();

                    MessageBox.Show("Tatil bilgisi başarıyla silindi.");

                    // DialogResult'ı Delete olarak ayarla
                    this.DialogResult = DialogResult.Abort; // veya başka bir değer kullanabilirsiniz
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }

    }
}
