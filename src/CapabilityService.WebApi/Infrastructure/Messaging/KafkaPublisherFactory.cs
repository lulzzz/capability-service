﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Configuration;

namespace DFDS.CapabilityService.WebApi.Infrastructure.Messaging
{
    public class KafkaPublisherFactory
    {
        private readonly KafkaConfiguration _configuration;

        public KafkaPublisherFactory(KafkaConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Producer<string, string> Create()
        {
            var config = _configuration.AsEnumerable().ToArray();
            
            return new Producer<string, string>(
                config: config,
                keySerializer: new StringSerializer(Encoding.UTF8),
                valueSerializer: new StringSerializer(Encoding.UTF8)
            );
        }

        public class KafkaConfiguration
        {
            private const string KEY_PREFIX = "CAPABILITY_SERVICE_KAFKA_";
            private readonly IConfiguration _configuration;

            public KafkaConfiguration(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            private string Key(string keyName) => string.Join("", KEY_PREFIX, keyName.ToUpper().Replace('.', '_'));

            private Tuple<string, string> GetConfiguration(string key)
            {
                var value = _configuration[Key(key)];

                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                return Tuple.Create<string, string>(key, value);
            }

            public IEnumerable<KeyValuePair<string, object>> AsEnumerable()
            {
                var configurationKeys = new[]
                {
                    "bootstrap.servers",
                    "broker.version.fallback",
                    "api.version.fallback.ms",
                    "ssl.ca.location",
                    "sasl.username",
                    "sasl.password",
                    "sasl.mechanisms",
                    "security.protocol",
                };

                var config = configurationKeys
                             .Select(key => GetConfiguration(key))
                             .Where(pair => pair != null)
                             .Select(pair => new KeyValuePair<string, object>(pair.Item1, pair.Item2))
                             .ToList();
                
                config.Add(new KeyValuePair<string, object>("request.timeout.ms", "3000"));

                return config;
            }
        }
    }
}