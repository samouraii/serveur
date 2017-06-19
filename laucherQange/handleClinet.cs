using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;

namespace laucherQange
{
    class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        private RichTextBox info;
        private void message(string message)
        {

            info.Dispatcher.Invoke(() => { info.AppendText(message); });
        }


        public void startClient(RichTextBox info, TcpClient inClient, string clineNo)
        {
            this.clientSocket = inClient;
            this.clNo = clineNo;
            Thread ctThread = new Thread(communication);
        }
        private void communication()
        {

            byte[] from = new byte[10025];
            byte[] send = new byte[1024];
            string fromClient;
            string sendClient;

            try
            {
                NetworkStream network = clientSocket.GetStream();
                network.Read(from, 0, (int)clientSocket.ReceiveBufferSize);
                fromClient = System.Text.Encoding.UTF8.GetString(from);
                message(fromClient);

                string [] valeur = dbConnect.selectLastVersion("Jeu");
                if(valeur[1] != fromClient)
                {
                    message("dois etre mis à jour");
                }

            }
            catch (Exception e)
            {
                info.AppendText("Bug  "+e.Message);
            }
          




        }

    }
}
