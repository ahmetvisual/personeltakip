using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

public static class IzinHelper
{
    public static void UpdateIzinTablolari(MySqlConnection con, MySqlTransaction transaction, int personelID,
                                            DateTime baslangicTarihi, DateTime bitisTarihi, decimal kalanGun, bool isAdding)
    {
        string izinTabloQuery = @"SELECT ID, BaslangicTarihi, BitisTarihi, KulHak, Devreden, DevirAlinan, YillikHak, Kidem 
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
                    YillikHak = reader.GetDecimal("YillikHak"),
                    Kidem = reader.GetInt32("Kidem")
                };
                izinTabloList.Add(record);
            }
        }

        // Eksik yıl var mı kontrol et ve ekle
        DateTime current = new DateTime(baslangicTarihi.Year, baslangicTarihi.Month, baslangicTarihi.Day);
        while (current < bitisTarihi)
        {
            bool exists = izinTabloList.Exists(x => current >= x.BaslangicTarihi && current < x.BitisTarihi);
            if (!exists)
            {
                // Personel bilgilerini çek
                string personelQuery = "SELECT DogumTarihi, AcilisIzinTarihi, EkAlan1 FROM personelbilgileri WHERE ID = @PersonelID";
                MySqlCommand personelCmd = new MySqlCommand(personelQuery, con, transaction);
                personelCmd.Parameters.AddWithValue("@PersonelID", personelID);
                DateTime dogumTarihi = DateTime.MinValue;
                DateTime acilisIzinTarihi = DateTime.MinValue;
                string calisanTuru = "İşçi";
                using (var reader = personelCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dogumTarihi = reader.GetDateTime("DogumTarihi");
                        acilisIzinTarihi = reader.GetDateTime("AcilisIzinTarihi");
                        calisanTuru = reader.GetString("EkAlan1");
                    }
                }
                // Kıdem yılını hesapla
                int kidemYili = current.Year - acilisIzinTarihi.Year + 1;
                if (acilisIzinTarihi.Date > current.AddYears(-kidemYili + 1)) kidemYili--;
                if (kidemYili < 1) kidemYili = 1;
                // Yıl aralığı
                DateTime yilBaslangic = new DateTime(current.Year, acilisIzinTarihi.Month, acilisIzinTarihi.Day);
                DateTime yilBitis = yilBaslangic.AddYears(1);
                // Yıllık hak hesapla
                int izinHakki = (yilBaslangic > DateTime.Today) ? 0 : HesaplaIzinHakki(calisanTuru, dogumTarihi, yilBitis, kidemYili, acilisIzinTarihi);
                // DevirAlinan'ı önceki yıldan al
                decimal devirAlinan = 0;
                if (izinTabloList.Count > 0)
                {
                    var onceki = izinTabloList.Last();
                    devirAlinan = onceki.Devreden;
                }
                // Yeni satırı ekle
                string insertQuery = @"INSERT INTO izintablolari (PersonelID, BaslangicTarihi, BitisTarihi, DevirAlinan, YillikHak, KulHak, Devreden, Kidem) 
                    VALUES (@PersonelID, @BaslangicTarihi, @BitisTarihi, @DevirAlinan, @YillikHak, 0, @Devreden, @Kidem)";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, con, transaction);
                insertCmd.Parameters.AddWithValue("@PersonelID", personelID);
                insertCmd.Parameters.AddWithValue("@BaslangicTarihi", yilBaslangic);
                insertCmd.Parameters.AddWithValue("@BitisTarihi", yilBitis);
                insertCmd.Parameters.AddWithValue("@DevirAlinan", devirAlinan);
                insertCmd.Parameters.AddWithValue("@YillikHak", izinHakki);
                insertCmd.Parameters.AddWithValue("@Devreden", devirAlinan + izinHakki);
                insertCmd.Parameters.AddWithValue("@Kidem", kidemYili);
                insertCmd.ExecuteNonQuery();
                // Listeye ekle
                izinTabloList.Add(new IzinTabloRecord {
                    ID = 0, // yeni eklendi, tekrar select ile çekilebilir
                    BaslangicTarihi = yilBaslangic,
                    BitisTarihi = yilBitis,
                    KulHak = 0,
                    Devreden = devirAlinan + izinHakki,
                    DevirAlinan = devirAlinan,
                    YillikHak = izinHakki,
                    Kidem = kidemYili
                });
            }
            current = current.AddYears(1);
        }
        // Güncelleme algoritmasını çalıştır
        izinTabloList = izinTabloList.OrderBy(x => x.BaslangicTarihi).ToList();
        decimal totalKalanGun = kalanGun;
        decimal previousDevreden = 0;
        for (int i = 0; i < izinTabloList.Count; i++)
        {
            var currentRecord = izinTabloList[i];
            DateTime overlapStart = baslangicTarihi > currentRecord.BaslangicTarihi ? baslangicTarihi : currentRecord.BaslangicTarihi;
            DateTime overlapEnd = bitisTarihi < currentRecord.BitisTarihi ? bitisTarihi : currentRecord.BitisTarihi;
            if (overlapStart <= overlapEnd)
            {
                decimal daysInThisRecord = (decimal)(overlapEnd - overlapStart).TotalDays + 1;
                if (totalKalanGun < daysInThisRecord)
                    daysInThisRecord = totalKalanGun;
                decimal yeniKulHak = currentRecord.KulHak;
                decimal yeniDevreden = currentRecord.Devreden;
                if (isAdding)
                {
                    yeniKulHak += daysInThisRecord;
                }
                else
                {
                    yeniKulHak -= daysInThisRecord;
                }
                yeniDevreden = currentRecord.YillikHak - yeniKulHak + currentRecord.DevirAlinan;
                string updateQuery = @"UPDATE izintablolari 
                                     SET KulHak = @KulHak, 
                                         Devreden = @Devreden 
                                     WHERE PersonelID = @PersonelID AND BaslangicTarihi = @BaslangicTarihi";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, con, transaction);
                updateCmd.Parameters.AddWithValue("@KulHak", yeniKulHak);
                updateCmd.Parameters.AddWithValue("@Devreden", yeniDevreden);
                updateCmd.Parameters.AddWithValue("@PersonelID", personelID);
                updateCmd.Parameters.AddWithValue("@BaslangicTarihi", currentRecord.BaslangicTarihi);
                updateCmd.ExecuteNonQuery();
                previousDevreden = yeniDevreden;
                totalKalanGun -= daysInThisRecord;
                if (totalKalanGun <= 0)
                    break;
            }
            else if (currentRecord.BaslangicTarihi > bitisTarihi)
            {
                string updateQuery = @"UPDATE izintablolari 
                                     SET DevirAlinan = @DevirAlinan 
                                     WHERE PersonelID = @PersonelID AND BaslangicTarihi = @BaslangicTarihi";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, con, transaction);
                updateCmd.Parameters.AddWithValue("@DevirAlinan", previousDevreden);
                updateCmd.Parameters.AddWithValue("@PersonelID", personelID);
                updateCmd.Parameters.AddWithValue("@BaslangicTarihi", currentRecord.BaslangicTarihi);
                updateCmd.ExecuteNonQuery();
            }
        }
    }

    // PersonelGiris.cs'deki HesaplaIzinHakki fonksiyonunun aynısı
    public static int HesaplaIzinHakki(string calisanTuru, DateTime dogumTarihi, DateTime bitisTarihi, int kidemYili, DateTime iseGirisTarihi)
    {
        int izinHakki = 0;
        int ageAtYear = bitisTarihi.Year - dogumTarihi.Year;
        if (dogumTarihi.Date > bitisTarihi.AddYears(-ageAtYear)) ageAtYear--;
        if (bitisTarihi >= new DateTime(2003, 6, 10))
        {
            if (ageAtYear < 18)
                izinHakki = 20;
            else if (kidemYili >= 15)
                izinHakki = 26;
            else if (kidemYili >= 6)
                izinHakki = 20;
            else
                izinHakki = 14;
            if (kidemYili < 15 && ageAtYear >= 50 && izinHakki < 20)
                izinHakki = 20;
        }
        else
        {
            if (ageAtYear < 18)
                izinHakki = 18;
            else if (kidemYili >= 15)
                izinHakki = 26;
            else if (kidemYili >= 6)
                izinHakki = 18;
            else
                izinHakki = 12;
            if (kidemYili < 15 && ageAtYear >= 50 && izinHakki < 20)
                izinHakki = 20;
        }
        return izinHakki;
    }

    public class IzinTabloRecord
    {
        public int ID { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public decimal KulHak { get; set; }
        public decimal Devreden { get; set; }
        public decimal DevirAlinan { get; set; }
        public decimal YillikHak { get; set; }
        public int Kidem { get; set; }
    }
}
