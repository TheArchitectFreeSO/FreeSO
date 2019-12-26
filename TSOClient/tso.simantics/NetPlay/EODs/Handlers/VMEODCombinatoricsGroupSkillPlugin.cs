using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.SimAntics.NetPlay.EODs.Utils;

namespace FSO.SimAntics.NetPlay.EODs.Handlers
{
    class VMEODCombinatoricsGroupSkillPlugin : VMEODHandler
    {
        private AbstractCombinatoricsPuzzleCardsDeck Deck;

        public VMEODCombinatoricsGroupSkillPlugin(VMEODServer server) : base(server)
        {
            Deck = AbstractCombinatoricsPuzzleCardsDeck.CreateNewDeck(AbstractCombinatoricsOrder.Seven);
        }

        public override void OnConnection(VMEODClient client)
        {
            client.Send("combinatorics_show", new byte[0]);
            base.OnConnection(client);
            Deck.DebugPrintAllCards();
        }
    }
}
