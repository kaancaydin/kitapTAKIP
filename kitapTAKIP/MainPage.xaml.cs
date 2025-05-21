
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;


namespace kitapTAKIP
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<StokModel> _kitapListesi = new ObservableCollection<StokModel>();
        private ObservableCollection<YazarKitapModel> yazarKitapListesi = new ObservableCollection<YazarKitapModel>();


        public MainPage()
        {
            InitializeComponent();
            StokCollectionView.ItemsSource = _kitapListesi; // bu collectionView'ları bağlamayı unutma
            yazarKitapCollectionView.ItemsSource = yazarKitapListesi;  // YazarKitapListesi'ni CollectionView'a bağla

        }

        static SQLiteConnection bag = new SQLiteConnection(@"Data Source =C:\Users\kaanc\source\repos\kitapTAKIP\database\ kitap.db");
        private async void OnCounterClicked(object sender, EventArgs e)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "CREATE TABLE IF NOT EXISTS books(" +
             "no INTEGER PRIMARY KEY AUTOINCREMENT," +
             "kitapAdi TEXT NOT NULL," +
             "yazar TEXT NOT NULL )";

            await DisplayAlert("Veritabanı Bağlandı", bag.State.ToString(), "Tamam");

            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            cmd.ExecuteNonQuery();
            BindingContext = this;


        }

        public class StokModel
        {
            public int no { get; set; }
            public string kitapAdi { get; set; }
            public string yazar { get; set; }
        }

        private  void Liste()
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "select * from books";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            SQLiteDataReader dr = cmd.ExecuteReader();
            _kitapListesi.Clear(); // Önce listeyi temizle
            while (dr.Read())
            {
                _kitapListesi.Add(new StokModel
                {
                    no = Convert.ToInt32(dr["no"]),
                    kitapAdi = dr["kitapAdi"].ToString(),
                    yazar = dr["yazar"].ToString()
                });
            }
            dr.Close();
        }



        void ekleme(string kitapAdi, string yazar)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "INSERT INTO books (kitapAdi, yazar) VALUES (@kitapAdi, @yazar)";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);

            cmd.Parameters.AddWithValue("@kitapAdi", kitapAdi);
            cmd.Parameters.AddWithValue("@yazar", yazar);

            cmd.ExecuteNonQuery();


        }

        static void Sil(int no)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "DELETE FROM books WHERE no = @no";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            cmd.Parameters.AddWithValue("@no", no);

            cmd.ExecuteNonQuery();
        }

        private void ekle_Clicked(object sender, EventArgs e)
        {
            string kitapAdi = txtKitap.Text;
            string yazar = txtYazar.Text;

            if (string.IsNullOrWhiteSpace(kitapAdi) || string.IsNullOrWhiteSpace(kitapAdi))
            {
                DisplayAlert("Hata", "Lütfen tüm alanları doğru bir şekilde doldurun.", "Tamam");
                return;
            }

            // Ekleme işlemi
            ekleme(kitapAdi, yazar);
            DisplayAlert("Başarılı", "Ürün başarıyla eklendi.", "Tamam");

            // Alanları temizle
            txtKitap.Text = "";
            txtYazar.Text = "";


            Liste();


        }

        private bool isListVisible = false;


        private void liste_Clicked(object sender, EventArgs e)
        {
            Liste();

            if (listeAD.IsVisible == false)
            {
                listeAD.IsVisible = true;
            }
            else
            {
                listeAD.IsVisible = false;
            }



            isListVisible = !isListVisible;  // Durumu tersine çevir

            if (isListVisible)
            {
                // Listeyi göster
                StokCollectionView.IsVisible = true;
                Liste();  // Listeyi güncelle
            }
            else
            {
                // Listeyi gizle
                StokCollectionView.IsVisible = false;
            }

        }

        void Guncelleme(int no, string yeniKitap, string yeniYazar)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();


            string sql = "UPDATE books SET kitapAdi = @yeniKitap, yazar = @yeniYazar WHERE no = @no";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);

            // Parametreleri ekliyoruz
            cmd.Parameters.AddWithValue("@no", no);
            cmd.Parameters.AddWithValue("@yeniKitap", yeniKitap);
            cmd.Parameters.AddWithValue("@yeniYazar", yeniYazar);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                DisplayAlert("Başarılı", "Kayıt başarıyla güncellendi.", "Tamam");
            }
            else
            {
                DisplayAlert("Hata", "Güncelleme işlemi başarısız oldu. Belirtilen ID bulunamadı.", "Tamam");
            }
        }

        private void guncelleme_Clicked(object sender, EventArgs e)
        {
            if (int.TryParse(noGuncel.Text, out int no) &&
            !string.IsNullOrWhiteSpace(adGuncel.Text) &&
            !string.IsNullOrWhiteSpace(yazarGuncel.Text))

            {
                Guncelleme(no, adGuncel.Text, yazarGuncel.Text);

                noGuncel.Text = "";
                adGuncel.Text = "";
                yazarGuncel.Text = "";

                Liste();
            }
            else
            {
                DisplayAlert("Hata", "Lütfen tüm alanları doğru bir şekilde doldurun.", "Tamam");
            }

        }
        public class Book
        {
            public string No { get; set; }
            public string KitapAdi { get; set; }
            public string Yazar { get; set; }
        }

        private void Arama(string kitapAdi, string yazarAdi)
        {
            if (bag.State == ConnectionState.Closed)
                bag.Open();

            string sql = "SELECT * FROM books WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(kitapAdi))
            {
                sql += " AND kitapAdi LIKE @aramaKitapAdi";
            }

            if (!string.IsNullOrWhiteSpace(yazarAdi))
            {
                sql += " AND yazar LIKE @aramaYazarAdi";
            }

            SQLiteCommand cmd = new SQLiteCommand(sql, bag);

            if (!string.IsNullOrWhiteSpace(kitapAdi))
            {
                cmd.Parameters.AddWithValue("@aramaKitapAdi", $"%{kitapAdi}%");
            }

            if (!string.IsNullOrWhiteSpace(yazarAdi))
            {
                cmd.Parameters.AddWithValue("@aramaYazarAdi", $"%{yazarAdi}%");
            }

            SQLiteDataReader dr = cmd.ExecuteReader();
            List<Book> aramaSonuclari = new List<Book>();

            while (dr.Read())
            {
                aramaSonuclari.Add(new Book
                {
                    No = dr["no"].ToString(),
                    KitapAdi = dr["kitapAdi"].ToString(),
                    Yazar = dr["yazar"].ToString()
                });
            }
            dr.Close();

            if (aramaSonuclari.Count == 0)
            {
                DisplayAlert("Sonuç Yok", "Aradığınız kriterlere uygun bir kayıt bulunamadı.", "Tamam");
                aramaSonuclariListView.IsVisible = false;  // ListView'ı gizle
            }
            else
            {
                aramaSonuclariListView.ItemsSource = aramaSonuclari;
                aramaSonuclariListView.IsVisible = true;  // ListView'ı göster
            }
        }

        private void aramaBTN_Clicked(object sender, EventArgs e)
        {
            string kitapAdi = txtKitapAdi.Text;
            string yazarAdi = txtYazarAdi.Text;

            if (string.IsNullOrWhiteSpace(kitapAdi) && string.IsNullOrWhiteSpace(yazarAdi))
            {
                DisplayAlert("Hata", "Lütfen bir arama terimi giriniz.", "Tamam");
                return;
            }

            Arama(kitapAdi, yazarAdi);
        }

        private void sil_Clicked(object sender, EventArgs e)
        {
            // Entry alanından ID al
            if (int.TryParse(txtSil.Text, out int no))
            {
                Sil(no);
                DisplayAlert("Başarılı", "Kayıt başarıyla silindi.", "Tamam");
                txtSil.Text = ""; // Alanı temizle
                Liste(); // Listeyi güncelle
            }
            else
            {
                DisplayAlert("Hata", "Lütfen geçerli bir ID giriniz.", "Tamam");
            }

        }

        // kaç kere okudu göster


        // YazarKitapModel sınıfı
        public class YazarKitapModel
        {
            public string Yazar { get; set; }
            public int KitapSayisi { get; set; }
        }

        // Buton tıklandığında yazarların kitap sayısını listelemek için
        private async void YazarKitapSayisiButton_Clicked(object sender, EventArgs e)
        {
            // Bağlantıyı aç
            if (bag.State == System.Data.ConnectionState.Closed)
                bag.Open();

            // SQL sorgusu: Yazarları grupla ve kitap sayısını hesapla
            string sql = "SELECT yazar, COUNT(*) AS kitapSayisi FROM books GROUP BY yazar";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            SQLiteDataReader dr = cmd.ExecuteReader();

            if (yazarKitapCollectionView.IsVisible == false)
            {
                yazarKitapCollectionView.IsVisible = true;
                sayiKitap.IsVisible = true; // Yazarların kitap sayısı başlığını göster
            }
            else
            {
                yazarKitapCollectionView.IsVisible = false;
                sayiKitap.IsVisible = false; // Başlığı gizle
            }


            yazarKitapListesi.Clear();  // Listeyi temizle

            // Verileri Collection'a ekle
            while (dr.Read())
            {
                yazarKitapListesi.Add(new YazarKitapModel
                {
                    Yazar = dr["yazar"].ToString(),
                    KitapSayisi = Convert.ToInt32(dr["kitapSayisi"])
                });
            }
            dr.Close();
        }

    }

}
