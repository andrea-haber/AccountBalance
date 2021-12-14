using System;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;

namespace AccountBalance3 {
    public class BalanceReadModel :
        ReadModelBase,
        IHandle<AccountMsgs.Credit>,
        IHandle<AccountMsgs.Debit> {
        public BalanceReadModel(Func<IListener> listener, Guid accountId) : base(nameof(BalanceReadModel), listener) {
            EventStream.Subscribe<AccountMsgs.Credit>(this);
            EventStream.Subscribe<AccountMsgs.Debit>(this);
            Start<Account>(accountId);
        }

        private int _balance;

        public void Handle(AccountMsgs.Credit message) {
            _balance += (int)message.Amount;
            Redraw();
        }

        public void Handle(AccountMsgs.Debit message) {
            _balance -= (int)message.Amount;
            Redraw();
        }

        private void Redraw() {
            Console.Clear();
            Console.WriteLine($"balance = {_balance}");
        }
    }
}