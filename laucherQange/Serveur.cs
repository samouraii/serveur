using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

using System.Threading;
using System.Windows.Controls;

namespace laucherQange
{
    class Serveur
    {
        public int port { get; set; }
        public TcpListener ServeurSocket { get; set; }
        private RichTextBox info;
        public string Message = "";

        public Serveur(RichTextBox info, int port = 6000) {
            this.port = port;
            this.info = info;
            ServeurSocket = new TcpListener(IPAddress.Any, this.port);


        }

        public Boolean run { get; set; }

        private void message(string message)
        {

            info.Dispatcher.Invoke(() => { info.AppendText(message); });
        }
        private IAsyncResult asyncR;
        private TcpClient clientSocket;
        int counter;
        Thread ctThread;
        private Thread ctNettoyage;
        private List<Thread> tableau;

        public void start()
        {
            run = true;
            
            // clientSocket = default(TcpClient);               
            
            ServeurSocket.Start();
            counter = 0;

            this.message(">> Serveur Start\n");
            tableau = new List<Thread>();

            ctThread = new Thread(startServ);
            ctThread.Start();
            
            ctNettoyage = new Thread(clearThread);
            ctNettoyage.Start();

        }
        private void clearThread()
        {
            while (run)
            {
                List<Thread> temporaire = new List<Thread>();
                foreach (Thread t in tableau)
                {
                    if (t.ThreadState == ThreadState.Stopped)
                    {
                        temporaire.Add(t);
                        t.Join();
                    }
                }
                foreach (Thread t in temporaire)
                {

                    tableau.Remove(t);

                }
                Thread.Sleep(6000);
            }

        }

        public void stopThreadTcp()
        {
            run = false;
            ServeurSocket.Stop();
        }

        private void startServ()
        {
            while (run)
            {
                counter++;
                bool exception = false;
                try
                {
                    clientSocket = ServeurSocket.AcceptTcpClient();
                }catch(Exception e)
                {
                    exception = true;
                }
                if (!exception)
                {
                    message(">> Client connected N " + counter.ToString());
                    //info.AppendText(">> client connected");
                    //handleClinet client = new handleClinet();
                    //client.startClient(info, clientSocket, counter.ToString());
                    Thread temp = new Thread(communication);
                    temp.Start();
                    tableau.Add(temp);
                }
                //communication();
            }
            if(clientSocket!=null)clientSocket.Close();

        }
        private void communication()
        {

            byte[] from = new byte[1024];
            byte[] send = new byte[1024];
            string fromClient="";
            string sendClient;

            try
            {
                NetworkStream network = clientSocket.GetStream();
                int i;

                    i = network.Read(from, 0, from.Length);
                    fromClient = System.Text.Encoding.ASCII.GetString(from, 0, i);
                    message(fromClient);
                string type = "";
                switch (fromClient.Split(' ')[0])
                {
                    case "J":
                        type = "Jeu";
                        break;
                    case "L":
                        type = "LauncherJeu";
                        break;
                    case "M":
                        type = "LauncherLauncher";
                        break;
                  
                        
                }
                fromClient = fromClient.Split(' ')[1];

                string[] valeur = dbConnect.selectLastVersion(type, fromClient);
                //message("\n"+fromClient.Length+"\n");
                message(valeur[0]);
                string test = dbConnect.selectLastVersion(type)[1];
         
                 if (valeur[1] == null && test == fromClient)
                {
                    message("Est a jour");
                    Byte[] byt = System.Text.Encoding.ASCII.GetBytes("ok");
                    network.Write(byt, 0, byt.Length);

                }
                else
                {
                    Byte[] byt = System.Text.Encoding.ASCII.GetBytes(valeur[0] + " " + valeur[1]);
                    network.Write(byt,0,byt.Length);
                }
                network.Close();
            }
            catch (Exception e)
            {
                message("erreur "+e.Message);
            }

            message("\n");

        }

        public void stop()
        {
            try {
                run = false;
                if(tableau!=null)foreach(Thread tpt in tableau)
                {
                    /*while (!tpt.IsAlive) ;
                    Thread.Sleep(1);*/
                    tpt.Join();
                }
                if (ServeurSocket!=null)ServeurSocket.Stop();
                //ServeurSocket.Stop();
                /*while (!ctThread.IsAlive) ;
                Thread.Sleep(1);
                ctThread.Abort();
                ctNettoyage.Abort();*/
                /* ctThread.Join();
                while (! ctNettoyage.IsAlive) ;
                Thread.Sleep(1);

                ctNettoyage.Join();*/
                // ctThread.Abort();

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, e.Message);
            }
            message(">> Serveur Stop\n");
        }




    }
}
