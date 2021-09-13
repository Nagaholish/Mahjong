using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    public class Context
    {
        public void Initialize()
        {
            Field = 0;
            Kyoku = 0;
            Honba = 0;
            TurnCount = 0;
            CurrentTurnPlayer = 0;
            CurrentOyaPlayer = 0;
            _players = new Player[4];
            for(int i=0; i< _players.Length; ++i)
            {
                var p = new Player();
                {
                    p.Initialize(i, 25000);
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
        /// 東一局=Field==0 && Kyoku==0
        /// 南オーラス=Field==1 && Kyoku==3
        /// </summary>
        public int Kyoku { get; private set; }
        public int Honba { get; private set; }

        public int TurnCount { get; private set; }
        public int CurrentTurnPlayer { get; private set; }
        public int CurrentOyaPlayer { get; private set; }
        public int[] WinnerPlayers { get; private set; }
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

        public void NextTurn()
        {
            CurrentTurnPlayer += 1;
            CurrentTurnPlayer %= _players.Length;
            ++TurnCount;
        }

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
                    Kyoku = 0;
                }
                Honba = 0;
                CurrentOyaPlayer += 1;
                CurrentOyaPlayer %= _players.Length;
                CurrentTurnPlayer = CurrentOyaPlayer;
            }
        }
        public bool IsEndKyoku()
        {
            //  流局
            //  TODO    上がった人がいた
            return GetPaiManager().IsEmpty();
        }
        public bool IsEndHanchan()
        {
            //  オーラス
            //  TODO    はコリ
            return Field == 1 && Kyoku == 3;
        }

        public int PlayerCount
        {
            get
            {
                if (_players == null) { throw new System.Exception(); }
                return _players.Length;
            }
        }

        public Player GetFocusPlayer()
        {
            return _players[CurrentTurnPlayer];
        }

        public IEnumerable<Player> GetPlayers()
        {
            return _players;

        }

        public PaiManager GetPaiManager()
        {
            return _paiManager;
        }

        private Player[] _players = null;
        private PaiManager _paiManager = null;
    }
}