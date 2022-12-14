using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Best Cahe Validation
//https://towardsdatascience.com/beat-cache-invalidation-in-asp-net-core-using-kafka-and-debezium-65cd1d80554d

namespace KafkaCache.CacheBuilder
{
    class KafkaConsumer
    {
         static void Main(string[] args)
        {
            var consumerConfig = new ConsumerConfig
            {
                GroupId = "products.cache.builder.group.id",
                BootstrapServers = "localhost:29092",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            var producerConfig = new ProducerConfig { BootstrapServers = "localhost:29092" };
            var cacheTopic = "products.cache";

            using (var c = new ConsumerBuilder<string, string>(consumerConfig).Build())
            {
                c.Subscribe("mysql.mystore.products");
                try
                {
                    using (var p = new ProducerBuilder<int, string>(producerConfig).Build())
                    {
                        while (true)
                        {
                            try
                            {
                                var cr = c.Consume();
                                var key = JsonConvert.DeserializeObject<ProductKey>(cr.Key);

                                Console.WriteLine(cr.Key.ToString());
                                p.ProduceAsync(cacheTopic, new Message<int, string> { Value = cr.Value, Key = key.Id });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw e;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }
    }
}
