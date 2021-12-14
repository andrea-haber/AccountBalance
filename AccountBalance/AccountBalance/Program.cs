using System;
using System.Text;
using EventStore.ClientAPI;
using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.EventStore;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;
using EventData = EventStore.ClientAPI.EventData;
using ExpectedVersion = EventStore.ClientAPI.ExpectedVersion;
using StreamPosition = EventStore.ClientAPI.StreamPosition;

namespace AccountBalance {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            IEventStoreConnection conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113");
            conn.Connected += (_, __) => Console.WriteLine("Connected");
            conn.ConnectAsync().Wait();

            conn.AppendToStreamAsync("Test", ExpectedVersion.Any, new[] {
                new EventData(
                    Guid.NewGuid(),
                    "TestEvent",
                    false,
                    Encoding.UTF8.GetBytes("Test event data"),
                    Encoding.UTF8.GetBytes("Test event Metadata"))
            });



            Console.WriteLine("Event Written");

            var evt = conn.ReadStreamEventsForwardAsync("Test", StreamPosition.Start, 1, false).Result;
            Console.WriteLine(Encoding.UTF8.GetString(evt.Events[0].Event.Data));
            Console.ReadKey();

            var nameBuilder = new PrefixedCamelCaseStreamNameBuilder();
            IStreamStoreConnection streamConn = new EventStoreConnectionWrapper(conn);
            var serializer = new JsonMessageSerializer();

            var repo = new StreamStoreRepository(
                            nameBuilder,
                            streamConn,
                            serializer);

            var accountId = Guid.NewGuid();
            repo.Save(new Account(accountId));


        }

        class AccountCreated : Message
        {
            public readonly Guid Id;

            public AccountCreated(Guid id)
            {
                Id = id;
            }
        }

        class Account : EventDrivenStateMachine
        {

            public Account(Guid id)
            {
                Setup();
                Raise(new AccountCreated(id));
            }


            private void Setup()
            {
                Register<AccountCreated>(evt => Id = evt.Id);
            }


        }

        class Credit : Event
        {
            // not impl
        }

        class MyReadModel :
                ReadModelBase,
                IHandle<Credit>
        {
            public MyReadModel(Func<IListener> listener, Guid accountGuid) : base("MyReadModel", listener)
            {
                Start<Account>();
                this.EventStream.Subscribe<Credit>(this);
            }

            public void Handle(Credit message)
            {
                throw new NotImplementedException();
            }
        }
    }
}
