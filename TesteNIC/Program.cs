using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TesteNIC
{
   
    class Program
    {
        public static void Main(string[] args)
        {
                
            if(args.Length == 2)
            {
                string cliente  = args[1];
                string servidor = args[0];

                Console.WriteLine("Iniciando....");
                while (true)
                {
                    Thread.Sleep(5000);
                    Sync(cliente, servidor);
                    Console.WriteLine();
                }

            }

        }

        public static void Sync(string cliente, string servidor)
        {
            DirectoryInfo dirCliente = new DirectoryInfo(cliente);
            DirectoryInfo dirServidor = new DirectoryInfo(servidor);

            SyncAll(dirCliente, dirServidor);
        }

        
        private static void SyncAll(DirectoryInfo cliente, DirectoryInfo servidor)
        {
            if (cliente.FullName == servidor.FullName || !cliente.Exists)
            {
                return;
            }
            if (!servidor.Exists)
            {
                Directory.CreateDirectory(servidor.FullName);
            }


            //Pega o nome de todos os arquivos do servidor e coloca em uma lista para ser deletados
            var targetFilesToDelete = servidor.GetFiles().Select(fi => fi.Name).ToList();
            foreach (FileInfo sourceFile in cliente.GetFiles())
            {
                //verifico se para cada arquivo que contem no diretório do cliente se ele existe na minha lista de a serem deletados (targetFilesToDelete)
                //se ele existe eu deleto da lista
                if (targetFilesToDelete.Contains(sourceFile.Name)) targetFilesToDelete.Remove(sourceFile.Name);

                var targetFile = new FileInfo(Path.Combine(servidor.FullName, sourceFile.Name));

                //verifico se ele  (arquivo da vez na lista do cliente) não existe no diretorio do servidor
                if (!targetFile.Exists)
                {
                    Console.WriteLine($"Copiando {sourceFile.FullName} para ${servidor}");

                    //aí eu copio
                    sourceFile.CopyTo(targetFile.FullName);
                }
                //se não eu verifico se a data de alteração dele (arquivo do cliente) é maior que a  do servidor
                else if (sourceFile.LastWriteTime > targetFile.LastWriteTime)
                {
                    Console.WriteLine($"Copiando {sourceFile.FullName} atualizado para ${servidor}");

                    //se for eu copio para o servidor
                    sourceFile.CopyTo(targetFile.FullName, true);
                }
            }

            //e os arquivos que sobraram na lista de a serem deletados vão ser deletados pois eles não existem mais no cliente
            foreach (var file in targetFilesToDelete)
            {

                Console.WriteLine($"Deletando {Path.Combine(servidor.FullName, file)} no ${servidor}");

                File.Delete(Path.Combine(servidor.FullName, file));
            }


            //pego todo os subdiretorios(subpastas) do diretorio do servidor 
            var targetSubDirsToDelete = servidor.GetDirectories().Select(fi => fi.Name).ToList();
            foreach (DirectoryInfo sourceSubDir in cliente.GetDirectories())
            {
                //pra cada subpasta do diretorio do cliente eu verifico se ela existe na lista de diretorio do servidor(targetSubDirsToDelete)
                if (targetSubDirsToDelete.Contains(sourceSubDir.Name))
                    targetSubDirsToDelete.Remove(sourceSubDir.Name);//se ela existe eu removo da lista de a serem deletados



                DirectoryInfo targetSubDir = new DirectoryInfo(Path.Combine(servidor.FullName, sourceSubDir.Name));
                Console.WriteLine($"Verificando subpasta {targetSubDir} no ${servidor.FullName}");

                SyncAll(sourceSubDir, targetSubDir);//chamo recursivamente para cada Subpasta do cliente 
            }

            //deleto  cada pasta que sobrou na lista de a serem deletados no servidor pois elas não existem mais no cliente.
            foreach (var subdir in targetSubDirsToDelete)
            {

                Console.WriteLine($"Deletando {subdir}");
                Directory.Delete(Path.Combine(servidor.FullName, subdir), true);
            }
        }


        private T Maior<T>(T a, T b) where T : IComparable => a.CompareTo(b) > 0 ? a : b;

    }
    
}
