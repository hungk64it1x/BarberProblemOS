using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Baber
{
    public partial class NomiClientiForm : Form
    {

        List<ListClient> LstClienti;

        public NomiClientiForm(List<ListClient> ListaClienti)
        {
            InitializeComponent();
            LstClienti = ListaClienti;
            Inizializzazione();
        }

        private void Inizializzazione()
        {
            LblCliente1.ForeColor = LstClienti[0].Color;
            TxtCliente1.Text = LstClienti[0].Name;
            LblCliente2.ForeColor = LstClienti[1].Color;
            TxtCliente2.Text = LstClienti[1].Name;
            LblCliente3.ForeColor = LstClienti[2].Color;
            TxtCliente3.Text = LstClienti[2].Name;
            LblCliente4.ForeColor = LstClienti[3].Color;
            TxtCliente4.Text = LstClienti[3].Name;
            LblCliente5.ForeColor = LstClienti[4].Color;
            TxtCliente5.Text = LstClienti[4].Name;
        }

        private void BtnSalva_Click(object sender, EventArgs e)
        {
            LstClienti[0].Name = TxtCliente1.Text;
            LstClienti[1].Name = TxtCliente2.Text;
            LstClienti[2].Name = TxtCliente3.Text;
            LstClienti[3].Name = TxtCliente4.Text;
            LstClienti[4].Name = TxtCliente5.Text;
        }

        private void ConvalidaTextbox(object sender, CancelEventArgs e)
        {
            TextBox x = sender as TextBox;
            if (!Vuoto(x.Text))
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                x.Select(0, x.Text.Length);
            }
        }

        public bool Vuoto(string stringa)
        {
            if (stringa.Length == 0 || stringa.Length > 11)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
