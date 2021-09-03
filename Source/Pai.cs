using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong
{
    /// <summary>
    /// パイの情報
    /// </summary>
    public class Pai
    {
        readonly public Group Group;
        readonly public Id Id;

        public Pai(Group g, Id i)
        {
            Group = g;
            Id = i;
        }

        public int Priority
        {
            get
            {
                return (int)Group * 100 + (int)Id;
            }
        }
    }

    /// <summary>
    /// 山に存在するパイ
    /// </summary>
    public sealed class DistributedPai : Pai
    {
        readonly public int Serial;

        public DistributedPai(Group g, Id i, int s)
            : base(g, i)
        {
            if (s < 0 || s >= (int)Constants.MaxPaiCount) throw new System.ArgumentException($"serial={s} is invalid.");

            Serial = s;
            Reset();
        }

        public void Reset()
        {
            IsTsumo = false;
            IsTrashed = false;
        }
        public bool IsTrashed
        {
            get;
            private set;
        }

        public void Trash() { IsTrashed = true; }


        public bool IsTsumo
        {
            get;
            private set;
        }

        public void Tsumo() { IsTsumo = true; }
    }


    public class PaiManager
    {
        private DistributedPai[] _distributedPais = new DistributedPai[(int)Constants.MaxPaiCount];
        private int _distributedIndex;
        
        public PaiManager()
        {
            
        }

        public void Initialize(int seed)
        {
            _distributedIndex = 0;

            int s = 0;
            //  数字牌
            for (int n = 0; n < (int)Constants.MaxIdDirection; ++n)
            {
                for (int g = 0; g < (int)Constants.MaxGroupNumber; ++g)
                {
                    for (int i = 0; i < (int)Constants.MaxIdNumber; ++i)
                    {
                        _distributedPais[s] = new DistributedPai(Group.Manz + g, Id.N1 + i, s);
                        ++s;
                    }
                }
            }
            //  字牌
            for (int n = 0; n < (int)Constants.MaxIdDirection; ++n)
            {
                for (int i = 0; i < (int)Constants.MaxIdJihai; ++i)
                {
                    _distributedPais[s] = new DistributedPai(Group.Jihai, Id.Ton + i, s);
                    ++s;
                }
            }

            //  シャッフル
            var random = new System.Random(seed);
            _distributedPais = _distributedPais.OrderBy(x => random.Next(_distributedPais.Length)).ToArray();
        }
        
        public void Dump()
        {
            string output = "\n";
            foreach(var p in _distributedPais)
            {
                output += $"{p.Group} / {p.Id} / {p.Serial}\n";
            }
            UnityEngine.Debug.Log(output);
        }

        public DistributedPai Distribute()
        {
            var p = _distributedPais[_distributedIndex];
            ++_distributedIndex;
            return p;
        }

        public bool IsEmpty()
        {
            return _distributedIndex == _distributedPais.Length;
        }
    }


}
