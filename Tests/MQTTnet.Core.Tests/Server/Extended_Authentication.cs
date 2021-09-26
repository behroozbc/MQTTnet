using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MQTTnet.Client.ExtendedAuthentication;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace MQTTnet.Tests.Server
{
    [TestClass]
    public sealed class Extended_Authentication : BaseTestClass
    {
        [DataTestMethod]
        [DataRow(MqttProtocolVersion.V310, true)]
        [DataRow(MqttProtocolVersion.V311, true)]
        [DataRow(MqttProtocolVersion.V500, false)]
        [TestMethod]
        public async Task Throw_Not_Supported_For_Unsupported_Version(MqttProtocolVersion protocolVersion, bool throwsException)
        {
            Exception validatorException = null;

            using (var testEnvironment = CreateTestEnvironment(protocolVersion))
            {
                await testEnvironment.StartServer(o =>
                {
                    o.WithConnectionValidator(c =>
                    {
                        try
                        {
                            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                            {
                                c.ExtendedAuthenticationHandler.SendAuthenticationDataAsync(Encoding.UTF8.GetBytes("D"), false, timeout.Token);
                            }
                        }
                        catch (Exception exception)
                        {
                            validatorException = exception;
                        }
                    });
                });

                await testEnvironment.ConnectClient(o => o
                    .WithExtendedAuthenticationHandler(new DemoExtendedAuthenticationHandler())
                    .WithCommunicationTimeout(TimeSpan.FromSeconds(5))
                    .WithAuthentication("Demo"));

                if (throwsException)
                {
                    Assert.IsNotNull(validatorException);
                }
                else
                {
                    Assert.AreEqual(null, validatorException);
                }
            }
        }

        [TestMethod]
        public async Task Full_Server_Initiated_Flow_On_Connect()
        {
            Exception validatorException = null;

            using (var testEnvironment = CreateTestEnvironment(MqttProtocolVersion.V500))
            {
                await testEnvironment.StartServer(o =>
                {
                    o.WithConnectionValidator(async c =>
                    {
                        try
                        {
                            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                            {
                                // The client should reply with 1 for A, 2 for B and 3 for C.
                                await c.ExtendedAuthenticationHandler.SendAuthenticationDataAsync(Encoding.UTF8.GetBytes("A"), true, timeout.Token);
                                var response1 = await c.ExtendedAuthenticationHandler.ReceiveAuthenticationDataAsync(timeout.Token);
                                await c.ExtendedAuthenticationHandler.SendAuthenticationDataAsync(Encoding.UTF8.GetBytes("B"), true, timeout.Token);
                                var response2 = await c.ExtendedAuthenticationHandler.ReceiveAuthenticationDataAsync(timeout.Token);
                                await c.ExtendedAuthenticationHandler.SendAuthenticationDataAsync(Encoding.UTF8.GetBytes("C"), false, timeout.Token);
                                var response3 = await c.ExtendedAuthenticationHandler.ReceiveAuthenticationDataAsync(timeout.Token);

                                var fullResponse = Encoding.UTF8.GetString(response1.AuthenticationData) + Encoding.UTF8.GetString(response2.AuthenticationData) + Encoding.UTF8.GetString(response3.AuthenticationData);

                                Assert.AreEqual("123", fullResponse);
                            }
                        }
                        catch (Exception exception)
                        {
                            validatorException = exception;
                        }
                    });
                });

                await testEnvironment.ConnectClient(o => o
                    .WithCommunicationTimeout(TimeSpan.FromSeconds(5))
                    .WithExtendedAuthenticationHandler(new DemoExtendedAuthenticationHandler())
                    .WithAuthentication("Demo"));

                Assert.AreEqual(null, validatorException);
            }
        }

        [TestMethod]
        public async Task Full_Server_Initiated_Flow_On_Connect_With_Failure()
        {
            using (var testEnvironment = CreateTestEnvironment(MqttProtocolVersion.V500))
            {
                 testEnvironment.IgnoreClientLogErrors = true;
                
                await testEnvironment.StartServer(o =>
                {
                    o.WithConnectionValidator(async c =>
                    {
                        try
                        {
                            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                            {
                                await c.ExtendedAuthenticationHandler.SendAuthenticationDataAsync(Encoding.UTF8.GetBytes("A"), true, timeout.Token);
                                var response1 = await c.ExtendedAuthenticationHandler.ReceiveAuthenticationDataAsync(timeout.Token);
                                await c.ExtendedAuthenticationHandler.SendAuthenticationDataAsync(Encoding.UTF8.GetBytes("B"), true, timeout.Token);
                                var response2 = await c.ExtendedAuthenticationHandler.ReceiveAuthenticationDataAsync(timeout.Token);
                                await c.ExtendedAuthenticationHandler.SendAuthenticationDataAsync(Encoding.UTF8.GetBytes("C"), false, timeout.Token);
                                var response3 = await c.ExtendedAuthenticationHandler.ReceiveAuthenticationDataAsync(timeout.Token);

                                var fullResponse = Encoding.UTF8.GetString(response1.AuthenticationData) + Encoding.UTF8.GetString(response2.AuthenticationData) + Encoding.UTF8.GetString(response3.AuthenticationData);

                                Assert.AreEqual("XXX", fullResponse);
                            }
                        }
                        catch
                        {
                            c.ReasonCode = MqttConnectReasonCode.NotAuthorized;
                        }
                    });
                });

                try
                {
                    await testEnvironment.ConnectClient(o => o
                        .WithCommunicationTimeout(TimeSpan.FromSeconds(5))
                        .WithExtendedAuthenticationHandler(new DemoExtendedAuthenticationHandler())
                        .WithAuthentication("Demo"));

                    Assert.Fail();
                }
                catch
                {
                }
            }
        }

        class DemoExtendedAuthenticationHandler : IMqttExtendedAuthenticationHandler
        {
            // TODO: Use "StartReAuthentication()";
            
            public Task StartExtendedAuthentication(MqttExtendedAuthenticationRequest initialRequest)
            {
                return Task.CompletedTask;
            }

            public Task EndExtendedAuthentication()
            {
                return Task.CompletedTask;
            }

            public Task HandleExtendedAuthenticationAsync(MqttExtendedAuthenticationContext context)
            {
                if (Encoding.UTF8.GetString(context.Request.AuthenticationData) == "A")
                {
                    context.Response = new MqttExtendedAuthenticationResponse
                    {
                        AuthenticationData = Encoding.UTF8.GetBytes("1")
                    };
                }

                if (Encoding.UTF8.GetString(context.Request.AuthenticationData) == "B")
                {
                    context.Response = new MqttExtendedAuthenticationResponse
                    {
                        AuthenticationData = Encoding.UTF8.GetBytes("2")
                    };
                }

                if (Encoding.UTF8.GetString(context.Request.AuthenticationData) == "C")
                {
                    context.Response = new MqttExtendedAuthenticationResponse
                    {
                        AuthenticationData = Encoding.UTF8.GetBytes("3")
                    };
                }

                return Task.CompletedTask;
            }
        }
    }
}