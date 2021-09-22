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
            Reset();
        }
        public void Reset()
        {
            if (_pais == null)_pais = new List<DistributedPai>();
            _pais.Clear();
            IsReach = false;
            IsIppatsu = false;
            IsDoubleReach = false;
        }
        
        public int Index { get; private set; }

        public int Score { get; private set; }

        private List<DistributedPai> _pais = default; 
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
            _pais.Sort((a, b) => a.CompareTo(b));
        }

        public void Reach()
        {
            IsReach = true;
        }
        public bool IsReach { get; private set; }

        public void Ippatsu()
        {
            IsIppatsu = true;
        }
        public bool IsIppatsu { get; private set; }

        public void DoubleReach()
        {
            IsDoubleReach = true;
        }
        public bool IsDoubleReach { get; private set; }
    }
}