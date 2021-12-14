using System;
using EventStore.ClientAPI;
using ReactiveDomain;
using ReactiveDomain.EventStore;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace AccountBalance3
{
    public class Application : IDisposable
    {
        private IStreamStoreConnection _conn;
        private IRepository _repo;
        private BalanceReadModel _readModel;
        private IDispatcher _dispatcher;
        private AccountSvc _accountSvc;

        //private readonly Guid _accountId = Guid.Parse("06AC5641-EDE6-466F-9B37-DD8304D05A84");
        private readonly Guid _accountId = Guid.NewGuid();

        public void Bootstrap()
        {
            var esConnection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113");
            _conn = new EventStoreConnectionWrapper(esConnection);
            esConnection.Connected += (_, __) => Console.WriteLine("Connected");
            esConnection.ConnectAsync().Wait();
            IStreamNameBuilder namer = new PrefixedCamelCaseStreamNameBuilder();
            IEventSerializer ser = new JsonMessageSerializer();
            _repo = new StreamStoreRepository(namer, _conn, ser);
            _dispatcher = new Dispatcher("main");
            _accountSvc = new AccountSvc(_dispatcher, _repo);
            try
            {
                _dispatcher.Send(new AccountMsgs.CreateAccount(_accountId));

            }
            catch (Exception e) 
            {
            }
            IListener listener = new StreamListener("Account", _conn, namer, ser);
            _readModel = new BalanceReadModel(() => listener, _accountId);
        }

        public void Run()
        {
            string[] cmd;
            do
            {

                cmd = Console.ReadLine().Split(' ');
                switch (cmd[0].ToLower())
                {
                    case "credit":
                        _dispatcher.Send(MessageBuilder
                            .New(() => new AccountMsgs.CreditAccount(
                                                 _accountId,
                                                 uint.Parse(cmd[1]))));
                        Console.WriteLine($"got credit {cmd[1]}");
                        break;
                    case "debit":
                        try
                        {
                            _dispatcher.Send(new AccountMsgs.DebitAccount(
                                                    _accountId,
                                                    uint.Parse(cmd[1])));
                            Console.WriteLine($"got debit {cmd[1]}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        break;
                }

            } while (cmd[0].ToLower() != "exit");
        }

        public void Dispose()
        {
            _readModel?.Dispose();
            _conn?.Dispose();
            _dispatcher.Dispose();
            _accountSvc.Dispose();
        }
    }
}