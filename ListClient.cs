using System;
using System.Drawing;

namespace Baber
{
    public class ListClient
    {
        private Color color;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string clientWait;

        public string ClientWait
        {
            get { return clientWait; }
            set { clientWait = value; }
        }

        private string clientEnter;

        public string ClientEnter
        {
            get { return clientEnter; }
            set { clientEnter = value; }
        }

        private string clientExit;

        public string ClientExit
        {
            get { return clientExit; }
            set { clientExit = value; }
        }

        private string clientBarber;

        public string ClientBarber
        {
            get { return clientBarber; }
            set { clientBarber = value; }
        }

        private string clientDry;

        public string ClientDry
        {
            get { return clientDry; }
            set { clientDry = value; }
        }

        public ListClient()
        {

        }

        public ListClient(string seat, string enter, string exit, string barber, string dry, string Name, Color color)
        {
            this.clientWait = seat;
            this.clientEnter = enter;
            this.clientExit = exit;
            this.clientBarber = barber;
            this.clientDry = dry;
            this.name = Name;
            this.color = color;
        }
    }
}
