using System.Collections.Generic;
using System.Linq;

namespace Mahjong
{
    /// <summary>
    /// パイの情報
    /// </summary>
    public class Pai
    {
        /// <summary>
        /// 絵柄
        /// </summary>
        readonly public Group Group;

        /// <summary>
        /// 数字、字種
        /// </summary>
        readonly public Id Id;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="g"></param>
        /// <param name="i"></param>
        public Pai(Group g, Id i)
        {
            Group = g;
            Id = i;
            CheckValidation();
        }

        /// <summary>
        /// 表示順
        /// </summary>
        public int Priority
        {
            get
            {
                return (int)Group * 100 + (int)Id;
            }
        }

        /// <summary>
        /// 同じ牌か判定
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsSame(Pai p)
        {
            return IsSame(p.Group, p.Id);
        }
        /// <summary>
        /// 同じ牌か判定
        /// </summary>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsSame(Group g, Id i)
        {
            return Group == g && Id == i;
        }

        /// <summary>
        /// Group・Idに整合が保たれているかチェック
        /// </summary>
        private void CheckValidation()
        {
            //  字牌なのに、数字牌ならエラー
            if (Group == Group.Jihai)
            {
                if (Id <= Id.N9)
                {
                    throw new System.Exception();
                }
            }
            //  絵柄牌なのに、字牌ならエラー
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
        /// <summary>
        /// 山の中の牌で一意の値
        /// </summary>
        readonly public int Serial;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="s"></param>
        public DistributedPai(Group g, Id i, int s)
            : base(g, i)
        {
            Serial = s;
            Reset();
        }

        /// <summary>
        /// 情報をリセット
        /// </summary>
        public void Reset()
        {
            IsTsumo = false;
            IsTrashed = false;
            SerialFuro = 0;
            _statusAgari = StatusAgari.None;
        }

        /// <summary>
        /// 捨てられた？
        /// </summary>
        public bool IsTrashed
        {
            get;
            private set;
        }
        /// <summary>
        /// 捨てられた
        /// </summary>
        public void Trash() { IsTrashed = true; }

        /// <summary>
        /// ツモられた？
        /// </summary>
        public bool IsTsumo
        {
            get;
            private set;
        }
        /// <summary>
        /// ツモられた
        /// </summary>
        public void Tsumo() { IsTsumo = true; }

        /// <summary>
        /// 副露した？
        /// </summary>
        public bool IsFuro
        {
            get { return SerialFuro != 0; }
        }
        /// <summary>
        /// 副露のセットシリアル
        /// 副露したセット内で同一の値
        /// 異なる副露同士では一意の値
        /// </summary>
        public int SerialFuro 
        {
            get;
            private set;
        }
        /// <summary>
        /// 副露する
        /// </summary>
        /// <param name="serial"></param>
        public void Furo(int serial) 
        {
            if (serial == 0) throw new System.Exception();
            SerialFuro = serial;
        }

        /// <summary>
        /// アガリ牌の状態
        /// </summary>
        private enum StatusAgari
        {
            None = 0,           //  初期値およびアガリ牌ではない
            TsumoAgari = 1,     //  ツモアガリ牌
            RonAgari = 2,       //  ロンアガリ牌
        }
        private StatusAgari _statusAgari = StatusAgari.None;
        /// <summary>
        /// アガリ牌設定
        /// </summary>
        /// <param name="tsumoAgari"></param>
        public void SetAgari(bool tsumoAgari)
        {
            if (tsumoAgari) _statusAgari = StatusAgari.TsumoAgari;
            else _statusAgari = StatusAgari.RonAgari;
        }
        /// <summary>
        /// ツモアガリの牌？
        /// </summary>
        public bool IsTsumoAgari { get { return _statusAgari == StatusAgari.TsumoAgari; } }
        /// <summary>
        /// ロンアガリの牌？
        /// </summary>
        public bool IsRonAgari { get { return _statusAgari == StatusAgari.RonAgari; } }
        /// <summary>
        /// アガリ方を問わないがアガリ牌？
        /// </summary>
        public bool IsAgari { get { return _statusAgari != StatusAgari.None; } }
    }


    

    /// <summary>
    /// 牌クラスの拡張メソッド
    /// </summary>
    public static class PaiExtention
    {
        private static Dictionary<Group, string> _shortGroupString = new Dictionary<Group, string>()
        {
            { Group.Manz,       "M"},
            { Group.Pinz,       "P"},
            { Group.Souz,       "S"},
            { Group.Jihai,      "J"},
            { Group.Invalid,    "x"},
        };
        private static Dictionary<Id, string> _shortIdString = new Dictionary<Id, string>()
        {
            { Id.N1,            "1" },
            { Id.N2,            "2" },
            { Id.N3,            "3" },
            { Id.N4,            "4" },
            { Id.N5,            "5" },
            { Id.N6,            "6" },
            { Id.N7,            "7" },
            { Id.N8,            "8" },
            { Id.N9,            "9" },
            { Id.Chun,          "C" },
            { Id.Haku,          " " },
            { Id.Hatsu,         "H" },
            { Id.Ton,           "T" },
            { Id.Nan,           "N" },
            { Id.Sha,           "S" },
            { Id.Pei,           "P" },
            { Id.Invalid,       "x" },
        };

