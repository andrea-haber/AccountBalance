using System;
using EventStore.ClientAPI;
using ReactiveDomain;
using ReactiveDomain.EventStore;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;
using ReactiveDomain.Util;


namespace AccountBalance2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var app = new Application();
            app.Bootstrap();
            app.Run();
            Console.ReadLine();
        }
    }

    public class Application
    {
        private IRepository _repo;
        private readonly Guid _accountId = Guid.Parse("06AC5641-EDE6-466F-9B37-DD8304D05A85");

        private BalanceReadModel _readModel;

        public void Bootstrap()
        {
            var conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113");
            conn.Connected += (_, __) => Console.WriteLine("Connected");
            conn.ConnectAsync().Wait();
            var namer = new PrefixedCamelCaseStreamNameBuilder();
            IStreamStoreConnection streamConn = new EventStoreConnectionWrapper(conn);

            var ser = new JsonMessageSerializer();

            _repo = new StreamStoreRepository(namer, streamConn, ser);

            try
            {
                _repo.Save(new Account(_accountId));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            IListener listener = new QueuedStreamListener("Account", streamConn, namer, ser);

            _readModel = new BalanceReadModel(() => listener, _accountId);
        }

        public void Run()
        {
            var line = Console.ReadLine();
            var cmd = line?.Split();

            do
            {
                if (line == null) continue;

                switch (cmd?[0].ToLower())
                {
                    case "credit":
                        var acct = _repo.GetById<Account>(_accountId);
                        acct.Credit(uint.Parse(cmd[1]));
                        _repo.Save(acct);
                        break;
                }
                cmd = Console.ReadLine()?.Split(' ');

            } while (cmd?[0].ToLower() != "exit");
        }
    }

    public class BalanceReadModel :
        ReadModelBase,
        IHandle<Credit>,
        IHandle<Debit>
    {

        public BalanceReadModel(Func<IListener> listener, Guid accountGuid) 
            : base("BalanceReadModel", listener)
        {
            EventStream.Subscribe<Credit>(this);
            EventStream.Subscribe<Debit>(this);
            Start<Account>(accountGuid);
        }

        private int _balance;

        public void Handle(Credit message)
        {
            _balance += (int) message.Amount;
            
            Redraw();
        }

        public void Handle(Debit message)
        {
            _balance -= (int) message.Amount;
            Redraw();
        }

        private void Redraw()
        {
            Console.Clear();
            Console.WriteLine($"balance = {_balance}");
        }
    }

    public class Account : EventDrivenStateMachine
    {
        private long _balance;

        private Account()
        {
            Setup();
        }

        public Account(Guid id) : this()
        {
            Raise(new AccountCreated(id));
        }



        private void Setup()
        {
            Register<AccountCreated>(evt => Id = evt.Id);
            Register<Debit>(Apply);
            Register<Credit>(Apply);
        }

        private void Apply(Debit @event)
        {
            _balance -= @event.Amount;
        }

        private void Apply(Credit @event)
        {
            _balance += @event.Amount;
        }

        public void Credit(uint amount)
        {
            Raise(new Credit(amount));
        }

        public void Debit(uint amount)
        {
            Ensure.Nonnegative(_balance - amount, "Balance");

            Raise(new Debit(amount));
        }
    }

    public class AccountCreated : Message
    {
        public readonly Guid Id;

        public AccountCreated(Guid id)
        {
            Id = id;
        }
    }

    public class Debit : Message
    {
        public readonly uint Amount;

        public Debit(uint amount)
        {
            Amount = amount;
        }
    }

    public class Credit : Message
    {
        public readonly uint Amount;

        public Credit(uint amount)
        {
            Amount = amount;
        }
    }
}