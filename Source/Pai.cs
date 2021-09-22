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
            CheckValidation();
        }

        public int Priority
        {
            get
            {
                return (int)Group * 100 + (int)Id;
            }
        }

        public bool IsSame(Pai p)
        {
            return IsSame(p.Group, p.Id);
        }
        public bool IsSame(Group g, Id i)
        {
            return Group == g && Id == i;
        }

        private void CheckValidation()
        {
            if (Group == Group.Jihai)
            {
                if (Id <= Id.N9)
                {
                    throw new System.Exception();
                }
            }
            else if (Group <= Group.Manz && Group <= Group.Souz)
            {
                if (Id >= Id.Ton)
                {
                    throw new System.Exception();
                }
            }
        }

        public override string ToString()
        {
            return this.ToShortString();
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
            SerialFuro = 0;
            _statusAgari = StatusAgari.None;
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


        public bool IsFuro
        {
            get { return SerialFuro != 0; }
        }
        public int SerialFuro 
        {
            get;
            private set;
        }
        public void Furo(int serial) { SerialFuro = serial; }

        private enum StatusAgari
        {
            None = 0,
            TsumoAgari = 1,
            RonAgari = 2,
        }
        private StatusAgari _statusAgari = StatusAgari.None;
        public void SetAgari(bool tsumoAgari)
        {
            if (tsumoAgari) _statusAgari = StatusAgari.TsumoAgari;
            else _statusAgari = StatusAgari.RonAgari;
        }
        public bool IsTsumoAgari { get { return _statusAgari == StatusAgari.TsumoAgari; } }
        public bool IsRonAgari { get { return _statusAgari == StatusAgari.RonAgari; } }
        public bool IsAgari { get { return _statusAgari != StatusAgari.None; } }
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

    public static class PaiExtention
    {
        private static Dictionary<Group, string> _shortGroupString = new Dictionary<Group, string>()
        {
            { Group.Manz, "M"},
            { Group.Pinz, "P"},
            { Group.Souz, "S"},
            { Group.Jihai, "J"},
            { Group.Invalid, "x"},
        };
        private static Dictionary<Id, string> _shortIdString = new Dictionary<Id, string>()
        {
            { Id.N1, "1" },
            { Id.N2, "2" },
            { Id.N3, "3" },
            { Id.N4, "4" },
            { Id.N5, "5" },
            { Id.N6, "6" },
            { Id.N7, "7" },
            { Id.N8, "8" },
            { Id.N9, "9" },
            { Id.Chun, "C" },
            { Id.Haku, " " },
            { Id.Hatsu, "H" },
            { Id.Ton, "T" },
            { Id.Nan, "N" },
            { Id.Sha, "S" },
            { Id.Pei, "P" },
            { Id.Invalid, "x" },
        };
        public static string ToShortString(this Pai p)
        {
            return string.Format($"{_shortGroupString[p.Group]}{_shortIdString[p.Id]}");
        }


        public static int CompareTo(this Pai p, Pai o)
        {
            return p.Priority.CompareTo(o.Priority);
        }
    }

    public static class DistributePaiExtention 
    {
        public static List<DistributedPai> AddShuntsu(this List<DistributedPai> pais, Group g, Id i)
        {
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i+1, pais.Count()))
                .AddInternal(new DistributedPai(g, i+2, pais.Count()));
        }
        public static List<DistributedPai> AddNakiShuntsu(this List<DistributedPai> pais, Group g, Id i)
        {
            int furo = pais.Count();
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: true))
                .AddInternal(new DistributedPai(g, i + 1, pais.Count()).SetFuro(furo, naki: true))
                .AddInternal(new DistributedPai(g, i + 2, pais.Count()).SetFuro(furo, naki: true));
        }
        public static List<DistributedPai> AddAnko(this List<DistributedPai> pais, Group g, Id i)
        {
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i, pais.Count()));
        }
        public static List<DistributedPai> AddMinko(this List<DistributedPai> pais, Group g, Id i, int nakiIndex)
        {
            int furo = pais.Count();
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 0))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 1))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 2));
        }
        public static List<DistributedPai> AddAnkantsu(this List<DistributedPai> pais, Group g, Id i)
        {
            var furo = pais.Count();
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false));
        }
        public static List<DistributedPai> AddMinkantsu(this List<DistributedPai> pais, Group g, Id i, int nakiIndex)
        {
            var furo = pais.Count();
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 0))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 1))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 2))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 3));
        }
        public static List<DistributedPai> AddHead(this List<DistributedPai> pais, Group g, Id i)
        {
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i, pais.Count()));
        }
        private static List<DistributedPai> AddInternal(this List<DistributedPai> pais, DistributedPai p)
        {
            if (pais.Any((_ => _.Serial == p.Serial))) throw new System.Exception();
            pais.Add(p);
            return pais;
        }

        private static DistributedPai SetFuro(this DistributedPai pai, int furo, bool naki)
        {
            pai.Furo(furo);
            if (naki) pai.Trash();
            return pai;
        }
    }
}
