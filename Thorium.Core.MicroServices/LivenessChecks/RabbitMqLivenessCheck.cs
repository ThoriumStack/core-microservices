using RabbitMQ.Client;
using Thorium.Core.MessageQueue.ConfigurationModel;
using Thorium.Core.MicroServices.Abstractions;
using Thorium.Core.MicroServices.ConfigurationModels;

namespace Thorium.Core.MicroServices.LivenessChecks
{
    public class RabbitMqLivenessCheck : ILivenessCheck
    {
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitMqLivenessCheck(RabbitMqSettings rabbitMqSettings)
        {
            _rabbitMqSettings = rabbitMqSettings;
        }

        public bool IsLive()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = _rabbitMqSettings.RabbitMqUsername;
            factory.Password = _rabbitMqSettings.RabbitMqPassword;
            factory.HostName = _rabbitMqSettings.RabbitMqHostname;

            IConnection conn = factory.CreateConnection();
            var open = conn.IsOpen;
            
            conn.Close();
            return open;
        }
    }
}