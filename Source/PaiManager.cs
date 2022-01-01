using System.Linq;

namespace Mahjong
{
    /// <summary>
    /// 牌マネージャ
    /// 山を扱う
    /// </summary>
    public class PaiManager : IPaiManager
    {
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="shuffle"></param>
        /// <param name="wanpai"></param>
        public void Initialize(int seed, bool shuffle = true, bool wanpai = true)
        {
            _distributedIndex = 0;
            _wanpaiBeginIndex = 0;
            _dorapaiBeginIndex = 0;
            _rinshanCount = 0;

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
            //  牌を初期化した数のチェック
            if (s < 0 || s >= (int)Constants.MaxPaiCount) throw new System.ArgumentException($"serial={s} is invalid.");



            //  シャッフル
            if (shuffle)
            {
                var random = new System.Random(seed);
                _distributedPais = _distributedPais.OrderBy(x => random.Next(_distributedPais.Length)).ToArray();
            }


            //  王牌分だけすすめる
            if (wanpai)
            {
                InitializeWanpaiDoras();
            }
        }

        /// <summary>
        /// 王牌の初期化
        /// およびドラの設定
        /// </summary>
        public void InitializeWanpaiDoras()
        {
            //  リンシャン分だけ回す
            _wanpaiBeginIndex = _distributedIndex;  //  覚えておく
            for (int i = 0; i < 4; ++i)
            {
                //  TODO
                Distribute();
            }

            //  ドラ
            _dorapaiBeginIndex = _distributedIndex;  //  覚えておく
            for (int i = 0; i < 10; ++i)
            {
                Distribute();
            }
        }

        /// <summary>
        /// 牌の情報表示
        /// </summary>
        public void Dump()
        {
            string output = "\n";
            foreach (var p in _distributedPais)
            {
                output += $"{p.Group} / {p.Id} / {p.Serial}\n";
            }
            UnityEngine.Debug.Log(output);
        }

        /// <summary>
        /// 山から配布
        /// </summary>
        /// <returns></returns>
        public DistributedPai Distribute()
        {
            var p = _distributedPais[_distributedIndex];
            ++_distributedIndex;
            return p;
        }

        /// <summary>
        /// 特定の牌を残りの山の先頭から指定牌にする
        /// すでに山にその牌が無い場合はスルー
        /// </summary>
        /// <param name="pais"></param>
        public void DebugDistribute(System.Collections.Generic.IEnumerable<System.Tuple<Group, Id>> pais)
        {
            int tempDisributeIndex = _distributedIndex;

            foreach (var t in pais)
            {
                for (int ii = tempDisributeIndex; ii < _distributedPais.Length; ++ii)
                {
                    if (_distributedPais[ii].IsSame(t.Item1, t.Item2))
                    {
                        var iPai = _distributedPais[ii];
                        var tPai = _distributedPais[tempDisributeIndex];

                        _distributedPais[tempDisributeIndex] = iPai;
                        _distributedPais[ii] = tPai;

                        ++tempDisributeIndex;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 山に牌が無い
        /// </summary>
        public bool IsEmpty()
        {
            return CountRemainedPais() == 0;
        }
        /// <summary>
        /// 山の残りの牌数
        /// </summary>
        public int CountRemainedPais()
        {
            return _distributedPais.Length - _distributedIndex;
        }

        private DistributedPai[] _distributedPais = new DistributedPai[(int)Constants.MaxPaiCount];
        private int _distributedIndex;
        private int _wanpaiBeginIndex;
        private int _dorapaiBeginIndex;
        private int _rinshanCount;
    }
}