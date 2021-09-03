using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    public class Player
    {
        public void Initialize(int index, int score)
        {
            Index = index;
            Score = score;
            _pais = new List<DistributedPai>();
        }
        
        public int Index { get; private set; }

        public int Score { get; private set; }

        public IEnumerable<Pai> Pais
        {
            get { return _pais; }
        }

        public void Tsumo(DistributedPai pai)
        {
            _pais.Add(pai);
            Sort();
        }

        public void Trash(int index)
        {
            _pais.RemoveAt(index);
            Sort();
        }

        public void Sort()
        {
            _pais.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        private List<DistributedPai> _pais = default;
    }
}