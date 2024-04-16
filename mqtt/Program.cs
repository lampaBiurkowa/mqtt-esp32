//sample base on https://blog.behroozbc.ir/mqtt-client-with-mqttnet-4-and-c
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using System.Text.Json;

var input1Topic = "test-topic-student-pak-analog-1";
var input2Topic = "test-topic-student-pak-analog-2";
var ledTopic = "test-topic-student-pak-led";
IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();

var builder = new MqttClientOptionsBuilder().WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer("test.mosquitto.org");

var options = new ManagedMqttClientOptionsBuilder()
                        .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                        .WithClientOptions(builder.Build())
                        .Build();

_mqttClient.ConnectedAsync += _mqttClient_ConnectedAsync;
_mqttClient.DisconnectedAsync += _mqttClient_DisconnectedAsync;
_mqttClient.ConnectingFailedAsync += _mqttClient_ConnectingFailedAsync;
_mqttClient.ApplicationMessageReceivedAsync += _mqqtClient_MessageReceived;
await _mqttClient.StartAsync(options);
await _mqttClient.SubscribeAsync(input1Topic);
await _mqttClient.SubscribeAsync(input2Topic);
var rand = new Random();
while (true)
{
    string json = JsonSerializer.Serialize(new { Blink = rand.Next(2), Sent = DateTime.UtcNow });
    await _mqttClient.EnqueueAsync(ledTopic, json);
    Console.WriteLine($"send: {json}");

    await Task.Delay(TimeSpan.FromSeconds(5));
}

Task _mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
{
    Console.WriteLine("Connected");
    return Task.CompletedTask;
};

Task _mqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
{
    Console.WriteLine("Disconnected");
    return Task.CompletedTask;
};

Task _mqttClient_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
{
    Console.WriteLine("Connection failed check network or broker!");
    return Task.CompletedTask;
}

Task _mqqtClient_MessageReceived(MqttApplicationMessageReceivedEventArgs msg)
{
    var payload = msg.ApplicationMessage.PayloadSegment;
    if (payload.Count > 0)
        Console.WriteLine($"rec: {msg.ApplicationMessage.Topic} - {payload[0]}");

    return Task.CompletedTask;
}
