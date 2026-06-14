using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net.Http; // HttpClient için gerekli

namespace personelizintakip
{
    public partial class LoginForm : Form
    {
        public static int LoggedInUserID; // Kullanıcı ID'sini global olarak tutmak için
        public static string LoggedInUser; // Kullanıcı adını global olarak tutmak için
        public static int UserYetki; // Yetkiyi global olarak tutmak için

        private static readonly byte[] key = Convert.FromBase64String("KkqVXvBPCzUQFvYzX8M+QeFDnm7LUQYaxRBRD7hK4nI=");
        private static readonly byte[] iv = Convert.FromBase64String("aVxO7TQLgX2FpCJYxKTxwQ==");

        public LoginForm()
        {
            InitializeComponent();

            // Şifre metin kutusunu yıldız(*) ile gizle
            textBox2.UseSystemPasswordChar = true;
            textBox2.KeyPress += new KeyPressEventHandler(textBox2_KeyPress);

            // Versiyon bilgisini göster
            labelVersion.Text = "Versiyon: " + GetCurrentVersion();

            // HTTP lisans kontrolü
            if (!CheckHttpAccess())
            {
                // Lisans doğrulaması başarısızsa formu kapatıyoruz
                this.Close();
                return;
            }

            // Form load olduğunda çalışacak event
            this.Load += LoginForm_Load;
        }