        /// <summary>
        /// PaiをXXという形で文字列にする
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string ToShortString(this Pai p)
        {
            return string.Format($"{_shortGroupString[p.Group]}{_shortIdString[p.Id]}");
        }

        /// <summary>
        /// Paiのソート用
        /// </summary>
        /// <param name="p"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static int CompareTo(this Pai p, Pai o)
        {
            return p.Priority.CompareTo(o.Priority);
        }
    }

    /// <summary>
    /// 手配デバッグ用機能
    /// </summary>
    public static class DistributePaiExtention 
    {
        /*
        public interface IPaiDistributor
        {
            DistributedPai Supply(Group g, Id i, int s);
        }
        public class SimplePaiDistributor : IPaiDistributor
        {
            public DistributedPai Supply(Group g, Id i, int s)
            {
                return new DistributedPai(g, i, s);
            }
        }
        public class FromPaiManagerPaiDistributor : IPaiDistributor
        {
            private PaiManager _paiManager;

            public FromPaiManagerPaiDistributor(PaiManager pm)
            {
                _paiManager = pm;
            }
            public DistributedPai Supply(Group g, Id i, int s)
            {
                return _paiManager.DebugDistribute(g, i);
            }
        }
        private static IPaiDistributor _distributor = new SimplePaiDistributor();

        public static DistributedPai Distribute(Group g, Id i, int s)
        {
            if (_distributor == null) { _distributor = new SimplePaiDistributor(); }

            return _distributor.Supply(g, i, s);
        }
        public static void SupplyFromPaiManager(PaiManager paiManager)
        {
            _distributor = new FromPaiManagerPaiDistributor(paiManager);

            return _distributor.Supply(g, i, s);
        }
        */
        /// <summary>
        /// シュンツをpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddShuntsu(this List<DistributedPai> pais, Group g, Id i)
        {
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i+1, pais.Count()))
                .AddInternal(new DistributedPai(g, i+2, pais.Count()));
        }
        /// <summary>
        /// 鳴きのシュンツをpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="nakiIndex"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddNakiShuntsu(this List<DistributedPai> pais, Group g, Id i, int nakiIndex)
        {
            int furo = pais.Count() + 1;
            return pais
                .AddInternal(new DistributedPai(g, i + 0, pais.Count()).SetFuro(furo, naki: nakiIndex == 0))
                .AddInternal(new DistributedPai(g, i + 1, pais.Count()).SetFuro(furo, naki: nakiIndex == 1))
                .AddInternal(new DistributedPai(g, i + 2, pais.Count()).SetFuro(furo, naki: nakiIndex == 2));
        }
        /// <summary>
        /// 暗刻をpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddAnko(this List<DistributedPai> pais, Group g, Id i)
        {
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i, pais.Count()));
        }
        /// <summary>
        /// 明刻をpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="nakiIndex"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddMinko(this List<DistributedPai> pais, Group g, Id i, int nakiIndex)
        {
            int furo = pais.Count() + 1;
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 0))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 1))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 2));
        }
        /// <summary>
        /// 暗槓をpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddAnkantsu(this List<DistributedPai> pais, Group g, Id i)
        {
            int furo = pais.Count() + 1;
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: false));
        }
        /// <summary>
        /// 明槓をpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="nakiIndex"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddMinkantsu(this List<DistributedPai> pais, Group g, Id i, int nakiIndex)
        {
            int furo = pais.Count() + 1;
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 0))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 1))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 2))
                .AddInternal(new DistributedPai(g, i, pais.Count()).SetFuro(furo, naki: nakiIndex == 3));
        }
        /// <summary>
        /// アタマをpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddHead(this List<DistributedPai> pais, Group g, Id i)
        {
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()))
                .AddInternal(new DistributedPai(g, i, pais.Count()));
        }
        /// <summary>
        /// 指定牌を１つpaisへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static List<DistributedPai> AddSingle(this List<DistributedPai> pais, Group g, Id i)
        {
            return pais
                .AddInternal(new DistributedPai(g, i, pais.Count()));
        }
        /// <summary>
        /// 指定牌をpaisへ追加する
        /// serialがかぶっていればエラー
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static List<DistributedPai> AddInternal(this List<DistributedPai> pais, DistributedPai p)
        {
            if (pais.Any((_ => _.Serial == p.Serial))) throw new System.Exception();
            pais.Add(p);
            return pais;
        }
        /// <summary>
        /// 副露指定する
        /// </summary>
        /// <param name="pai"></param>
        /// <param name="furo"></param>
        /// <param name="naki"></param>
        /// <returns></returns>
        private static DistributedPai SetFuro(this DistributedPai pai, int furo, bool naki)
        {
            pai.Furo(furo);
            if (naki) pai.Trash();
            return pai;
        }
    }
}
