## Queue Performance


### Run Rabbitmq 
docker run --rm -d --hostname my-rabbit -p 15672:15672 -p 5672:5672 --name some-rabbit rabbitmq:3-management

### Tests
Amqp can be tested with core.

MsMQ can be tested on windows.

ZeroMQ seems to be completely useless. So I was not able to get any sensible results here.


### My results.

#### Persistent queues
AmqpSender -> 1000 * 10 bytes    = 00:09:214
AmqpReceiver ->                  = 00:00:700
AmqpSender -> 1000 * 10240 bytes = 00:14:475
AmqpReceiver ->                  = 00:00:725
AmqpSender -> 10000 * 10 bytes   = 01:31:944
AmqpReceiver ->                  = 00:04:464
AmqpSender -> 10000 * 10240 bytes= 02:23:267
AmqpReceiver ->                  = 00:04:781
MsMQSender -> 1000 * 10 bytes    = 00:00:220
MsMQReceiver ->                  = 00:00:429
MsMQSender -> 1000 * 10240 bytes = 00:00:870
MsMQReceiver ->                  = 00:00:426
MsMQSender -> 10000 * 10 bytes   = 00:01:236
MsMQReceiver ->                  = 00:03:785
MsMQSender -> 10000 * 10240 bytes= 00:03:880
MsMQReceiver ->                  = 00:03:862

