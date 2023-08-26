using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        string serverAddress = "127.0.0.1"; 
        int serverPort = 3000;

        try
        {
            using (TcpClient client = new TcpClient(serverAddress, serverPort))
            using (NetworkStream stream = client.GetStream())
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Console.WriteLine("Connected to the BetaCrew server.");

                SendStreamAllPacketsRequest(stream);

                List<StockPacket> packets = ReceivePackets(reader);

                GenerateJsonOutput(packets);

                Console.WriteLine("Data retrieval and JSON generation complete.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void SendStreamAllPacketsRequest(NetworkStream stream)
    {
        byte[] requestPayload = new byte[] { 1 };
        stream.Write(requestPayload, 0, requestPayload.Length);
    }

    static List<StockPacket> ReceivePackets(BinaryReader reader)
    {
        List<StockPacket> packets = new List<StockPacket>();

        while (reader.BaseStream.CanRead)
        {
            try
            {
                string symbol = Encoding.ASCII.GetString(reader.ReadBytes(4));
                char buySellIndicator = reader.ReadChar();
                int quantity = reader.ReadInt32();
                int price = reader.ReadInt32();
                int packetSequence = reader.ReadInt32();

                StockPacket packet = new StockPacket
                {
                    Symbol = symbol,
                    BuySellIndicator = buySellIndicator,
                    Quantity = quantity,
                    Price = price,
                    PacketSequence = packetSequence
                };

                packets.Add(packet);
            }
            catch (EndOfStreamException)
            {
                break;
            }
        }

        return packets;
    }

    static void GenerateJsonOutput(List<StockPacket> packets)
    {
        string json = JsonConvert.SerializeObject(packets, Formatting.Indented);

        File.WriteAllText("stock_data.json", json);
        Console.WriteLine("JSON output file created: stock_data.json");
    }
}

class StockPacket
{
    public string? Symbol { get; set; }
    public char BuySellIndicator { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int PacketSequence { get; set; }
}
