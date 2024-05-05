using System.Net.Sockets;
using System.Net;
using System.Text;

class Server
{
    static readonly object _lock = new object();
    static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
    private static int YOUR_KEY = 3;

    static void Main(string[] args)
    {
        int count = 1;

        TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
        ServerSocket.Start();

        while (true)
        {
            TcpClient client = ServerSocket.AcceptTcpClient();
            lock (_lock) list_clients.Add(count, client);
            Console.WriteLine("Someone connected!!");

            Thread t = new Thread(handle_clients);
            t.Start(count);
            count++;
        }
    }

    public static void handle_clients(object o)
    {
        int id = (int)o;
        TcpClient client;

        lock (_lock) client = list_clients[id];

        while (true)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int byte_count = stream.Read(buffer, 0, buffer.Length);

            if (byte_count == 0)
            {
                break;
            }

            string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
            broadcast(data);
            string decryptedData = DeCode(data, YOUR_KEY);
            //Console.WriteLine(data);
            Console.WriteLine("Client {0}: {1}", id, data);
            Console.WriteLine("DeCoding Client {0}: {1}", id, decryptedData);
        }

        lock (_lock) list_clients.Remove(id);
        client.Client.Shutdown(SocketShutdown.Both);
        client.Close();
    }

    public static void broadcast(string data)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

        lock (_lock)
        {
            foreach (TcpClient c in list_clients.Values)
            {
                NetworkStream stream = c.GetStream();

                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }

    private static char Cipher(char c, int key)
    {
        if(!char.IsLetter(c))
        {
            return c;
        }
        char offset = char.IsUpper(c) ? 'A' : 'a';
        return (char)((c + key - offset) % 26 + offset);
    }

    private static string EnCode(string input, int key)
    {
        string output = string.Empty;
        foreach(char c in input)
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