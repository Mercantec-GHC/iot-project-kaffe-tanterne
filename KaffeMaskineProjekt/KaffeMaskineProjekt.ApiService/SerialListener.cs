using System.IO.Ports;
using System.Net.Http;
using System.Threading.Tasks;

class SerialListener
{
    static SerialPort _serialPort;
    static HttpClient _client = new HttpClient();

    public static async Task Main(string[] args)
    {
        _serialPort = new SerialPort("COM3", 9600);
        _serialPort.DataReceived += SerialDataReceived;
        _serialPort.Open();

        Console.WriteLine("Listening to Arduino...");
        await Task.Delay(-1);
    }

    public static async void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string data = _serialPort.ReadLine().Trim();
        if (data == "Pressed")
        {
            int orderId = 1;
            var response = await _client.PutAsync($"https://localhost:7134/api/Orders/ButtonServe/{orderId}", null);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Button press sent to API.");
            }
            else
            {
                Console.WriteLine($"API error: {response.StatusCode}");
            }
        }
    }
}