        // Form açıldığında (Load) çağrılacak metod
        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Eğer kullanıcı daha önce "Beni Hatırla" işaretlemişse:
            if (Properties.Settings.Default.RememberMe)
            {
                // Kaydedilen kullanıcı adını getir
                string savedUser = Properties.Settings.Default.SavedUsername;
                // Kaydedilen şifre şifreli olarak tutuluyor; çözüp getiriyoruz
                string savedPassEncrypted = Properties.Settings.Default.SavedPassword;

                if (!string.IsNullOrEmpty(savedUser) && !string.IsNullOrEmpty(savedPassEncrypted))
                {
                    try
                    {
                        string decryptedPass = Decrypt(savedPassEncrypted);
                        textBox1.Text = savedUser;
                        textBox2.Text = decryptedPass;
                        checkBox1.Checked = true; // Çek kutusunu işaretliyoruz
                    }
                    catch
                    {
                        // Eğer şifre çözme sırasında bir hata olduysa, ayarları sıfırlayalım
                        Properties.Settings.Default.SavedUsername = "";
                        Properties.Settings.Default.SavedPassword = "";
                        Properties.Settings.Default.RememberMe = false;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Enter tuşunun varsayılan davranışını engelle
                button1_Click(this, new EventArgs()); // Giriş butonunun click olayını tetikle
            }
        }

        private bool CheckHttpAccess()
        {
            // Yeni PHP dosyasının URL'sini belirliyoruz
            string httpAddress = "https://ztlicense.com/checklicense_ziylantaban_personeltakip.php";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(httpAddress);
                request.Timeout = 5000; // 5 saniye zaman aşımı
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseText = reader.ReadToEnd();
                            // JSON yanıtını çözümleyin
                            dynamic jsonResponse = JsonConvert.DeserializeObject(responseText);

                            // Debug bilgilerini logla (isteğe bağlı)
                            if (jsonResponse.debug != null)
                            {
                                Console.WriteLine("Start Date: " + jsonResponse.debug.startDate);
                                Console.WriteLine("Current Date: " + jsonResponse.debug.currentDate);
                                Console.WriteLine("Valid Until Date: " + jsonResponse.debug.validUntilDate);
                            }

                            // Lisans durumu kontrolü
                            if (jsonResponse.status == "ok")
                            {
                                return true; // Lisans geçerli
                            }
                            else if (jsonResponse.status == "error" && jsonResponse.message != null)
                            {
                                MessageBox.Show(jsonResponse.message.ToString(), "Lisans Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Application.Exit();
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lisans doğrulama hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            return false;
        }

        private async Task<bool> FileExistsWithTimeout(string filePath, int timeoutMilliseconds)
        {
            using (var cancellationTokenSource = new System.Threading.CancellationTokenSource())
            {
                Task<bool> task = Task.Run(() => File.Exists(filePath), cancellationTokenSource.Token);
                if (await Task.WhenAny(task, Task.Delay(timeoutMilliseconds)) == task)
                {
                    return task.Result;
                }
                else
                {
                    throw new TimeoutException($"Dosya erişimi zaman aşımına uğradı: {filePath}");
                }
            }
        }

        private async Task<string> ReadAllTextWithTimeout(string filePath, int timeoutMilliseconds)
        {
            using (var cancellationTokenSource = new System.Threading.CancellationTokenSource())
            {
                Task<string> task = Task.Run(() => File.ReadAllText(filePath), cancellationTokenSource.Token);
                if (await Task.WhenAny(task, Task.Delay(timeoutMilliseconds)) == task)
                {
                    return task.Result;
                }
                else
                {
                    throw new TimeoutException($"Dosya okuma işlemi zaman aşımına uğradı: {filePath}");
                }
            }
        }

        private string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        private string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = textBox1.Text;
            string sifre = textBox2.Text;

            // -------------------------
            //  “Beni Hatırla” Mantığı
            // -------------------------
            if (checkBox1.Checked)
            {
                // Kullanıcı “Beni Hatırla” işaretlediyse:
                Properties.Settings.Default.SavedUsername = kullaniciAdi;
                // Şifreyi şifreleyip kaydet
                Properties.Settings.Default.SavedPassword = Encrypt(sifre);
                Properties.Settings.Default.RememberMe = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                // İşaretli değilse, önceki kayıtlı bilgileri sıfırla:
                Properties.Settings.Default.SavedUsername = "";
                Properties.Settings.Default.SavedPassword = "";
                Properties.Settings.Default.RememberMe = false;
                Properties.Settings.Default.Save();
            }
            // ----------------------------------------

            using (MySqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT YonetID, Yetki FROM yonetheader WHERE KullaniciAdi = @KullaniciAdi AND Sifre = @Sifre",
                    con);
                cmd.Parameters.AddWithValue("@KullaniciAdi", kullaniciAdi);
                cmd.Parameters.AddWithValue("@Sifre", sifre);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // YonetID'nin null olup olmadığını kontrol et
                        if (reader["YonetID"] != DBNull.Value)
                        {
                            LoggedInUserID = Convert.ToInt32(reader["YonetID"]);
                        }
                        else
                        {
                            MessageBox.Show("YonetID bilgisi alınamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; // Eğer YonetID boş ise işlemi sonlandır
                        }

                        // Yetki'yi al
                        if (reader["Yetki"] != DBNull.Value)
                        {
                            bool yetkiBit = Convert.ToBoolean(reader["Yetki"]);
                            UserYetki = yetkiBit ? 1 : 0;
                        }
                        else
                        {
                            UserYetki = 0;
                        }

                        LoggedInUser = kullaniciAdi;

                        // Modül bilgilerini al
                        reader.Close();

                        cmd = new MySqlCommand(
                            "SELECT ModulAdi, Yetki FROM yetkiheader WHERE Kullanici = @KullaniciAdi",
                            con);
                        cmd.Parameters.AddWithValue("@KullaniciAdi", kullaniciAdi);
                        MySqlDataReader moduleReader = cmd.ExecuteReader();

                        Dictionary<string, bool> moduleAccess = new Dictionary<string, bool>();
                        while (moduleReader.Read())
                        {
                            string modulAdi = moduleReader["ModulAdi"].ToString();
                            bool yetki;
                            if (moduleReader["Yetki"] is bool)
                            {
                                yetki = (bool)moduleReader["Yetki"];
                            }
                            else
                            {
                                yetki = Convert.ToBoolean(moduleReader["Yetki"]);
                            }
                            moduleAccess[modulAdi] = yetki;
                        }
                        moduleReader.Close();

                        // Ana formu açarken yetkileri ilet
                        this.Hide();
                        Form1 mainForm = new Form1();
                        mainForm.SetModulePermissions(moduleAccess);
                        mainForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı adı veya şifre hatalı!");
                    }
                }
            }
        }

        private string GetCurrentVersion()
        {
            return $"{Application.ProductVersion}";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // (Varsa başka işlemler)
        }
    }
}
