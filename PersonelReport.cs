using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraReports.UI;
using MySql.Data.MySqlClient;
using DevExpress.XtraPrinting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace personelizintakip
{
    public partial class PersonelReport : Form
    {
        public PersonelReport()
        {
            InitializeComponent();
            this.Load += new EventHandler(PersonelReport_Load); // Form yüklendiğinde çağrılacak metot
        }

        private void PersonelReport_Load(object sender, EventArgs e)
        {
            LoadDepartmentsIntoCheckedComboBox();
            LoadAllDataToGrid(); // Form açıldığında tüm listeyi doldur

            // DataGridView tasarım ayarları
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#34495e");
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font(dataGridView1.Font, System.Drawing.FontStyle.Bold);
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dataGridView1.BackgroundColor = System.Drawing.Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightSteelBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
        }

        private void LoadDepartmentsIntoCheckedComboBox()
        {
            try
            {
                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT DISTINCT Departman FROM personel_izin_ozet_view ORDER BY Departman ASC";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    // CheckedComboBoxEdit öğelerini temizle
                    checkedComboBoxEdit1.Properties.Items.Clear();

                    // Departmanları ekle
                    while (reader.Read())
                    {
                        string department = reader["Departman"].ToString();
                        if (!string.IsNullOrEmpty(department))
                        {
                            checkedComboBoxEdit1.Properties.Items.Add(department, CheckState.Unchecked);
                        }
                    }

                    // Departman metnini göster
                    // Form ilk açıldığında "Departman" yazsın:
                    checkedComboBoxEdit1.Text = "Departman";
                    checkedComboBoxEdit1.RefreshEditValue();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Departmanlar yüklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadAllDataToGrid()
        {
            try
            {
                var dataSource = GetAllDataSource(); // Tüm veriyi al
                if (dataSource is DataTable dataTable)
                {
                    dataGridView1.DataSource = dataTable; // DataGridView'e tüm veriyi ata

                    // "Akit" sütununu gizle
                    if (dataGridView1.Columns.Contains("Akit"))
                    {
                        dataGridView1.Columns["Akit"].Visible = false;
                    }

                    // Sütun genişliklerini DataGridView alanını dolduracak şekilde ayarla
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tüm veri yüklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                // DataGridView'den veriyi al ve rapora gönder
                if (dataGridView1.DataSource is DataTable dataTable)
                {
                    ShowReport(dataTable);
                }
                else
                {
                    MessageBox.Show("Rapor oluşturmak için önce listeleme yapın.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rapor oluşturulurken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            try
            {
                // Departman seçimine göre listele
                var dataSource = GetDataSource(); // Seçilen departmanlara göre veri al
                if (dataSource is DataTable dataTable)
                {
                    dataGridView1.DataSource = dataTable; // DataGridView'e veriyi ata

                    // "Akit" sütununu gizle
                    if (dataGridView1.Columns.Contains("Akit"))
                    {
                        dataGridView1.Columns["Akit"].Visible = false;
                    }

                    // Sütun genişliklerini DataGridView alanını dolduracak şekilde ayarla
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri listeleme sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private object GetAllDataSource()
        {
            try
            {
                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT * FROM personel_izin_ozet_view"; // Tüm veriyi getir
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tüm veri çekilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private object GetDataSource()
        {
            try
            {
                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    List<string> selectedDepartments = new List<string>();

                    // Seçilen öğeleri al
                    foreach (var item in checkedComboBoxEdit1.Properties.Items)
                    {
                        CheckedListBoxItem listItem = item as CheckedListBoxItem;
                        if (listItem != null && listItem.CheckState == CheckState.Checked)
                        {
                            selectedDepartments.Add(listItem.Value.ToString());
                        }
                    }

                    string query = "SELECT * FROM personel_izin_ozet_view";

                    // Eğer seçili departman var ise onları filtrele
                    if (selectedDepartments.Count > 0)
                    {
                        string departmentsInClause = string.Join(", ", selectedDepartments.Select((s, i) => $"@dep{i}"));
                        query += $" WHERE Departman IN ({departmentsInClause})";

                        // checkButton1 seçili ise Akit=0 olanları da ayrıca filtrele
                        if (checkButton1.Checked)
                        {
                            query += " AND Akit=0";
                        }

                        MySqlCommand command = new MySqlCommand(query, connection);
                        for (int i = 0; i < selectedDepartments.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@dep{i}", selectedDepartments[i]);
                        }

                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                    else
                    {
                        // Seçili departman yoksa sadece checkButton1 durumuna göre filtre uygula
                        if (checkButton1.Checked)
                        {
                            query += " WHERE Akit=0";
                        }

                        MySqlCommand command = new MySqlCommand(query, connection);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri çekilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void ShowReport(DataTable dataTable)
        {
            try
            {
                string reportPath = Application.StartupPath + @"\Rapor\personellist.repx";

                if (!System.IO.File.Exists(reportPath))
                {
                    MessageBox.Show("Rapor dosyası bulunamadı: " + reportPath, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                XtraReport report = new XtraReport();
                report.LoadLayout(reportPath);

                report.DataSource = dataTable;
                report.DataMember = dataTable.TableName;

                // Raporu önceden hazırla
                report.CreateDocument();

                // Rapor yazdırma aracını oluştur
                ReportPrintTool printTool = new ReportPrintTool(report);

                // Burada ShowMarginsWarning özelliğini false yapın.
                printTool.PrintingSystem.ShowMarginsWarning = false;

                // Raporu ön izleme diyalogunda göster
                printTool.ShowPreviewDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rapor oluşturulurken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
