using System.IO.Ports;
using System.Net.Http;
using System.Threading.Tasks;

class SerialListener
{
    static SerialPort _serialPort;
    static HttpClient _client = new HttpClient();

    static async Task Main(string[] args)
    {
        _serialPort = new SerialPort("COM3", 9600);
        _serialPort.DataReceived += SerialDataReceived;
        _serialPort.Open();

        Console.WriteLine("Listening to Arduino...");
        await Task.Delay(-1);
    }

    private static async void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string data = _serialPort.ReadLine().Trim();
        if (data == "Pressed")
        {
            await _client.PostAsync("https://localhost:7134/api/Measurements/PostButtonPress", new StringContent(""));
            Console.WriteLine("Button press sent to API.");
        }
    }
}
