using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using System.IO.Compression;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace laucherQange
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private Serveur s;
       // private DiffEngineLevel _level;
        string tdestT;
        string tTypeT;
        private object listLock = new object();
        string tVersionT;
        private int finishUpdate = 0;
        private int finishUpdateMax = 1;
        public MainWindow()
        {
            InitializeComponent();
            s = new Serveur(this.servInfo, 6000);
        }
        private string GetFileName()
        {
            string fname = string.Empty;
            OpenFileDialog dlg = new OpenFileDialog();
            

            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                fname = dlg.FileName;
            }
            
            return fname;
        }
        private bool ValidFile(string fname)
        {
            if (fname != string.Empty)
            {
                if (File.Exists(fname))
                {
                    return true;
                }
            }
            return false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();
            this.tSource.Text = dialog.SelectedPath;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();
            this.tDest.Text = dialog.SelectedPath;
           // this.tDest.Text = GetFileName();
        }
        string sFile;
        string dFile;
        string chemin;
        string[] text;
        BackgroundWorker t2 = new BackgroundWorker();
        BackgroundWorker t = new BackgroundWorker();
        private void button_Click(object sender, RoutedEventArgs e)
        {
           sFile = tSource.Text.Trim();
            dFile = tDest.Text.Trim();
           

            if (false)
            {
                MessageBox.Show("Source file name is invalid.", "Invalid File");
                tSource.Focus();
                return;
            }
            if (false)
            {
                MessageBox.Show("Destination file name is invalid.", "Invalid File");
                tDest.Focus();
                return;
            }
            if(tType == null)
            {
                MessageBox.Show( "Invalid Type de mise à jour");
                tType.Focus();
                return;
            }
            if (tVesrion.Text == null)
            {
                MessageBox.Show("Invalid Version");
                tType.Focus();
                return;
            }
            finishUpdateMax = 0;
            finishUpdate = 0;
            tdestT = tDest.Text;
            tTypeT = tType.Text;
            tVersionT = tVesrion.Text;

          
            button.IsEnabled = false;
            tType.IsEnabled = false;
            text = dbConnect.selectLastVersion(tType.Text);
            chemin = dFile + "\\" + tType.Text + "\\" + tVesrion.Text;
            if (text[0] != "" && text[0]!= null && text[0] != "-1")
            {
                t2 = new BackgroundWorker();
                t2.DoWork += new DoWorkEventHandler(threadVerification);
                //t2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(finithreadVerification);
                //t2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FinTotal);
                t2.WorkerReportsProgress = true;
                t2.ProgressChanged += new ProgressChangedEventHandler(progressBar);
                t2.RunWorkerAsync();
                finishUpdateMax++;
            }
            else
            {
               //copier fichier dans le répertoire
               DirectoryInfo tutu = Directory.CreateDirectory(dFile + "\\"+tType.Text+"\\"+tVesrion.Text);
               //Changer pour copier directory
            }

            finishUpdateMax++;
            Directory.CreateDirectory(dFile + "\\" + tType.Text + "\\" + tVesrion.Text + "\\jeu\\zip");
            t = new BackgroundWorker();
            t.DoWork += new DoWorkEventHandler(thread);
            t.RunWorkerCompleted += new RunWorkerCompletedEventHandler(finithread);
            t.RunWorkerAsync();
        }

        private void thread(object sender, DoWorkEventArgs e)
        {
            try
            {
                ZipFile.CreateFromDirectory(sFile, chemin + "\\jeu\\zip\\jeu.zip");
                ZipFile.ExtractToDirectory(chemin + "\\jeu\\zip\\jeu.zip", chemin + "\\jeu\\normal");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void finithread(object sender, RunWorkerCompletedEventArgs e)
        {
            finishUpdate++;
            if(finishUpdate == finishUpdateMax)
            {
                FinTotal();
            }
        }

        //Console.WriteLine(msg);
        private int nbThread = 12;
        private List<string> newVersion;
        private List<string> AncienneVersion;
        private int newVersionCount = 0;
        //int time;
        private void threadVerification(object sender, DoWorkEventArgs e)
        {//ici la modification
            bool parsed = true;
            if (parsed)
            {
                fin = 0;
                AncienneVersion = this.nbfichier(dFile + text[0] + "\\jeu\\normal",  new List<string>());
                newVersion = this.nbfichier(sFile, new List<string>());
                files = new List<filupdate>();
                
                //On supprime les fichiers qui ne sont pas présent dans la nouvelle version
                foreach (string oldVersion in AncienneVersion)
                {
                    if (!newVersion.Contains(oldVersion))
                    {
                        filupdate tempo = new filupdate();
                        tempo.Fichier = oldVersion;
                        tempo.Type = 2;
                        files.Add(tempo);
                    }
                }

                counter = 0;
                newVersionCount = newVersion.Count;
                //time = DateTime.Now.Millisecond;
                Directory.CreateDirectory(chemin + "\\patch\\");
                List<BackgroundWorker> travailleur = new List<BackgroundWorker>();
                for (int i = 0; i < nbThread; i++)
                {
                    BackgroundWorker travail = new BackgroundWorker();
                    travail.DoWork += new DoWorkEventHandler(calculStart); 
                    travail.RunWorkerCompleted += new RunWorkerCompletedEventHandler(finiThreadVerification);
                    travail.RunWorkerAsync();
                }
                /*while(fin != nbThread)
                {
                    Thread.Sleep(1000);
                }*/
                //List<filupdate> files = vérificationFichier(dFile + text[0] + "\\jeu\\normal", sFile, AncienneVersion, newVersion);
                //vérificationFichier(dFile + text[0] + "\\jeu\\normal", sFile, AncienneVersion, newVersion, newVersion.Count);
                //WriteFile(files);
            }
            else
            {
                MessageBox.Show("Entrez un chiffre en entier dans thread");
            }
        }

        int fin = 0;
        private void finiThreadVerification(object sender, RunWorkerCompletedEventArgs e)
        {
            fin++;
            if (nbThread == fin)
            {
                WriteFile(files);
                ZipFile.CreateFromDirectory(chemin + "\\patch\\", chemin + "\\update.zip");
                MessageBox.Show("compteur: "+compteur + " // newVersionCount: " + newVersionCount + " // counter: " + counter, "compteur: " + compteur + " // newVersionCount: " + newVersionCount + " // counter: " + counter);
                finishUpdate++;
                if (finishUpdate == finishUpdateMax)
                {
                    FinTotal();
                }
            }
        }

        private void calculStart(object sender, DoWorkEventArgs e)
        {
            while (counter < newVersionCount)
            {
                string fichierNew = "";
                lock (listLock)
                {
                    fichierNew = newVersion[0];
                    newVersion.RemoveAt(0);
                    counter++;
                }
                vérificationFichier(dFile + text[0] + "\\jeu\\normal", sFile, AncienneVersion, fichierNew, newVersionCount, counter);
                
            }
        }

        private int valeurStop=0;
        private void FinTotal(/*object sender, RunWorkerCompletedEventArgs e*/)
        {
           
           if ( !t.IsBusy && !t2.IsBusy)
            {
                int i = dbConnect.insertVersion(tVesrion.Text, tType.Text, "pas encore fait", "\\" + tType.Text + "\\" + tVesrion.Text);
                tType.IsEnabled = true;
                //this.button.IsEnabled = true;
            }
        }

        private void progressBar(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }
        
        
        private void WriteFile(List<filupdate> mtable)
        {
             
                if (!Directory.Exists(chemin))
                {
                     Directory.CreateDirectory(chemin);
                }
              
               
                File.WriteAllText(chemin+"\\patch\\update"+ tTypeT + ".json", JsonConvert.SerializeObject(mtable));
                
                // serialize JSON directly to a file
                using(StreamWriter file = File.CreateText(chemin+ "\\patch\\update" + tTypeT + ".json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, mtable);
                }
                //dbConnect.initialise();
               // dbConnect.insertVersion(tVesrion.Text, tType.Text, "pasEncoreFait", "\\" + tType.Text + "\\" + tVesrion.Text);
        }


      /*  private List<Jupdate> BinaryDiff(string sFile, string dFile)
        {
            //this.Cursor = Cursors.Wait;

            DiffList_BinaryFile sLF = null;
            DiffList_BinaryFile dLF = null;
            try
            {
                sLF = new DiffList_BinaryFile(sFile);
                dLF = new DiffList_BinaryFile(dFile);
             
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message, "File Error");
                return null ;
            }

            try
            {
                double time = 0;
                DiffEngine de = new DiffEngine();
                
                int value = 0;
                if (sLF.Count() > dLF.Count()) value = (int)dLF.Count() / 5000; 
                else value = (int)sLF.Count() / 5000;
                ArrayList rep = new ArrayList();
                for (int i = 0; i < value; i++)
                {
                    if (sLF.Count() > i * 1000 && dLF.Count() > i * 5000)
                    {
                        time = de.ProcessDiff(new DiffList_BinaryFile(sLF.ByteList.ToList<byte>().GetRange(i * 5000, 5000).ToArray()), new DiffList_BinaryFile(dLF.ByteList.ToList<byte>().GetRange(i * 5000, 5000).ToArray()), _level);
                    }
                    else
                    {
                        int count;
                        if (sLF.Count() > dLF.Count()) count = dLF.Count();
                        else count = sLF.Count();
                                
                        time = de.ProcessDiff(new DiffList_BinaryFile(sLF.ByteList.ToList<byte>().GetRange(i * 5000, count).ToArray()), new DiffList_BinaryFile(dLF.ByteList.ToList<byte>().GetRange(i * 5000, count).ToArray()), _level);
                    }

                    rep.AddRange( de.DiffReport());
                }
               
               
                

                List<Jupdate> mtable = new List<Jupdate>() ;
               
               foreach (DifferenceEngine.DiffResultSpan t in rep)
                {
                    Jupdate v= new Jupdate(t.SourceIndex, t.DestIndex, t.Status.ToString(), t.Length);
                    
                    if(t.Status == DiffResultSpanStatus.AddDestination || t.Status == DiffResultSpanStatus.Replace)
                    {
                        try
                        {
                            for (int i = 0; i < t.Length; i++)
                            {
                               
                                v.AddModif( i, dLF.ByteList[t.DestIndex + i]);

                               // MessageBox.Show(dLF.ByteList[0]);
                            }
                        }
                        catch (Exception ex)
                        {

                            MessageBox.Show(ex.Message, "File Error");
                            return null;
                        }
                    }
                    mtable.Add(v);
                }

                //retour liste

                return mtable;


            }
            catch (Exception ex)
            {
                
                string tmp = string.Format("{0}{1}{1}***STACK***{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
                MessageBox.Show(tmp, "Compare Error");
                return null;
            }
           
        }
        */
        public List<string> nbfichier(string dossierRoot, List<string> i, string dossierRoot2=null)
        {
           
            if (dossierRoot2 == null) dossierRoot2 = dossierRoot;
           
            foreach (string dossier in Directory.GetDirectories(dossierRoot)) {
            
                   nbfichier(dossier,i, dossierRoot2);
                
            }
            String[] listFilesCurr = Directory.GetFiles(dossierRoot);
            foreach (string rowfile in listFilesCurr)
            {
                
                i.Add(rowfile.Remove(0,dossierRoot2.Length));
            }
            return i;
        }

        private void ecrirelabel(double pourcentage, string label)
        {
           labelFichier.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
           {
               labelFichier.Content = label;
           }));
            LabelPourcentage.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                LabelPourcentage.Content = pourcentage + " %";
            }));
        }

        private void fileUpdate(string fichier, string newFichier, string patchFile)
        {
            try
            {
                /*using (FileStream output = new FileStream(chemin + patchFile, FileMode.OpenOrCreate))
                BinaryPatchUtility.Create(File.ReadAllBytes(fichier), File.ReadAllBytes(newFichier), output);
                System.GC.Collect();*/
                var processInfo = new ProcessStartInfo("java.exe", "-Xmx20g -jar jbsdiff.jar diff "+ fichier + " " +newFichier+ " " + chemin + patchFile)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process proc;

                if ((proc = Process.Start(processInfo)) == null)
                {
                    throw new InvalidOperationException("??");
                }

                proc.WaitForExit();
                int exitCode = proc.ExitCode;
                proc.Close();
            }
            catch (Exception ex1)
            {
                MessageBox.Show(ex1.ToString() + ex1.Message, ex1.ToString() + ex1.Message);
            }
        }

        bool compareFileWithSha(string file1, string file2)
        {
            return calculShaOfFile(file1) == calculShaOfFile(file2); 
        }

        string calculShaOfFile(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return formatted.ToString();
                }
            }
        }

        /****old****
        private List<filupdate> vérificationFichier(string route, string routeinit, List<string> oldVersion, List<string> newVersion)
        {
            List<filupdate> files = new List<filupdate>();
            try
            {
                //Faire avant car sinon va se faire plein de fois
                Directory.CreateDirectory(chemin + "\\patch\\");

            //ici 
            int counter = 0;
            foreach (string t in newVersion)
            {
                counter++;

                //A faire ailleurs car newVersion contiendra un seul fichier
                ecrirelabel((counter * 100) / newVersion.Count, t);

                //t2.ReportProgress((int)(counter * 100) / newVersion.Count);
                int valuer = oldVersion.IndexOf(t);
                string laroute = "";
                if (valuer != -1) laroute = oldVersion[valuer];
                filupdate tempo = new filupdate();

                if (valuer != -1 && System.IO.File.Exists(route + laroute))
                {

                    tempo.Fichier = t;
                    tempo.Updateur = "/patch/" + counter + ".patch";
                    //   tempo.JupdateList =   BinaryDiff(route + laroute, routeinit + t);
                    if (!compareFileWithSha(route + laroute, routeinit + t))
                    {
                        fileUpdate(route + laroute, routeinit + t, tempo.Updateur);
                    }
                    // ici faire calculer les fichier avec le nouveau système et retourner la liste des fichier à modifiers 

                    tempo.Type = 1;
                }
                else
                {
                    tempo.Fichier = t;
                    tempo.Type = 0;
                }
                files.Add(tempo);

            }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error");
                return null;
            }
            return files;
        }
       // fin old****/

        private int compteur = 0;
        private int counter = 0;
        private List<filupdate> files;
        private void vérificationFichier(string route,string routeinit,List<string> oldVersion, string newVersion, int totalFichier, int counterP)
        {
            compteur++;
            //A faire ailleurs car newVersion contiendra un seul fichier
            ecrirelabel((counterP * 100) / totalFichier, newVersion);

            //t2.ReportProgress((int)(counter * 100) / newVersion.Count);
            int valuer = oldVersion.IndexOf(newVersion);
            string laroute ="";
            if(valuer != -1)laroute = oldVersion[valuer];
            filupdate tempo=  new filupdate();
            bool needWrite = true;
            if ( valuer != -1 && System.IO.File.Exists(route + laroute))
            {
                //   tempo.JupdateList =   BinaryDiff(route + laroute, routeinit + t);
                if (!compareFileWithSha(route + laroute, routeinit + newVersion))
                {
                    tempo.Fichier = newVersion;
                    tempo.Updateur = "/patch/" + (counterP + 1) + ".patch";
                    fileUpdate(route + laroute, routeinit + newVersion, tempo.Updateur);
                }else
                {
                    needWrite = false;
                }
                // ici faire calculer les fichier avec le nouveau système et retourner la liste des fichier à modifiers 

                tempo.Type = 1;
            }
            else
            {
                tempo.Fichier = newVersion;
                tempo.Type = 0;
            }
            if (needWrite)
            {
                files.Add(tempo);
            }
        }


        
        
        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            stop.IsEnabled = true;
            button2.IsEnabled = false;
            s.start();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            s.run = false;
            s.stop();
            stop.IsEnabled = false;
            button2.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            s.run = false;
            s.stop();
        }



        private List<Jupdate> BinaryDiffTempo(string sFile, string dFile)
        {
            List<Jupdate> listUpdate = new List<Jupdate>();
         //   DiffList_BinaryFile sLF = null;
       //     DiffList_BinaryFile dLF = null;
            try
            {
             //   sLF = new DiffList_BinaryFile(sFile);
             //   dLF = new DiffList_BinaryFile(dFile);

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "File Error");
                return null;
            }

            try
            {
                int compteur = 0;
               // int sizeSource = sLF.ByteList.Count();
              //  int sizeDest = dLF.ByteList.Count();
               // foreach (byte t in sLF.ByteList)
                {
                //    if (sizeSource >= compteur && sizeDest >= compteur)
               //     {

                //    }
                 //   else if(sizeDest < compteur)
                //    {

                //    }

                        compteur++;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "File Error");
                return null;
            }




            return listUpdate;
        }

        private void tSource_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private string nextVersion(string version)
        {
            string[] versionSp = version.Split('.');
            for(int i = versionSp.Length-1; i>=0; i--)
            {
                if (versionSp[i] == "9")
                {
                    versionSp[i] = "0";
                }else
                {
                    versionSp[i] = (Int32.Parse(versionSp[i])+1)+"";
                    break;
                }
            }
            string newVersion = "";
            for (int i = 0; i < versionSp.Length; i++)
            {
                if (i != versionSp.Length - 1) {
                    newVersion += versionSp[i] + ".";
                }
                else
                {
                    newVersion += versionSp[i];
                }
            }
            return newVersion;
        }

        private void tType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string type = (e.AddedItems[0] as System.Windows.Controls.ComboBoxItem).Content as string;
            tSource.Text = type == "Jeu" ? "C:\\Users\\iolacorp\\Desktop\\SyncthingsStockage\\JeuCompile" :
                type == "LauncherJeu" ? "C:\\Users\\iolacorp\\Desktop\\QangaServer\\LauncherClient" :
                type == "LauncherLauncher" ? "C:\\Users\\iolacorp\\Desktop\\QangaServer\\LauncherLauncher" : "";
            string[] versions = dbConnect.selectLastVersion(type);
            string version = "0.0.0.1";
            if (versions[0] != "" && versions[0] != null && versions[0] != "-1")
            {
                version = nextVersion(versions[versions.Length-1]);
            }
            tVesrion.Text = version;
            button.IsEnabled = true;
        }
    }
}
