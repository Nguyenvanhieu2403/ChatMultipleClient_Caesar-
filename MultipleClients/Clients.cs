using System.Net.Sockets;
using System.Net;
using System.Text;

class Clients
{
    private static int YOUR_KEY = 3;
    static void Main(string[] args)
    {
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        int port = 5000;
        TcpClient client = new TcpClient();
        client.Connect(ip, port);
        Console.WriteLine("client connected!!");
        NetworkStream ns = client.GetStream();
        Thread thread = new Thread(o => ReceiveData((TcpClient)o));

        thread.Start(client);

        string s;
        while (!string.IsNullOrEmpty((s = Console.ReadLine())))
        {
            string encryptedData = EnCode(s, YOUR_KEY);
            byte[] buffer = Encoding.ASCII.GetBytes(encryptedData);
            ns.Write(buffer, 0, buffer.Length);
        }

        client.Client.Shutdown(SocketShutdown.Send);
        thread.Join();
        ns.Close();
        client.Close();
        Console.WriteLine("disconnect from server!!");
        Console.ReadKey();
    }

    static void ReceiveData(TcpClient client)
    {
        NetworkStream ns = client.GetStream();
        byte[] receivedBytes = new byte[1024];
        int byte_count;

        while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
        {
            //Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            string receivedData = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
            string decryptedData = DeCode(receivedData, YOUR_KEY); // Thay YOUR_KEY bằng giá trị khóa của bạn
            Console.Write(decryptedData);
        }
    }

    private static char Cipher(char c, int key)
    {
        if (!char.IsLetter(c))
        {
            return c;
        }
        char offset = char.IsUpper(c) ? 'A' : 'a';
        return (char)((c + key - offset) % 26 + offset);
    }

    private static string EnCode(string input, int key)
    {
        string output = string.Empty;
        foreach (char c in input)
        {
            output += Cipher(c, key);
        }
        return output;
    }

    private static string DeCode(string input, int key)
    {
        return EnCode(input, 26 - key);
    }
}