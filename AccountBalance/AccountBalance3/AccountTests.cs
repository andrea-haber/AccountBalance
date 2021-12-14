using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveDomain.Messaging;
using Xunit;

namespace AccountBalance3
{
    public class AccountTests
    {
        [Fact]
        public void can_credit()
        {
            var id = Guid.NewGuid();
            var a = new Account(id, CorrelatedMessage.NewRoot());
            const uint amt = 100U;
            a.Credit(amt, CorrelatedMessage.NewRoot());
            Assert.True(a.HasRecordedEvents);
            var evts = a.TakeEvents();
            Assert.Equal(2, evts.Length);
            Assert.IsType<AccountMsgs.AccountCreated>(evts[0]);
            Assert.Equal(id, ((AccountMsgs.AccountCreated)evts[0]).AccountId);
            Assert.IsType<AccountMsgs.Credit>(evts[1]);
            Assert.Equal(amt, ((AccountMsgs.Credit)evts[1]).Amount);
        }
        [Fact]
        public void can_not_debit_empty_account()
        {
            var id = Guid.NewGuid();
            var a = new Account(id, CorrelatedMessage.NewRoot());

            const uint amt = 100U;
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Debit(amt, CorrelatedMessage.NewRoot()));
        }
    }
}
