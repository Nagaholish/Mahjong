using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    /// <summary>
    /// プレイヤー情報
    /// </summary>
    public class Player
    {
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="index">0だからといって起家とは限らない</param>
        /// <param name="score">初期所持点数</param>
        public void Initialize(int index, int score)
        {
            Index = index;
            Score = score;
            Reset();
        }

        /// <summary>
        /// リセット
        /// 1局ごとにリセットされる
        /// </summary>
        public void Reset()
        {
            if (_pais == null)_pais = new List<DistributedPai>();
            _pais.Clear();
            IsReach = false;
            IsIppatsu = false;
            IsDoubleReach = false;
        }
        
        /// <summary>
        /// 管理番号
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// 現在の点数
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// 点数計算
        /// </summary>
        /// <param name="value">valueが負なら減算</param>
        public void AddScore(int value)
        {
            Score += value;
        }

        /// <summary>
        /// 手持ち牌
        /// 13牌、14牌、14牌以上になっていることもある
        /// </summary>
        public IEnumerable<DistributedPai> MyPais
        {
            get { return _pais; }
        }

        /// <summary>
        /// ツモる
        /// </summary>
        /// <param name="pai"></param>
        public void Tsumo(DistributedPai pai)
        {
            _pais.Add(pai);
            Sort();
        }

        /// <summary>
        /// 捨てる
        /// </summary>
        /// <param name="index"></param>
        public void Trash(int index)
        {
            _pais.RemoveAt(index);
            Sort();
        }

        /// <summary>
        /// 並び替える
        /// </summary>
        public void Sort()
        {
            _pais.Sort((a, b) => a.CompareTo(b));
        }

        /// <summary>
        /// リーチする
        /// </summary>
        public void Reach()
        {
            IsReach = true;
        }
        /// <summary>
        /// リーチ中？
        /// </summary>
        public bool IsReach { get; private set; }

        /// <summary>
        /// 一発適用する
        /// </summary>
        public void Ippatsu()
        {
            IsIppatsu = true;
        }
        /// <summary>
        /// 一発適用される？
        /// </summary>
        public bool IsIppatsu { get; private set; }

        /// <summary>
        /// ダブルリーチする
        /// </summary>
        public void DoubleReach()
        {
            IsDoubleReach = true;
        }
        /// <summary>
        /// ダブルリーチ中？
        /// </summary>
        public bool IsDoubleReach { get; private set; }


        private List<DistributedPai> _pais = default;

    }
}