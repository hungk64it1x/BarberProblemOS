using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Baber
{
   

    public partial class BarbiereForm : Form
    {
        Random rnd = new Random();  //Random

        string dir = Directory.GetCurrentDirectory() + @"\Image\";           
        List<ListClient> ListClient;    //Tạo 1 danh sách khách hàng         

        bool automatic = true;    //Biến bool tự động

        int numOfSeat = 4;      

#region Vari_Delay

        decimal minGenInput; //Thời gian max chờ khách vào (dùng cho tự động)
        decimal minGenOutpu; //Thời gian min chờ khách vào (dùng cho tự động)
        decimal timeToCut;   //Thời gian cắt tóc

#endregion

#region Semaphores:

        Semaphore SEM_client = new Semaphore(0, 4);
        Semaphore SEM_barber = new Semaphore(0, 1);
        Mutex mutex = new Mutex();
        int checkSeat = 0;

#endregion

        //                                    1      2      3      4
        bool[] SeatOccupate = new bool[] { false, false, false, false };  //Dùng để cho khách biết vị trí nào còn trống để ngồi

        List<int> ClientOrder = new List<int>();  //Xác định thứ tự đến -> thợ có thể chọn để cắt tóc

        Thread Now_Barber;  // Thread


        public BarbiereForm()
        {
            #region IMAGE

                ListClient = new List<ListClient>(new ListClient[] {
                    new ListClient(dir + "SedutoCliente1.png",     dir + "EntrataCliente1.png",     dir + "UscitaCliente1.png",    dir + "BarbiereCliente1.png",     dir + "AsciugaCliente1.png",      "Vũ Hùng",    Color.Blue),      
                    new ListClient(dir + "SedutoCliente2.png",     dir + "EntrataCliente2.png",     dir + "UscitaCliente2.png",    dir + "BarbiereCliente2.png",     dir + "AsciugaCliente2.png",      "Công Phượng",    Color.Green),      
                    new ListClient(dir + "SedutoCliente3.png",     dir + "EntrataCliente3.png",     dir + "UscitaCliente3.png",    dir + "BarbiereCliente3.png",     dir + "AsciugaCliente3.png",      "Sơn Tùng",     Color.Red),       
                    new ListClient(dir + "SedutoCliente4.png",     dir + "EntrataCliente4.png",     dir + "UscitaCliente4.png",    dir + "BarbiereCliente4.png",     dir + "AsciugaCliente4.png",      "Khá Bảnh",   Color.Yellow),      
                    new ListClient(dir + "SedutoCliente5.png",     dir + "EntrataCliente5.png",     dir + "UscitaCliente5.png",    dir + "BarbiereCliente5.png",     dir + "AsciugaCliente5.png",      "Huấn Hoa Hồng",     Color.LightBlue),
                    new ListClient(dir + "SedutoCliente1.png",     dir + "EntrataCliente1.png",     dir + "UscitaCliente1.png",    dir + "BarbiereCliente1.png",     dir + "AsciugaCliente1.png",      "Trâm Anh",    Color.Blue),
                    new ListClient(dir + "SedutoCliente2.png",     dir + "EntrataCliente2.png",     dir + "UscitaCliente2.png",    dir + "BarbiereCliente2.png",     dir + "AsciugaCliente2.png",      "Ngọc Trinh",    Color.Green),
                    new ListClient(dir + "SedutoCliente3.png",     dir + "EntrataCliente3.png",     dir + "UscitaCliente3.png",    dir + "BarbiereCliente3.png",     dir + "AsciugaCliente3.png",      "Hoài Linh",     Color.Red),
                    new ListClient(dir + "SedutoCliente4.png",     dir + "EntrataCliente4.png",     dir + "UscitaCliente4.png",    dir + "BarbiereCliente4.png",     dir + "AsciugaCliente4.png",      "Trấn Thành",   Color.Yellow),
                    new ListClient(dir + "SedutoCliente5.png",     dir + "EntrataCliente5.png",     dir + "UscitaCliente5.png",    dir + "BarbiereCliente5.png",     dir + "AsciugaCliente5.png",      "Bùi Anh Tuấn",     Color.LightBlue),
                });

            #endregion

            InitializeComponent();
            ListView.CheckForIllegalCrossThreadCalls = false;  

            InizializzazionePictureBoxAndDelays();

            Now_Barber = new Thread(new ThreadStart(Barbiere));      // Khởi tạo 1 luồng mới
            Now_Barber.IsBackground = true;
            Now_Barber.Start();                                 // Bắt đầu luồng
        }

        private void InizializzazionePictureBoxAndDelays()
        {
            // PictureBox
            ImgBoxBarbiere.Image = Image.FromFile(dir + "BarbiereDorme.png");
            ImgBoxEntrata.Image = Image.FromFile(dir + "PortaChiusa.png");
            ImgBoxUscita.Image = Image.FromFile(dir + "PortaChiusa.png");
            ImgBoxSedia1.Image = Image.FromFile(dir + "SediaVuota.png");
            ImgBoxSedia2.Image = Image.FromFile(dir + "SediaVuota.png");
            ImgBoxSedia3.Image = Image.FromFile(dir + "SediaVuota.png");
            ImgBoxSedia4.Image = Image.FromFile(dir + "SediaVuota.png");

            // Delay
            minGenInput = txtMax.Value;
            minGenOutpu = txtMin.Value;
            timeToCut = txtTimeCut.Value;
        }

#region THREADS

        //THREAD
        private void Client()
        {
            int indexClient = rnd.Next(0, 9);  //Khởi tạo ngẫu nhiên khách hàng mới
            Client_Start(indexClient);  //Khách hàng vào cửa hàng
            mutex.WaitOne();
            if (checkSeat < numOfSeat)        //Kiểm tra xem còn ghế trống không
            {
                checkSeat++;
                int indexLeaveToCut = ClientSeat(indexClient);
                SEM_client.Release();
                mutex.ReleaseMutex();
                ClientOrder.Add(indexClient);       //Thêm khách hàng
                SEM_barber.WaitOne();
                ClientLeaveSeat(indexLeaveToCut);                   
            }
            else       //Hết ghế
            {
                mutex.ReleaseMutex();
                ClientNotSatisfied(indexClient); //Khách bỏ về
            }
        }

        // THREAD BARBER
        private void Barbiere()
        {     
            while (true)
            {
                SEM_client.WaitOne();
                mutex.WaitOne();
                checkSeat--;
                SEM_barber.Release();
                mutex.ReleaseMutex();
                BarberHaircut(ClientOrder[0]);    
            }
        }

#endregion

#region HairCut

        private void Client_Start(int indexClient)
        {
            WriteConsole("+ Khách hàng đang vào: " + ListClient[indexClient].Name, ListClient[indexClient].Color); 
            LBoxClient.Items.Insert(0, ListClient[indexClient].Name);
            ImgBoxEntrata.Image = Image.FromFile(ListClient[indexClient].ClientEnter);
            Thread.Sleep(500);
            ImgBoxEntrata.Image = Image.FromFile(dir + "PortaChiusa.png");
        }


        private int ClientSeat(int indexClient)
        {
            if (SeatOccupate[0] == false)
            {
                ImgBoxSedia1.Image = Image.FromFile(ListClient[indexClient].ClientWait);
                SeatOccupate[0] = true;
                WriteConsole("Khách hàng" + ListClient[indexClient].Name + " ngồi trên ghế 1.", ListClient[indexClient].Color);
                return 1;
            }
            else if (SeatOccupate[1] == false)
            {
                ImgBoxSedia2.Image = Image.FromFile(ListClient[indexClient].ClientWait);
                SeatOccupate[1] = true;
                WriteConsole("Khách hàng " + ListClient[indexClient].Name + " đang ngồi trên ghế 2.", ListClient[indexClient].Color);
                return 2;
            }
            else if (SeatOccupate[2] == false)
            {
                ImgBoxSedia3.Image = Image.FromFile(ListClient[indexClient].ClientWait);
                SeatOccupate[2] = true;
                WriteConsole("Khách hàng " + ListClient[indexClient].Name + " đang ngồi trên ghế 3.", ListClient[indexClient].Color);
                return 3;
            }
            else
            {
                ImgBoxSedia4.Image = Image.FromFile(ListClient[indexClient].ClientWait);
                SeatOccupate[3] = true;
                WriteConsole("Khách hàng " + ListClient[indexClient].Name + " đang ngồi trên ghế 4.", ListClient[indexClient].Color);
                return 4;
            }
        }
        //Hàm cho biết khách hàng không chờ được và đi về
        private void ClientNotSatisfied(int indexClient)
        {
            ImgBoxUscita.Image = Image.FromFile(ListClient[indexClient].ClientEnter);
            WriteConsole("-- Khách hàng " + ListClient[indexClient].Name + " không có ghế chờ và đã đi về.", ListClient[indexClient].Color );
            LBoxClient.Items.RemoveAt(0);
            Thread.Sleep(500);
            ImgBoxUscita.Image = Image.FromFile(dir + "PortaChiusa.png");
        }

        //Hàm xác định khách rời ghế để xuống cắt tóc
        private void ClientLeaveSeat(int indexLeaveToCut)
        {
            if (indexLeaveToCut == 1)
            {
                ImgBoxSedia1.Image = Image.FromFile(dir + "SediaVuota.png");
                SeatOccupate[0] = false;
            }
            else if (indexLeaveToCut == 2)
            {
                ImgBoxSedia2.Image = Image.FromFile(dir + "SediaVuota.png");
                SeatOccupate[1] = false;
            }
            else if (indexLeaveToCut == 3)
            {
                ImgBoxSedia3.Image = Image.FromFile(dir + "SediaVuota.png");
                SeatOccupate[2] = false;
            }
            else if (indexLeaveToCut == 4)
            {
                ImgBoxSedia4.Image = Image.FromFile(dir + "SediaVuota.png");
                SeatOccupate[3] = false;
            }
        }

        //Hàm xác định khách cắt tóc xong và rời cửa hàng
        private void BarberHaircut(int indexClient)
        {
            ClientOrder.RemoveAt(0);      //Xóa khách hàng trong list chờ

            WriteConsole(">> Khách hàng " + ListClient[indexClient].Name + " đang cắt tóc.", ListClient[indexClient].Color);

            ImgBoxBarbiere.Image = Image.FromFile(ListClient[indexClient].ClientBarber);
            Thread.Sleep(Convert.ToInt32(timeToCut * 500));  

            ImgBoxBarbiere.Image = Image.FromFile(ListClient[indexClient].ClientDry);
            Thread.Sleep(Convert.ToInt32(timeToCut * 500));  

            ImgBoxUscita.Image = Image.FromFile(ListClient[indexClient].ClientExit);
            ImgBoxBarbiere.Image = Image.FromFile(dir + "BarbiereAspetta.png");

            WriteConsole(">> Khách hàng " + ListClient[indexClient].Name + " đã cắt tóc xong!", ListClient[indexClient].Color);

            Thread.Sleep(500);
            WriteConsole("-- Khách hàng " + ListClient[indexClient].Name + " đã thanh toán và đi về", ListClient[indexClient].Color);
            LBoxClient.Items.RemoveAt(LBoxClient.Items.Count - 1);

            ImgBoxUscita.Image = Image.FromFile(dir + "PortaChiusa.png");
            ImgBoxBarbiere.Image = Image.FromFile(dir + "BarbiereDorme.png");
        }

        //In ra màn hình thông báo
        private void WriteConsole(string text, Color color)
        {
            ListViewItem temp = new ListViewItem();
            temp.Text = text;
            temp.ForeColor = color;
            LConsole.Items.Insert(0, temp);
            LConsole.EnsureVisible(0);
        }

        //Tự động
        private void ModeAutomatic()
        {
            while(automatic == true)
            {
                int GeneraOgni = rnd.Next(Convert.ToInt32(minGenOutpu * 1000), Convert.ToInt32(minGenInput * 1000));
                Thread.Sleep(GeneraOgni);
                Thread now_client = new Thread(new ThreadStart(Client));            
                now_client.IsBackground = true;
                now_client.Start();                 
            }
        }

#endregion

#region EventForm

        //Thiết lập chế độ
        private void RbAutomatic_CheckedChanged(object sender, EventArgs e)
        {
            if (RbAutomatic.Checked == true)
            {
                automatic = true;
                BtnAvvia.Enabled = true;
                BtnReset.Enabled = true;
                BtnAddCustomer.Enabled = false;
                LConsole.Items.Add("Đã thiết lập chế độ tự động");
            }
            else
            {
                automatic = false;
                BtnAddCustomer.Enabled = true;
                BtnAvvia.BackColor = Color.LimeGreen;
                BtnAvvia.Text = "Start";
                BtnAvvia.Enabled = false;
                BtnReset.Enabled = false;
                LConsole.Items.Add("Đã thiết lập chế độ thủ công");
            }
            
        }

        private void BtnAddCustomer_Click(object sender, EventArgs e)
        {
            Thread Nuovo_cliente = new Thread(new ThreadStart(Client));
            Nuovo_cliente.IsBackground = true;
            Nuovo_cliente.Start();
        }

        private void BtnAvvia_Click(object sender, EventArgs e)
        {
            if (RbAutomatic.Checked == true && BtnAvvia.Text == "Start")
            {
                Thread Mod_Automatica = new Thread(new ThreadStart(ModeAutomatic));
                Mod_Automatica.IsBackground = true;
                Mod_Automatica.Start();
                BtnAvvia.BackColor = Color.Yellow;
                BtnAvvia.Text = "Stop";
            }
            else if (RbAutomatic.Checked == true && BtnAvvia.Text == "Stop")
            {
                automatic = false;
                BtnAvvia.BackColor = Color.LimeGreen;
                BtnAvvia.Text = "Start";
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            var NewForm = new BarbiereForm();
            this.Dispose(false);
            NewForm.Show();          
        }

        // Kiểm tra giá trị sinh thấp hơn giá trị sinh cao hơn, nếu không thì đặt lại 2 hộp văn bản -> thay đổi giá trị trễ
        private void ValoreCambiatoTextbox(object sender, EventArgs e)  
        {
            if (txtMin.Value > txtMax.Value)
            {
                txtMax.Value = 10;
                txtMin.Value = 1;
            }
            minGenInput = txtMax.Value;
            minGenOutpu = txtMin.Value;
        }

        //Thay đổi thời gian cắt tóc
        private void TxtTempTaglio_ValueChanged(object sender, EventArgs e)
        {
            timeToCut = txtTimeCut.Value;
        }

        private void BtnEsci_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BarbiereForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            BarbiereForm.ActiveForm.Dispose();
        }

        private void BtnNomiClienti_Click(object sender, EventArgs e)
        {
            var NuovoForm = new NomiClientiForm(ListClient);
            NuovoForm.ShowDialog();
        }


        #endregion

  
    }
}
