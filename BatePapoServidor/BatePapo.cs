using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;

namespace BatePapoServidor
{
    public class StatuEventArgs : EventArgs
    {
        private string EveMgs;

        public string EventMgs
        {
            get { return EveMgs; }
            set { EveMgs = value; }
        }

        public StatuEventArgs(string stEveMgs)
        {
            EveMgs = stEveMgs;
        }
    }
    
    public delegate void StatuEventHandler(object sender, StatuEventArgs e);
        
    class BatePapo
    {
        public static Hashtable htUser = new Hashtable(40);
        public static Hashtable htConcta = new Hashtable(40);
        private IPAddress endIP;
        private TcpClient tcpClient;
        public static event StatuEventHandler StatusChanged;
        private static StatuEventArgs e;
        private Thread thEscuta;
        private TcpListener ListeCliente;//tlsCliente
        bool Rodando = false;

        public BatePapo(IPAddress AddressIP)
        {
            endIP = AddressIP;
        } 

        public static void AddUser(TcpClient tcpUser, string stname)
        {
            BatePapo.htUser.Add(stname, tcpUser);

            BatePapo.htConcta.Add(tcpUser, stname);

            EnviaMgsAdmin(htConcta[tcpUser] + " Entrou...");
        }

        public static void RemoveUser(TcpClient tcpUser)
        {
            if (htConcta[tcpUser] != null)
            {
                EnviaMgsAdmin(htConcta[tcpUser] + " saiu...");

                BatePapo.htUser.Remove(BatePapo.htConcta[tcpUser]);

                BatePapo.htConcta.Remove(tcpUser);
            }

        }

        public static void onChaged(StatuEventArgs e)
        {
            StatuEventHandler statuHandles = StatusChanged;

            if (statuHandles != null)
            {
                statuHandles(null, e);
            }

        }

        public static void EnviaMgsAdmin(string mgs)
        {
            StreamWriter swsender;

            e = new StatuEventArgs("Administrador: " + mgs);
            onChaged(e);

            TcpClient[] tcpClient = new TcpClient[BatePapo.htUser.Count];
            BatePapo.htUser.Values.CopyTo(tcpClient, 0);

            for (int i = 0; i < tcpClient.Length; i++)
            {
                try
                {
                    if (mgs.Trim() == "" || tcpClient[i] == null)
                    {
                        continue;
                    }
                    swsender = new StreamWriter(tcpClient[i].GetStream());
                    swsender.WriteLine("Administrador: " + mgs);
                    swsender.Flush();
                    swsender = null;
                }
                catch
                {
                    RemoveUser(tcpClient[i]);
                }
            }
        }

        public static void EnviaMgs(string Origem, string Mgs)
        {
            StreamWriter swSender;

            e = new StatuEventArgs(Origem + " Disse: " + Mgs);
            onChaged(e);

            TcpClient[] tcpClient = new TcpClient[BatePapo.htUser.Count];
            BatePapo.htUser.Values.CopyTo(tcpClient, 0);

            for (int i = 0; i < tcpClient.Length; i++)
            {
                try
                {
                    if (Mgs.Trim() == "" || tcpClient[i] == null)
                    {
                        continue;
                    }
                    swSender = new StreamWriter(tcpClient[i].GetStream());
                    swSender.WriteLine(Origem + " Disse: " + Mgs);
                    swSender.Flush();
                    swSender = null;
                }
                catch 
                {
                    RemoveUser(tcpClient[i]);
                }
            }
        }

        public void IniciaBatePapo()
        {
            try
            {
                IPAddress iplan = endIP;

                ListeCliente = new TcpListener(iplan, 2502);

                ListeCliente.Start();

                Rodando = true;

                thEscuta = new Thread(MantemBatePapo);
                thEscuta.Start();

            }
            catch (Exception erro)
            {
                
                throw erro;
            }
        }

        private void MantemBatePapo()
        {
            while (Rodando == true)
            {
                tcpClient = ListeCliente.AcceptTcpClient();
                Concta Connection = new Concta(tcpClient);
            }
        }

    }

    class Concta
    {
        TcpClient tcpClieat; //tcpCliente
        private Thread tSender; //thrSender
        private StreamReader sReceptor; //srReceptor
        private StreamWriter sEnviado; //swEnviador
        private string userAtual; //usuarioAtual
        private string srResposta; //strResposta

        public Concta(TcpClient tcpconn)
        {
            tcpClieat = tcpconn;
            tSender = new Thread(Addclieat);
            tSender.Start();
        }

        private void FechaConcta()
        {
            tcpClieat.Close();
            sReceptor.Close();
            sEnviado.Close();
        }

        private void Addclieat()
        {
            sReceptor = new StreamReader(tcpClieat.GetStream());

            sEnviado = new StreamWriter(tcpClieat.GetStream());

            userAtual = sReceptor.ReadLine();

            if (userAtual != "")
            {
                if (BatePapo.htUser.Contains(userAtual) == true)
                {
                    sEnviado.WriteLine("0|Este nome de usuário já existe.");
                    sEnviado.Flush();
                    FechaConcta();
                    return;
                }
                else if (userAtual == "Administrador")
                {
                    sEnviado.WriteLine("0|Este nome de usuário é reservado.");
                    sEnviado.Flush();
                    FechaConcta();
                    return;
                }
                else
                {
                    sEnviado.WriteLine("1");
                    sEnviado.Flush();
                    BatePapo.AddUser(tcpClieat, userAtual);
                }
            }
            else
            {
                this.FechaConcta();
                return;
            }
            try
            {
                while ((srResposta = sReceptor.ReadLine()) != "")
                {
                    if (srResposta == null)
                    {
                        BatePapo.RemoveUser(tcpClieat);
                    }
                    else
                    {
                        BatePapo.EnviaMgs(userAtual, srResposta);
                    }
                }
            }
            catch 
            {

                BatePapo.RemoveUser(tcpClieat);
            }
        }
    }
}
