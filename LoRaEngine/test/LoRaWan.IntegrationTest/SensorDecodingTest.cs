

using System;
using System.Threading.Tasks;
using Xunit;

namespace LoRaWan.IntegrationTest
{
    // Tests sensor decoding test (http, reflection)
    [Collection("ArduinoSerialCollection")] // run in serial
    public class SensorDecodingTest: IClassFixture<IntegrationTestFixture>, IDisposable
    {
        private readonly IntegrationTestFixture testFixture;
        private LoRaArduinoSerial lora;
        

        public SensorDecodingTest(IntegrationTestFixture testFixture)
        {
            this.testFixture = testFixture;
            this.lora = LoRaArduinoSerial.CreateFromPort(testFixture.Configuration.LeafDeviceSerialPort);
            this.testFixture.ClearNetworkServerLogEvents();
        }

        public void Dispose()
        {
            this.lora?.Dispose();
            this.lora = null;
            GC.SuppressFinalize(this);

        }


        // Ensures that http sensor decoder decodes payload
        // Uses device Device11_OTAA
        [Fact]
        public async Task SensorDecoder_HttpBased_ValueSensorDecoder_DecodesPayload()
        {
            var device = this.testFixture.Device11_OTAA;
            Console.WriteLine($"Starting {nameof(SensorDecoder_HttpBased_ValueSensorDecoder_DecodesPayload)} using device {device.DeviceID}");      

            await lora.setDeviceModeAsync(LoRaArduinoSerial._device_mode_t.LWOTAA);
            await lora.setIdAsync(device.DevAddr, device.DeviceID, device.AppEUI);
            await lora.setKeyAsync(device.NwkSKey, device.AppSKey, device.AppKey);

            await lora.SetupLora(this.testFixture.Configuration.LoraRegion);

            var joinSucceeded = await lora.setOTAAJoinAsyncWithRetry(LoRaArduinoSerial._otaa_join_cmd_t.JOIN, 20000, 5);

            if (!joinSucceeded)
            {                
                Assert.True(joinSucceeded, "Join failed");
            }

            // wait 1 second after joined
            await Task.Delay(Constants.DELAY_FOR_SERIAL_AFTER_JOIN); 

            
            await lora.transferPacketWithConfirmedAsync("1234", 10);

            await Task.Delay(Constants.DELAY_BETWEEN_MESSAGES);

            // +CMSG: ACK Received
            await AssertUtils.ContainsWithRetriesAsync("+CMSG: ACK Received", this.lora.SerialLogs);

            // Find "0000000000000011: message '{"value":1234}' sent to hub" in network server logs
            await this.testFixture.ValidateNetworkServerEventLogStartsWithAsync($"{device.DeviceID}: message '{{\"value\":1234}}' sent to hub");

            this.lora.ClearSerialLogs();
            testFixture.ClearNetworkServerLogEvents();
        }

        // Ensures that reflect based sensor decoder decodes payload
        // Uses Device12_OTAA
        [Fact]
        public async Task SensorDecoder_ReflectionBased_ValueSensorDecoder_DecodesPayload()
        {
            var device = this.testFixture.Device12_OTAA;
            Console.WriteLine($"Starting {nameof(SensorDecoder_HttpBased_ValueSensorDecoder_DecodesPayload)} using device {device.DeviceID}");      

            await lora.setDeviceModeAsync(LoRaArduinoSerial._device_mode_t.LWOTAA);
            await lora.setIdAsync(device.DevAddr, device.DeviceID, device.AppEUI);
            await lora.setKeyAsync(device.NwkSKey, device.AppSKey, device.AppKey);

            await lora.SetupLora(this.testFixture.Configuration.LoraRegion);

            var joinSucceeded = await lora.setOTAAJoinAsyncWithRetry(LoRaArduinoSerial._otaa_join_cmd_t.JOIN, 20000, 5);

            if (!joinSucceeded)
            {                
                Assert.True(joinSucceeded, "Join failed");
            }

            // wait 1 second after joined
            await Task.Delay(Constants.DELAY_FOR_SERIAL_AFTER_JOIN); 

            
            await lora.transferPacketWithConfirmedAsync("4321", 10);

            await Task.Delay(Constants.DELAY_BETWEEN_MESSAGES);

            // +CMSG: ACK Received
            await AssertUtils.ContainsWithRetriesAsync("+CMSG: ACK Received", this.lora.SerialLogs);

            // Find "0000000000000011: message '{"value":1234}' sent to hub" in network server logs
            await this.testFixture.ValidateNetworkServerEventLogStartsWithAsync($"{device.DeviceID}: message '{{\"value\":4321}}' sent to hub");

            this.lora.ClearSerialLogs();
            testFixture.ClearNetworkServerLogEvents();
        }
    }

}