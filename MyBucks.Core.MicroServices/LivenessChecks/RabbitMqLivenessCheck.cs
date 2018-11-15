using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.ConfigurationModels;
using RabbitMQ.Client;

namespace MyBucks.Core.MicroServices.LivenessChecks
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

            return conn.IsOpen;
        }
    }
}