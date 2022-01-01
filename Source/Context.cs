using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    /// <summary>
    /// ルールや場の情報の集合
    /// および他システムへの窓口
    /// </summary>
    public class Context
    {
        /// <summary>
        /// 初期化
        /// ゲームのリセット
        /// </summary>
        public void Initialize()
        {
            Field = 0;
            Kyoku = 1;
            Honba = 1;
            TurnCount = 0;
            CurrentTurnPlayer = 0;
            CurrentOyaPlayer = 0;
            _players = new Player[4];
            for(int i=0; i< _players.Length; ++i)
            {
                var p = new Player();
                {
                    p.Initialize(i, (int)Constants.DefaultScore);
                }
                _players[i] = p;
            }
            _paiManager = new PaiManager();
        }

        /// <summary>
        /// 東場=0
        /// 南場=1
        /// 西場=2
        /// 北場=3
        /// </summary>
        public int Field { get; private set; }
        public Id FieldToId
        { 
            get
            {
                switch (Field) 
                {
                    case 0: return Id.Ton;
                    case 1: return Id.Nan;
                    case 2: return Id.Sha;
                    case 3: return Id.Pei;
                }
                throw new System.Exception();
            }
        }
        /// <summary>
        /// 局
        /// 東一局=Field==0 && Kyoku==1
        /// 南オーラス=Field==1 && Kyoku==4
        /// </summary>
        public int Kyoku { get; private set; }
        
        /// <summary>
        /// 本場
        /// 一本場=1
        /// 二本場=2
        /// </summary>
        public int Honba { get; private set; }

        /// <summary>
        /// ターンカウント
        /// 一人がツモして捨てるまでを1ターンとする
        /// 1巡回ると4ターンになる
        /// </summary>
        public int TurnCount { get; private set; }
        
        /// <summary>
        /// 現在ターンのプレイヤー
        /// </summary>
        public int CurrentTurnPlayer { get; private set; }

        /// <summary>
        /// 現在の親のプレイヤー
        /// </summary>
        public int CurrentOyaPlayer { get; private set; }

        /// <summary>
        /// 親判定
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsOyaPlayer(int index)
        {
            return index == CurrentOyaPlayer;
        }

        /// <summary>
        /// アガリプレイヤーのindex
        /// 配列なのはダブロン、トリロンの可能性のため
        /// </summary>
        public int[] WinnerPlayers { get; private set; }

        /// <summary>
        /// 指定プレイヤーの風牌を取得する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Id PlayerToId(int index)
        {
            var oya = CurrentOyaPlayer;
            for (int i=0; i< PlayerCount; ++i)
            {
                if ((oya + i) % PlayerCount == index)
                {
                    return Id.Ton + i;
                }
            }
            throw new System.Exception();
        }

        /// <summary>
        /// 次の巡への処理
        /// </summary>
        public void NextTurn()
        {
            CurrentTurnPlayer += 1;
            CurrentTurnPlayer %= _players.Length;
            ++TurnCount;
        }

        /// <summary>
        /// 次の局への処理
        /// </summary>
        /// <param name="retry">連チャンしているならtrue</param>
        public void NextKyoku(bool retry)
        {
            if (retry)
            {
                Honba += 1;
                CurrentTurnPlayer = CurrentOyaPlayer;
            }
            else
            {
                Kyoku += 1;
                if (Kyoku > 3)
                {
                    Field += 1;
                    Kyoku = 1;
                }
                Honba = 1;
                CurrentOyaPlayer += 1;
                CurrentOyaPlayer %= _players.Length;
                CurrentTurnPlayer = CurrentOyaPlayer;
            }
            TurnCount = 0;
        }

        /// <summary>
        /// 1局終了判定
        /// </summary>
        /// <returns></returns>
        public bool IsEndKyoku()
        {
            //  流局
            //  TODO    上がった人がいた
            return _paiManager.IsEmpty();
        }

        /// <summary>
        /// 半荘終了判定
        /// </summary>
        /// <returns></returns>
        public bool IsEndHanchan()
        {
            //  オーラス
            //  TODO    はコリ
            return Field == 1 && Kyoku == 4;
        }

        /// <summary>
        /// プレイヤー数取得
        /// </summary>
        public int PlayerCount
        {
            get
            {
                if (_players == null) { throw new System.Exception(); }
                return _players.Length;
            }
        }

        /// <summary>
        /// 現在ターンプレイヤーを取得
        /// </summary>
        /// <returns></returns>
        public Player GetCurrentTurnPlayer()
        {
            return _players[CurrentTurnPlayer];
        }

        /// <summary>
        /// 全プレイヤー取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Player> GetPlayers()
        {
            return _players;
        }

        public void InitializePaiManager(IPaiManager manager)
        {
            if (null == manager) throw new System.Exception("PaiManager is null.");

            _paiManager = manager;
        }
        /// <summary>
        /// 牌マネージャ取得
        /// 廃止予定
        /// </summary>
        /// <returns></returns>
        public IPaiManager GetPaiManager()
        {
            return _paiManager;
        }
        /// <summary>
        /// 場の牌が空か？
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyPais()
        {
            return _paiManager.IsEmpty();
        }

        /// <summary>
        /// 表ドラ取得
        /// 最大４つまで
        /// </summary>
        public IEnumerable<Pai> OmoteDoras
        {
            get { return _omoteDoras; }
        }
        /// <summary>
        /// 裏ドラ取得
        /// 最大４つまで
        /// </summary>
        public IEnumerable<Pai> UraDoras
        {
            get { return _uraDoras; }
        }
        /// <summary>
        /// 表ドラ追加
        /// </summary>
        /// <param name="p"></param>
        public void AddDora(Pai p)
        {
            _omoteDoras.Add(p);
        }
        /// <summary>
        /// 裏ドラ追加
        /// </summary>
        /// <param name="p"></param>
        public void AddUraDora(Pai p)
        {
            _uraDoras.Add(p);
        }

        private Player[] _players = null;
        private IPaiManager _paiManager = null;
        private List<Pai> _omoteDoras = new List<Pai>(capacity: 4);
        private List<Pai> _uraDoras = new List<Pai>(capacity: 4);
    }
}