using System.Text;
using System.Text.Json;

namespace Gamification.NativeMessagingHost;

class Program{
    private static string LogFilePath = Path.Combine(AppContext.BaseDirectory, "native_host_msg_logs.txt");
    private static string APIBase = "https://localhost:7131/SiteMonitor";
    
    static async Task Main(string[] args){
        Log("Native Host Started");

        try{
            while (true){
                string? incomingJson = ReadMessage(Console.OpenStandardInput());
                if (string.IsNullOrEmpty(incomingJson)){
                    Log("Received null or empty message, or stream ended. Exiting.");
                    break;
                }

                // Log($"Received from Chrome: {incomingJson}");

                //Process the message
                var request = JsonSerializer.Deserialize<MessageBase>(incomingJson);

                var response = new MessageBase{ Text = $"Native host processed: {request?.Text}" };
                string outgoingJson = JsonSerializer.Serialize(response);

                // Log($"Sending to Chrome: {outgoingJson}");
                WriteMessage(Console.OpenStandardOutput(), outgoingJson);
            }
        }
        catch (Exception ex){
            Log($"ERROR: {ex.Message}\n{ex.StackTrace}");
        }
        finally{
            Log("Native Host Stopped");
            await NotifyBrowserClosure();
        }
    }

    public static string ReadMessage(Stream stdin){
        byte[] lengthBytes = new byte[4];
        int bytesRead = stdin.Read(lengthBytes, 0, 4);

        if (bytesRead == 0) return null;
        if (bytesRead < 4){
            Log($"ReadMessage error: Expected 4 length bytes, got {bytesRead}");
            throw new IOException("Failed to read message length (not enough bytes).");
        }

        int messageLength = BitConverter.ToInt32(lengthBytes, 0);
        // Log($"Message length: {messageLength}");

        if (messageLength == 0){
            Log("ReadMessage Error: Received empty message");
            return "{}";
        }

        if (messageLength > 2 * 1024){//Safety limit of 2kb
            Log($"ReadMessage Error: Message length {messageLength} exceeds safety limit.");
            throw new IOException($"Message length {messageLength} is too large.");
        }

        byte[] messageByte = new byte[messageLength];
        bytesRead = 0;
        int currentChunkRead;
        while (bytesRead < messageLength &&
               (currentChunkRead = stdin.Read(messageByte, bytesRead, messageLength - bytesRead)) > 0){
            bytesRead += currentChunkRead;
        }

        if (bytesRead < messageLength){
            Log($"ReadMessage Error: Expected {messageLength} message bytes, got {bytesRead}");
            throw new IOException("Failed to read complete message (not enough bytes).");
        }
        
        return Encoding.UTF8.GetString(messageByte);
    }

    public static void WriteMessage(Stream stdout, string jsonMessage){
        byte[] messageBytes =  Encoding.UTF8.GetBytes(jsonMessage);
        int messageLength = messageBytes.Length;
        byte[] lengthByte = BitConverter.GetBytes(messageLength);
        
        stdout.Write(lengthByte, 0, 4);
        stdout.Write(messageBytes, 0, messageBytes.Length);
        stdout.Flush();
    }

    public static void Log(string message){
        try{
            File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}");
        }
        catch{
            // Ignore logging errors if they occur (e.g., permissions)
        }
    }

    public static async Task NotifyBrowserClosure(){
        string finalAPI = APIBase + "/BrowserClosed";
        try{
            HttpClient client = new HttpClient();
            using HttpResponseMessage res = await client.GetAsync(finalAPI);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex){
            Log("Couldn't notify!. Error: " + ex.Message + " " + ex.StackTrace);
        }
    }
    
    // Simple DTO for messages
    public class MessageBase{
        public string? Text { get; set; }
    }
}