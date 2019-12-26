using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.SimAntics.NetPlay.EODs.Utils
{
    public class AbstractCombinatoricsPuzzleCardsDeck
    {
        private List<AbstractCombinatoricsPuzzleCard> Deck;
        private List<AbstractCombinatoricsPuzzleCard> DeadCards;
        private Random Random = new Random();

        public double DeckID { get; }
        public AbstractCombinatoricsOrder PrimeOrder { get; }

        public static AbstractCombinatoricsPuzzleCardsDeck CreateNewDeck(AbstractCombinatoricsOrder order)
        {
            return new AbstractCombinatoricsPuzzleCardsDeck(order);
        }

        private AbstractCombinatoricsPuzzleCardsDeck(AbstractCombinatoricsOrder order)
        {
            DeckID = System.DateTime.Now.ToOADate();
            PrimeOrder = order;
            int orderInt = (int)order;
            int count = orderInt * orderInt + orderInt + 1;
            Deck = new List<AbstractCombinatoricsPuzzleCard>(count);
            DeadCards = new List<AbstractCombinatoricsPuzzleCard>(count);
            for (int index = 0; index < count; index++)
                Deck.Add(new AbstractCombinatoricsPuzzleCard(DeckID, order));

            // make the cards
            int deckIndex = 0;
            // first cards (order * order)
            for (int i = 0; i < orderInt; i++)
            {
                for (int j = 0; j < orderInt; j++)
                {
                    for (int k = 0; k < orderInt; k++)
                        Deck[deckIndex].Lines.Add((byte)((i * k + j) % orderInt * orderInt + k));
                    Deck[deckIndex].Lines.Add((byte)(orderInt * orderInt + i));
                    deckIndex++;
                }
            }
            // following cards (order)
            for (int i = 0; i < orderInt; i++)
            {
                for (int j = 0; j < orderInt; j++)
                    Deck[deckIndex].Lines.Add((byte)(j * orderInt + i));
                Deck[deckIndex].Lines.Add((byte)(orderInt * orderInt + orderInt));
                deckIndex++;
            }
            // final card
            for (int i = 0; i <= orderInt; i++)
                Deck[deckIndex].Lines.Add((byte)(orderInt * orderInt + i));

            // shuffle it
            Shuffle();
        }

        public void ResetAndShuffle()
        {
            lock (Deck)
            {
                if (DeadCards.Count > 0)
                    Deck.AddRange(DeadCards);
            }
            Shuffle();
        }

        public AbstractCombinatoricsPuzzleCard DrawCard()
        {
            AbstractCombinatoricsPuzzleCard card = null;
            if (Deck.Count > 0)
            {
                lock (Deck)
                {
                    card = Deck[0];
                    Deck.Remove(card);
                    DeadCards.Insert(0, card);
                }
            }
            return card;
        }

        public void ReplaceCard(AbstractCombinatoricsPuzzleCard card)
        {
            if (card != null && DeadCards.Contains(card))
            {
                Deck.Add(card);
                DeadCards.Remove(card);
            }
        }

        /// <summary>
        /// Richard Durstenfeld version of Fisher–Yates shuffle -- To shuffle an array a of n elements(indices 0..n-1):
        ///   for i from n−1 downto 1 do
        ///   j ← random integer such that 0 ≤ j ≤ i
        ///   exchange a[j] and a[i]
        /// </summary>
        private void Shuffle()
        {
            lock (Deck)
            {
                AbstractCombinatoricsPuzzleCard tempCard = null;
                for (int index = Deck.Count - 1; index > 0; index--)
                {
                    int random = Random.Next(0, index + 1);
                    if (index != random)
                    {
                        tempCard = Deck[index];
                        Deck[index] = Deck[random];
                        Deck[random] = tempCard;
                    }
                }
            }
        }

        public void DebugPrintAllCards()
        {
            lock (Deck) {
                for(int i = 0; i < Deck.Count; i++)
                {
                    var card = Deck[i];
                    Console.Write("Card #" + i + ": ");
                    foreach (var line in card.Lines)
                        Console.Write(" " + line);
                    Console.WriteLine("");
                }
            }
        }

    }

    public class AbstractCombinatoricsPuzzleCard
    {
        public double DeckID { get; }
        internal List<byte> Lines { get; }

        internal AbstractCombinatoricsPuzzleCard(double id, AbstractCombinatoricsOrder order)
        {
            DeckID = id;
            Lines = new List<byte>((int)order + 1);
        }
        public int TotalLines
        {
            get { return Lines.Capacity; }
        }
        public bool CheckSolution(byte line)
        {
            return Lines.Contains(line);
        }
    }

    public enum AbstractCombinatoricsOrder: byte
    {
        One = 1,
        Two = 2,
        Three = 3,
        Five = 5,
        Seven = 7,
        Eleven = 11,
        Thirteen = 13,
        // all prime numbers work, but 13 and below keeps us in bytes, because, Total = Order * Order + Order + 1
    }
}